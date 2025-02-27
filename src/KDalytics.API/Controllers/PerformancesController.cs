using KDalytics.API.Models.DTOs;
using KDalytics.Core.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;

namespace KDalytics.API.Controllers;

/// <summary>
/// パフォーマンス情報を管理するコントローラ
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PerformancesController : ControllerBase
{
    private readonly IPlayerPerformanceRepository _performanceRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<PerformancesController> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PerformancesController(
        IPlayerPerformanceRepository performanceRepository,
        IPlayerRepository playerRepository,
        ILogger<PerformancesController> logger)
    {
        _performanceRepository = performanceRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <summary>
    /// プレイヤーのパフォーマンス統計を取得
    /// </summary>
    /// <param name="request">パフォーマンス統計リクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>パフォーマンス統計情報</returns>
    [HttpPost("stats")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PerformanceStatsResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PerformanceStatsResponseDto>> GetPlayerPerformanceStats(
        PerformanceStatsRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Puuid))
        {
            return BadRequest("プレイヤーIDを指定してください。");
        }

        try
        {
            var player = await _playerRepository.GetPlayerByPuuidAsync(request.Puuid, cancellationToken);
            if (player == null)
            {
                return NotFound($"プレイヤーID '{request.Puuid}' が見つかりません。");
            }

            var stats = await _performanceRepository.GetPlayerPerformanceStatsAsync(
                request.Puuid,
                request.From,
                request.To,
                request.GameMode,
                cancellationToken);

            return Ok(PerformanceStatsResponseDto.FromEntity(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤーパフォーマンス統計取得中にエラーが発生しました。PUUID: {Puuid}", request.Puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤーパフォーマンス統計の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーのエージェント別パフォーマンスを取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時（デフォルト: 30日前）</param>
    /// <param name="to">終了日時（デフォルト: 現在）</param>
    /// <param name="minGames">最低試合数（デフォルト: 0）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>エージェント別パフォーマンス情報</returns>
    [HttpGet("player/{puuid}/agents")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, AgentPerformanceDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dictionary<string, AgentPerformanceDto>>> GetPlayerAgentPerformance(
        string puuid,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int minGames = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _playerRepository.GetPlayerByPuuidAsync(puuid, cancellationToken);
            if (player == null)
            {
                return NotFound($"プレイヤーID '{puuid}' が見つかりません。");
            }

            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var agentPerformances = await _performanceRepository.GetPlayerAgentPerformanceAsync(
                puuid,
                fromDate,
                toDate,
                minGames,
                cancellationToken);

            if (agentPerformances.Count == 0)
            {
                return NotFound($"プレイヤーID '{puuid}' のエージェント別パフォーマンス情報が見つかりません。");
            }

            return Ok(agentPerformances.ToDictionary(
                kvp => kvp.Key,
                kvp => AgentPerformanceDto.FromEntity(kvp.Value)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤーエージェント別パフォーマンス取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤーエージェント別パフォーマンスの取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーのマップ別パフォーマンスを取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時（デフォルト: 30日前）</param>
    /// <param name="to">終了日時（デフォルト: 現在）</param>
    /// <param name="minGames">最低試合数（デフォルト: 0）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>マップ別パフォーマンス情報</returns>
    [HttpGet("player/{puuid}/maps")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, MapPerformanceDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dictionary<string, MapPerformanceDto>>> GetPlayerMapPerformance(
        string puuid,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int minGames = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _playerRepository.GetPlayerByPuuidAsync(puuid, cancellationToken);
            if (player == null)
            {
                return NotFound($"プレイヤーID '{puuid}' が見つかりません。");
            }

            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var mapPerformances = await _performanceRepository.GetPlayerMapPerformanceAsync(
                puuid,
                fromDate,
                toDate,
                minGames,
                cancellationToken);

            if (mapPerformances.Count == 0)
            {
                return NotFound($"プレイヤーID '{puuid}' のマップ別パフォーマンス情報が見つかりません。");
            }

            return Ok(mapPerformances.ToDictionary(
                kvp => kvp.Key,
                kvp => MapPerformanceDto.FromEntity(kvp.Value)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤーマップ別パフォーマンス取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤーマップ別パフォーマンスの取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// 複数プレイヤーのKDAランキングを取得
    /// </summary>
    /// <param name="request">KDAランキングリクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>KDAランキング情報</returns>
    [HttpPost("kda-ranking")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<KdaRankingResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<KdaRankingResponseDto>>> GetPlayersKdaRanking(
        KdaRankingRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Puuids == null || request.Puuids.Count == 0)
        {
            return BadRequest("プレイヤーIDのリストを指定してください。");
        }

        try
        {
            var ranking = await _performanceRepository.GetPlayersKdaRankingAsync(
                request.Puuids,
                request.From,
                request.To,
                request.GameMode,
                request.MinGames,
                cancellationToken);

            return Ok(ranking.Select(KdaRankingResponseDto.FromRankingData).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤーKDAランキング取得中にエラーが発生しました。");
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤーKDAランキングの取得中にエラーが発生しました。");
        }
    }
}