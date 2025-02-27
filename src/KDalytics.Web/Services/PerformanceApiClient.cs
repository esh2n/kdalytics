using KDalytics.Web.Models;

namespace KDalytics.Web.Services;

/// <summary>
/// パフォーマンス関連のAPI通信を行うクライアント
/// </summary>
public class PerformanceApiClient
{
    private readonly ApiClient _apiClient;
    private const string BaseUrl = "api/performances";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="apiClient">APIクライアント</param>
    public PerformanceApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// プレイヤーのパフォーマンス統計を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時</param>
    /// <param name="to">終了日時</param>
    /// <param name="gameMode">ゲームモード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>パフォーマンス統計情報</returns>
    public async Task<PerformanceStatsModel?> GetPlayerPerformanceStatsAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        string? gameMode = null,
        CancellationToken cancellationToken = default)
    {
        var request = new PerformanceStatsRequest
        {
            Puuid = puuid,
            From = from ?? DateTime.UtcNow.AddDays(-30),
            To = to ?? DateTime.UtcNow,
            GameMode = gameMode
        };

        return await _apiClient.PostAsync<PerformanceStatsRequest, PerformanceStatsModel>($"{BaseUrl}/stats", request, cancellationToken);
    }

    /// <summary>
    /// プレイヤーのエージェント別パフォーマンスを取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時</param>
    /// <param name="to">終了日時</param>
    /// <param name="minGames">最低試合数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>エージェント別パフォーマンス情報</returns>
    public async Task<Dictionary<string, AgentPerformanceModel>?> GetPlayerAgentPerformanceAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        int minGames = 0,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/player/{puuid}/agents";
        var queryParams = new List<string>();

        if (from.HasValue)
        {
            queryParams.Add($"from={from.Value:o}");
        }

        if (to.HasValue)
        {
            queryParams.Add($"to={to.Value:o}");
        }

        if (minGames > 0)
        {
            queryParams.Add($"minGames={minGames}");
        }

        if (queryParams.Count > 0)
        {
            url += $"?{string.Join("&", queryParams)}";
        }

        return await _apiClient.GetAsync<Dictionary<string, AgentPerformanceModel>>(url, cancellationToken);
    }

    /// <summary>
    /// プレイヤーのマップ別パフォーマンスを取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時</param>
    /// <param name="to">終了日時</param>
    /// <param name="minGames">最低試合数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>マップ別パフォーマンス情報</returns>
    public async Task<Dictionary<string, MapPerformanceModel>?> GetPlayerMapPerformanceAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        int minGames = 0,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/player/{puuid}/maps";
        var queryParams = new List<string>();

        if (from.HasValue)
        {
            queryParams.Add($"from={from.Value:o}");
        }

        if (to.HasValue)
        {
            queryParams.Add($"to={to.Value:o}");
        }

        if (minGames > 0)
        {
            queryParams.Add($"minGames={minGames}");
        }

        if (queryParams.Count > 0)
        {
            url += $"?{string.Join("&", queryParams)}";
        }

        return await _apiClient.GetAsync<Dictionary<string, MapPerformanceModel>>(url, cancellationToken);
    }

    /// <summary>
    /// 複数プレイヤーのKDAランキングを取得
    /// </summary>
    /// <param name="puuids">プレイヤーIDのリスト</param>
    /// <param name="from">開始日時</param>
    /// <param name="to">終了日時</param>
    /// <param name="gameMode">ゲームモード</param>
    /// <param name="minGames">最低試合数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>KDAランキング情報</returns>
    public async Task<List<KdaRankingModel>?> GetPlayersKdaRankingAsync(
        List<string> puuids,
        DateTime? from = null,
        DateTime? to = null,
        string? gameMode = null,
        int minGames = 1,
        CancellationToken cancellationToken = default)
    {
        var request = new KdaRankingRequest
        {
            Puuids = puuids,
            From = from ?? DateTime.UtcNow.AddDays(-30),
            To = to ?? DateTime.UtcNow,
            GameMode = gameMode,
            MinGames = minGames
        };

        return await _apiClient.PostAsync<KdaRankingRequest, List<KdaRankingModel>>($"{BaseUrl}/kda-ranking", request, cancellationToken);
    }
}