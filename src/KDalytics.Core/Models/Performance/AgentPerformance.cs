namespace KDalytics.Core.Models.Performance;

/// <summary>
/// エージェント別のパフォーマンス
/// </summary>
public record AgentPerformance
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
    /// K/Dレシオ
    /// </summary>
    public float KdRatio { get; init; }

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
    /// 総ダメージ
    /// </summary>
    public int TotalDamage { get; init; }

    /// <summary>
    /// 総ヘッドショット数
    /// </summary>
    public int TotalHeadshots { get; init; }

    /// <summary>
    /// 総ボディショット数
    /// </summary>
    public int TotalBodyshots { get; init; }

    /// <summary>
    /// 総レッグショット数
    /// </summary>
    public int TotalLegshots { get; init; }

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// 平均KDAレシオを計算します
    /// </summary>
    /// <returns>平均KDAレシオ = (平均キル + 平均アシスト) / 平均デス</returns>
    public float GetAvgKdaRatio()
    {
        if (AverageDeaths <= 0)
            return AverageKills + AverageAssists;

        return (AverageKills + AverageAssists) / AverageDeaths;
    }

    /// <summary>
    /// 勝率を表示用文字列として取得します
    /// </summary>
    /// <returns>「60% (12勝8敗)」形式の文字列</returns>
    public string GetWinRateDisplay()
    {
        return $"{WinRate * 100:F1}% ({GamesWon}勝{Losses}敗)";
    }

    /// <summary>
    /// KDAを表示用文字列として取得します
    /// </summary>
    /// <returns>「K/D/A: 12.2/4.5/7.8 (4.44)」形式の文字列</returns>
    public string GetKdaDisplay()
    {
        return $"K/D/A: {AverageKills:F1}/{AverageDeaths:F1}/{AverageAssists:F1} ({GetAvgKdaRatio():F2})";
    }
    /// <summary>
    /// エージェントパフォーマンスを試合パフォーマンスからマージします
    /// </summary>
    /// <param name="other">追加のパフォーマンス情報</param>
    /// <returns>マージされた新しいAgentPerformanceインスタンス</returns>
    public AgentPerformance MergeWith(AgentPerformance other)
    {
        // 同じエージェントでない場合はそのまま返す
        if (AgentName != other.AgentName)
            return this;

        int totalGamesPlayed = GamesPlayed + other.GamesPlayed;

        // 新しい平均を計算
        float newAvgKills = (AverageKills * GamesPlayed + other.AverageKills * other.GamesPlayed) / totalGamesPlayed;
        float newAvgDeaths = (AverageDeaths * GamesPlayed + other.AverageDeaths * other.GamesPlayed) / totalGamesPlayed;
        float newAvgAssists = (AverageAssists * GamesPlayed + other.AverageAssists * other.GamesPlayed) / totalGamesPlayed;

        return new AgentPerformance
        {
            AgentName = AgentName,
            GamesPlayed = totalGamesPlayed,
            GamesWon = GamesWon + other.GamesWon,
            Losses = Losses + other.Losses,
            WinRate = totalGamesPlayed > 0 ? (float)(GamesWon + other.GamesWon) / totalGamesPlayed : 0,
            TotalKills = TotalKills + other.TotalKills,
            TotalDeaths = TotalDeaths + other.TotalDeaths,
            TotalAssists = TotalAssists + other.TotalAssists,
            KdRatio = (TotalDeaths + other.TotalDeaths) > 0 ? (float)(TotalKills + other.TotalKills) / (TotalDeaths + other.TotalDeaths) : (TotalKills + other.TotalKills),
            AverageKills = newAvgKills,
            AverageDeaths = newAvgDeaths,
            AverageAssists = newAvgAssists,
            TotalDamage = TotalDamage + other.TotalDamage,
            TotalHeadshots = TotalHeadshots + other.TotalHeadshots,
            HeadshotPercentage = (TotalHeadshots + other.TotalHeadshots) > 0 ? (float)(TotalHeadshots + other.TotalHeadshots) / ((TotalHeadshots + other.TotalHeadshots) + (TotalBodyshots + other.TotalBodyshots) + (TotalLegshots + other.TotalLegshots)) : 0
        };
    }
}