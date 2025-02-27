namespace KDalytics.Core.Interfaces.Services;

/// <summary>
/// Henrik APIクライアントのインターフェース
/// </summary>
public interface IHenrikApiClient
{
    /// <summary>
    /// プレイヤー情報を取得
    /// </summary>
    /// <param name="name">プレイヤー名</param>
    /// <param name="tag">タグ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤー情報レスポンス</returns>
    Task<AccountInfoResponse> GetPlayerInfoAsync(string name, string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// PUUIDからプレイヤー情報を取得
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤー情報レスポンス</returns>
    Task<AccountInfoResponse> GetPlayerInfoByPuuidAsync(string puuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーのMMR情報を取得
    /// </summary>
    /// <param name="region">リージョン</param>
    /// <param name="name">プレイヤー名</param>
    /// <param name="tag">タグ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>MMR情報レスポンス</returns>
    Task<MmrInfoResponse> GetPlayerMmrAsync(string region, string name, string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// PUUIDからプレイヤーのMMR情報を取得
    /// </summary>
    /// <param name="region">リージョン</param>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>MMR情報レスポンス</returns>
    Task<MmrInfoResponse> GetPlayerMmrByPuuidAsync(string region, string puuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの最近の試合履歴を取得
    /// </summary>
    /// <param name="region">リージョン</param>
    /// <param name="name">プレイヤー名</param>
    /// <param name="tag">タグ</param>
    /// <param name="count">取得する試合数（最大5）</param>
    /// <param name="mode">ゲームモード（例: "competitive"）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合履歴レスポンス</returns>
    Task<MatchlistResponse> GetPlayerMatchesAsync(string region, string name, string tag, int count = 5, string mode = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// PUUIDからプレイヤーの最近の試合履歴を取得
    /// </summary>
    /// <param name="region">リージョン</param>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="count">取得する試合数（最大5）</param>
    /// <param name="mode">ゲームモード（例: "competitive"）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合履歴レスポンス</returns>
    Task<MatchlistResponse> GetPlayerMatchesByPuuidAsync(string region, string puuid, int count = 5, string mode = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// 特定の試合の詳細を取得
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合詳細レスポンス</returns>
    Task<MatchDetailsResponse> GetMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ストアされた特定の試合の詳細を取得（過去の保存データ）
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合詳細レスポンス</returns>
    Task<MatchDetailsResponse> GetStoredMatchDetailsAsync(string matchId, CancellationToken cancellationToken = default);
}

#region Response Models

/// <summary>
/// アカウント情報のレスポンスモデル
/// </summary>
public class AccountInfoResponse
{
    /// <summary>
    /// ステータス（200=成功）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// データ
    /// </summary>
    public AccountInfoData? Data { get; set; }
}

/// <summary>
/// アカウント情報のデータモデル
/// </summary>
public class AccountInfoData
{
    /// <summary>
    /// プレイヤーのPUUID
    /// </summary>
    public string Puuid { get; set; } = string.Empty;

    /// <summary>
    /// リージョン
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// アカウントレベル
    /// </summary>
    public int AccountLevel { get; set; }

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// タグ
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// カード情報
    /// </summary>
    public CardInfo? Card { get; set; }

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public long LastUpdate { get; set; }
}

/// <summary>
/// カード情報
/// </summary>
public class CardInfo
{
    /// <summary>
    /// カードID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// カード名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// アセットパス
    /// </summary>
    public string? Asset { get; set; }
}

/// <summary>
/// MMR情報のレスポンスモデル
/// </summary>
public class MmrInfoResponse
{
    /// <summary>
    /// ステータス（200=成功）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// データ
    /// </summary>
    public MmrInfoData? Data { get; set; }
}

/// <summary>
/// MMR情報のデータモデル
/// </summary>
public class MmrInfoData
{
    /// <summary>
    /// プレイヤーのPUUID
    /// </summary>
    public string Puuid { get; set; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// タグ
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// 現在のティア数値（例: 24 = Immortal 3）
    /// </summary>
    public int CurrentTier { get; set; }

    /// <summary>
    /// 現在のティア名（例: "Immortal 3"）
    /// </summary>
    public string CurrentTierPatched { get; set; } = string.Empty;

    /// <summary>
    /// ティア内のランキング（0-100）
    /// </summary>
    public int RankingInTier { get; set; }

    /// <summary>
    /// 前回の試合からのMMR変化
    /// </summary>
    public int MmrChangeToLastGame { get; set; }

    /// <summary>
    /// MMR
    /// </summary>
    public int Mmr { get; set; }

    /// <summary>
    /// シーズン別のMMR情報
    /// </summary>
    public Dictionary<string, SeasonMmrInfo> BySeasonMmrData { get; set; } = new();
}

/// <summary>
/// シーズン別MMR情報
/// </summary>
public class SeasonMmrInfo
{
    /// <summary>
    /// 試合数
    /// </summary>
    public int NumberOfGames { get; set; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// 最終ランク（ティア数値）
    /// </summary>
    public int FinalRank { get; set; }

    /// <summary>
    /// 最終ランク名
    /// </summary>
    public string FinalRankPatched { get; set; } = string.Empty;
}

/// <summary>
/// 試合履歴のレスポンスモデル
/// </summary>
public class MatchlistResponse
{
    /// <summary>
    /// ステータス（200=成功）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// データ
    /// </summary>
    public List<MatchlistData> Data { get; set; } = new();
}

/// <summary>
/// 試合履歴のデータモデル
/// </summary>
public class MatchlistData
{
    /// <summary>
    /// 試合ID
    /// </summary>
    public string MatchId { get; set; } = string.Empty;

    /// <summary>
    /// ゲームスタート時間
    /// </summary>
    public long GameStart { get; set; }

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string GameMode { get; set; } = string.Empty;

    /// <summary>
    /// キュータイプ
    /// </summary>
    public string QueueType { get; set; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; set; } = string.Empty;

    /// <summary>
    /// シーズンID
    /// </summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>
    /// メタデータ（v4 APIレスポンス用）
    /// </summary>
    public MatchMetadataV4? Metadata { get; set; }
}

/// <summary>
/// v4 API用の試合メタデータ
/// </summary>
public class MatchMetadataV4
{
    /// <summary>
    /// 試合ID
    /// </summary>
    public string Match_id { get; set; } = string.Empty;

    /// <summary>
    /// マップ情報
    /// </summary>
    public MapInfo? Map { get; set; }

    /// <summary>
    /// ゲームバージョン
    /// </summary>
    public string Game_version { get; set; } = string.Empty;

    /// <summary>
    /// ゲーム時間（ミリ秒）
    /// </summary>
    public long Game_length_in_ms { get; set; }

    /// <summary>
    /// ゲーム開始時間
    /// </summary>
    public string Started_at { get; set; } = string.Empty;

    /// <summary>
    /// 完了フラグ
    /// </summary>
    public bool Is_completed { get; set; }

    /// <summary>
    /// キュー情報
    /// </summary>
    public QueueInfo? Queue { get; set; }

    /// <summary>
    /// シーズン情報
    /// </summary>
    public SeasonInfo? Season { get; set; }

    /// <summary>
    /// プラットフォーム
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// リージョン
    /// </summary>
    public string Region { get; set; } = string.Empty;
}

/// <summary>
/// シーズン情報
/// </summary>
public class SeasonInfo
{
    /// <summary>
    /// シーズンID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// シーズン短縮名
    /// </summary>
    public string Short { get; set; } = string.Empty;
}

/// <summary>
/// マップ情報
/// </summary>
public class MapInfo
{
    /// <summary>
    /// マップID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// マップ名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// キュー情報
/// </summary>
public class QueueInfo
{
    /// <summary>
    /// キューID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// キュー名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// モード情報
/// </summary>
public class ModeInfo
{
    /// <summary>
    /// モードID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// モード名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 試合詳細のレスポンスモデル
/// </summary>
public class MatchDetailsResponse
{
    /// <summary>
    /// ステータス（200=成功）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// データ
    /// </summary>
    public MatchDetailsData? Data { get; set; }
}

/// <summary>
/// 試合詳細のデータモデル
/// </summary>
public class MatchDetailsData
{
    /// <summary>
    /// メタデータ
    /// </summary>
    public MatchMetadata? Metadata { get; set; }

    /// <summary>
    /// プレイヤー情報
    /// </summary>
    public List<MatchPlayerData> Players { get; set; } = new();

    /// <summary>
    /// チーム情報
    /// </summary>
    public List<MatchTeamData> Teams { get; set; } = new();

    /// <summary>
    /// ラウンド情報
    /// </summary>
    public List<MatchRoundData> Rounds { get; set; } = new();
}

/// <summary>
/// 試合メタデータ
/// </summary>
public class MatchMetadata
{
    /// <summary>
    /// 試合ID
    /// </summary>
    public string MatchId { get; set; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; set; } = string.Empty;

    /// <summary>
    /// マップ名
    /// </summary>
    public string MapName { get; set; } = string.Empty;

    /// <summary>
    /// ゲームレングス（ミリ秒）
    /// </summary>
    public long GameLength { get; set; }

    /// <summary>
    /// ゲームスタート時間
    /// </summary>
    public long GameStart { get; set; }

    /// <summary>
    /// ゲームバージョン
    /// </summary>
    public string GameVersion { get; set; } = string.Empty;

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string GameMode { get; set; } = string.Empty;

    /// <summary>
    /// シーズンID
    /// </summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>
    /// リージョン
    /// </summary>
    public string Region { get; set; } = string.Empty;
}

/// <summary>
/// プレイヤーの試合データ
/// </summary>
public class MatchPlayerData
{
    /// <summary>
    /// プレイヤーのPUUID
    /// </summary>
    public string Puuid { get; set; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// チームID
    /// </summary>
    public string Team_id { get; set; } = string.Empty;

    /// <summary>
    /// プラットフォーム
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// パーティID
    /// </summary>
    public string Party_id { get; set; } = string.Empty;

    /// <summary>
    /// エージェント情報
    /// </summary>
    public AgentInfo? Agent { get; set; }

    /// <summary>
    /// 統計情報
    /// </summary>
    public PlayerStats? Stats { get; set; }

    /// <summary>
    /// アビリティ使用回数
    /// </summary>
    public AbilityCasts? Ability_casts { get; set; }

    /// <summary>
    /// ティア情報
    /// </summary>
    public TierInfo? Tier { get; set; }

    /// <summary>
    /// カスタマイズ情報
    /// </summary>
    public CustomizationInfo? Customization { get; set; }

    /// <summary>
    /// アカウントレベル
    /// </summary>
    public int Account_level { get; set; }

    /// <summary>
    /// セッションプレイ時間（ミリ秒）
    /// </summary>
    public long Session_playtime_in_ms { get; set; }

    /// <summary>
    /// 行動情報
    /// </summary>
    public BehaviorInfo? Behavior { get; set; }

    /// <summary>
    /// 経済情報
    /// </summary>
    public EconomyInfo? Economy { get; set; }

    // 後方互換性のためのプロパティ
    [System.Text.Json.Serialization.JsonIgnore]
    public string Team
    {
        get => Team_id;
        set => Team_id = value;
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public string Character
    {
        get => Agent?.Name ?? string.Empty;
        set { if (Agent == null) Agent = new AgentInfo(); Agent.Name = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Level
    {
        get => Account_level;
        set => Account_level = value;
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int CurrentTier
    {
        get => Tier?.Id ?? 0;
        set { if (Tier == null) Tier = new TierInfo(); Tier.Id = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public string CurrentTierPatched
    {
        get => Tier?.Name ?? string.Empty;
        set { if (Tier == null) Tier = new TierInfo(); Tier.Name = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Score
    {
        get => Stats?.Score ?? 0;
        set { if (Stats == null) Stats = new PlayerStats(); Stats.Score = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Kills
    {
        get => Stats?.Kills ?? 0;
        set { if (Stats == null) Stats = new PlayerStats(); Stats.Kills = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Deaths
    {
        get => Stats?.Deaths ?? 0;
        set { if (Stats == null) Stats = new PlayerStats(); Stats.Deaths = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Assists
    {
        get => Stats?.Assists ?? 0;
        set { if (Stats == null) Stats = new PlayerStats(); Stats.Assists = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Headshots
    {
        get => Stats?.Headshots ?? 0;
        set { if (Stats == null) Stats = new PlayerStats(); Stats.Headshots = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Bodyshots
    {
        get => Stats?.Bodyshots ?? 0;
        set { if (Stats == null) Stats = new PlayerStats(); Stats.Bodyshots = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Legshots
    {
        get => Stats?.Legshots ?? 0;
        set { if (Stats == null) Stats = new PlayerStats(); Stats.Legshots = value; }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Damage
    {
        get => Stats?.Damage?.Dealt ?? 0;
        set
        {
            if (Stats == null) Stats = new PlayerStats();
            if (Stats.Damage == null) Stats.Damage = new DamageInfo();
            Stats.Damage.Dealt = value;
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public int Shutdowns { get; set; }
}

/// <summary>
/// エージェント情報
/// </summary>
public class AgentInfo
{
    /// <summary>
    /// エージェントID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// エージェント名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// プレイヤー統計情報
/// </summary>
public class PlayerStats
{
    /// <summary>
    /// スコア
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// キル数
    /// </summary>
    public int Kills { get; set; }

    /// <summary>
    /// デス数
    /// </summary>
    public int Deaths { get; set; }

    /// <summary>
    /// アシスト数
    /// </summary>
    public int Assists { get; set; }

    /// <summary>
    /// ヘッドショット数
    /// </summary>
    public int Headshots { get; set; }

    /// <summary>
    /// ボディショット数
    /// </summary>
    public int Bodyshots { get; set; }

    /// <summary>
    /// レッグショット数
    /// </summary>
    public int Legshots { get; set; }

    /// <summary>
    /// ダメージ情報
    /// </summary>
    public DamageInfo? Damage { get; set; }
}

/// <summary>
/// ダメージ情報
/// </summary>
public class DamageInfo
{
    /// <summary>
    /// 与ダメージ
    /// </summary>
    public int Dealt { get; set; }

    /// <summary>
    /// 被ダメージ
    /// </summary>
    public int Received { get; set; }
}

/// <summary>
/// アビリティ使用回数
/// </summary>
public class AbilityCasts
{
    /// <summary>
    /// グレネード使用回数
    /// </summary>
    public int Grenade { get; set; }

    /// <summary>
    /// アビリティ1使用回数
    /// </summary>
    public int Ability1 { get; set; }

    /// <summary>
    /// アビリティ2使用回数
    /// </summary>
    public int Ability2 { get; set; }

    /// <summary>
    /// アルティメット使用回数
    /// </summary>
    public int Ultimate { get; set; }
}

/// <summary>
/// ティア情報
/// </summary>
public class TierInfo
{
    /// <summary>
    /// ティアID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ティア名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// カスタマイズ情報
/// </summary>
public class CustomizationInfo
{
    /// <summary>
    /// カードID
    /// </summary>
    public string Card { get; set; } = string.Empty;

    /// <summary>
    /// タイトルID
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// レベルボーダーID
    /// </summary>
    public string? Preferred_level_border { get; set; }
}

/// <summary>
/// 行動情報
/// </summary>
public class BehaviorInfo
{
    /// <summary>
    /// AFK回数
    /// </summary>
    public float Afk_rounds { get; set; }

    /// <summary>
    /// フレンドリーファイア情報
    /// </summary>
    public FriendlyFireInfo? Friendly_fire { get; set; }

    /// <summary>
    /// スポーンにいたラウンド数
    /// </summary>
    public float Rounds_in_spawn { get; set; }
}

/// <summary>
/// フレンドリーファイア情報
/// </summary>
public class FriendlyFireInfo
{
    /// <summary>
    /// 受けたフレンドリーファイア
    /// </summary>
    public float Incoming { get; set; }

    /// <summary>
    /// 与えたフレンドリーファイア
    /// </summary>
    public float Outgoing { get; set; }
}

/// <summary>
/// 経済情報
/// </summary>
public class EconomyInfo
{
    /// <summary>
    /// 消費情報
    /// </summary>
    public EconomyValueInfo? Spent { get; set; }

    /// <summary>
    /// 装備価値情報
    /// </summary>
    public EconomyValueInfo? Loadout_value { get; set; }
}

/// <summary>
/// 経済価値情報
/// </summary>
public class EconomyValueInfo
{
    /// <summary>
    /// 合計値
    /// </summary>
    public int Overall { get; set; }

    /// <summary>
    /// 平均値
    /// </summary>
    public float Average { get; set; }
}

/// <summary>
/// チームの試合データ
/// </summary>
public class MatchTeamData
{
    /// <summary>
    /// チームID
    /// </summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// 勝利したか
    /// </summary>
    public bool WonRounds { get; set; }

    /// <summary>
    /// ラウンド数
    /// </summary>
    public int RoundsWon { get; set; }
}

/// <summary>
/// ラウンドデータ
/// </summary>
public class MatchRoundData
{
    /// <summary>
    /// ラウンド番号
    /// </summary>
    public int RoundNumber { get; set; }

    /// <summary>
    /// 勝利チーム
    /// </summary>
    public string WinningTeam { get; set; } = string.Empty;

    /// <summary>
    /// プラント有無
    /// </summary>
    public bool BombPlanted { get; set; }

    /// <summary>
    /// ディフューズ有無
    /// </summary>
    public bool BombDefused { get; set; }
}

#endregion