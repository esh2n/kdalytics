using KDalytics.Core.Models.Player;

namespace KDalytics.API.Models.DTOs;

/// <summary>
/// プレイヤー情報のレスポンスDTO
/// </summary>
public record PlayerResponseDto
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
    /// PlayerEntityからDTOを作成
    /// </summary>
    /// <param name="entity">プレイヤーエンティティ</param>
    /// <returns>プレイヤーレスポンスDTO</returns>
    public static PlayerResponseDto FromEntity(PlayerEntity entity) => new()
    {
        Puuid = entity.Puuid,
        GameName = entity.GameName,
        TagLine = entity.TagLine,
        Region = entity.Region,
        AccountLevel = entity.AccountLevel,
        IsTracked = entity.IsTracked,
        LastUpdated = entity.LastUpdated
    };
}

/// <summary>
/// プレイヤー検索リクエストDTO
/// </summary>
public record PlayerSearchRequestDto
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
/// プレイヤートラッキング設定リクエストDTO
/// </summary>
public record PlayerTrackingRequestDto
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
/// プレイヤーランク情報レスポンスDTO
/// </summary>
public record PlayerRankResponseDto
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
    /// PlayerRankEntityからDTOを作成
    /// </summary>
    /// <param name="entity">プレイヤーランクエンティティ</param>
    /// <param name="playerName">プレイヤー名</param>
    /// <param name="tagLine">タグライン</param>
    /// <returns>プレイヤーランクレスポンスDTO</returns>
    public static PlayerRankResponseDto FromEntity(PlayerRankEntity entity, string playerName, string tagLine) => new()
    {
        Puuid = entity.Puuid,
        GameName = playerName,
        TagLine = tagLine,
        CurrentTier = entity.CurrentTier,
        CurrentTierName = entity.CurrentTierPatched,
        RankingInTier = entity.RankingInTier,
        Mmr = entity.Mmr,
        MmrChange = entity.MmrChangeToLastGame,
        LastUpdated = entity.LastUpdated
    };
}