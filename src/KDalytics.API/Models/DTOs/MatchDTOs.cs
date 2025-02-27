using KDalytics.Core.Models.Match;
using KDalytics.Core.Models.Performance;

namespace KDalytics.API.Models.DTOs;

/// <summary>
/// 試合情報のレスポンスDTO
/// </summary>
public record MatchResponseDto
{
    /// <summary>
    /// 試合ID
    /// </summary>
    public string MatchId { get; init; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; init; } = string.Empty;

    /// <summary>
    /// マップ名
    /// </summary>
    public string MapName { get; init; } = string.Empty;

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string GameMode { get; init; } = string.Empty;

    /// <summary>
    /// 試合開始時間
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// 試合時間（ミリ秒）
    /// </summary>
    public long GameLength { get; init; }

    /// <summary>
    /// リージョン
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// シーズンID
    /// </summary>
    public string SeasonId { get; init; } = string.Empty;

    /// <summary>
    /// チーム情報
    /// </summary>
    public List<TeamDataDto> Teams { get; init; } = new();

    /// <summary>
    /// プレイヤーパフォーマンス情報
    /// </summary>
    public List<PlayerMatchPerformanceDto> Players { get; init; } = new();

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; }

    /// <summary>
    /// MatchEntityからDTOを作成
    /// </summary>
    /// <param name="entity">試合エンティティ</param>
    /// <returns>試合レスポンスDTO</returns>
    public static MatchResponseDto FromEntity(MatchEntity entity) => new()
    {
        MatchId = entity.MatchId,
        MapId = entity.MapId,
        MapName = entity.MapName,
        GameMode = entity.GameMode,
        StartTime = entity.StartTime,
        GameLength = entity.GameLength,
        Region = entity.Region,
        SeasonId = entity.SeasonId,
        Teams = entity.Teams.Select(TeamDataDto.FromEntity).ToList(),
        Players = entity.Players.Select(PlayerMatchPerformanceDto.FromEntity).ToList(),
        LastUpdated = entity.LastUpdated
    };
}

/// <summary>
/// チーム情報DTO
/// </summary>
public record TeamDataDto
{
    /// <summary>
    /// チームID
    /// </summary>
    public string TeamId { get; init; } = string.Empty;

    /// <summary>
    /// 勝利したかどうか
    /// </summary>
    public bool HasWon { get; init; }

    /// <summary>
    /// 取得したラウンド数
    /// </summary>
    public int RoundsWon { get; init; }

    /// <summary>
    /// TeamDataからDTOを作成
    /// </summary>
    /// <param name="entity">チームデータエンティティ</param>
    /// <returns>チーム情報DTO</returns>
    public static TeamDataDto FromEntity(TeamData entity) => new()
    {
        TeamId = entity.TeamId,
        HasWon = entity.HasWon,
        RoundsWon = entity.RoundsWon
    };
}

/// <summary>
/// プレイヤーの試合パフォーマンスDTO
/// </summary>
public record PlayerMatchPerformanceDto
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 試合ID
    /// </summary>
    public string MatchId { get; init; } = string.Empty;

    /// <summary>
    /// チームID
    /// </summary>
    public string TeamId { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string PlayerName { get; init; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string TagLine { get; init; } = string.Empty;

    /// <summary>
    /// エージェント名
    /// </summary>
    public string AgentName { get; init; } = string.Empty;

    /// <summary>
    /// スコア
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// キル数
    /// </summary>
    public int Kills { get; init; }

    /// <summary>
    /// デス数
    /// </summary>
    public int Deaths { get; init; }

    /// <summary>
    /// アシスト数
    /// </summary>
    public int Assists { get; init; }

    /// <summary>
    /// 与ダメージ
    /// </summary>
    public int DamageDealt { get; init; }

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// PlayerMatchPerformanceからDTOを作成
    /// </summary>
    /// <param name="entity">プレイヤーパフォーマンスエンティティ</param>
    /// <returns>プレイヤーパフォーマンスDTO</returns>
    public static PlayerMatchPerformanceDto FromEntity(PlayerMatchPerformance entity) => new()
    {
        Puuid = entity.Puuid,
        MatchId = entity.MatchId,
        TeamId = entity.TeamId,
        PlayerName = entity.PlayerName,
        TagLine = entity.TagLine,
        AgentName = entity.AgentName,
        Score = entity.Score,
        Kills = entity.Kills,
        Deaths = entity.Deaths,
        Assists = entity.Assists,
        DamageDealt = entity.DamageDealt,
        HeadshotPercentage = entity.GetHeadshotPercentage()
    };
}

/// <summary>
/// 試合検索リクエストDTO
/// </summary>
public record MatchFilterRequestDto
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 開始日時
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// 終了日時
    /// </summary>
    public DateTime? To { get; init; }

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string? GameMode { get; init; }

    /// <summary>
    /// スキップする件数
    /// </summary>
    public int? Skip { get; init; }

    /// <summary>
    /// 取得する件数
    /// </summary>
    public int? Take { get; init; }
}