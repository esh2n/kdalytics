using KDalytics.Core.Models.Match;

namespace KDalytics.Core.Interfaces.Repository;

/// <summary>
/// 試合情報のリポジトリインターフェイス
/// </summary>
public interface IMatchRepository
{
    /// <summary>
    /// 試合情報をUpsert（挿入または更新）する
    /// </summary>
    /// <param name="match">試合情報エンティティ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新または挿入された試合情報エンティティ</returns>
    Task<MatchEntity> UpsertMatchAsync(MatchEntity match, CancellationToken cancellationToken = default);

    /// <summary>
    /// 試合IDで試合情報を取得する
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合情報エンティティ、存在しない場合はnull</returns>
    Task<MatchEntity?> GetMatchByIdAsync(string matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの最近の試合を取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="count">取得件数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合情報エンティティのリスト（最新順）</returns>
    Task<List<MatchEntity>> GetPlayerRecentMatchesAsync(string puuid, int count = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの試合をフィルタして取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="from">取得開始日時</param>
    /// <param name="to">取得終了日時</param>
    /// <param name="gameMode">ゲームモード（指定しない場合は全て）</param>
    /// <param name="skip">スキップする件数</param>
    /// <param name="take">取得する件数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>フィルタされた試合情報エンティティのリスト</returns>
    Task<List<MatchEntity>> GetPlayerMatchesWithFilterAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        string? gameMode = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの最新試合IDを取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>最新の試合ID、存在しない場合はnull</returns>
    Task<string?> GetPlayerLatestMatchIdAsync(string puuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 日別の試合件数を取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="days">過去何日分を取得するか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>日付と試合件数のディクショナリ</returns>
    Task<Dictionary<DateTime, int>> GetPlayerMatchCountByDayAsync(string puuid, int days = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// マップごとの試合件数を取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="from">取得開始日時</param>
    /// <param name="to">取得終了日時</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>マップ名と試合件数のディクショナリ</returns>
    Task<Dictionary<string, int>> GetPlayerMatchCountByMapAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
}