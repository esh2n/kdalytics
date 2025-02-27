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
/// プラットフォームプロパティ用のカスタムJsonConverter
/// </summary>
public class PlatformConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // オブジェクトの場合は "pc" を返す
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // オブジェクトをスキップ
            int depth = 1;
            while (depth > 0 && reader.Read())
            {
                if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.StartArray)
                {
                    depth++;
                }
                else if (reader.TokenType == JsonTokenType.EndObject || reader.TokenType == JsonTokenType.EndArray)
                {
                    depth--;
                }
            }
            return "pc";
        }

        // 文字列の場合はそのまま返す
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? "pc";
        }

        // その他の場合は "pc" を返す
        return "pc";
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // APIレスポンスはキャメルケース
            IgnoreReadOnlyProperties = false,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = {
                new JsonStringEnumConverter(),
                new PlatformConverter() // プラットフォーム用のカスタムコンバーター
            }
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

        // v4 APIレスポンスを直接デシリアライズ
        var response = await SendRequestAsync<MatchlistResponse>(endpoint, cancellationToken);
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

        // v4 APIレスポンスを直接デシリアライズ
        var response = await SendRequestAsync<MatchlistResponse>(endpoint, cancellationToken);
        return response;
    }

    /// <summary>
    /// v2 APIレスポンス用の中間モデル
    /// </summary>
    private class MatchDetailsResponseV2
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("data")]
        public MatchDetailsDataV2? Data { get; set; }
    }

    /// <summary>
    /// v2 APIレスポンス用の中間データモデル
    /// </summary>
    private class MatchDetailsDataV2
    {
        [JsonPropertyName("metadata")]
        public MatchMetadataV2? Metadata { get; set; }

        [JsonPropertyName("players")]
        public Dictionary<string, List<MatchPlayerDataV2>> Players { get; set; } = new();

        [JsonPropertyName("teams")]
        public Dictionary<string, MatchTeamDataV2> Teams { get; set; } = new();

        [JsonPropertyName("rounds")]
        public List<MatchRoundDataV2> Rounds { get; set; } = new();
    }

    /// <summary>
    /// v2 APIレスポンス用のメタデータモデル
    /// </summary>
    private class MatchMetadataV2
    {
        [JsonPropertyName("map")]
        public string Map { get; set; } = string.Empty;

        [JsonPropertyName("game_version")]
        public string Game_version { get; set; } = string.Empty;

        [JsonPropertyName("game_length")]
        public long Game_length { get; set; }

        [JsonPropertyName("game_start")]
        public long Game_start { get; set; }

        [JsonPropertyName("game_start_patched")]
        public string Game_start_patched { get; set; } = string.Empty;

        [JsonPropertyName("rounds_played")]
        public int Rounds_played { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

        [JsonPropertyName("queue")]
        public string Queue { get; set; } = string.Empty;

        [JsonPropertyName("season_id")]
        public string Season_id { get; set; } = string.Empty;

        [JsonPropertyName("matchid")]
        public string Matchid { get; set; } = string.Empty;

        [JsonPropertyName("region")]
        public string Region { get; set; } = string.Empty;

        [JsonPropertyName("cluster")]
        public string Cluster { get; set; } = string.Empty;
    }

    /// <summary>
    /// v2 APIレスポンス用のプレイヤーデータモデル
    /// </summary>
    private class MatchPlayerDataV2
    {
        [JsonPropertyName("puuid")]
        public string Puuid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("character")]
        public string Character { get; set; } = string.Empty;

        [JsonPropertyName("currenttier")]
        public int Currenttier { get; set; }

        [JsonPropertyName("currenttier_patched")]
        public string Currenttier_patched { get; set; } = string.Empty;

        [JsonPropertyName("player_card")]
        public string Player_card { get; set; } = string.Empty;

        [JsonPropertyName("player_title")]
        public string Player_title { get; set; } = string.Empty;

        [JsonPropertyName("party_id")]
        public string Party_id { get; set; } = string.Empty;

        [JsonPropertyName("session_playtime")]
        public SessionPlaytimeV2? Session_playtime { get; set; }

        [JsonPropertyName("assets")]
        public AssetsV2? Assets { get; set; }

        [JsonPropertyName("stats")]
        public StatsV2? Stats { get; set; }

        [JsonPropertyName("ability_casts")]
        public AbilityCastsV2? Ability_casts { get; set; }

        [JsonPropertyName("platform")]
        [JsonConverter(typeof(PlatformConverter))]
        public string Platform { get; set; } = string.Empty;
    }

    /// <summary>
    /// v2 APIレスポンス用のセッションプレイタイムモデル
    /// </summary>
    private class SessionPlaytimeV2
    {
        [JsonPropertyName("minutes")]
        public int Minutes { get; set; }

        [JsonPropertyName("seconds")]
        public int Seconds { get; set; }

        [JsonPropertyName("milliseconds")]
        public int Milliseconds { get; set; }
    }

    /// <summary>
    /// v2 APIレスポンス用のアセットモデル
    /// </summary>
    private class AssetsV2
    {
        [JsonPropertyName("card")]
        public AssetV2? Card { get; set; }

        [JsonPropertyName("agent")]
        public AssetV2? Agent { get; set; }
    }

    /// <summary>
    /// v2 APIレスポンス用のアセット詳細モデル
    /// </summary>
    private class AssetV2
    {
        [JsonPropertyName("small")]
        public string Small { get; set; } = string.Empty;

        [JsonPropertyName("large")]
        public string Large { get; set; } = string.Empty;

        [JsonPropertyName("wide")]
        public string Wide { get; set; } = string.Empty;
    }

    /// <summary>
    /// v2 APIレスポンス用の統計情報モデル
    /// </summary>
    private class StatsV2
    {
        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("kills")]
        public int Kills { get; set; }

        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("assists")]
        public int Assists { get; set; }

        [JsonPropertyName("bodyshots")]
        public int Bodyshots { get; set; }

        [JsonPropertyName("headshots")]
        public int Headshots { get; set; }

        [JsonPropertyName("legshots")]
        public int Legshots { get; set; }
    }

    /// <summary>
    /// v2 APIレスポンス用のアビリティ使用回数モデル
    /// </summary>
    private class AbilityCastsV2
    {
        [JsonPropertyName("c_cast")]
        public int C_cast { get; set; }

        [JsonPropertyName("q_cast")]
        public int Q_cast { get; set; }

        [JsonPropertyName("e_cast")]
        public int E_cast { get; set; }

        [JsonPropertyName("x_cast")]
        public int X_cast { get; set; }
    }

    /// <summary>
    /// v2 APIレスポンス用のチームデータモデル
    /// </summary>
    private class MatchTeamDataV2
    {
        [JsonPropertyName("has_won")]
        public bool Has_won { get; set; }

        [JsonPropertyName("rounds_won")]
        public int Rounds_won { get; set; }

        [JsonPropertyName("rounds_lost")]
        public int Rounds_lost { get; set; }

        [JsonPropertyName("team_id")]
        public string Team_id { get; set; } = string.Empty;
    }

    /// <summary>
    /// v2 APIレスポンス用のラウンドデータモデル
    /// </summary>
    private class MatchRoundDataV2
    {
        [JsonPropertyName("winning_team")]
        public string Winning_team { get; set; } = string.Empty;

        [JsonPropertyName("end_type")]
        public string End_type { get; set; } = string.Empty;

        [JsonPropertyName("bomb_planted")]
        public bool Bomb_planted { get; set; }

        [JsonPropertyName("bomb_defused")]
        public bool Bomb_defused { get; set; }

        [JsonPropertyName("plant_events")]
        public PlantEventV2? Plant_events { get; set; }

        [JsonPropertyName("defuse_events")]
        public DefuseEventV2? Defuse_events { get; set; }
    }

    /// <summary>
    /// v2 APIレスポンス用のプラントイベントモデル
    /// </summary>
    private class PlantEventV2
    {
        [JsonPropertyName("plant_location")]
        public LocationV2? Plant_location { get; set; }

        [JsonPropertyName("planted_by")]
        public PlayerV2? Planted_by { get; set; }

        [JsonPropertyName("plant_site")]
        public string Plant_site { get; set; } = string.Empty;

        [JsonPropertyName("plant_time_in_round")]
        public int Plant_time_in_round { get; set; }

        [JsonPropertyName("player_locations_on_plant")]
        public List<PlayerLocationV2> Player_locations_on_plant { get; set; } = new();
    }

    /// <summary>
    /// v2 APIレスポンス用のディフューズイベントモデル
    /// </summary>
    private class DefuseEventV2
    {
        [JsonPropertyName("defuse_location")]
        public LocationV2? Defuse_location { get; set; }

        [JsonPropertyName("defused_by")]
        public PlayerV2? Defused_by { get; set; }

        [JsonPropertyName("defuse_time_in_round")]
        public int Defuse_time_in_round { get; set; }

        [JsonPropertyName("player_locations_on_defuse")]
        public List<PlayerLocationV2> Player_locations_on_defuse { get; set; } = new();
    }

    /// <summary>
    /// v2 APIレスポンス用の位置情報モデル
    /// </summary>
    private class LocationV2
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    /// <summary>
    /// v2 APIレスポンス用のプレイヤーモデル
    /// </summary>
    private class PlayerV2
    {
        [JsonPropertyName("puuid")]
        public string Puuid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;
    }

    /// <summary>
    /// v2 APIレスポンス用のプレイヤー位置情報モデル
    /// </summary>
    private class PlayerLocationV2
    {
        [JsonPropertyName("player_puuid")]
        public string Player_puuid { get; set; } = string.Empty;

        [JsonPropertyName("player_display_name")]
        public string Player_display_name { get; set; } = string.Empty;

        [JsonPropertyName("player_team")]
        public string Player_team { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public LocationV2? Location { get; set; }

        [JsonPropertyName("view_radians")]
        public float View_radians { get; set; }
    }

    /// <summary>
    /// v4 APIレスポンス用の中間モデル
    /// </summary>
    private class MatchDetailsResponseV4
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("data")]
        public MatchDetailsDataV4? Data { get; set; }
    }

    /// <summary>
    /// v4 APIレスポンス用の中間データモデル
    /// </summary>
    private class MatchDetailsDataV4
    {
        [JsonPropertyName("metadata")]
        public MatchMetadataV4? Metadata { get; set; }

        [JsonPropertyName("players")]
        public List<MatchPlayerDataV4> Players { get; set; } = new();

        [JsonPropertyName("teams")]
        public List<MatchTeamDataV4> Teams { get; set; } = new();

        [JsonPropertyName("rounds")]
        public List<MatchRoundDataV4> Rounds { get; set; } = new();

        [JsonPropertyName("kills")]
        public List<KillDataV4> Kills { get; set; } = new();
    }

    /// <summary>
    /// v4 APIレスポンス用のプレイヤーデータモデル
    /// </summary>
    private class MatchPlayerDataV4
    {
        [JsonPropertyName("puuid")]
        public string Puuid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("team_id")]
        public string Team_id { get; set; } = string.Empty;

        [JsonPropertyName("platform")]
        [JsonConverter(typeof(PlatformConverter))]
        public string Platform { get; set; } = string.Empty;

        [JsonPropertyName("party_id")]
        public string Party_id { get; set; } = string.Empty;

        [JsonPropertyName("agent")]
        public AgentInfo? Agent { get; set; }

        [JsonPropertyName("stats")]
        public PlayerStats? Stats { get; set; }

        [JsonPropertyName("ability_casts")]
        public AbilityCasts? Ability_casts { get; set; }

        [JsonPropertyName("tier")]
        public TierInfo? Tier { get; set; }

        [JsonPropertyName("customization")]
        public CustomizationInfo? Customization { get; set; }

        [JsonPropertyName("account_level")]
        public int Account_level { get; set; }

        [JsonPropertyName("session_playtime_in_ms")]
        public long Session_playtime_in_ms { get; set; }

        [JsonPropertyName("behavior")]
        public BehaviorInfo? Behavior { get; set; }

        [JsonPropertyName("economy")]
        public EconomyInfo? Economy { get; set; }
    }

    /// <summary>
    /// v4 APIレスポンス用のチームデータモデル
    /// </summary>
    private class MatchTeamDataV4
    {
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        [JsonPropertyName("won")]
        public bool Won { get; set; }

        [JsonPropertyName("rounds")]
        public RoundsInfoV4? Rounds { get; set; }
    }

    /// <summary>
    /// v4 APIレスポンス用のラウンド情報
    /// </summary>
    private class RoundsInfoV4
    {
        [JsonPropertyName("won")]
        public int Won { get; set; }

        [JsonPropertyName("lost")]
        public int Lost { get; set; }
    }

    /// <summary>
    /// v4 APIレスポンス用のラウンドデータモデル
    /// </summary>
    private class MatchRoundDataV4
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("winning_team")]
        public string WinningTeam { get; set; } = string.Empty;

        [JsonPropertyName("plant")]
        public object? Plant { get; set; }

        [JsonPropertyName("defuse")]
        public object? Defuse { get; set; }
    }

    /// <summary>
    /// v4 APIレスポンス用のキルデータモデル
    /// </summary>
    private class KillDataV4
    {
        [JsonPropertyName("time_in_round_in_ms")]
        public int TimeInRoundInMs { get; set; }

        [JsonPropertyName("time_in_match_in_ms")]
        public int TimeInMatchInMs { get; set; }

        [JsonPropertyName("round")]
        public int Round { get; set; }

        [JsonPropertyName("killer")]
        public PlayerInfoV4? Killer { get; set; }

        [JsonPropertyName("victim")]
        public PlayerInfoV4? Victim { get; set; }
    }

    /// <summary>
    /// v4 APIレスポンス用のプレイヤー情報
    /// </summary>
    private class PlayerInfoV4
    {
        [JsonPropertyName("puuid")]
        public string Puuid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;
    }

    /// <inheritdoc />
    public async Task<MatchDetailsResponse> GetMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
            throw new ArgumentNullException(nameof(matchId));

        // Henrik APIのv4エンドポイントを使用
        // v4エンドポイントはリージョン指定が必要だが、matchIdからリージョンを特定できないため
        // 複数のリージョンを試す
        string[] regions = { "ap", "na", "eu", "kr", "latam", "br" };
        
        foreach (var region in regions)
        {
            try
            {
                // v4 APIエンドポイント: /valorant/v4/match/{region}/{matchId}
                string endpoint = $"/valorant/v4/match/{region}/{matchId}";
                
                _logger.LogInformation($"Henrik API リクエスト: {endpoint}");
                
                var response = await SendRequestAsync<MatchDetailsResponse>(endpoint, cancellationToken);
                if (response.Status == 200 && response.Data != null)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"リージョン {region} でのマッチ詳細取得に失敗: {ex.Message}");
                // 次のリージョンを試す
                continue;
            }
        }
        
        // すべてのリージョンで失敗した場合、v2 APIを試す（フォールバック）
        try
        {
            _logger.LogInformation("v4 APIでの取得に失敗したため、v2 APIを試みます");
            string endpoint = $"/valorant/v2/match/{matchId}";
            return await SendRequestAsync<MatchDetailsResponse>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "すべてのAPIエンドポイントでマッチ詳細の取得に失敗しました: {MatchId}", matchId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MatchDetailsResponse> GetStoredMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
            throw new ArgumentNullException(nameof(matchId));

        // Henrik APIのv4エンドポイントを使用
        // v4エンドポイントはリージョン指定が必要だが、matchIdからリージョンを特定できないため
        // 複数のリージョンを試す
        string[] regions = { "ap", "na", "eu", "kr", "latam", "br" };
        
        foreach (var region in regions)
        {
            try
            {
                // v4 APIエンドポイント: /valorant/v4/match/{region}/{matchId}/saved
                string endpoint = $"/valorant/v4/match/{region}/{matchId}/saved";
                
                _logger.LogInformation($"Henrik API リクエスト: {endpoint}");
                
                var response = await SendRequestAsync<MatchDetailsResponse>(endpoint, cancellationToken);
                if (response.Status == 200 && response.Data != null)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"リージョン {region} での保存済みマッチ詳細取得に失敗: {ex.Message}");
                // 次のリージョンを試す
                continue;
            }
        }
        
        // すべてのリージョンで失敗した場合、v2 APIを試す（フォールバック）
        try
        {
            _logger.LogInformation("v4 APIでの取得に失敗したため、v2 APIを試みます");
            string endpoint = $"/valorant/v2/match/saved/{matchId}";
            return await SendRequestAsync<MatchDetailsResponse>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "すべてのAPIエンドポイントで保存済みマッチ詳細の取得に失敗しました: {MatchId}", matchId);
            throw;
        }
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
                            // v4 APIレスポンスの場合
                            if (endpoint.Contains("/valorant/v4/"))
                            {
                                _logger.LogInformation("v4 APIレスポンスをデシリアライズします");
                                var v4Response = JsonSerializer.Deserialize<MatchDetailsResponseV4>(content, _jsonOptions);
                                if (v4Response == null || v4Response.Data == null)
                                {
                                    throw new JsonException("v4 APIレスポンスのデシリアライズに失敗しました");
                                }

                                // v4レスポンスをMatchDetailsResponseに変換
                                var matchResponse = new MatchDetailsResponse
                                {
                                    Status = v4Response.Status,
                                    Data = new MatchDetailsData
                                    {
                                        Players = new List<MatchPlayerData>(),
                                        Teams = new List<MatchTeamData>(),
                                        Rounds = new List<MatchRoundData>()
                                    }
                                };

                                // データを使用
                                var v4Data = v4Response.Data;
                                if (v4Data == null)
                                {
                                    throw new JsonException("v4 APIレスポンスのデータがnullです");
                                }

                                // メタデータの変換
                                if (v4Data.Metadata != null)
                                {
                                    matchResponse.Data.Metadata = new MatchMetadata
                                    {
                                        MatchId = v4Data.Metadata.Match_id,
                                        MapId = v4Data.Metadata.Map?.Id ?? string.Empty,
                                        MapName = v4Data.Metadata.Map?.Name ?? string.Empty,
                                        GameLength = v4Data.Metadata.Game_length_in_ms,
                                        GameVersion = v4Data.Metadata.Game_version,
                                        GameMode = v4Data.Metadata.Queue?.Id ?? string.Empty,
                                        SeasonId = v4Data.Metadata.Season?.Id ?? string.Empty,
                                        Region = v4Data.Metadata.Region
                                    };

                                    // ISO 8601形式の日時文字列をUnixタイムスタンプに変換
                                    if (!string.IsNullOrEmpty(v4Data.Metadata.Started_at))
                                    {
                                        if (DateTime.TryParse(v4Data.Metadata.Started_at, out DateTime startTime))
                                        {
                                            matchResponse.Data.Metadata.GameStart = ((DateTimeOffset)startTime).ToUnixTimeMilliseconds();
                                        }
                                    }
                                }

                                // プレイヤーデータの変換
                                foreach (var v4Player in v4Data.Players)
                                {
                                    var player = new MatchPlayerData
                                    {
                                        Puuid = v4Player.Puuid,
                                        Name = v4Player.Name,
                                        Tag = v4Player.Tag,
                                        Team_id = v4Player.Team_id,
                                        Platform = v4Player.Platform,
                                        Party_id = v4Player.Party_id,
                                        Agent = v4Player.Agent,
                                        Stats = v4Player.Stats,
                                        Ability_casts = v4Player.Ability_casts,
                                        Tier = v4Player.Tier,
                                        Customization = v4Player.Customization,
                                        Account_level = v4Player.Account_level,
                                        Session_playtime_in_ms = v4Player.Session_playtime_in_ms,
                                        Behavior = v4Player.Behavior,
                                        Economy = v4Player.Economy
                                    };

                                    matchResponse.Data.Players.Add(player);
                                }

                                // チームデータの変換
                                foreach (var v4Team in v4Data.Teams)
                                {
                                    var team = new MatchTeamData
                                    {
                                        TeamId = v4Team.TeamId,
                                        WonRounds = v4Team.Won
                                    };

                                    if (v4Team.Rounds != null)
                                    {
                                        team.RoundsWon = v4Team.Rounds.Won;
                                    }

                                    matchResponse.Data.Teams.Add(team);
                                }

                                // ラウンドデータの変換
                                foreach (var v4Round in v4Data.Rounds)
                                {
                                    var round = new MatchRoundData
                                    {
                                        RoundNumber = v4Round.Id,
                                        WinningTeam = v4Round.WinningTeam,
                                        BombPlanted = v4Round.Plant != null,
                                        BombDefused = v4Round.Defuse != null
                                    };

                                    matchResponse.Data.Rounds.Add(round);
                                }

                                return (T)(object)matchResponse;
                            }
                            else
                            {
                                // v2 APIレスポンスも中間モデルを使用してデシリアライズ
                                _logger.LogInformation("v2 APIレスポンスをデシリアライズします");
                                var v2Response = JsonSerializer.Deserialize<MatchDetailsResponseV2>(content, _jsonOptions);
                                if (v2Response == null || v2Response.Data == null)
                                {
                                    throw new JsonException("v2 APIレスポンスのデシリアライズに失敗しました");
                                }

                                // v2レスポンスをMatchDetailsResponseに変換
                                var matchResponse = new MatchDetailsResponse
                                {
                                    Status = v2Response.Status,
                                    Data = new MatchDetailsData
                                    {
                                        Players = new List<MatchPlayerData>(),
                                        Teams = new List<MatchTeamData>(),
                                        Rounds = new List<MatchRoundData>()
                                    }
                                };

                                // メタデータの変換
                                if (v2Response.Data.Metadata != null)
                                {
                                    matchResponse.Data.Metadata = new MatchMetadata
                                    {
                                        MatchId = v2Response.Data.Metadata.Matchid,
                                        MapName = v2Response.Data.Metadata.Map,
                                        GameLength = v2Response.Data.Metadata.Game_length,
                                        GameStart = v2Response.Data.Metadata.Game_start,
                                        GameVersion = v2Response.Data.Metadata.Game_version,
                                        GameMode = v2Response.Data.Metadata.Mode,
                                        SeasonId = v2Response.Data.Metadata.Season_id,
                                        Region = v2Response.Data.Metadata.Region
                                    };
                                }

                                // プレイヤーデータの変換
                                foreach (var teamPlayers in v2Response.Data.Players)
                                {
                                    foreach (var v2Player in teamPlayers.Value)
                                    {
                                        var player = new MatchPlayerData
                                        {
                                            Puuid = v2Player.Puuid,
                                            Name = v2Player.Name,
                                            Tag = v2Player.Tag,
                                            Team_id = v2Player.Team,
                                            Platform = v2Player.Platform,
                                            Party_id = v2Player.Party_id,
                                            Account_level = v2Player.Level
                                        };

                                        // エージェント情報
                                        if (!string.IsNullOrEmpty(v2Player.Character))
                                        {
                                            player.Agent = new AgentInfo
                                            {
                                                Name = v2Player.Character
                                            };
                                        }

                                        // 統計情報
                                        if (v2Player.Stats != null)
                                        {
                                            player.Stats = new PlayerStats
                                            {
                                                Score = v2Player.Stats.Score,
                                                Kills = v2Player.Stats.Kills,
                                                Deaths = v2Player.Stats.Deaths,
                                                Assists = v2Player.Stats.Assists,
                                                Bodyshots = v2Player.Stats.Bodyshots,
                                                Headshots = v2Player.Stats.Headshots,
                                                Legshots = v2Player.Stats.Legshots
                                            };
                                        }

                                        // ティア情報
                                        if (!string.IsNullOrEmpty(v2Player.Currenttier_patched))
                                        {
                                            player.Tier = new TierInfo
                                            {
                                                Id = v2Player.Currenttier,
                                                Name = v2Player.Currenttier_patched
                                            };
                                        }

                                        matchResponse.Data.Players.Add(player);
                                    }
                                }

                                // チームデータの変換
                                foreach (var teamEntry in v2Response.Data.Teams)
                                {
                                    var teamId = teamEntry.Key;
                                    var v2Team = teamEntry.Value;
                                    
                                    var team = new MatchTeamData
                                    {
                                        TeamId = teamId, // キーをチームIDとして使用
                                        WonRounds = v2Team.Has_won,
                                        RoundsWon = v2Team.Rounds_won
                                    };

                                    matchResponse.Data.Teams.Add(team);
                                }

                                // ラウンドデータの変換
                                foreach (var v2Round in v2Response.Data.Rounds)
                                {
                                    var round = new MatchRoundData
                                    {
                                        WinningTeam = v2Round.Winning_team,
                                        BombPlanted = v2Round.Bomb_planted,
                                        BombDefused = v2Round.Bomb_defused
                                    };

                                    matchResponse.Data.Rounds.Add(round);
                                }

                                _logger.LogDebug("APIリクエスト成功: {Endpoint}", endpoint);
                                return (T)(object)matchResponse;
                            }
                        }
                        else if (typeof(T) == typeof(MatchlistResponse))
                        {
                            // MatchlistResponseの場合、v4 APIレスポンスの特別な処理
                            var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                            if (result == null)
                            {
                                throw new JsonException("APIレスポンスのデシリアライズに失敗しました");
                            }

                            // v4 APIレスポンスの場合、Metadataからマッチ情報をMatchIdにコピーする
                            if (endpoint.Contains("/valorant/v4/"))
                            {
                                var matchlistResponse = (MatchlistResponse)(object)result;
                                if (matchlistResponse.Status == 200 && matchlistResponse.Data != null)
                                {
                                    foreach (var matchData in matchlistResponse.Data)
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
                                return (T)(object)matchlistResponse;
                            }

                            _logger.LogDebug("APIリクエスト成功: {Endpoint}", endpoint);
                            return result;
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
}