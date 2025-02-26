namespace KDalytics.Core.Models.Performance;

/// <summary>
/// パフォーマンス統計情報
/// </summary>
public record PerformanceStats
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 集計期間（開始）
    /// </summary>
    public DateTime From { get; init; }

    /// <summary>
    /// 集計期間（終了）
    /// </summary>
    public DateTime To { get; init; }

    /// <summary>
    /// 総試合数
    /// </summary>
    public int TotalMatches { get; init; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int Wins { get; init; }

    /// <summary>
    /// 敗北数
    /// </summary>
    public int Losses { get; init; }

    /// <summary>
    /// 勝率
    /// </summary>
    public float WinRate => TotalMatches > 0 ? (float)Wins / TotalMatches : 0;

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
    /// KDAレシオ
    /// </summary>
    public float KdaRatio => TotalDeaths > 0
        ? (float)(TotalKills + TotalAssists) / TotalDeaths
        : TotalKills + TotalAssists;

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// エージェント別統計
    /// </summary>
    public Dictionary<string, AgentPerformance> AgentStats { get; init; } = new();

    /// <summary>
    /// マップ別統計
    /// </summary>
    public Dictionary<string, MapPerformance> MapStats { get; init; } = new();

    /// <summary>
    /// 期間を文字列として取得します
    /// </summary>
    /// <returns>「2023/01/01 - 2023/01/31」形式の日付範囲</returns>
    public string GetPeriodText()
    {
        return $"{From:yyyy/MM/dd} - {To:yyyy/MM/dd}";
    }

    /// <summary>
    /// 勝率を表示用文字列として取得します
    /// </summary>
    /// <returns>「60% (12勝8敗)」形式の文字列</returns>
    public string GetWinRateDisplay()
    {
        return $"{WinRate * 100:F1}% ({Wins}勝{Losses}敗)";
    }

    /// <summary>
    /// KDAを表示用文字列として取得します
    /// </summary>
    /// <returns>「K/D/A: 120/45/78 (4.4)」形式の文字列</returns>
    public string GetKdaDisplay()
    {
        return $"K/D/A: {TotalKills}/{TotalDeaths}/{TotalAssists} ({KdaRatio:F1})";
    }

    /// <summary>
    /// 平均KDAを表示用文字列として取得します
    /// </summary>
    /// <returns>「平均 K/D/A: 12.0/4.5/7.8 (4.4)」形式の文字列</returns>
    public string GetAvgKdaDisplay()
    {
        if (TotalMatches <= 0)
            return "データなし";

        float avgKills = (float)TotalKills / TotalMatches;
        float avgDeaths = (float)TotalDeaths / TotalMatches;
        float avgAssists = (float)TotalAssists / TotalMatches;

        return $"平均 K/D/A: {avgKills:F1}/{avgDeaths:F1}/{avgAssists:F1} ({KdaRatio:F1})";
    }

    /// <summary>
    /// ベストエージェントを取得します
    /// </summary>
    /// <param name="minGames">最低試合数（デフォルト: 3）</param>
    /// <returns>最も勝率の高いエージェントのパフォーマンス、または条件を満たすエージェントがない場合はnull</returns>
    public AgentPerformance? GetBestAgent(int minGames = 3)
    {
        return AgentStats.Values
            .Where(a => a.TimesPlayed >= minGames)
            .OrderByDescending(a => a.WinRate)
            .ThenByDescending(a => a.GetAvgKdaRatio())
            .FirstOrDefault();
    }

    /// <summary>
    /// ベストマップを取得します
    /// </summary>
    /// <param name="minGames">最低試合数（デフォルト: 3）</param>
    /// <returns>最も勝率の高いマップのパフォーマンス、または条件を満たすマップがない場合はnull</returns>
    public MapPerformance? GetBestMap(int minGames = 3)
    {
        return MapStats.Values
            .Where(m => m.TimesPlayed >= minGames)
            .OrderByDescending(m => m.WinRate)
            .ThenByDescending(m => m.GetAvgKdaRatio())
            .FirstOrDefault();
    }
}