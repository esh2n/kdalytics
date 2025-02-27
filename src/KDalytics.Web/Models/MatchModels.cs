namespace KDalytics.Web.Models;

/// <summary>
/// 試合情報のモデル
/// </summary>
public record MatchModel
{
    /// <summary>
    /// 試合ID
    /// </summary>
    public string MatchId { get; init; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; init; } = string.Empty;

    /// <summary>
    /// マップ名
    /// </summary>
    public string MapName { get; init; } = string.Empty;

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string GameMode { get; init; } = string.Empty;

    /// <summary>
    /// 試合開始時間
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// 試合時間（ミリ秒）
    /// </summary>
    public long GameLength { get; init; }

    /// <summary>
    /// リージョン
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// シーズンID
    /// </summary>
    public string SeasonId { get; init; } = string.Empty;

    /// <summary>
    /// チーム情報
    /// </summary>
    public List<TeamDataModel> Teams { get; init; } = new();

    /// <summary>
    /// プレイヤーパフォーマンス情報
    /// </summary>
    public List<PlayerMatchPerformanceModel> Players { get; init; } = new();

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; }

    /// <summary>
    /// 試合時間（表示用）
    /// </summary>
    public string FormattedGameLength => TimeSpan.FromMilliseconds(GameLength).ToString(@"mm\:ss");

    /// <summary>
    /// 試合日時（表示用）
    /// </summary>
    public string FormattedStartTime => StartTime.LocalDateTime.ToString("yyyy/MM/dd HH:mm");
}

/// <summary>
/// チーム情報モデル
/// </summary>
public record TeamDataModel
{
    /// <summary>
    /// チームID
    /// </summary>
    public string TeamId { get; init; } = string.Empty;

    /// <summary>
    /// 勝利したかどうか
    /// </summary>
    public bool HasWon { get; init; }

    /// <summary>
    /// 取得したラウンド数
    /// </summary>
    public int RoundsWon { get; init; }
}

/// <summary>
/// プレイヤーの試合パフォーマンスモデル
/// </summary>
public record PlayerMatchPerformanceModel
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 試合ID
    /// </summary>
    public string MatchId { get; init; } = string.Empty;

    /// <summary>
    /// チームID
    /// </summary>
    public string TeamId { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string PlayerName { get; init; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string TagLine { get; init; } = string.Empty;

    /// <summary>
    /// エージェント名
    /// </summary>
    public string AgentName { get; init; } = string.Empty;

    /// <summary>
    /// スコア
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// キル数
    /// </summary>
    public int Kills { get; init; }

    /// <summary>
    /// デス数
    /// </summary>
    public int Deaths { get; init; }

    /// <summary>
    /// アシスト数
    /// </summary>
    public int Assists { get; init; }

    /// <summary>
    /// 与ダメージ
    /// </summary>
    public int DamageDealt { get; init; }

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// 表示用の名前（PlayerName#TagLine）
    /// </summary>
    public string DisplayName => $"{PlayerName}#{TagLine}";

    /// <summary>
    /// KDAの表示用文字列
    /// </summary>
    public string KdaDisplay => $"{Kills}/{Deaths}/{Assists}";

    /// <summary>
    /// K/Dレシオ
    /// </summary>
    public float KdRatio => Deaths > 0 ? (float)Kills / Deaths : Kills;

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio => Deaths > 0 ? (float)(Kills + Assists) / Deaths : Kills + Assists;

    /// <summary>
    /// ヘッドショット率（表示用）
    /// </summary>
    public string HeadshotPercentageDisplay => $"{HeadshotPercentage:P1}";
}

/// <summary>
/// 試合検索リクエストモデル
/// </summary>
public record MatchFilterRequest
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 開始日時
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// 終了日時
    /// </summary>
    public DateTime? To { get; init; }

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string? GameMode { get; init; }

    /// <summary>
    /// スキップする件数
    /// </summary>
    public int? Skip { get; init; }

    /// <summary>
    /// 取得する件数
    /// </summary>
    public int? Take { get; init; }
}