using System;

namespace KDalytics.Infrastructure.Configuration;

/// <summary>
/// API設定
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Henrik API設定
    /// </summary>
    public HenrikApiSettings Henrik { get; set; } = new();

    /// <summary>
    /// Tracker Network API設定
    /// </summary>
    public TrackerApiSettings Tracker { get; set; } = new();
}

/// <summary>
/// Henrik API設定
/// </summary>
public class HenrikApiSettings
{
    /// <summary>
    /// ベースURL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.henrikdev.xyz/valorant";

    /// <summary>
    /// APIキー
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// リクエスト間の最小待機時間（ミリ秒）
    /// </summary>
    public int RequestDelayMs { get; set; } = 2000; // デフォルトで2秒の遅延

    /// <summary>
    /// リトライ回数
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// リトライ間隔の初期値（ミリ秒）
    /// </summary>
    public int RetryInitialDelayMs { get; set; } = 1000;

    /// <summary>
    /// リトライ間隔の増加倍率
    /// </summary>
    public double RetryBackoffMultiplier { get; set; } = 2.0;
}

/// <summary>
/// Tracker Network API設定
/// </summary>
public class TrackerApiSettings
{
    /// <summary>
    /// ベースURL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.tracker.gg/api/v2/valorant";

    /// <summary>
    /// APIキー
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// リクエスト間の最小待機時間（ミリ秒）
    /// </summary>
    public int RequestDelayMs { get; set; } = 2000;

    /// <summary>
    /// リトライ回数
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// リトライ間隔の初期値（ミリ秒）
    /// </summary>
    public int RetryInitialDelayMs { get; set; } = 1000;

    /// <summary>
    /// リトライ間隔の増加倍率
    /// </summary>
    public double RetryBackoffMultiplier { get; set; } = 2.0;
}