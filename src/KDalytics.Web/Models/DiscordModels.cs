using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KDalytics.Web.Models
{
    /// <summary>
    /// Discordサーバー（ギルド）のモデル
    /// </summary>
    public class DiscordServerModel
    {
        /// <summary>
        /// サーバーID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// サーバー名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// サーバーアイコンのURL
        /// </summary>
        [JsonPropertyName("icon_url")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// メンバー数
        /// </summary>
        [JsonPropertyName("member_count")]
        public int MemberCount { get; set; }

        /// <summary>
        /// KDalyticsボットが参加しているかどうか
        /// </summary>
        [JsonPropertyName("has_bot")]
        public bool HasBot { get; set; }

        /// <summary>
        /// ボットが追加された日時
        /// </summary>
        [JsonPropertyName("bot_added_at")]
        public DateTime? BotAddedAt { get; set; }

        /// <summary>
        /// トラッキング中のプレイヤー数
        /// </summary>
        [JsonPropertyName("tracked_player_count")]
        public int TrackedPlayerCount { get; set; }

        /// <summary>
        /// サーバーの設定
        /// </summary>
        [JsonPropertyName("settings")]
        public DiscordServerSettings? Settings { get; set; }
    }

    /// <summary>
    /// Discordサーバーの設定
    /// </summary>
    public class DiscordServerSettings
    {
        /// <summary>
        /// 通知チャンネルID
        /// </summary>
        [JsonPropertyName("notification_channel_id")]
        public string? NotificationChannelId { get; set; }

        /// <summary>
        /// 試合結果の通知を有効にするかどうか
        /// </summary>
        [JsonPropertyName("match_notifications_enabled")]
        public bool MatchNotificationsEnabled { get; set; } = true;

        /// <summary>
        /// ランク変更の通知を有効にするかどうか
        /// </summary>
        [JsonPropertyName("rank_change_notifications_enabled")]
        public bool RankChangeNotificationsEnabled { get; set; } = true;

        /// <summary>
        /// 週間サマリーの通知を有効にするかどうか
        /// </summary>
        [JsonPropertyName("weekly_summary_enabled")]
        public bool WeeklySummaryEnabled { get; set; } = false;

        /// <summary>
        /// 週間サマリーの曜日（0=日曜日, 1=月曜日, ..., 6=土曜日）
        /// </summary>
        [JsonPropertyName("weekly_summary_day")]
        public int WeeklySummaryDay { get; set; } = 0;
    }

    /// <summary>
    /// Discordユーザーのモデル
    /// </summary>
    public class DiscordUserModel
    {
        /// <summary>
        /// ユーザーID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// ユーザー名
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// ディスクリミネーター（#以降の番号）
        /// </summary>
        [JsonPropertyName("discriminator")]
        public string? Discriminator { get; set; }

        /// <summary>
        /// アバターのURL
        /// </summary>
        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// ユーザーが所有するサーバー一覧
        /// </summary>
        [JsonPropertyName("owned_servers")]
        public List<DiscordServerModel>? OwnedServers { get; set; }

        /// <summary>
        /// ユーザーが管理者権限を持つサーバー一覧
        /// </summary>
        [JsonPropertyName("admin_servers")]
        public List<DiscordServerModel>? AdminServers { get; set; }

        /// <summary>
        /// ユーザーが参加しているサーバー一覧
        /// </summary>
        [JsonPropertyName("member_servers")]
        public List<DiscordServerModel>? MemberServers { get; set; }
    }

    /// <summary>
    /// Discord認証レスポンスのモデル
    /// </summary>
    public class DiscordAuthResponse
    {
        /// <summary>
        /// アクセストークン
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// トークンタイプ
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// 有効期限（秒）
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// リフレッシュトークン
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// スコープ
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }
}