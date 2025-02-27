using System;

namespace KDalytics.Infrastructure.Configuration;

/// <summary>
/// Elasticsearch設定
/// </summary>
public class ElasticsearchSettings
{
    /// <summary>
    /// 接続URL（カンマ区切りで複数指定可能）
    /// </summary>
    public string[] Urls { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 基本認証ユーザー名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 基本認証パスワード
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// APIキー
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 最大リトライ回数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// タイムアウト（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// インデックス名のプレフィックス
    /// </summary>
    public string IndexPrefix { get; set; } = "valorant-";

    /// <summary>
    /// インデックス設定
    /// </summary>
    public ElasticsearchIndexSettings IndexSettings { get; set; } = new();
}

/// <summary>
/// Elasticsearchインデックス設定
/// </summary>
public class ElasticsearchIndexSettings
{
    /// <summary>
    /// プレイヤーインデックス名
    /// </summary>
    public string PlayerIndex => "players";

    /// <summary>
    /// プレイヤーランクインデックス名
    /// </summary>
    public string PlayerRankIndex => "player-ranks";

    /// <summary>
    /// 試合インデックス名
    /// </summary>
    public string MatchIndex => "matches";

    /// <summary>
    /// プレイヤーパフォーマンスインデックス名
    /// </summary>
    public string PlayerPerformanceIndex => "player-performances";

    /// <summary>
    /// トラッキング設定インデックス名
    /// </summary>
    public string TrackingConfigIndex => "tracking-configs";

    /// <summary>
    /// シャード数
    /// </summary>
    public int NumberOfShards { get; set; } = 1;

    /// <summary>
    /// レプリカ数
    /// </summary>
    public int NumberOfReplicas { get; set; } = 0;

    /// <summary>
    /// リフレッシュ間隔
    /// </summary>
    public string RefreshInterval { get; set; } = "5s";

    /// <summary>
    /// ディスクウォーターマーク（高）
    /// </summary>
    public string HighDiskWatermark { get; set; } = "85%";

    /// <summary>
    /// ディスクウォーターマーク（低）
    /// </summary>
    public string LowDiskWatermark { get; set; } = "75%";

    /// <summary>
    /// インデックス名を生成
    /// </summary>
    /// <param name="prefix">プレフィックス</param>
    /// <param name="baseName">ベース名</param>
    /// <returns>完全なインデックス名</returns>
    public string GetFullIndexName(string prefix, string baseName)
    {
        return $"{prefix}{baseName}";
    }
}