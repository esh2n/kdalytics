namespace KDalytics.Web.Models;

/// <summary>
/// プレイヤー情報のモデル
/// </summary>
public record PlayerModel
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string GameName { get; init; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string TagLine { get; init; } = string.Empty;

    /// <summary>
    /// リージョン
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// アカウントレベル
    /// </summary>
    public int AccountLevel { get; init; }

    /// <summary>
    /// トラッキング対象かどうか
    /// </summary>
    public bool IsTracked { get; init; }

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; }

    /// <summary>
    /// 表示用の名前（GameName#TagLine）
    /// </summary>
    public string DisplayName => $"{GameName}#{TagLine}";
}

/// <summary>
/// プレイヤー検索リクエストモデル
/// </summary>
public record PlayerSearchRequest
{
    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string Tag { get; init; } = string.Empty;
}

/// <summary>
/// プレイヤートラッキング設定リクエストモデル
/// </summary>
public record PlayerTrackingRequest
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// トラッキング対象にするかどうか
    /// </summary>
    public bool Track { get; init; }
}

/// <summary>
/// プレイヤーランク情報モデル
/// </summary>
public record PlayerRankModel
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string GameName { get; init; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string TagLine { get; init; } = string.Empty;

    /// <summary>
    /// 現在のランク（数値）
    /// </summary>
    public int CurrentTier { get; init; }

    /// <summary>
    /// 現在のランク（表示名）
    /// </summary>
    public string CurrentTierName { get; init; } = string.Empty;

    /// <summary>
    /// ティア内のランキング（0-100）
    /// </summary>
    public int RankingInTier { get; init; }

    /// <summary>
    /// MMR
    /// </summary>
    public int Mmr { get; init; }

    /// <summary>
    /// 前回の試合からのMMR変化
    /// </summary>
    public int MmrChange { get; init; }

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; }

    /// <summary>
    /// 表示用の名前（GameName#TagLine）
    /// </summary>
    public string DisplayName => $"{GameName}#{TagLine}";
}