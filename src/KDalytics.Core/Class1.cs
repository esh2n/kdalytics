using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KDalytics.Core;

/// <summary>
/// KDalyticsのコアロジックを提供します
/// </summary>
public static class KDalyticsCore
{
    /// <summary>
    /// KDalyticsコアサービスの依存性を登録します
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>構成されたサービスコレクション</returns>
    public static IServiceCollection AddKDalyticsCore(this IServiceCollection services)
    {
        // サービスの登録は実装側で行うため、ここではインターフェースの登録は行いません

        return services;
    }
}

/// <summary>
/// KDalyticsの運用オプションを定義します
/// </summary>
public class KDalyticsOptions
{
    /// <summary>
    /// Henrik API設定
    /// </summary>
    public HenrikApiOptions HenrikApi { get; set; } = new();

    /// <summary>
    /// Tracker API設定
    /// </summary>
    public TrackerApiOptions TrackerApi { get; set; } = new();

    /// <summary>
    /// Elasticsearch設定
    /// </summary>
    public ElasticsearchOptions Elasticsearch { get; set; } = new();

    /// <summary>
    /// Discord設定
    /// </summary>
    public DiscordOptions Discord { get; set; } = new();
}

/// <summary>
/// Henrik API設定
/// </summary>
public class HenrikApiOptions
{
    /// <summary>
    /// APIキー
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 基本URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.henrikdev.xyz";

    /// <summary>
    /// タイムアウト（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// リトライ回数
    /// </summary>
    public int RetryCount { get; set; } = 3;
}

/// <summary>
/// Tracker API設定
/// </summary>
public class TrackerApiOptions
{
    /// <summary>
    /// APIキー
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 基本URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.tracker.gg/api/v2/valorant";

    /// <summary>
    /// タイムアウト（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 15;
}

/// <summary>
/// Elasticsearch設定
/// </summary>
public class ElasticsearchOptions
{
    /// <summary>
    /// URLリスト
    /// </summary>
    public List<string> Urls { get; set; } = new();

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// パスワード
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// プレイヤーインデックス名
    /// </summary>
    public string PlayerIndex { get; set; } = "kdalytics-players";

    /// <summary>
    /// プレイヤーランクインデックス名
    /// </summary>
    public string PlayerRankIndex { get; set; } = "kdalytics-player-ranks";

    /// <summary>
    /// 試合インデックス名
    /// </summary>
    public string MatchIndex { get; set; } = "kdalytics-matches";

    /// <summary>
    /// パフォーマンスインデックス名
    /// </summary>
    public string PerformanceIndex { get; set; } = "kdalytics-performances";
}

/// <summary>
/// Discord設定
/// </summary>
public class DiscordOptions
{
    /// <summary>
    /// Botトークン
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// 通知チャンネルID
    /// </summary>
    public string NotificationChannelId { get; set; } = string.Empty;

    /// <summary>
    /// Webhookを使用するか
    /// </summary>
    public bool UseWebhook { get; set; } = false;

    /// <summary>
    /// WebhookのURL
    /// </summary>
    public string? WebhookUrl { get; set; }
}
