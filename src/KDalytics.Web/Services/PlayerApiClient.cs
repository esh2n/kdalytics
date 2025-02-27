using KDalytics.Web.Models;

namespace KDalytics.Web.Services;

/// <summary>
/// プレイヤー関連のAPI通信を行うクライアント
/// </summary>
public class PlayerApiClient
{
    private readonly ApiClient _apiClient;
    private const string BaseUrl = "api/players";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="apiClient">APIクライアント</param>
    public PlayerApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// プレイヤーIDでプレイヤー情報を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤー情報</returns>
    public async Task<PlayerModel?> GetPlayerByPuuidAsync(string puuid, CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<PlayerModel>($"{BaseUrl}/{puuid}", cancellationToken);
    }

    /// <summary>
    /// 名前とタグでプレイヤー情報を検索
    /// </summary>
    /// <param name="name">プレイヤー名</param>
    /// <param name="tag">タグライン</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤー情報</returns>
    public async Task<PlayerModel?> SearchPlayerAsync(string name, string tag, CancellationToken cancellationToken = default)
    {
        var request = new PlayerSearchRequest
        {
            Name = name,
            Tag = tag
        };

        return await _apiClient.PostAsync<PlayerSearchRequest, PlayerModel>($"{BaseUrl}/search", request, cancellationToken);
    }

    /// <summary>
    /// プレイヤーのトラッキング設定を更新
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="track">トラッキング対象にするかどうか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新されたプレイヤー情報</returns>
    public async Task<PlayerModel?> UpdateTrackingAsync(string puuid, bool track, CancellationToken cancellationToken = default)
    {
        var request = new PlayerTrackingRequest
        {
            Puuid = puuid,
            Track = track
        };

        return await _apiClient.PostAsync<PlayerTrackingRequest, PlayerModel>($"{BaseUrl}/tracking", request, cancellationToken);
    }

    /// <summary>
    /// トラッキング対象のプレイヤー一覧を取得
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>トラッキング対象のプレイヤー一覧</returns>
    public async Task<List<PlayerModel>?> GetTrackedPlayersAsync(CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<List<PlayerModel>>($"{BaseUrl}/tracked", cancellationToken);
    }

    /// <summary>
    /// プレイヤーのランク情報を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーのランク情報</returns>
    public async Task<PlayerRankModel?> GetPlayerRankAsync(string puuid, CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<PlayerRankModel>($"{BaseUrl}/{puuid}/rank", cancellationToken);
    }

    /// <summary>
    /// プレイヤーのランク履歴を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時</param>
    /// <param name="to">終了日時</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーのランク履歴</returns>
    public async Task<List<PlayerRankModel>?> GetPlayerRankHistoryAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/{puuid}/rank/history";

        if (from.HasValue)
        {
            url += $"?from={from.Value:o}";
            if (to.HasValue)
            {
                url += $"&to={to.Value:o}";
            }
        }
        else if (to.HasValue)
        {
            url += $"?to={to.Value:o}";
        }

        return await _apiClient.GetAsync<List<PlayerRankModel>>(url, cancellationToken);
    }
}