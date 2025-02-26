using KDalytics.Core.Models.Player;

namespace KDalytics.Core.Interfaces.Repository;

/// <summary>
/// プレイヤー情報のリポジトリインターフェイス
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// プレイヤーをUpsert（挿入または更新）する
    /// </summary>
    /// <param name="player">プレイヤーエンティティ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新または挿入されたプレイヤーエンティティ</returns>
    Task<PlayerEntity> UpsertPlayerAsync(PlayerEntity player, CancellationToken cancellationToken = default);

    /// <summary>
    /// PUUIDでプレイヤーを取得する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーエンティティ、存在しない場合はnull</returns>
    Task<PlayerEntity?> GetPlayerByPuuidAsync(string puuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 名前とタグでプレイヤーを取得する
    /// </summary>
    /// <param name="name">プレイヤー名</param>
    /// <param name="tag">タグライン</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーエンティティ、存在しない場合はnull</returns>
    Task<PlayerEntity?> GetPlayerByNameTagAsync(string name, string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// トラッキング対象のプレイヤー一覧を取得する
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>トラッキング対象のプレイヤーエンティティのリスト</returns>
    Task<List<PlayerEntity>> GetTrackedPlayersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤートラッキング設定を取得または作成する
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤートラッキング設定</returns>
    Task<PlayerTrackingConfig> GetOrCreateTrackingConfigAsync(string puuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// プレイヤートラッキング設定を更新する
    /// </summary>
    /// <param name="config">更新するトラッキング設定</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新されたトラッキング設定</returns>
    Task<PlayerTrackingConfig> UpdateTrackingConfigAsync(PlayerTrackingConfig config, CancellationToken cancellationToken = default);
}