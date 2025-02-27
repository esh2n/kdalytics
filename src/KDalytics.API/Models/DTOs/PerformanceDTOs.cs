using KDalytics.Core.Models.Performance;

namespace KDalytics.API.Models.DTOs;

/// <summary>
/// パフォーマンス統計情報のレスポンスDTO
/// </summary>
public record PerformanceStatsResponseDto
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 集計期間（開始）
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// 集計期間（終了）
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// 総試合数
    /// </summary>
    public int MatchesPlayed { get; init; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int MatchesWon { get; init; }

    /// <summary>
    /// 敗北数
    /// </summary>
    public int Losses { get; init; }

    /// <summary>
    /// 勝率
    /// </summary>
    public float WinRate { get; init; }

    /// <summary>
    /// 総キル数
    /// </summary>
    public int TotalKills { get; init; }

    /// <summary>
    /// 総デス数
    /// </summary>
    public int TotalDeaths { get; init; }

    /// <summary>
    /// 総アシスト数
    /// </summary>
    public int TotalAssists { get; init; }

    /// <summary>
    /// 平均キル数
    /// </summary>
    public float AverageKills { get; init; }

    /// <summary>
    /// 平均デス数
    /// </summary>
    public float AverageDeaths { get; init; }

    /// <summary>
    /// 平均アシスト数
    /// </summary>
    public float AverageAssists { get; init; }

    /// <summary>
    /// K/Dレシオ
    /// </summary>
    public float KdRatio { get; init; }

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// 最も使用したエージェント
    /// </summary>
    public string MostPlayedAgent { get; init; } = string.Empty;

    /// <summary>
    /// エージェント別統計
    /// </summary>
    public Dictionary<string, AgentPerformanceDto> AgentStats { get; init; } = new();

    /// <summary>
    /// マップ別統計
    /// </summary>
    public Dictionary<string, MapPerformanceDto> MapStats { get; init; } = new();

    /// <summary>
    /// PerformanceStatsからDTOを作成
    /// </summary>
    /// <param name="entity">パフォーマンス統計エンティティ</param>
    /// <returns>パフォーマンス統計レスポンスDTO</returns>
    public static PerformanceStatsResponseDto FromEntity(PerformanceStats entity) => new()
    {
        Puuid = entity.Puuid,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        MatchesPlayed = entity.MatchesPlayed,
        MatchesWon = entity.MatchesWon,
        Losses = entity.Losses,
        WinRate = entity.WinRate,
        TotalKills = entity.TotalKills,
        TotalDeaths = entity.TotalDeaths,
        TotalAssists = entity.TotalAssists,
        AverageKills = entity.AverageKills,
        AverageDeaths = entity.AverageDeaths,
        AverageAssists = entity.AverageAssists,
        KdRatio = entity.KdRatio,
        KdaRatio = entity.KdaRatio,
        HeadshotPercentage = entity.HeadshotPercentage,
        MostPlayedAgent = entity.MostPlayedAgent,
        AgentStats = entity.AgentStats.ToDictionary(
            kvp => kvp.Key,
            kvp => AgentPerformanceDto.FromEntity(kvp.Value)
        ),
        MapStats = entity.MapStats.ToDictionary(
            kvp => kvp.Key,
            kvp => MapPerformanceDto.FromEntity(kvp.Value)
        )
    };
}

/// <summary>
/// エージェント別パフォーマンスDTO
/// </summary>
public record AgentPerformanceDto
{
    /// <summary>
    /// エージェント名
    /// </summary>
    public string AgentName { get; init; } = string.Empty;

    /// <summary>
    /// 使用回数
    /// </summary>
    public int GamesPlayed { get; init; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int GamesWon { get; init; }

    /// <summary>
    /// 敗北数
    /// </summary>
    public int Losses { get; init; }

    /// <summary>
    /// 勝率
    /// </summary>
    public float WinRate { get; init; }

    /// <summary>
    /// 平均キル
    /// </summary>
    public float AverageKills { get; init; }

    /// <summary>
    /// 平均デス
    /// </summary>
    public float AverageDeaths { get; init; }

    /// <summary>
    /// 平均アシスト
    /// </summary>
    public float AverageAssists { get; init; }

    /// <summary>
    /// K/Dレシオ
    /// </summary>
    public float KdRatio { get; init; }

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

    /// <summary>
    /// ヘッドショット率
    /// </summary>
    public float HeadshotPercentage { get; init; }

    /// <summary>
    /// AgentPerformanceからDTOを作成
    /// </summary>
    /// <param name="entity">エージェントパフォーマンスエンティティ</param>
    /// <returns>エージェントパフォーマンスDTO</returns>
    public static AgentPerformanceDto FromEntity(AgentPerformance entity) => new()
    {
        AgentName = entity.AgentName,
        GamesPlayed = entity.GamesPlayed,
        GamesWon = entity.GamesWon,
        Losses = entity.Losses,
        WinRate = entity.WinRate,
        AverageKills = entity.AverageKills,
        AverageDeaths = entity.AverageDeaths,
        AverageAssists = entity.AverageAssists,
        KdRatio = entity.KdRatio,
        KdaRatio = entity.GetAvgKdaRatio(),
        HeadshotPercentage = entity.HeadshotPercentage
    };
}

/// <summary>
/// マップ別パフォーマンスDTO
/// </summary>
public record MapPerformanceDto
{
    /// <summary>
    /// マップ名
    /// </summary>
    public string MapName { get; init; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; init; } = string.Empty;

