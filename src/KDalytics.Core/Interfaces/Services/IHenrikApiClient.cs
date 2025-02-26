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
    public string Team { get; set; } = string.Empty;

    /// <summary>
    /// レベル
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 使用キャラクター
    /// </summary>
    public string Character { get; set; } = string.Empty;

    /// <summary>
    /// 現在のランク（ティア数値）
    /// </summary>
    public int CurrentTier { get; set; }

    /// <summary>
    /// 現在のランク名
    /// </summary>
    public string CurrentTierPatched { get; set; } = string.Empty;

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
    /// シャットダウン数
    /// </summary>
    public int Shutdowns { get; set; }

    /// <summary>
    /// 与ダメージ
    /// </summary>
    public int Damage { get; set; }

    /// <summary>
    /// ヘッドショット
    /// </summary>
    public int Headshots { get; set; }

    /// <summary>
    /// ボディショット
    /// </summary>
    public int Bodyshots { get; set; }

    /// <summary>
    /// レッグショット
    /// </summary>
    public int Legshots { get; set; }
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