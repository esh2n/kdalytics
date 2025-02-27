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
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // 未知のプロパティを無視する設定を追加
            IgnoreReadOnlyProperties = false,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() }
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

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v1/account/{name}/{tag} または /valorant/v2/account/{name}/{tag}
        string endpoint = $"/valorant/v1/account/{encodedName}/{encodedTag}";

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

        return await SendRequestAsync<AccountInfoResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccountInfoResponse> GetPlayerInfoByPuuidAsync(string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
            throw new ArgumentNullException(nameof(puuid));

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v1/by-puuid/account/{puuid}
        string endpoint = $"/valorant/v1/by-puuid/account/{puuid}";

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

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

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v2/mmr/{region}/{name}/{tag}
        string endpoint = $"/valorant/v2/mmr/{region}/{encodedName}/{encodedTag}";

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

        return await SendRequestAsync<MmrInfoResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MmrInfoResponse> GetPlayerMmrByPuuidAsync(string region, string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(region))
            throw new ArgumentNullException(nameof(region));
        if (string.IsNullOrEmpty(puuid))
            throw new ArgumentNullException(nameof(puuid));

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v2/by-puuid/mmr/{region}/{puuid}
        string endpoint = $"/valorant/v2/by-puuid/mmr/{region}/{puuid}";

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

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

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v4/matches/{region}/{platform}/{name}/{tag}
        string platform = "pc"; // VALORANTはPC版のみなのでpcを固定
        string endpoint = $"/valorant/v4/matches/{region}/{platform}/{encodedName}/{encodedTag}?size={count}";

        if (!string.IsNullOrEmpty(mode))
        {
            endpoint += $"&mode={WebUtility.UrlEncode(mode)}";
        }

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

        var response = await SendRequestAsync<MatchlistResponse>(endpoint, cancellationToken);

        // v4 APIレスポンスの場合、Metadataからマッチ情報をMatchIdにコピーする
        if (response.Status == 200 && response.Data != null)
        {
            foreach (var matchData in response.Data)
            {
                if (matchData.Metadata != null && !string.IsNullOrEmpty(matchData.Metadata.Match_id))
                {
                    // MetadataのMatch_idをMatchIdフィールドにコピー
                    matchData.MatchId = matchData.Metadata.Match_id;

                    // その他の情報も必要に応じてコピー
                    if (matchData.GameStart == 0 && !string.IsNullOrEmpty(matchData.Metadata.Started_at))
                    {
                        // ISO 8601形式の日時文字列をUnixタイムスタンプに変換
                        if (DateTime.TryParse(matchData.Metadata.Started_at, out DateTime startTime))
                        {
                            matchData.GameStart = ((DateTimeOffset)startTime).ToUnixTimeMilliseconds();
                        }
                    }

                    // マップ情報をコピー
                    if (matchData.Metadata.Map != null)
                    {
                        if (string.IsNullOrEmpty(matchData.MapId) && !string.IsNullOrEmpty(matchData.Metadata.Map.Id))
                            matchData.MapId = matchData.Metadata.Map.Id;
                    }

                    // キュー情報をコピー
                    if (matchData.Metadata.Queue != null)
                    {
                        if (string.IsNullOrEmpty(matchData.QueueType) && !string.IsNullOrEmpty(matchData.Metadata.Queue.Id))
                            matchData.QueueType = matchData.Metadata.Queue.Id;

                        // キュー情報からゲームモードを設定
                        if (string.IsNullOrEmpty(matchData.GameMode) && !string.IsNullOrEmpty(matchData.Metadata.Queue.Id))
                            matchData.GameMode = matchData.Metadata.Queue.Id;
                    }
                }
            }
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<MatchlistResponse> GetPlayerMatchesByPuuidAsync(string region, string puuid, int count = 20, string mode = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(region))
            throw new ArgumentNullException(nameof(region));
        if (string.IsNullOrEmpty(puuid))
            throw new ArgumentNullException(nameof(puuid));

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v4/by-puuid/matches/{region}/{platform}/{puuid}
        // platformはpcまたはconsoleを指定（デフォルトはpc）
        string platform = "pc"; // VALORANTはPC版のみなのでpcを固定
        string endpoint = $"/valorant/v4/by-puuid/matches/{region}/{platform}/{puuid}?size={count}";

        if (!string.IsNullOrEmpty(mode))
        {
            endpoint += $"&mode={WebUtility.UrlEncode(mode)}";
        }

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

        var response = await SendRequestAsync<MatchlistResponse>(endpoint, cancellationToken);

        // v4 APIレスポンスの場合、Metadataからマッチ情報をMatchIdにコピーする
        if (response.Status == 200 && response.Data != null)
        {
            foreach (var matchData in response.Data)
            {
                if (matchData.Metadata != null && !string.IsNullOrEmpty(matchData.Metadata.Match_id))
                {
                    // MetadataのMatch_idをMatchIdフィールドにコピー
                    matchData.MatchId = matchData.Metadata.Match_id;

                    // その他の情報も必要に応じてコピー
                    if (matchData.GameStart == 0 && !string.IsNullOrEmpty(matchData.Metadata.Started_at))
                    {
                        // ISO 8601形式の日時文字列をUnixタイムスタンプに変換
                        if (DateTime.TryParse(matchData.Metadata.Started_at, out DateTime startTime))
                        {
                            matchData.GameStart = ((DateTimeOffset)startTime).ToUnixTimeMilliseconds();
                        }
                    }

                    // キュー情報からゲームモードを設定
                    if (string.IsNullOrEmpty(matchData.GameMode) && matchData.Metadata.Queue != null && !string.IsNullOrEmpty(matchData.Metadata.Queue.Id))
                        matchData.GameMode = matchData.Metadata.Queue.Id;

                    // マップ情報をコピー
                    if (matchData.Metadata.Map != null)
                    {
                        if (string.IsNullOrEmpty(matchData.MapId) && !string.IsNullOrEmpty(matchData.Metadata.Map.Id))
                            matchData.MapId = matchData.Metadata.Map.Id;
                    }

                    // キュー情報をコピー
                    if (matchData.Metadata.Queue != null)
                    {
                        if (string.IsNullOrEmpty(matchData.QueueType) && !string.IsNullOrEmpty(matchData.Metadata.Queue.Id))
                            matchData.QueueType = matchData.Metadata.Queue.Id;
                    }
                }
            }
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<MatchDetailsResponse> GetMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
            throw new ArgumentNullException(nameof(matchId));

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v2/match/{matchId}
        string endpoint = $"/valorant/v2/match/{matchId}";

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

        return await SendRequestAsync<MatchDetailsResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MatchDetailsResponse> GetStoredMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
            throw new ArgumentNullException(nameof(matchId));

        // Henrik APIの最新エンドポイントを使用
        // 正しいエンドポイント: /valorant/v2/match/saved/{matchId}
        string endpoint = $"/valorant/v2/match/saved/{matchId}";

        _logger.LogInformation($"Henrik API リクエスト: {endpoint}");

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

                    try
                    {
                        // 特別な処理が必要な型かどうかを確認
                        if (typeof(T) == typeof(MatchDetailsResponse))
                        {
                            // MatchDetailsResponseの場合、手動でデシリアライズ
                            return (T)(object)DeserializeMatchDetailsResponse(content);
                        }
                        else
                        {
                            // 通常のデシリアライズ
                            var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                            if (result == null)
                            {
                                throw new JsonException("APIレスポンスのデシリアライズに失敗しました");
                            }

                            _logger.LogDebug("APIリクエスト成功: {Endpoint}", endpoint);
                            return result;
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "JSONデシリアライズエラー: {Endpoint}, Content: {Content}", endpoint, content.Substring(0, Math.Min(500, content.Length)));
                        throw;
                    }
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

    /// <summary>
    /// MatchDetailsResponseを手動でデシリアライズするメソッド
    /// </summary>
    /// <param name="json">JSONデータ</param>
    /// <returns>デシリアライズされたMatchDetailsResponse</returns>
    private MatchDetailsResponse DeserializeMatchDetailsResponse(string json)
    {
        _logger.LogInformation("MatchDetailsResponseを手動でデシリアライズします");

        // 一度JsonDocumentとして解析
        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            // 基本的な構造を作成
            var matchResponse = new MatchDetailsResponse
            {
                Status = doc.RootElement.GetProperty("status").GetInt32(),
                Data = new MatchDetailsData
                {
                    Players = new List<MatchPlayerData>(),
                    Teams = new List<MatchTeamData>(),
                    Rounds = new List<MatchRoundData>()
                }
            };

            // 必要なプロパティがあれば設定
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                // メタデータの処理
                if (dataElement.TryGetProperty("metadata", out var metadataElement))
                {
                    try
                    {
                        matchResponse.Data.Metadata = JsonSerializer.Deserialize<MatchMetadata>(
                            metadataElement.GetRawText(), _jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "メタデータのデシリアライズに失敗しました");
                    }
                }

                // teamsの処理
                if (dataElement.TryGetProperty("teams", out var teamsElement) &&
                    teamsElement.ValueKind == JsonValueKind.Array)
                {
                    try
                    {
                        matchResponse.Data.Teams = JsonSerializer.Deserialize<List<MatchTeamData>>(
                            teamsElement.GetRawText(), _jsonOptions) ?? new List<MatchTeamData>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "チームデータのデシリアライズに失敗しました");
                    }
                }

                // roundsの処理
                if (dataElement.TryGetProperty("rounds", out var roundsElement) &&
                    roundsElement.ValueKind == JsonValueKind.Array)
                {
                    try
                    {
                        matchResponse.Data.Rounds = JsonSerializer.Deserialize<List<MatchRoundData>>(
                            roundsElement.GetRawText(), _jsonOptions) ?? new List<MatchRoundData>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ラウンドデータのデシリアライズに失敗しました");
                    }
                }

                // playersの処理
                if (dataElement.TryGetProperty("players", out var playersElement))
                {
                    // playersが配列の場合
                    if (playersElement.ValueKind == JsonValueKind.Array)
                    {
                        _logger.LogInformation("playersフィールドは配列形式です");

                        try
                        {
                            // 各プレイヤーを個別にデシリアライズして追加
                            var playersList = new List<MatchPlayerData>();

                            foreach (var playerElement in playersElement.EnumerateArray())
                            {
                                try
                                {
                                    // プロパティ名をスネークケースからキャメルケースに変換するオプションを設定
                                    var playerOptions = new JsonSerializerOptions(_jsonOptions)
                                    {
                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                    };

                                    // プレイヤーデータの変数をブロックの外で宣言
                                    MatchPlayerData? playerData = null;

                                    // プレイヤーデータをJSONドキュメントとして解析
                                    using (JsonDocument playerDoc = JsonDocument.Parse(playerElement.GetRawText()))
                                    {
                                        // platformプロパティがオブジェクトかどうかを確認
                                        bool hasPlatformObject = false;
                                        if (playerDoc.RootElement.TryGetProperty("platform", out var platformElement))
                                        {
                                            hasPlatformObject = platformElement.ValueKind == JsonValueKind.Object;
                                        }

                                        if (hasPlatformObject)
                                        {
                                            // platformがオブジェクトの場合、一時的にJSONを修正
                                            var jsonObj = System.Text.Json.JsonDocument.Parse(playerElement.GetRawText()).RootElement;
                                            var modifiedJson = new System.Text.Json.Nodes.JsonObject();

                                            // すべてのプロパティをコピー
                                            foreach (var prop in jsonObj.EnumerateObject())
                                            {
                                                if (prop.Name == "platform" && prop.Value.ValueKind == JsonValueKind.Object)
                                                {
                                                    // platformがオブジェクトの場合、"pc"という文字列に置き換え
                                                    modifiedJson.Add("platform", "pc");
                                                    _logger.LogWarning("platformプロパティがオブジェクト型のため、文字列'pc'に置換しました");
                                                }
                                                else
                                                {
                                                    modifiedJson.Add(prop.Name, System.Text.Json.Nodes.JsonNode.Parse(prop.Value.GetRawText()));
                                                }
                                            }

                                            // 修正したJSONをデシリアライズ
                                            playerData = JsonSerializer.Deserialize<MatchPlayerData>(
                                                modifiedJson.ToJsonString(), playerOptions);
                                        }
                                        else
                                        {
                                            // 通常のデシリアライズ
                                            playerData = JsonSerializer.Deserialize<MatchPlayerData>(
                                                playerElement.GetRawText(), playerOptions);
                                        }
                                    }

                                    if (playerData != null)
                                    {
                                        playersList.Add(playerData);
                                    }
                                }
                                catch (Exception playerEx)
                                {
                                    _logger.LogError(playerEx, "プレイヤーデータのデシリアライズに失敗しました");
                                }
                            }

                            matchResponse.Data.Players = playersList;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "プレイヤーデータ配列の処理中にエラーが発生しました");
                        }
                    }
                    // playersがオブジェクトの場合
                    else if (playersElement.ValueKind == JsonValueKind.Object)
                    {
                        _logger.LogWarning("playersフィールドがオブジェクト形式です。APIレスポンス形式が変更された可能性があります。");

                        try
                        {
                            // playersがオブジェクト形式の場合、各プロパティを個別にデシリアライズ
                            var playersList = new List<MatchPlayerData>();

                            foreach (var property in playersElement.EnumerateObject())
                            {
                                try
                                {
                                    // プロパティ値が配列の場合（チーム別プレイヤーリストなど）
                                    if (property.Value.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var playerElement in property.Value.EnumerateArray())
                                        {
                                            try
                                            {
                                                var playerOptions = new JsonSerializerOptions(_jsonOptions)
                                                {
                                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                                };

                                                // プレイヤーデータの変数をブロックの外で宣言
                                                MatchPlayerData? playerData = null;

                                                // プレイヤーデータをJSONドキュメントとして解析
                                                using (JsonDocument playerDoc = JsonDocument.Parse(playerElement.GetRawText()))
                                                {
                                                    // platformプロパティがオブジェクトかどうかを確認
                                                    bool hasPlatformObject = false;
                                                    if (playerDoc.RootElement.TryGetProperty("platform", out var platformElement))
                                                    {
                                                        hasPlatformObject = platformElement.ValueKind == JsonValueKind.Object;
                                                    }

                                                    if (hasPlatformObject)
                                                    {
                                                        // platformがオブジェクトの場合、一時的にJSONを修正
                                                        var jsonObj = System.Text.Json.JsonDocument.Parse(playerElement.GetRawText()).RootElement;
                                                        var modifiedJson = new System.Text.Json.Nodes.JsonObject();

                                                        // すべてのプロパティをコピー
                                                        foreach (var prop in jsonObj.EnumerateObject())
                                                        {
                                                            if (prop.Name == "platform" && prop.Value.ValueKind == JsonValueKind.Object)
                                                            {
                                                                // platformがオブジェクトの場合、"pc"という文字列に置き換え
                                                                modifiedJson.Add("platform", "pc");
                                                                _logger.LogWarning("platformプロパティがオブジェクト型のため、文字列'pc'に置換しました");
                                                            }
                                                            else
                                                            {
                                                                modifiedJson.Add(prop.Name, System.Text.Json.Nodes.JsonNode.Parse(prop.Value.GetRawText()));
                                                            }
                                                        }

                                                        // 修正したJSONをデシリアライズ
                                                        playerData = JsonSerializer.Deserialize<MatchPlayerData>(
                                                            modifiedJson.ToJsonString(), playerOptions);
                                                    }
                                                    else
                                                    {
                                                        // 通常のデシリアライズ
                                                        playerData = JsonSerializer.Deserialize<MatchPlayerData>(
                                                            playerElement.GetRawText(), playerOptions);
                                                    }
                                                }

                                                if (playerData != null)
                                                {
                                                    playersList.Add(playerData);
                                                }
                                            }
                                            catch (Exception playerEx)
                                            {
                                                _logger.LogError(playerEx, "プレイヤー配列要素のデシリアライズに失敗しました: {PropertyName}", property.Name);
                                            }
                                        }
                                    }
                                    // プロパティ値がオブジェクトの場合（単一プレイヤーなど）
                                    else if (property.Value.ValueKind == JsonValueKind.Object)
                                    {
                                        try
                                        {
                                            var playerOptions = new JsonSerializerOptions(_jsonOptions)
                                            {
                                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                            };

                                            // プレイヤーデータの変数をブロックの外で宣言
                                            MatchPlayerData? playerData = null;

                                            // プレイヤーデータをJSONドキュメントとして解析
                                            using (JsonDocument playerDoc = JsonDocument.Parse(property.Value.GetRawText()))
                                            {
                                                // platformプロパティがオブジェクトかどうかを確認
                                                bool hasPlatformObject = false;
                                                if (playerDoc.RootElement.TryGetProperty("platform", out var platformElement))
                                                {
                                                    hasPlatformObject = platformElement.ValueKind == JsonValueKind.Object;
                                                }

                                                if (hasPlatformObject)
                                                {
                                                    // platformがオブジェクトの場合、一時的にJSONを修正
                                                    var jsonObj = System.Text.Json.JsonDocument.Parse(property.Value.GetRawText()).RootElement;
                                                    var modifiedJson = new System.Text.Json.Nodes.JsonObject();

                                                    // すべてのプロパティをコピー
                                                    foreach (var prop in jsonObj.EnumerateObject())
                                                    {
                                                        if (prop.Name == "platform" && prop.Value.ValueKind == JsonValueKind.Object)
                                                        {
                                                            // platformがオブジェクトの場合、"pc"という文字列に置き換え
                                                            modifiedJson.Add("platform", "pc");
                                                            _logger.LogWarning("platformプロパティがオブジェクト型のため、文字列'pc'に置換しました");
                                                        }
                                                        else
                                                        {
                                                            modifiedJson.Add(prop.Name, System.Text.Json.Nodes.JsonNode.Parse(prop.Value.GetRawText()));
                                                        }
                                                    }

                                                    // 修正したJSONをデシリアライズ
                                                    playerData = JsonSerializer.Deserialize<MatchPlayerData>(
                                                        modifiedJson.ToJsonString(), playerOptions);
                                                }
                                                else
                                                {
                                                    // 通常のデシリアライズ
                                                    playerData = JsonSerializer.Deserialize<MatchPlayerData>(
                                                        property.Value.GetRawText(), playerOptions);
                                                }
                                            }

                                            if (playerData != null)
                                            {
                                                playersList.Add(playerData);
                                            }
                                        }
                                        catch (Exception playerEx)
                                        {
                                            _logger.LogError(playerEx, "プレイヤーオブジェクトのデシリアライズに失敗しました: {PropertyName}", property.Name);
                                        }
                                    }
                                }
                                catch (Exception propEx)
                                {
                                    _logger.LogError(propEx, "プレイヤープロパティの処理中にエラーが発生しました: {PropertyName}", property.Name);
                                }
                            }

                            matchResponse.Data.Players = playersList;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "playersオブジェクトの処理中にエラーが発生しました");
                        }
                    }
                }
            }

            return matchResponse;
        }
    }
}