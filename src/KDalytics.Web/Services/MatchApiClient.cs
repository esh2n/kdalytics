using KDalytics.Web.Models;

namespace KDalytics.Web.Services;

/// <summary>
/// 試合関連のAPI通信を行うクライアント
/// </summary>
public class MatchApiClient
{
    private readonly ApiClient _apiClient;
    private const string BaseUrl = "api/matches";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="apiClient">APIクライアント</param>
    public MatchApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// 試合IDで試合情報を取得
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合情報</returns>
    public async Task<MatchModel?> GetMatchByIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<MatchModel>($"{BaseUrl}/{matchId}", cancellationToken);
    }

    /// <summary>
    /// プレイヤーの最近の試合を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="count">取得件数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合情報のリスト</returns>
    public async Task<List<MatchModel>?> GetPlayerRecentMatchesAsync(
        string puuid,
        int count = 5,
        CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<List<MatchModel>>($"{BaseUrl}/player/{puuid}/recent?count={count}", cancellationToken);
    }

    /// <summary>
    /// プレイヤーの試合をフィルタして取得
    /// </summary>
    /// <param name="request">フィルタリクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>フィルタされた試合情報のリスト</returns>
    public async Task<List<MatchModel>?> GetPlayerMatchesWithFilterAsync(
        MatchFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _apiClient.PostAsync<MatchFilterRequest, List<MatchModel>>($"{BaseUrl}/filter", request, cancellationToken);
    }

    /// <summary>
    /// 試合内のプレイヤーパフォーマンスを取得
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーパフォーマンス情報のリスト</returns>
    public async Task<List<PlayerMatchPerformanceModel>?> GetMatchPerformancesAsync(
        string matchId,
        CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<List<PlayerMatchPerformanceModel>>($"{BaseUrl}/{matchId}/performances", cancellationToken);
    }

    /// <summary>
    /// プレイヤーの特定試合でのパフォーマンスを取得
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーパフォーマンス情報</returns>
    public async Task<PlayerMatchPerformanceModel?> GetPlayerPerformanceInMatchAsync(
        string matchId,
        string puuid,
        CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<PlayerMatchPerformanceModel>($"{BaseUrl}/{matchId}/player/{puuid}", cancellationToken);
    }

    /// <summary>
    /// 日別の試合件数を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="days">過去何日分を取得するか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>日付と試合件数のディクショナリ</returns>
    public async Task<Dictionary<DateTime, int>?> GetPlayerMatchCountByDayAsync(
        string puuid,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<Dictionary<DateTime, int>>($"{BaseUrl}/player/{puuid}/count-by-day?days={days}", cancellationToken);
    }

    /// <summary>
    /// マップごとの試合件数を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時</param>
    /// <param name="to">終了日時</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>マップ名と試合件数のディクショナリ</returns>
    public async Task<Dictionary<string, int>?> GetPlayerMatchCountByMapAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/player/{puuid}/count-by-map";

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

        return await _apiClient.GetAsync<Dictionary<string, int>>(url, cancellationToken);
    }
}