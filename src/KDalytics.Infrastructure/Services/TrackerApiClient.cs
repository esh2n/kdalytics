using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using KDalytics.Core.Interfaces.Services;
using KDalytics.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KDalytics.Infrastructure.Services;

/// <summary>
/// Tracker Network API クライアントの実装
/// </summary>
public class TrackerApiClient : ITrackerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TrackerApiClient> _logger;
    private readonly TrackerApiSettings _settings;
    private readonly SemaphoreSlim _requestThrottler = new SemaphoreSlim(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="httpClient">HTTPクライアント</param>
    /// <param name="options">API設定</param>
    /// <param name="logger">ロガー</param>
    public TrackerApiClient(
        HttpClient httpClient,
        IOptions<ApiSettings> options,
        ILogger<TrackerApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = options?.Value?.Tracker ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // ベースURLとデフォルトヘッダーの設定
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // APIキーが設定されている場合はヘッダーに追加
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("TRN-Api-Key", _settings.ApiKey);
        }

        // JSONシリアライズオプション
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc />
    public async Task<TrackerProfileResponse> GetPlayerProfileAsync(string name, string tag, bool forceCollect = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentNullException(nameof(tag));

        string encodedName = WebUtility.UrlEncode(name);
        string encodedTag = WebUtility.UrlEncode(tag);
        string endpoint = $"/standard/profile/riot/{encodedName}%23{encodedTag}";

        // 強制取得フラグが設定されている場合はパラメータを追加
        if (forceCollect)
        {
            endpoint += "?forceCollect=true";
        }

        return await SendRequestAsync<TrackerProfileResponse>(endpoint, cancellationToken);
    }

    /// <summary>
    /// APIリクエストを送信して結果を取得する内部メソッド
    /// </summary>
    /// <typeparam name="T">レスポンス型</typeparam>
    /// <param name="endpoint">APIエンドポイント</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>APIレスポンス</returns>
    private async Task<T> SendRequestAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        // リクエスト間隔を制御するためのセマフォ取得
        await _requestThrottler.WaitAsync(cancellationToken);

        try
        {
            // 前回のリクエストからの経過時間を計算
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            var minimumRequestInterval = TimeSpan.FromMilliseconds(_settings.RequestDelayMs);

            // 最小リクエスト間隔を満たしていない場合は待機
            if (timeSinceLastRequest < minimumRequestInterval)
            {
                var delayTime = minimumRequestInterval - timeSinceLastRequest;
                _logger.LogDebug("APIリクエスト間隔制限のため{DelayMs}ms待機します", delayTime.TotalMilliseconds);
                await Task.Delay(delayTime, cancellationToken);
            }

            // リトライロジック
            int retryCount = 0;
            int maxRetries = _settings.RetryCount;
            int initialDelayMs = _settings.RetryInitialDelayMs;
            double backoffMultiplier = _settings.RetryBackoffMultiplier;

            while (true)
            {
                try
                {
                    _lastRequestTime = DateTime.UtcNow;
                    var response = await _httpClient.GetAsync(endpoint, cancellationToken);

                    // レート制限に達した場合は待機してリトライ
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        if (retryCount >= maxRetries)
                        {
                            _logger.LogWarning("APIレート制限に達し、最大リトライ回数を超えました: {Endpoint}", endpoint);
                            throw new HttpRequestException($"APIレート制限に達し、リトライに失敗しました: {response.StatusCode}");
                        }

                        int delayMs = (int)(initialDelayMs * Math.Pow(backoffMultiplier, retryCount));
                        _logger.LogInformation("APIレート制限に達しました。{DelayMs}ms後にリトライします（リトライ: {RetryCount}/{MaxRetries}）", delayMs, retryCount + 1, maxRetries);
                        await Task.Delay(delayMs, cancellationToken);
                        retryCount++;
                        continue;
                    }

                    // 404の場合は特別なハンドリング（プレイヤーが見つからない場合など）
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogInformation("APIエンドポイントでデータが見つかりませんでした: {Endpoint}", endpoint);
                        // デフォルト値を持つオブジェクトを返す
                        return JsonSerializer.Deserialize<T>("{}", _jsonOptions)
                            ?? throw new JsonException("デフォルトオブジェクトのデシリアライズに失敗しました");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning("APIリクエストエラー: {StatusCode} - {Error} - {Endpoint}", response.StatusCode, errorContent, endpoint);
                        throw new HttpRequestException($"APIリクエストが失敗しました: {response.StatusCode}, {errorContent}");
                    }

                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    if (result == null)
                    {
                        throw new JsonException("APIレスポンスのデシリアライズに失敗しました");
                    }

                    _logger.LogDebug("APIリクエスト成功: {Endpoint}", endpoint);
                    return result;
                }
                catch (HttpRequestException ex) when (retryCount < maxRetries &&
                    (ex.Message.Contains("TooManyRequests") ||
                     ex.Message.Contains("500") ||
                     ex.Message.Contains("503") ||
                     ex.Message.Contains("タイムアウト")))
                {
                    // サーバーエラーやタイムアウトの場合もリトライ
                    int delayMs = (int)(initialDelayMs * Math.Pow(backoffMultiplier, retryCount));
                    _logger.LogWarning("APIリクエスト一時エラー、{DelayMs}ms後にリトライします（リトライ: {RetryCount}/{MaxRetries}）: {Error}", delayMs, retryCount + 1, maxRetries, ex.Message);
                    await Task.Delay(delayMs, cancellationToken);
                    retryCount++;
                }
                catch (Exception ex)
                {
                    // その他のエラーは即時失敗
                    _logger.LogError(ex, "APIリクエスト処理中にエラーが発生しました: {Endpoint}", endpoint);
                    throw;
                }
            }
        }
        finally
        {
            // セマフォを解放
            _requestThrottler.Release();
        }
    }
}