    /// <summary>
    /// プレイ回数
    /// </summary>
    public int GamesPlayed { get; init; }

    /// <summary>
    /// 勝利数
    /// </summary>
    public int GamesWon { get; init; }

    /// <summary>
    /// 敗北数
    /// </summary>
    public int Losses { get; init; }

    /// <summary>
    /// 勝率
    /// </summary>
    public float WinRate { get; init; }

    /// <summary>
    /// 平均キル
    /// </summary>
    public float AverageKills { get; init; }

    /// <summary>
    /// 平均デス
    /// </summary>
    public float AverageDeaths { get; init; }

    /// <summary>
    /// 平均アシスト
    /// </summary>
    public float AverageAssists { get; init; }

    /// <summary>
    /// K/Dレシオ
    /// </summary>
    public float KdRatio { get; init; }

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

    /// <summary>
    /// MapPerformanceからDTOを作成
    /// </summary>
    /// <param name="entity">マップパフォーマンスエンティティ</param>
    /// <returns>マップパフォーマンスDTO</returns>
    public static MapPerformanceDto FromEntity(MapPerformance entity) => new()
    {
        MapName = entity.MapName,
        MapId = entity.MapId,
        GamesPlayed = entity.GamesPlayed,
        GamesWon = entity.GamesWon,
        Losses = entity.Losses,
        WinRate = entity.WinRate,
        AverageKills = entity.AverageKills,
        AverageDeaths = entity.AverageDeaths,
        AverageAssists = entity.AverageAssists,
        KdRatio = entity.KdRatio,
        KdaRatio = entity.GetAvgKdaRatio()
    };
}

/// <summary>
/// パフォーマンス統計リクエストDTO
/// </summary>
public record PerformanceStatsRequestDto
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// 開始日時
    /// </summary>
    public DateTime From { get; init; } = DateTime.UtcNow.AddDays(-30);

    /// <summary>
    /// 終了日時
    /// </summary>
    public DateTime To { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string? GameMode { get; init; }
}

/// <summary>
/// KDAランキングレスポンスDTO
/// </summary>
public record KdaRankingResponseDto
{
    /// <summary>
    /// プレイヤーID
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public string PlayerName { get; init; } = string.Empty;

    /// <summary>
    /// タグライン
    /// </summary>
    public string TagLine { get; init; } = string.Empty;

    /// <summary>
    /// KDAレシオ
    /// </summary>
    public float KdaRatio { get; init; }

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
    /// プレイした試合数
    /// </summary>
    public int GamesPlayed { get; init; }

    /// <summary>
    /// KDAランキングデータからDTOを作成
    /// </summary>
    /// <param name="data">KDAランキングデータ</param>
    /// <returns>KDAランキングレスポンスDTO</returns>
    public static KdaRankingResponseDto FromRankingData(
        (string Puuid, string PlayerName, string TagLine, float KdaRatio, int Kills, int Deaths, int Assists, int GamesPlayed) data) => new()
        {
            Puuid = data.Puuid,
            PlayerName = data.PlayerName,
            TagLine = data.TagLine,
            KdaRatio = data.KdaRatio,
            Kills = data.Kills,
            Deaths = data.Deaths,
            Assists = data.Assists,
            GamesPlayed = data.GamesPlayed
        };
}

/// <summary>
/// KDAランキングリクエストDTO
/// </summary>
public record KdaRankingRequestDto
{
    /// <summary>
    /// プレイヤーIDのリスト
    /// </summary>
    public List<string> Puuids { get; init; } = new();

    /// <summary>
    /// 開始日時
    /// </summary>
    public DateTime From { get; init; } = DateTime.UtcNow.AddDays(-30);

    /// <summary>
    /// 終了日時
    /// </summary>
    public DateTime To { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// ゲームモード
    /// </summary>
    public string? GameMode { get; init; }

    /// <summary>
    /// 最低試合数
    /// </summary>
    public int MinGames { get; init; } = 1;
}