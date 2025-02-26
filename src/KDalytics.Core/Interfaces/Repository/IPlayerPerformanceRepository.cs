using KDalytics.Core.Models.Performance;

namespace KDalytics.Core.Interfaces.Repository;

/// <summary>
/// プレイヤーパフォーマンス情報のリポジトリインターフェイス
/// </summary>
public interface IPlayerPerformanceRepository
{
    /// <summary>
    /// パフォーマンス情報をUpsert（挿入または更新）する
    /// </summary>
    /// <param name="performance">パフォーマンス情報</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新または挿入されたパフォーマンス情報</returns>
    Task<PlayerMatchPerformance> UpsertPerformanceAsync(PlayerMatchPerformance performance, CancellationToken cancellationToken = default);

    /// <summary>
    /// 試合IDに関連するすべてのパフォーマンスを取得する
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>パフォーマンス情報のリスト</returns>
    Task<List<PlayerMatchPerformance>> GetPerformancesByMatchIdAsync(string matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの特定試合でのパフォーマンスを取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>パフォーマンス情報、存在しない場合はnull</returns>
    Task<PlayerMatchPerformance?> GetPlayerPerformanceInMatchAsync(string puuid, string matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの特定期間内のパフォーマンス統計を取得
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="from">取得開始日時</param>
    /// <param name="to">取得終了日時</param>
    /// <param name="gameMode">ゲームモード（指定しない場合は全て）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>パフォーマンス統計情報</returns>
    Task<PerformanceStats> GetPlayerPerformanceStatsAsync(
        string puuid,
        DateTime from,
        DateTime to,
        string? gameMode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーのエージェント別パフォーマンスを取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="from">取得開始日時</param>
    /// <param name="to">取得終了日時</param>
    /// <param name="minGames">最低試合数（フィルタリング用）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>エージェント名とそのパフォーマンス情報のディクショナリ</returns>
    Task<Dictionary<string, AgentPerformance>> GetPlayerAgentPerformanceAsync(
        string puuid,
        DateTime from,
        DateTime to,
        int minGames = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーのマップ別パフォーマンスを取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="from">取得開始日時</param>
    /// <param name="to">取得終了日時</param>
    /// <param name="minGames">最低試合数（フィルタリング用）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>マップ名とそのパフォーマンス情報のディクショナリ</returns>
    Task<Dictionary<string, MapPerformance>> GetPlayerMapPerformanceAsync(
        string puuid,
        DateTime from,
        DateTime to,
        int minGames = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数プレイヤーのKDAランキングを取得する
    /// </summary>
    /// <param name="puuids">プレイヤーIDのリスト</param>
    /// <param name="from">取得開始日時</param>
    /// <param name="to">取得終了日時</param>
    /// <param name="gameMode">ゲームモード（指定しない場合は全て）</param>
    /// <param name="minGames">最低試合数（フィルタリング用）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーのパフォーマンス情報のリスト（KDA降順）</returns>
    Task<List<(string Puuid, string PlayerName, string TagLine, float KdaRatio, int Kills, int Deaths, int Assists, int GamesPlayed)>>
        GetPlayersKdaRankingAsync(
            IEnumerable<string> puuids,
            DateTime from,
            DateTime to,
            string? gameMode = null,
            int minGames = 1,
            CancellationToken cancellationToken = default);
}