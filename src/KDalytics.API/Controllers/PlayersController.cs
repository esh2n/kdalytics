using KDalytics.API.Models.DTOs;
using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KDalytics.API.Controllers;

/// <summary>
/// プレイヤー情報を管理するコントローラ
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerRankRepository _playerRankRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerPerformanceRepository _performanceRepository;
    private readonly IHenrikApiClient _henrikApiClient;
    private readonly IValorantDataMapper _dataMapper;
    private readonly ILogger<PlayersController> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PlayersController(
        IPlayerRepository playerRepository,
        IPlayerRankRepository playerRankRepository,
        IMatchRepository matchRepository,
        IPlayerPerformanceRepository performanceRepository,
        IHenrikApiClient henrikApiClient,
        IValorantDataMapper dataMapper,
        ILogger<PlayersController> logger)
    {
        _playerRepository = playerRepository;
        _playerRankRepository = playerRankRepository;
        _matchRepository = matchRepository;
        _performanceRepository = performanceRepository;
        _henrikApiClient = henrikApiClient;
        _dataMapper = dataMapper;
        _logger = logger;
    }

    /// <summary>
    /// プレイヤーIDでプレイヤー情報を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤー情報</returns>
    [HttpGet("{puuid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponseDto>> GetPlayerByPuuid(string puuid, CancellationToken cancellationToken)
    {
        try
        {
            var player = await _playerRepository.GetPlayerByPuuidAsync(puuid, cancellationToken);
            if (player == null)
            {
                return NotFound($"プレイヤーID '{puuid}' が見つかりません。");
            }

            return Ok(PlayerResponseDto.FromEntity(player));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤー取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤー情報の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// 名前とタグでプレイヤー情報を検索
    /// </summary>
    /// <param name="request">検索リクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤー情報</returns>
    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlayerResponseDto>> SearchPlayer(PlayerSearchRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Tag))
        {
            return BadRequest("プレイヤー名とタグを指定してください。");
        }

        try
        {
            // まずローカルDBを検索
            var player = await _playerRepository.GetPlayerByNameTagAsync(request.Name, request.Tag, cancellationToken);

            // 見つからない場合はAPIから取得
            if (player == null)
            {
                try
                {
                    var apiResponse = await _henrikApiClient.GetPlayerInfoAsync(request.Name, request.Tag, cancellationToken);
                    if (apiResponse.Status != 200 || apiResponse.Data == null)
                    {
                        return NotFound($"プレイヤー '{request.Name}#{request.Tag}' が見つかりません。");
                    }

                    // エンティティに変換して保存
                    player = _dataMapper.MapToPlayerEntity(apiResponse);
                    player = await _playerRepository.UpsertPlayerAsync(player, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Henrik APIからのプレイヤー取得中にエラーが発生しました。Name: {Name}, Tag: {Tag}", request.Name, request.Tag);
                    return StatusCode(StatusCodes.Status500InternalServerError, "外部APIからのプレイヤー情報取得中にエラーが発生しました。");
                }
            }

            return Ok(PlayerResponseDto.FromEntity(player));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤー検索中にエラーが発生しました。Name: {Name}, Tag: {Tag}", request.Name, request.Tag);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤー情報の検索中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーのトラッキング設定を更新
    /// </summary>
    /// <param name="request">トラッキング設定リクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>更新されたプレイヤー情報</returns>
    [HttpPost("tracking")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlayerResponseDto>> UpdateTracking(PlayerTrackingRequestDto request, CancellationToken cancellationToken)
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

            // トラッキング設定を更新
            var updatedPlayer = player.WithTracking(request.Track);
            updatedPlayer = await _playerRepository.UpsertPlayerAsync(updatedPlayer, cancellationToken);

            // トラッキング設定も更新
            var config = await _playerRepository.GetOrCreateTrackingConfigAsync(request.Puuid, cancellationToken);
            config = config with { IsActive = request.Track };
            await _playerRepository.UpdateTrackingConfigAsync(config, cancellationToken);

            // トラッキングを有効にする場合、過去の試合データを取得
            if (request.Track)
            {
                try
                {
                    _logger.LogInformation("プレイヤー {Puuid} の過去の試合データを取得します", request.Puuid);

                    // 1シーズン分（約90日）の試合データを取得
                    int matchCount = 100; // 最大取得数

                    var apiResponse = await _henrikApiClient.GetPlayerMatchesByPuuidAsync(
                        player.Region,
                        request.Puuid,
                        matchCount,
                        "", // すべてのゲームモード
                        cancellationToken);

                    if (apiResponse.Status == 200 && apiResponse.Data.Count > 0)
                    {
                        _logger.LogInformation("プレイヤー {Puuid} の過去の試合データを {Count} 件取得しました",
                            request.Puuid, apiResponse.Data.Count);

                        // 各試合の詳細を取得して保存（非同期で並行処理）
                        var tasks = new List<Task>();
                        foreach (var matchData in apiResponse.Data)
                        {
                            // matchIdがnullまたは空でないことを確認
                            if (string.IsNullOrEmpty(matchData.MatchId))
                            {
                                continue; // このマッチデータはスキップして次へ
                            }

                            // 試合詳細を取得して保存する処理を非同期タスクとして追加
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    var matchDetailsResponse = await _henrikApiClient.GetMatchDetailsAsync(
                                        matchData.MatchId, cancellationToken);

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
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "試合 {MatchId} の詳細取得中にエラーが発生しました", matchData.MatchId);
                                }
                            }));
                        }

                        // すべてのタスクが完了するのを待つ（最大30秒）
                        await Task.WhenAny(
                            Task.WhenAll(tasks),
                            Task.Delay(TimeSpan.FromSeconds(30))
                        );

                        _logger.LogInformation("プレイヤー {Puuid} の過去の試合データの取得と保存を完了しました", request.Puuid);
                    }
                }
                catch (Exception ex)
                {
                    // 過去の試合データ取得中のエラーはログに記録するが、トラッキング設定自体は成功とする
                    _logger.LogError(ex, "プレイヤー {Puuid} の過去の試合データ取得中にエラーが発生しました", request.Puuid);
                }
            }

            return Ok(PlayerResponseDto.FromEntity(updatedPlayer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤートラッキング設定更新中にエラーが発生しました。PUUID: {Puuid}", request.Puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤートラッキング設定の更新中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// トラッキング対象のプレイヤー一覧を取得
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>トラッキング対象のプレイヤー一覧</returns>
    [HttpGet("tracked")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlayerResponseDto>))]
    public async Task<ActionResult<List<PlayerResponseDto>>> GetTrackedPlayers(CancellationToken cancellationToken)
    {
        try
        {
            var players = await _playerRepository.GetTrackedPlayersAsync(cancellationToken);
            return Ok(players.Select(PlayerResponseDto.FromEntity).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "トラッキング対象プレイヤー一覧取得中にエラーが発生しました。");
            return StatusCode(StatusCodes.Status500InternalServerError, "トラッキング対象プレイヤー一覧の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーのランク情報を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーのランク情報</returns>
    [HttpGet("{puuid}/rank")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerRankResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerRankResponseDto>> GetPlayerRank(string puuid, CancellationToken cancellationToken)
    {
        try
        {
            var player = await _playerRepository.GetPlayerByPuuidAsync(puuid, cancellationToken);
            if (player == null)
            {
                return NotFound($"プレイヤーID '{puuid}' が見つかりません。");
            }

            var rank = await _playerRankRepository.GetLatestPlayerRankAsync(puuid, cancellationToken);
            if (rank == null)
            {
                return NotFound($"プレイヤーID '{puuid}' のランク情報が見つかりません。");
            }

            return Ok(PlayerRankResponseDto.FromEntity(rank, player.GameName, player.TagLine));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤーランク情報取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤーランク情報の取得中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// プレイヤーのランク履歴を取得
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">開始日時（デフォルト: 30日前）</param>
    /// <param name="to">終了日時（デフォルト: 現在）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>プレイヤーのランク履歴</returns>
    [HttpGet("{puuid}/rank/history")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlayerRankResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PlayerRankResponseDto>>> GetPlayerRankHistory(
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

            var rankHistory = await _playerRankRepository.GetPlayerRankHistoryAsync(puuid, fromDate, toDate, cancellationToken);
            if (rankHistory.Count == 0)
            {
                return NotFound($"指定期間内のプレイヤーID '{puuid}' のランク履歴が見つかりません。");
            }

            return Ok(rankHistory.Select(r => PlayerRankResponseDto.FromEntity(r, player.GameName, player.TagLine)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プレイヤーランク履歴取得中にエラーが発生しました。PUUID: {Puuid}", puuid);
            return StatusCode(StatusCodes.Status500InternalServerError, "プレイヤーランク履歴の取得中にエラーが発生しました。");
        }
    }
}