namespace KDalytics.Core.Models.Player;

/// <summary>
/// プレイヤーのトラッキング設定
/// </summary>
public record PlayerTrackingConfig
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// Discordサーバー/チャンネルID（通知先）
    /// </summary>
    public string DiscordChannelId { get; init; } = string.Empty;

    /// <summary>
    /// 試合終了時の通知を有効にするか
    /// </summary>
    public bool EnableMatchNotifications { get; init; } = true;

    /// <summary>
    /// 日次サマリーの通知を有効にするか
    /// </summary>
    public bool EnableDailySummary { get; init; } = true;

    /// <summary>
    /// ランク変更時の通知を有効にするか
    /// </summary>
    public bool EnableRankChangeNotifications { get; init; } = true;

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 設定の更新されたコピーを生成します
    /// </summary>
    /// <returns>新しいPlayerTrackingConfigインスタンス（最終更新日時が現在時刻）</returns>
    public PlayerTrackingConfig WithUpdatedTimestamp() => this with { LastUpdated = DateTime.UtcNow };

    /// <summary>
    /// 試合通知設定を変更したコピーを生成します
    /// </summary>
    /// <param name="enable">試合通知を有効にするかどうか</param>
    /// <returns>新しいPlayerTrackingConfigインスタンス（試合通知設定が変更済み）</returns>
    public PlayerTrackingConfig WithMatchNotifications(bool enable) =>
        this with { EnableMatchNotifications = enable, LastUpdated = DateTime.UtcNow };

    /// <summary>
    /// 日次サマリー通知設定を変更したコピーを生成します
    /// </summary>
    /// <param name="enable">日次サマリー通知を有効にするかどうか</param>
    /// <returns>新しいPlayerTrackingConfigインスタンス（日次サマリー通知設定が変更済み）</returns>
    public PlayerTrackingConfig WithDailySummaryNotifications(bool enable) =>
        this with { EnableDailySummary = enable, LastUpdated = DateTime.UtcNow };

    /// <summary>
    /// ランク変更通知設定を変更したコピーを生成します
    /// </summary>
    /// <param name="enable">ランク変更通知を有効にするかどうか</param>
    /// <returns>新しいPlayerTrackingConfigインスタンス（ランク変更通知設定が変更済み）</returns>
    public PlayerTrackingConfig WithRankChangeNotifications(bool enable) =>
        this with { EnableRankChangeNotifications = enable, LastUpdated = DateTime.UtcNow };
}