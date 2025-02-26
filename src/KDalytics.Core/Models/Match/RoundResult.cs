namespace KDalytics.Core.Models.Match;

/// <summary>
/// ラウンドの結果情報
/// </summary>
public record RoundResult
{
    /// <summary>
    /// ラウンド番号
    /// </summary>
    public int RoundNum { get; init; }

    /// <summary>
    /// ラウンドの勝者 (攻撃側/防衛側)
    /// </summary>
    public string WinningTeam { get; init; } = string.Empty;

    /// <summary>
    /// スパイクの設置有無
    /// </summary>
    public bool BombPlanted { get; init; }

    /// <summary>
    /// スパイクの解除有無
    /// </summary>
    public bool BombDefused { get; init; }

    /// <summary>
    /// ラウンド結果の要約を取得します
    /// </summary>
    /// <returns>ラウンド結果の簡潔な説明</returns>
    public string GetSummary()
    {
        string winnerDisplay = WinningTeam.ToLowerInvariant() switch
        {
            "attack" or "attackers" => "攻撃側",
            "defense" or "defenders" => "防衛側",
            "red" => "レッドチーム",
            "blue" => "ブルーチーム",
            _ => WinningTeam
        };

        string result = $"ラウンド{RoundNum}: {winnerDisplay}の勝利";

        if (BombPlanted)
        {
            result += " (スパイク設置";
            if (BombDefused)
            {
                result += "・解除)";
            }
            else
            {
                result += ")";
            }
        }
        else if (WinningTeam.ToLowerInvariant() is "attack" or "attackers" or "red")
        {
            result += " (殲滅)";
        }

        return result;
    }
}