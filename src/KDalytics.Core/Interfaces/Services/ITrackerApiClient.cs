namespace KDalytics.Core.Interfaces.Services;

/// <summary>
/// Tracker Network APIクライアントのインターフェース
/// </summary>
public interface ITrackerApiClient
{
    /// <summary>
    /// プレイヤープロフィールを取得
    /// </summary>
    /// <param name="name">プレイヤー名</param>
    /// <param name="tag">タグ</param>
    /// <param name="forceCollect">強制的に最新データを取得するか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤープロフィールレスポンス</returns>
    Task<TrackerProfileResponse> GetPlayerProfileAsync(string name, string tag, bool forceCollect = false, CancellationToken cancellationToken = default);
}

#region Response Models

/// <summary>
/// Tracker Networkプロフィールレスポンスモデル
/// </summary>
public class TrackerProfileResponse
{
    /// <summary>
    /// プロフィールデータ
    /// </summary>
    public TrackerProfileData? Data { get; set; }
}

/// <summary>
/// プロフィールデータ
/// </summary>
public class TrackerProfileData
{
    /// <summary>
    /// プラットフォーム情報
    /// </summary>
    public TrackerPlatformInfo? PlatformInfo { get; set; }

    /// <summary>
    /// ユーザー情報
    /// </summary>
    public TrackerUserInfo? UserInfo { get; set; }

    /// <summary>
    /// セグメント情報（エージェント別、マップ別など）
    /// </summary>
    public List<TrackerSegment> Segments { get; set; } = new();

    /// <summary>
    /// 統計情報
    /// </summary>
    public List<TrackerStat> Stats { get; set; } = new();

    /// <summary>
    /// 最近の試合
    /// </summary>
    public List<TrackerMatch> Matches { get; set; } = new();
}

/// <summary>
/// プラットフォーム情報
/// </summary>
public class TrackerPlatformInfo
{
    /// <summary>
    /// プラットフォームスラッグ
    /// </summary>
    public string PlatformSlug { get; set; } = string.Empty;

    /// <summary>
    /// プラットフォーム名
    /// </summary>
    public string PlatformName { get; set; } = string.Empty;

    /// <summary>
    /// プラットフォームユーザー識別子
    /// </summary>
    public string PlatformUserIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// プラットフォームユーザーハンドル
    /// </summary>
    public string PlatformUserHandle { get; set; } = string.Empty;

    /// <summary>
    /// アバターURL
    /// </summary>
    public string AvatarUrl { get; set; } = string.Empty;
}

/// <summary>
/// ユーザー情報
/// </summary>
public class TrackerUserInfo
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 表示名
    /// </summary>
    public string UserDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// ランク情報
    /// </summary>
    public TrackerRank? CurrentRank { get; set; }
}

/// <summary>
/// ランク情報
/// </summary>
public class TrackerRank
{
    /// <summary>
    /// ランク名
    /// </summary>
    public string RankName { get; set; } = string.Empty;

    /// <summary>
    /// ティア
    /// </summary>
    public int Tier { get; set; }

    /// <summary>
    /// ランクスコア
    /// </summary>
    public int RankScore { get; set; }

    /// <summary>
    /// ランクイメージURL
    /// </summary>
    public string RankImage { get; set; } = string.Empty;
}

/// <summary>
/// セグメント情報（エージェント別、マップ別など）
/// </summary>
public class TrackerSegment
{
    /// <summary>
    /// セグメントタイプ（agent, map, weapon など）
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// セグメント属性
    /// </summary>
    public TrackerSegmentAttributes? Attributes { get; set; }

    /// <summary>
    /// 統計情報
    /// </summary>
    public Dictionary<string, TrackerSegmentStat> Stats { get; set; } = new();
}

/// <summary>
/// セグメント属性
/// </summary>
public class TrackerSegmentAttributes
{
    /// <summary>
    /// エージェント名
    /// </summary>
    public string? Agent { get; set; }

    /// <summary>
    /// マップ名
    /// </summary>
    public string? Map { get; set; }

    /// <summary>
    /// 武器名
    /// </summary>
    public string? Weapon { get; set; }
}

/// <summary>
/// セグメント統計情報
/// </summary>
public class TrackerSegmentStat
{
    /// <summary>
    /// 統計名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 表示値
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// 値
    /// </summary>
    public double Value { get; set; }
}

/// <summary>
/// 統計情報
/// </summary>
public class TrackerStat
{
    /// <summary>
    /// 統計名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 表示値
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// 値
    /// </summary>
    public double Value { get; set; }
}

/// <summary>
/// 試合情報
/// </summary>
public class TrackerMatch
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; set; } = string.Empty;

    /// <summary>
    /// モード
    /// </summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>
    /// 日時
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 試合統計情報
    /// </summary>
    public Dictionary<string, TrackerMatchStat> Stats { get; set; } = new();
}

/// <summary>
/// 試合統計情報
/// </summary>
public class TrackerMatchStat
{
    /// <summary>
    /// 表示名
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 表示カテゴリ
    /// </summary>
    public string? DisplayCategory { get; set; }

    /// <summary>
    /// 表示値
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// 値
    /// </summary>
    public double Value { get; set; }
}

#endregion