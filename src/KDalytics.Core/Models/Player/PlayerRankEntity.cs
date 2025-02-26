using KDalytics.Core.Enums;

namespace KDalytics.Core.Models.Player;

/// <summary>
/// プレイヤーのMMR(マッチメイキングレーティング)情報
/// </summary>
public record PlayerRankEntity
{
    /// <summary>
    /// プレイヤーID (外部キー)
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 現在のランクID (数値表現、例: 24 = Immortal 3)
    /// </summary>
    public int CurrentTier { get; init; }

    /// <summary>
    /// 現在のランク名 (人間可読形式、例: "Immortal 3")
    /// </summary>
    public string CurrentTierPatched { get; init; } = string.Empty;

    /// <summary>
    /// 現在のティア内でのランクレーティング (0-100)
    /// </summary>
    public int RankingInTier { get; init; }

    /// <summary>
    /// 最後の試合でのMMR変動値
    /// </summary>
    public int MmrChangeToLastGame { get; init; }

    /// <summary>
    /// エピソード/アクトのID
    /// </summary>
    public string SeasonId { get; init; } = string.Empty;

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 現在のランクを取得します
    /// </summary>
    public Rank Rank => RankExtensions.FromId(CurrentTier);

    /// <summary>
    /// ランク情報のフォーマットされた文字列を取得します
    /// </summary>
    /// <returns>「プラチナ 2 (75RR)」のようなフォーマットされた文字列</returns>
    public string GetFormattedRank()
    {
        return $"{Rank.ToDisplayString()} ({RankingInTier}RR)";
    }

    /// <summary>
    /// MMR変化の文字列表現を取得します（+/- 付きの数値）
    /// </summary>
    /// <returns>"+15" や "-20" のような形式のMMR変化</returns>
    public string GetMmrChangeString()
    {
        string sign = MmrChangeToLastGame >= 0 ? "+" : "";
        return $"{sign}{MmrChangeToLastGame}";
    }

    /// <summary>
    /// プレイヤーのランク情報の更新されたコピーを生成します
    /// </summary>
    /// <returns>新しいPlayerRankEntityインスタンス（最終更新日時が現在時刻）</returns>
    public PlayerRankEntity WithUpdatedTimestamp() => this with { LastUpdated = DateTime.UtcNow };
}