namespace KDalytics.Web.Models;

/// <summary>
/// パフォーマンス統計情報のモデル
/// </summary>
public record PerformanceStatsModel
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 集計期間（開始）
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// 集計期間（終了）
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// 総試合数
    /// </summary>
    public int MatchesPlayed { get; init; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int MatchesWon { get; init; }

    /// <summary>
    /// 敗北数
    /// </summary>
    public int Losses { get; init; }

    /// <summary>
    /// 勝率
    /// </summary>
    public float WinRate { get; init; }

    /// <summary>
    /// 総キル数
    /// </summary>
    public int TotalKills { get; init; }

    /// <summary>
    /// 総デス数
    /// </summary>
    public int TotalDeaths { get; init; }

    /// <summary>
    /// 総アシスト数
    /// </summary>
    public int TotalAssists { get; init; }

    /// <summary>
    /// 平均キル数
    /// </summary>
    public float AverageKills { get; init; }

    /// <summary>
    /// 平均デス数
    /// </summary>
    public float AverageDeaths { get; init; }

    /// <summary>
    /// 平均アシスト数
    /// </summary>
    public float AverageAssists { get; init; }

    /// <summary>
    /// K/Dレシオ
    /// </summary>
    public float KdRatio { get; init; }

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// 最も使用したエージェント
    /// </summary>
    public string MostPlayedAgent { get; init; } = string.Empty;

    /// <summary>
    /// エージェント別統計
    /// </summary>
    public Dictionary<string, AgentPerformanceModel> AgentStats { get; init; } = new();

    /// <summary>
    /// マップ別統計
    /// </summary>
    public Dictionary<string, MapPerformanceModel> MapStats { get; init; } = new();

    /// <summary>
    /// 勝率（表示用）
    /// </summary>
    public string WinRateDisplay => $"{WinRate:P1}";

    /// <summary>
    /// KDA（表示用）
    /// </summary>
    public string KdaDisplay => $"{AverageKills:F1}/{AverageDeaths:F1}/{AverageAssists:F1}";

    /// <summary>
    /// K/Dレシオ（表示用）
    /// </summary>
    public string KdRatioDisplay => $"{KdRatio:F2}";

    /// <summary>
    /// KDAレシオ（表示用）
    /// </summary>
    public string KdaRatioDisplay => $"{KdaRatio:F2}";

    /// <summary>
    /// ヘッドショット率（表示用）
    /// </summary>
    public string HeadshotPercentageDisplay => $"{HeadshotPercentage:P1}";
}

/// <summary>
/// エージェント別パフォーマンスモデル
/// </summary>
public record AgentPerformanceModel
{
    /// <summary>
    /// エージェント名
    /// </summary>
    public string AgentName { get; init; } = string.Empty;

    /// <summary>
    /// 使用回数
    /// </summary>
    public int GamesPlayed { get; init; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int GamesWon { get; init; }

    /// <summary>
    /// 敗北数
    /// </summary>
    public int Losses { get; init; }

    /// <summary>
    /// 勝率
    /// </summary>
    public float WinRate { get; init; }

    /// <summary>
    /// 平均キル
    /// </summary>
    public float AverageKills { get; init; }

    /// <summary>
    /// 平均デス
    /// </summary>
    public float AverageDeaths { get; init; }

    /// <summary>
    /// 平均アシスト
    /// </summary>
    public float AverageAssists { get; init; }

    /// <summary>
    /// K/Dレシオ
    /// </summary>
    public float KdRatio { get; init; }

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// 勝率（表示用）
    /// </summary>
    public string WinRateDisplay => $"{WinRate:P1}";

    /// <summary>
    /// KDA（表示用）
    /// </summary>
    public string KdaDisplay => $"{AverageKills:F1}/{AverageDeaths:F1}/{AverageAssists:F1}";

    /// <summary>
    /// K/Dレシオ（表示用）
    /// </summary>
    public string KdRatioDisplay => $"{KdRatio:F2}";

    /// <summary>
    /// KDAレシオ（表示用）
    /// </summary>
    public string KdaRatioDisplay => $"{KdaRatio:F2}";

    /// <summary>
    /// ヘッドショット率（表示用）
    /// </summary>
    public string HeadshotPercentageDisplay => $"{HeadshotPercentage:P1}";
}

/// <summary>
/// マップ別パフォーマンスモデル
/// </summary>
public record MapPerformanceModel
{
    /// <summary>
    /// マップ名
    /// </summary>
    public string MapName { get; init; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; init; } = string.Empty;

    /// <summary>
    /// プレイ回数
    /// </summary>
    public int GamesPlayed { get; init; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int GamesWon { get; init; }

    /// <summary>
    /// 敗北数
    /// </summary>
    public int Losses { get; init; }

    /// <summary>
    /// 勝率
    /// </summary>
    public float WinRate { get; init; }

    /// <summary>
    /// 平均キル
    /// </summary>
    public float AverageKills { get; init; }

    /// <summary>
    /// 平均デス
    /// </summary>
    public float AverageDeaths { get; init; }

    /// <summary>
    /// 平均アシスト
    /// </summary>
    public float AverageAssists { get; init; }

    /// <summary>
    /// K/Dレシオ
    /// </summary>
    public float KdRatio { get; init; }

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

    /// <summary>
    /// 勝率（表示用）
    /// </summary>
    public string WinRateDisplay => $"{WinRate:P1}";

    /// <summary>
    /// KDA（表示用）
    /// </summary>
    public string KdaDisplay => $"{AverageKills:F1}/{AverageDeaths:F1}/{AverageAssists:F1}";

    /// <summary>
    /// K/Dレシオ（表示用）
    /// </summary>
    public string KdRatioDisplay => $"{KdRatio:F2}";

    /// <summary>
    /// KDAレシオ（表示用）
    /// </summary>
    public string KdaRatioDisplay => $"{KdaRatio:F2}";
}

/// <summary>
/// パフォーマンス統計リクエストモデル
/// </summary>
public record PerformanceStatsRequest
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 開始日時
    /// </summary>
    public DateTime From { get; init; } = DateTime.UtcNow.AddDays(-30);

    /// <summary>
    /// 終了日時
    /// </summary>
    public DateTime To { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string? GameMode { get; init; }
}

/// <summary>
/// KDAランキングモデル
/// </summary>
public record KdaRankingModel
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string PlayerName { get; init; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string TagLine { get; init; } = string.Empty;

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

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
    /// プレイした試合数
    /// </summary>
    public int GamesPlayed { get; init; }

    /// <summary>
    /// 表示用の名前（PlayerName#TagLine）
    /// </summary>
    public string DisplayName => $"{PlayerName}#{TagLine}";

    /// <summary>
    /// KDA（表示用）
    /// </summary>
    public string KdaDisplay => $"{Kills}/{Deaths}/{Assists}";

    /// <summary>
    /// KDAレシオ（表示用）
    /// </summary>
    public string KdaRatioDisplay => $"{KdaRatio:F2}";
}

/// <summary>
/// KDAランキングリクエストモデル
/// </summary>
public record KdaRankingRequest
{
    /// <summary>
    /// プレイヤーIDのリスト
    /// </summary>
    public List<string> Puuids { get; init; } = new();

    /// <summary>
    /// 開始日時
    /// </summary>
    public DateTime From { get; init; } = DateTime.UtcNow.AddDays(-30);

    /// <summary>
    /// 終了日時
    /// </summary>
    public DateTime To { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string? GameMode { get; init; }

    /// <summary>
    /// 最低試合数
    /// </summary>
    public int MinGames { get; init; } = 1;
}