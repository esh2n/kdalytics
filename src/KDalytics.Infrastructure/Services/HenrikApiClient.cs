using System;
using System.Collections.Generic;
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
/// Henrik API クライアントの実装
/// </summary>
public class HenrikApiClient : IHenrikApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HenrikApiClient> _logger;
    private readonly HenrikApiSettings _settings;
    private readonly SemaphoreSlim _requestThrottler = new SemaphoreSlim(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="httpClient">HTTPクライアント</param>
    /// <param name="options">API設定</param>
    /// <param name="logger">ロガー</param>
    public HenrikApiClient(
        HttpClient httpClient,
        IOptions<ApiSettings> options,
        ILogger<HenrikApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = options?.Value?.Henrik ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // ベースURLとデフォルトヘッダーの設定
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // APIキーが設定されている場合はヘッダーに追加
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", _settings.ApiKey);
        }

        // JSONシリアライズオプション
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc />
    public async Task<AccountInfoResponse> GetPlayerInfoAsync(string name, string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentNullException(nameof(tag));

        string encodedName = WebUtility.UrlEncode(name);
        string encodedTag = WebUtility.UrlEncode(tag);
        string endpoint = $"/v1/account/{encodedName}/{encodedTag}";

        return await SendRequestAsync<AccountInfoResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccountInfoResponse> GetPlayerInfoByPuuidAsync(string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
            throw new ArgumentNullException(nameof(puuid));

        string endpoint = $"/v1/by-puuid/account/{puuid}";

        return await SendRequestAsync<AccountInfoResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MmrInfoResponse> GetPlayerMmrAsync(string region, string name, string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(region))
            throw new ArgumentNullException(nameof(region));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentNullException(nameof(tag));

        string encodedName = WebUtility.UrlEncode(name);
        string encodedTag = WebUtility.UrlEncode(tag);
        string endpoint = $"/v2/mmr/{region}/{encodedName}/{encodedTag}";

        return await SendRequestAsync<MmrInfoResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MmrInfoResponse> GetPlayerMmrByPuuidAsync(string region, string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(region))
            throw new ArgumentNullException(nameof(region));
        if (string.IsNullOrEmpty(puuid))
            throw new ArgumentNullException(nameof(puuid));

        string endpoint = $"/v2/by-puuid/mmr/{region}/{puuid}";

        return await SendRequestAsync<MmrInfoResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MatchlistResponse> GetPlayerMatchesAsync(string region, string name, string tag, int count = 5, string mode = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(region))
            throw new ArgumentNullException(nameof(region));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentNullException(nameof(tag));

        string encodedName = WebUtility.UrlEncode(name);
        string encodedTag = WebUtility.UrlEncode(tag);
        string endpoint = $"/v3/matches/{region}/{encodedName}/{encodedTag}?size={count}";

        if (!string.IsNullOrEmpty(mode))
        {
            endpoint += $"&mode={WebUtility.UrlEncode(mode)}";
        }

        return await SendRequestAsync<MatchlistResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MatchlistResponse> GetPlayerMatchesByPuuidAsync(string region, string puuid, int count = 5, string mode = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(region))
            throw new ArgumentNullException(nameof(region));
        if (string.IsNullOrEmpty(puuid))
            throw new ArgumentNullException(nameof(puuid));

        string endpoint = $"/v3/by-puuid/matches/{region}/{puuid}?size={count}";

        if (!string.IsNullOrEmpty(mode))
        {
            endpoint += $"&mode={WebUtility.UrlEncode(mode)}";
        }

        return await SendRequestAsync<MatchlistResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MatchDetailsResponse> GetMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
            throw new ArgumentNullException(nameof(matchId));

        string endpoint = $"/v2/match/{matchId}";

        return await SendRequestAsync<MatchDetailsResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MatchDetailsResponse> GetStoredMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
            throw new ArgumentNullException(nameof(matchId));

        string endpoint = $"/v2/match/saved/{matchId}";

        return await SendRequestAsync<MatchDetailsResponse>(endpoint, cancellationToken);
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