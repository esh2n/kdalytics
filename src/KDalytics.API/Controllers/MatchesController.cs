using KDalytics.API.Models.DTOs;
using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KDalytics.API.Controllers;

/// <summary>
/// 試合情報を管理するコントローラ
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerPerformanceRepository _performanceRepository;
    private readonly IHenrikApiClient _henrikApiClient;
    private readonly IValorantDataMapper _dataMapper;
    private readonly ILogger<MatchesController> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MatchesController(
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        IPlayerPerformanceRepository performanceRepository,
        IHenrikApiClient henrikApiClient,
        IValorantDataMapper dataMapper,
        ILogger<MatchesController> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _performanceRepository = performanceRepository;
        _henrikApiClient = henrikApiClient;
        _dataMapper = dataMapper;
        _logger = logger;
    }

    /// <summary>
    /// 試合IDで試合情報を取得
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合情報</returns>
    [HttpGet("{matchId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchResponseDto>> GetMatchById(string matchId, CancellationToken cancellationToken)
    {
        try
        {
            var match = await _matchRepository.GetMatchByIdAsync(matchId, cancellationToken);
            if (match == null)
            {
                // ローカルDBに存在しない場合、APIから取得を試みる
                try
                {
                    var apiResponse = await _henrikApiClient.GetMatchDetailsAsync(matchId, cancellationToken);
                    if (apiResponse.Status != 200 || apiResponse.Data == null)
                    {
                        // 保存済みデータからの取得を試みる
                        apiResponse = await _henrikApiClient.GetStoredMatchDetailsAsync(matchId, cancellationToken);
                        if (apiResponse.Status != 200 || apiResponse.Data == null)
                        {
                            return NotFound($"試合ID '{matchId}' が見つかりません。");
                        }
                    }

                    // エンティティに変換して保存
                    match = _dataMapper.MapToMatchEntity(apiResponse);
                    match = await _matchRepository.UpsertMatchAsync(match, cancellationToken);

                    // プレイヤーパフォーマンスも保存
                    var performances = _dataMapper.MapToPlayerPerformances(apiResponse);
                    foreach (var performance in performances)
                    {
                        await _performanceRepository.UpsertPerformanceAsync(performance, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Henrik APIからの試合取得中にエラーが発生しました。MatchID: {MatchId}", matchId);
                    return StatusCode(StatusCodes.Status500InternalServerError, "外部APIからの試合情報取得中にエラーが発生しました。");
                }
            }

            return Ok(MatchResponseDto.FromEntity(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "試合取得中にエラーが発生しました。MatchID: {MatchId}", matchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "試合情報の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーの最近の試合を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="count">取得件数（デフォルト: 5）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合情報のリスト</returns>
    [HttpGet("player/{puuid}/recent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MatchResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MatchResponseDto>>> GetPlayerRecentMatches(
        string puuid,
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _playerRepository.GetPlayerByPuuidAsync(puuid, cancellationToken);
            if (player == null)
            {
                return NotFound($"プレイヤーID '{puuid}' が見つかりません。");
            }

            var matches = await _matchRepository.GetPlayerRecentMatchesAsync(puuid, count, cancellationToken);
            if (matches.Count == 0)
            {
                // ローカルDBに存在しない場合、APIから取得を試みる
                try
                {
                    var apiResponse = await _henrikApiClient.GetPlayerMatchesByPuuidAsync(player.Region, puuid, count, "", cancellationToken);
                    if (apiResponse.Status != 200 || apiResponse.Data.Count == 0)
                    {
                        return NotFound($"プレイヤーID '{puuid}' の最近の試合が見つかりません。");
                    }

                    // 各試合の詳細を取得して保存
                    foreach (var matchData in apiResponse.Data)
                    {
                        var matchDetailsResponse = await _henrikApiClient.GetMatchDetailsAsync(matchData.MatchId, cancellationToken);
                        if (matchDetailsResponse.Status == 200 && matchDetailsResponse.Data != null)
                        {
                            var matchEntity = _dataMapper.MapToMatchEntity(matchDetailsResponse);
                            await _matchRepository.UpsertMatchAsync(matchEntity, cancellationToken);

                            // プレイヤーパフォーマンスも保存
                            var performances = _dataMapper.MapToPlayerPerformances(matchDetailsResponse);
                            foreach (var performance in performances)
                            {
                                await _performanceRepository.UpsertPerformanceAsync(performance, cancellationToken);
                            }

                            matches.Add(matchEntity);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Henrik APIからのプレイヤー試合取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
                    return StatusCode(StatusCodes.Status500InternalServerError, "外部APIからのプレイヤー試合情報取得中にエラーが発生しました。");
                }
            }

            return Ok(matches.Select(MatchResponseDto.FromEntity).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤー試合取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤー試合情報の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーの試合をフィルタして取得
    /// </summary>
    /// <param name="request">フィルタリクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>フィルタされた試合情報のリスト</returns>
    [HttpPost("filter")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MatchResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MatchResponseDto>>> GetPlayerMatchesWithFilter(
        MatchFilterRequestDto request,
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

            var matches = await _matchRepository.GetPlayerMatchesWithFilterAsync(
                request.Puuid,
                request.From,
                request.To,
                request.GameMode,
                request.Skip,
                request.Take,
                cancellationToken);

            if (matches.Count == 0)
            {
                return NotFound($"条件に一致する試合が見つかりません。");
            }

            return Ok(matches.Select(MatchResponseDto.FromEntity).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤー試合フィルタ中にエラーが発生しました。PUUID: {Puuid}", request.Puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤー試合情報のフィルタ中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// 試合内のプレイヤーパフォーマンスを取得
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーパフォーマンス情報のリスト</returns>
    [HttpGet("{matchId}/performances")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlayerMatchPerformanceDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PlayerMatchPerformanceDto>>> GetMatchPerformances(
        string matchId,
        CancellationToken cancellationToken)
    {
        try
        {
            var performances = await _performanceRepository.GetPerformancesByMatchIdAsync(matchId, cancellationToken);
            if (performances.Count == 0)
            {
                return NotFound($"試合ID '{matchId}' のパフォーマンス情報が見つかりません。");
            }

            return Ok(performances.Select(PlayerMatchPerformanceDto.FromEntity).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "試合パフォーマンス取得中にエラーが発生しました。MatchID: {MatchId}", matchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "試合パフォーマンス情報の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーの特定試合でのパフォーマンスを取得
    /// </summary>
    /// <param name="matchId">試合ID</param>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーパフォーマンス情報</returns>
    [HttpGet("{matchId}/player/{puuid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerMatchPerformanceDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerMatchPerformanceDto>> GetPlayerPerformanceInMatch(
        string matchId,
        string puuid,
        CancellationToken cancellationToken)
    {
        try
        {
            var performance = await _performanceRepository.GetPlayerPerformanceInMatchAsync(puuid, matchId, cancellationToken);
            if (performance == null)
            {
                return NotFound($"プレイヤーID '{puuid}' の試合ID '{matchId}' でのパフォーマンス情報が見つかりません。");
            }

            return Ok(PlayerMatchPerformanceDto.FromEntity(performance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤー試合パフォーマンス取得中にエラーが発生しました。PUUID: {Puuid}, MatchID: {MatchId}", puuid, matchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤー試合パフォーマンス情報の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// 日別の試合件数を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="days">過去何日分を取得するか（デフォルト: 30）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>日付と試合件数のディクショナリ</returns>
    [HttpGet("player/{puuid}/count-by-day")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<DateTime, int>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dictionary<DateTime, int>>> GetPlayerMatchCountByDay(
        string puuid,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _playerRepository.GetPlayerByPuuidAsync(puuid, cancellationToken);
            if (player == null)
            {
                return NotFound($"プレイヤーID '{puuid}' が見つかりません。");
            }

            var matchCounts = await _matchRepository.GetPlayerMatchCountByDayAsync(puuid, days, cancellationToken);
            return Ok(matchCounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤー日別試合件数取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤー日別試合件数の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// マップごとの試合件数を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時（デフォルト: 30日前）</param>
    /// <param name="to">終了日時（デフォルト: 現在）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>マップ名と試合件数のディクショナリ</returns>
    [HttpGet("player/{puuid}/count-by-map")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, int>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dictionary<string, int>>> GetPlayerMatchCountByMap(
        string puuid,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
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

            var mapCounts = await _matchRepository.GetPlayerMatchCountByMapAsync(puuid, fromDate, toDate, cancellationToken);
            return Ok(mapCounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤーマップ別試合件数取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤーマップ別試合件数の取得中にエラーが発生しました。");
        }
    }
}