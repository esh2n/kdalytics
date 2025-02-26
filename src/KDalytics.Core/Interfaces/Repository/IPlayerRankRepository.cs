using KDalytics.Core.Models.Player;

namespace KDalytics.Core.Interfaces.Repository;

/// <summary>
/// プレイヤーのランク情報のリポジトリインターフェイス
/// </summary>
public interface IPlayerRankRepository
{
    /// <summary>
    /// ランク情報をUpsert（挿入または更新）する
    /// </summary>
    /// <param name="rank">ランク情報エンティティ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新または挿入されたランク情報エンティティ</returns>
    Task<PlayerRankEntity> UpsertPlayerRankAsync(PlayerRankEntity rank, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの最新ランク情報を取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>最新のランク情報エンティティ、存在しない場合はnull</returns>
    Task<PlayerRankEntity?> GetLatestPlayerRankAsync(string puuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーのランク履歴を取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="from">取得開始日時</param>
    /// <param name="to">取得終了日時</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ランク情報エンティティのリスト（時系列順）</returns>
    Task<List<PlayerRankEntity>> GetPlayerRankHistoryAsync(string puuid, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤーの特定シーズンのランク情報を取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="seasonId">シーズンID（エピソード/アクト）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>指定シーズンのランク情報エンティティ、存在しない場合はnull</returns>
    Task<PlayerRankEntity?> GetPlayerRankBySeasonAsync(string puuid, string seasonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ランク変更を検出する（以前のランクと比較して変更があるかチェック）
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="newRank">新しいランク情報</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ランク変更の有無とその詳細情報</returns>
    Task<(bool HasChanged, PlayerRankEntity? OldRank, PlayerRankEntity NewRank)> DetectRankChangeAsync(
        string puuid,
        PlayerRankEntity newRank,
        CancellationToken cancellationToken = default);
}