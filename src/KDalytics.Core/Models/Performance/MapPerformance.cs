namespace KDalytics.Core.Models.Performance;

/// <summary>
/// マップ別のパフォーマンス
/// </summary>
public record MapPerformance
{
    /// <summary>
    /// マップ名
    /// </summary>
    public string MapName { get; init; } = string.Empty;

    /// <summary>
    /// プレイ回数
    /// </summary>
    public int TimesPlayed { get; init; }

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
    public float WinRate => TimesPlayed > 0 ? (float)Wins / TimesPlayed : 0;

    /// <summary>
    /// 平均スコア
    /// </summary>
    public float AvgScore { get; init; }

    /// <summary>
    /// 平均キル
    /// </summary>
    public float AvgKills { get; init; }

    /// <summary>
    /// 平均デス
    /// </summary>
    public float AvgDeaths { get; init; }

    /// <summary>
    /// 平均アシスト
    /// </summary>
    public float AvgAssists { get; init; }

    /// <summary>
    /// 平均KDAレシオを計算します
    /// </summary>
    /// <returns>平均KDAレシオ = (平均キル + 平均アシスト) / 平均デス</returns>
    public float GetAvgKdaRatio()
    {
        if (AvgDeaths <= 0)
            return AvgKills + AvgAssists;

        return (AvgKills + AvgAssists) / AvgDeaths;
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
    /// <returns>「K/D/A: 12.2/4.5/7.8 (4.44)」形式の文字列</returns>
    public string GetKdaDisplay()
    {
        return $"K/D/A: {AvgKills:F1}/{AvgDeaths:F1}/{AvgAssists:F1} ({GetAvgKdaRatio():F2})";
    }

    /// <summary>
    /// マップパフォーマンスを試合パフォーマンスからマージします
    /// </summary>
    /// <param name="other">追加のパフォーマンス情報</param>
    /// <returns>マージされた新しいMapPerformanceインスタンス</returns>
    public MapPerformance MergeWith(MapPerformance other)
    {
        // 同じマップでない場合はそのまま返す
        if (MapName != other.MapName)
            return this;

        int totalTimesPlayed = TimesPlayed + other.TimesPlayed;

        // 新しい平均を計算
        float newAvgScore = (AvgScore * TimesPlayed + other.AvgScore * other.TimesPlayed) / totalTimesPlayed;
        float newAvgKills = (AvgKills * TimesPlayed + other.AvgKills * other.TimesPlayed) / totalTimesPlayed;
        float newAvgDeaths = (AvgDeaths * TimesPlayed + other.AvgDeaths * other.TimesPlayed) / totalTimesPlayed;
        float newAvgAssists = (AvgAssists * TimesPlayed + other.AvgAssists * other.TimesPlayed) / totalTimesPlayed;

        return new MapPerformance
        {
            MapName = MapName,
            TimesPlayed = totalTimesPlayed,
            Wins = Wins + other.Wins,
            Losses = Losses + other.Losses,
            AvgScore = newAvgScore,
            AvgKills = newAvgKills,
            AvgDeaths = newAvgDeaths,
            AvgAssists = newAvgAssists
        };
    }
}