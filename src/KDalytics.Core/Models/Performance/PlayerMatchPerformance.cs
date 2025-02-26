namespace KDalytics.Core.Models.Performance;

/// <summary>
/// 試合内のプレイヤーパフォーマンス情報
/// </summary>
public record PlayerMatchPerformance
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
    /// 使用したエージェントID
    /// </summary>
    public string AgentId { get; init; } = string.Empty;

    /// <summary>
    /// エージェント名
    /// </summary>
    public string AgentName { get; init; } = string.Empty;

    /// <summary>
    /// 総合スコア
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
    /// ヘッドショット回数
    /// </summary>
    public int Headshots { get; init; }

    /// <summary>
    /// ボディショット回数
    /// </summary>
    public int Bodyshots { get; init; }

    /// <summary>
    /// レッグショット回数
    /// </summary>
    public int Legshots { get; init; }

    /// <summary>
    /// プレイヤーのRiot IDを取得します
    /// </summary>
    /// <returns>「PlayerName#TagLine」形式の文字列</returns>
    public string GetRiotId() => $"{PlayerName}#{TagLine}";

    /// <summary>
    /// KDAレシオを計算します
    /// </summary>
    /// <returns>KDAレシオ = (キル + アシスト) / デス</returns>
    public float GetKdaRatio()
    {
        if (Deaths == 0)
            return Kills + Assists; // デスが0の場合、キル+アシストをそのまま返す

        return (float)(Kills + Assists) / Deaths;
    }

    /// <summary>
    /// KDAを表示用文字列として取得します
    /// </summary>
    /// <returns>「K/D/A: 12/4/7 (4.75)」形式の文字列</returns>
    public string GetKdaDisplay()
    {
        return $"K/D/A: {Kills}/{Deaths}/{Assists} ({GetKdaRatio():F2})";
    }

    /// <summary>
    /// ヘッドショット率を計算します
    /// </summary>
    /// <returns>ヘッドショット率（%）</returns>
    public float GetHeadshotPercentage()
    {
        int totalShots = Headshots + Bodyshots + Legshots;
        if (totalShots == 0)
            return 0;

        return (float)Headshots / totalShots * 100;
    }

    /// <summary>
    /// 平均与ダメージを計算します
    /// </summary>
    /// <param name="rounds">ラウンド数（デフォルト: 0 = 計算不能）</param>
    /// <returns>ラウンドあたりの平均与ダメージ</returns>
    public float GetAverageDamagePerRound(int rounds = 0)
    {
        if (rounds <= 0)
            return 0;

        return (float)DamageDealt / rounds;
    }
}