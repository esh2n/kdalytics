namespace KDalytics.Core.Models.Match;

/// <summary>
/// 試合内のチーム情報
/// </summary>
public record TeamData
{
    /// <summary>
    /// チームID (red/blue または attack/defense)
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

    /// <summary>
    /// ラウンドでの勝利情報
    /// </summary>
    public List<RoundResult> RoundResults { get; init; } = new();

    /// <summary>
    /// チーム表示名を取得します
    /// </summary>
    /// <returns>「攻撃側」「防衛側」などのチーム表示名</returns>
    public string GetTeamDisplayName()
    {
        return TeamId.ToLowerInvariant() switch
        {
            "red" => "レッドチーム",
            "blue" => "ブルーチーム",
            "attack" or "attackers" => "攻撃側",
            "defense" or "defenders" => "防衛側",
            _ => TeamId
        };
    }

    /// <summary>
    /// チームの勝敗結果を表示用文字列として取得します
    /// </summary>
    /// <returns>「勝利」または「敗北」</returns>
    public string GetResultDisplayText() => HasWon ? "勝利" : "敗北";
}