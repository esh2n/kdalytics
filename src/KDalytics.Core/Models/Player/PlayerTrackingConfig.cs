using System;

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
    /// トラッキング対象かどうか
    /// </summary>
    public bool IsTracked { get; init; } = true;

    /// <summary>
    /// トラッキング間隔
    /// </summary>
    public TimeSpan TrackingInterval { get; init; } = TimeSpan.FromHours(1);

    /// <summary>
    /// 最後にトラッキングした日時
    /// </summary>
    public DateTime? LastTrackedAt { get; init; }

    /// <summary>
    /// 次回トラッキング予定日時
    /// </summary>
    public DateTime NextTrackingAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// アクティブかどうか
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Discord通知が有効かどうか
    /// </summary>
    public bool DiscordNotificationsEnabled { get; init; } = true;

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
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 設定の更新されたコピーを生成します
    /// </summary>
    /// <returns>新しいPlayerTrackingConfigインスタンス（更新日時が現在時刻）</returns>
    public PlayerTrackingConfig WithUpdatedTimestamp() => this with { UpdatedAt = DateTime.UtcNow };

    /// <summary>
    /// 試合通知設定を変更したコピーを生成します
    /// </summary>
    /// <param name="enable">試合通知を有効にするかどうか</param>
    /// <returns>新しいPlayerTrackingConfigインスタンス（試合通知設定が変更済み）</returns>
    public PlayerTrackingConfig WithMatchNotifications(bool enable) =>
        this with { EnableMatchNotifications = enable, UpdatedAt = DateTime.UtcNow };

    /// <summary>
    /// 日次サマリー通知設定を変更したコピーを生成します
    /// </summary>
    /// <param name="enable">日次サマリー通知を有効にするかどうか</param>
    /// <returns>新しいPlayerTrackingConfigインスタンス（日次サマリー通知設定が変更済み）</returns>
    public PlayerTrackingConfig WithDailySummaryNotifications(bool enable) =>
        this with { EnableDailySummary = enable, UpdatedAt = DateTime.UtcNow };

    /// <summary>
    /// ランク変更通知設定を変更したコピーを生成します
    /// </summary>
    /// <param name="enable">ランク変更通知を有効にするかどうか</param>
    /// <returns>新しいPlayerTrackingConfigインスタンス（ランク変更通知設定が変更済み）</returns>
    public PlayerTrackingConfig WithRankChangeNotifications(bool enable) =>
        this with { EnableRankChangeNotifications = enable, UpdatedAt = DateTime.UtcNow };

    /// <summary>
    /// トラッキング状態を変更したコピーを生成します
    /// </summary>
    /// <param name="isTracked">トラッキング対象とするかどうか</param>
    /// <returns>新しいPlayerTrackingConfigインスタンス（トラッキング状態が変更済み）</returns>
    public PlayerTrackingConfig WithTracking(bool isTracked) =>
        this with { IsTracked = isTracked, UpdatedAt = DateTime.UtcNow };
}