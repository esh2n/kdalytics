using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Models.Performance;
using KDalytics.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace KDalytics.Infrastructure.Repositories;

/// <summary>
/// Elasticsearchを使用したプレイヤーパフォーマンスリポジトリの実装
/// </summary>
public class ElasticsearchPlayerPerformanceRepository : ElasticsearchRepositoryBase, IPlayerPerformanceRepository
{
    private readonly string _performanceIndexName;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="elasticClient">Elasticsearchクライアント</param>
    /// <param name="options">Elasticsearch設定</param>
    /// <param name="logger">ロガー</param>
    public ElasticsearchPlayerPerformanceRepository(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> options,
        ILogger<ElasticsearchPlayerPerformanceRepository> logger)
        : base(elasticClient, options, logger)
    {
        _performanceIndexName = GetIndexName(_settings.IndexSettings.PlayerPerformanceIndex);

        // インデックスを初期化
        EnsureIndexCreatedAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<PlayerMatchPerformance> UpsertPerformanceAsync(PlayerMatchPerformance performance, CancellationToken cancellationToken = default)
    {
        if (performance == null)
        {
            throw new ArgumentNullException(nameof(performance));
        }

        if (string.IsNullOrEmpty(performance.Puuid))
        {
            throw new ArgumentException("PUUIDは必須です", nameof(performance));
        }

        if (string.IsNullOrEmpty(performance.MatchId))
        {
            throw new ArgumentException("試合IDは必須です", nameof(performance));
        }

        // パフォーマンスのIDはプレイヤーIDと試合IDの組み合わせで一意に
        string id = $"{performance.Puuid}_{performance.MatchId}";

        return await IndexDocumentAsync(_performanceIndexName, performance, id, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PlayerMatchPerformance>> GetPerformancesByMatchIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
        {
            throw new ArgumentNullException(nameof(matchId));
        }

        return await SearchDocumentsAsync<PlayerMatchPerformance>(_performanceIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m.Term(t => t.Field(f => f.MatchId).Value(matchId)))
                )
            )
            .Size(100) // 1試合のプレイヤーは最大で10人程度
            .Sort(so => so.Ascending(f => f.TeamId)), // チーム順にソート
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlayerMatchPerformance?> GetPlayerPerformanceInMatchAsync(string puuid, string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        if (string.IsNullOrEmpty(matchId))
        {
            throw new ArgumentNullException(nameof(matchId));
        }

        // 直接IDで検索できるようにする
        string id = $"{puuid}_{matchId}";
        return await GetDocumentAsync<PlayerMatchPerformance>(_performanceIndexName, id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PerformanceStats> GetPlayerPerformanceStatsAsync(
        string puuid,
        DateTime from,
        DateTime to,
        string? gameMode = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // クエリ構築
        var searchDescriptor = new SearchDescriptor<PlayerMatchPerformance>()
            .Index(_performanceIndexName)
            .Query(q =>
            {
                var boolQuery = q.Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.Puuid).Value(puuid)),
                        m => m.DateRange(r => r
                            .Field(f => f.GameStartTimestamp)
                            .GreaterThanOrEquals(from)
                            .LessThanOrEquals(to)
                        )
                    )
                );

                // ゲームモードフィルタ（指定された場合）
                if (!string.IsNullOrEmpty(gameMode))
                {
                    boolQuery = q.Bool(b => b
                        .Must(
                            m => m.Term(t => t.Field(f => f.Puuid).Value(puuid)),
                            m => m.DateRange(r => r
                                .Field(f => f.GameStartTimestamp)
                                .GreaterThanOrEquals(from)
                                .LessThanOrEquals(to)
                            ),
                            m => m.Term(t => t.Field("gameMode").Value(gameMode.ToLowerInvariant()))
                        )
                    );
                }

                return boolQuery;
            })
            .Size(0) // 集計のみ
            .Aggregations(a => a
                .Sum("total_kills", s => s.Field(f => f.Kills))
                .Sum("total_deaths", s => s.Field(f => f.Deaths))
                .Sum("total_assists", s => s.Field(f => f.Assists))
                .Sum("total_score", s => s.Field(f => f.Score))
                .Sum("total_damage", s => s.Field(f => f.DamageDealt))
                .Sum("total_headshots", s => s.Field(f => f.Headshots))
                .Sum("total_bodyshots", s => s.Field(f => f.Bodyshots))
                .Sum("total_legshots", s => s.Field(f => f.Legshots))
                .ValueCount("matches_played", vc => vc.Field(f => f.MatchId))
                .Terms("agents", t => t
                    .Field(f => f.AgentName.Suffix("keyword"))
                    .Size(20)
                )
                .Terms("won_matches", t => t
                    .Field("matchWon")
                    .Size(2)
                )
            );

        var response = await _elasticClient.SearchAsync<PlayerMatchPerformance>(searchDescriptor, cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError("パフォーマンス統計クエリエラー: {Error}", response.DebugInformation);
            // エラー時でも空の結果を返す
            return new PerformanceStats
            {
                Puuid = puuid,
                StartDate = from,
                EndDate = to
            };
        }

        // 集計結果から統計情報を構築
        int matchesPlayed = (int)response.Aggregations.ValueCount("matches_played").Value;
        int totalKills = (int)response.Aggregations.Sum("total_kills").Value;
        int totalDeaths = (int)response.Aggregations.Sum("total_deaths").Value;
        int totalAssists = (int)response.Aggregations.Sum("total_assists").Value;
        int totalScore = (int)response.Aggregations.Sum("total_score").Value;
        int totalDamage = (int)response.Aggregations.Sum("total_damage").Value;
        int totalHeadshots = (int)response.Aggregations.Sum("total_headshots").Value;
        int totalBodyshots = (int)response.Aggregations.Sum("total_bodyshots").Value;
        int totalLegshots = (int)response.Aggregations.Sum("total_legshots").Value;

        // 勝ち試合数の集計
        int matchesWon = 0;
        var wonMatches = response.Aggregations.Terms("won_matches");
        if (wonMatches != null)
        {
            foreach (var bucket in wonMatches.Buckets)
            {
                if (bucket.Key.ToString() == "true")
                {
                    matchesWon = (int)bucket.DocCount;
                }
            }
        }

        // 最もプレイしたエージェントの特定
        string mostPlayedAgent = string.Empty;
        var agentTerms = response.Aggregations.Terms("agents");
        if (agentTerms != null && agentTerms.Buckets.Any())
        {
            mostPlayedAgent = agentTerms.Buckets.First().Key.ToString() ?? string.Empty;
        }

        // 平均値の計算
        float averageKills = 0;
        float averageDeaths = 0;
        float averageAssists = 0;
        float averageScore = 0;
        float averageDamage = 0;
        float averageHeadshots = 0;
        float averageBodyshots = 0;
        float averageLegshots = 0;
        float winRate = 0;
        float kdRatio = 0;
        float headshotPercentage = 0;

        if (matchesPlayed > 0)
        {
            averageKills = (float)totalKills / matchesPlayed;
            averageDeaths = (float)totalDeaths / matchesPlayed;
            averageAssists = (float)totalAssists / matchesPlayed;
            averageScore = (float)totalScore / matchesPlayed;
            averageDamage = (float)totalDamage / matchesPlayed;
            averageHeadshots = (float)totalHeadshots / matchesPlayed;
            averageBodyshots = (float)totalBodyshots / matchesPlayed;
            averageLegshots = (float)totalLegshots / matchesPlayed;
            winRate = (float)matchesWon / matchesPlayed;
        }

        // K/D比とHSPの計算
        if (totalDeaths > 0)
        {
            kdRatio = (float)totalKills / totalDeaths;
        }

        int totalShots = totalHeadshots + totalBodyshots + totalLegshots;
        if (totalShots > 0)
        {
            headshotPercentage = (float)totalHeadshots / totalShots;
        }

        var stats = new PerformanceStats
        {
            Puuid = puuid,
            StartDate = from,
            EndDate = to,
            MatchesPlayed = matchesPlayed,
            MatchesWon = matchesWon,
            TotalKills = totalKills,
            TotalDeaths = totalDeaths,
            TotalAssists = totalAssists,
            TotalScore = totalScore,
            TotalDamage = totalDamage,
            TotalHeadshots = totalHeadshots,
            TotalBodyshots = totalBodyshots,
            TotalLegshots = totalLegshots,
            AverageKills = averageKills,
            AverageDeaths = averageDeaths,
            AverageAssists = averageAssists,
            AverageScore = averageScore,
            AverageDamage = averageDamage,
            AverageHeadshots = averageHeadshots,
            AverageBodyshots = averageBodyshots,
            AverageLegshots = averageLegshots,
            WinRate = winRate,
            KdRatio = kdRatio,
            HeadshotPercentage = headshotPercentage,
            MostPlayedAgent = mostPlayedAgent
        };

        return stats;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, AgentPerformance>> GetPlayerAgentPerformanceAsync(
        string puuid,
        DateTime from,
        DateTime to,
        int minGames = 0,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // エージェント別集計クエリ
        var response = await AggregateAsync<PlayerMatchPerformance>(_performanceIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.Puuid).Value(puuid)),
                        m => m.DateRange(r => r
                            .Field(f => f.GameStartTimestamp)
                            .GreaterThanOrEquals(from)
                            .LessThanOrEquals(to)
                        )
                    )
                )
            )
            .Size(0) // 集計のみ
            .Aggregations(a => a
                .Terms("by_agent", t => t
                    .Field(f => f.AgentName.Suffix("keyword"))
                    .Size(50) // 十分な数のエージェントを取得
                    .Aggregations(aa => aa
                        .Sum("kills", s => s.Field(f => f.Kills))
                        .Sum("deaths", s => s.Field(f => f.Deaths))
                        .Sum("assists", s => s.Field(f => f.Assists))
                        .Sum("damage", s => s.Field(f => f.DamageDealt))
                        .Sum("headshots", s => s.Field(f => f.Headshots))
                        .Sum("bodyshots", s => s.Field(f => f.Bodyshots))
                        .Sum("legshots", s => s.Field(f => f.Legshots))
                        .Terms("wins", wt => wt
                            .Field("matchWon")
                            .Size(2)
                        )
                    )
                )
            ),
            cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError("エージェント別パフォーマンスクエリエラー: {Error}", response.DebugInformation);
            return new Dictionary<string, AgentPerformance>();
        }

        var result = new Dictionary<string, AgentPerformance>();
        var agentTerms = response.Aggregations.Terms("by_agent");

        if (agentTerms != null)
        {
            foreach (var agentBucket in agentTerms.Buckets)
            {
                string agent = agentBucket.Key.ToString() ?? "Unknown";
                int gamesPlayed = (int)agentBucket.DocCount;

                // 最小ゲーム数でフィルタ
                if (gamesPlayed < minGames)
                {
                    continue;
                }

                // 勝利数を集計
                int wins = 0;
                var winTerms = agentBucket.Terms("wins");
                if (winTerms != null)
                {
                    foreach (var winBucket in winTerms.Buckets)
                    {
                        if (winBucket.Key.ToString() == "true")
                        {
                            wins = (int)winBucket.DocCount;
                        }
                    }
                }

                // 集計値の取得
                int kills = (int)agentBucket.Sum("kills").Value;
                int deaths = (int)agentBucket.Sum("deaths").Value;
                int assists = (int)agentBucket.Sum("assists").Value;
                int damageDealt = (int)agentBucket.Sum("damage").Value;
                int headshots = (int)agentBucket.Sum("headshots").Value;
                int bodyshots = (int)agentBucket.Sum("bodyshots").Value;
                int legshots = (int)agentBucket.Sum("legshots").Value;

                // エージェントパフォーマンスを構築
                var agentPerf = new AgentPerformance
                {
                    AgentName = agent,
                    GamesPlayed = gamesPlayed,
                    GamesWon = wins,
                    WinRate = gamesPlayed > 0 ? (float)wins / gamesPlayed : 0,
                    TotalKills = kills,
                    TotalDeaths = deaths,
                    TotalAssists = assists,
                    KdRatio = deaths > 0 ? (float)kills / deaths : kills,
                    AverageKills = gamesPlayed > 0 ? (float)kills / gamesPlayed : 0,
                    AverageDeaths = gamesPlayed > 0 ? (float)deaths / gamesPlayed : 0,
                    AverageAssists = gamesPlayed > 0 ? (float)assists / gamesPlayed : 0,
                    TotalDamage = damageDealt,
                    TotalHeadshots = headshots
                };

                // ヘッドショット率の計算
                int totalShots = headshots + bodyshots + legshots;
                float headshotPercentage = 0;
                if (totalShots > 0)
                {
                    headshotPercentage = (float)headshots / totalShots;
                }

                // 更新されたエージェントパフォーマンスを作成
                agentPerf = new AgentPerformance
                {
                    AgentName = agentPerf.AgentName,
                    GamesPlayed = agentPerf.GamesPlayed,
                    GamesWon = agentPerf.GamesWon,
                    WinRate = agentPerf.WinRate,
                    TotalKills = agentPerf.TotalKills,
                    TotalDeaths = agentPerf.TotalDeaths,
                    TotalAssists = agentPerf.TotalAssists,
                    KdRatio = agentPerf.KdRatio,
                    AverageKills = agentPerf.AverageKills,
                    AverageDeaths = agentPerf.AverageDeaths,
                    AverageAssists = agentPerf.AverageAssists,
                    TotalDamage = agentPerf.TotalDamage,
                    TotalHeadshots = agentPerf.TotalHeadshots,
                    TotalBodyshots = bodyshots,
                    TotalLegshots = legshots,
                    HeadshotPercentage = headshotPercentage
                };

                result[agent] = agentPerf;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, MapPerformance>> GetPlayerMapPerformanceAsync(
        string puuid,
        DateTime from,
        DateTime to,
        int minGames = 0,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // この場合、追加のマップ情報（mapName, mapId）はparent-join型がないと難しいので、
        // matchIdでマッチング後にマップ情報を結合する方針でいきます。

        // まずはパフォーマンス情報を取得
        var performances = await SearchDocumentsAsync<PlayerMatchPerformance>(_performanceIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.Puuid).Value(puuid)),
                        m => m.DateRange(r => r
                            .Field(f => f.GameStartTimestamp)
                            .GreaterThanOrEquals(from)
                            .LessThanOrEquals(to)
                        )
                    )
                )
            )
            .Size(1000) // 適切な数に調整
            .Source(src => src.Includes(i => i
                .Fields(
                    f => f.MatchId,
                    f => f.Kills,
                    f => f.Deaths,
                    f => f.Assists,
                    f => f.DamageDealt,
                    f => f.Headshots,
                    f => f.Bodyshots,
                    f => f.Legshots,
                    f => f.TeamId
                )
            )),
            cancellationToken);

        if (!performances.Any())
        {
            return new Dictionary<string, MapPerformance>();
        }

        // 試合IDのリストを取得
        var matchIds = performances.Select(p => p.MatchId).Distinct().ToList();

        // 試合情報を取得（マップ情報を含む）
        var matches = await SearchDocumentsAsync<object>(GetIndexName(_settings.IndexSettings.MatchIndex), s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Terms(t => t
                            .Field("matchId")
                            .Terms(matchIds)
                        )
                    )
                )
            )
            .Size(matchIds.Count)
            .Source(src => src.Includes(i => i
                .Fields(
                    "matchId",
                    "mapName",
                    "mapId",
                    "teams"
                )
            )),
            cancellationToken);

        // 試合IDからマップ情報へのマッピング
        var matchToMap = new Dictionary<string, (string MapName, string MapId)>();
        var matchToTeams = new Dictionary<string, List<(string TeamId, bool WonMatch)>>();

        foreach (var match in matches)
        {
            var matchId = match.GetType().GetProperty("matchId")?.GetValue(match)?.ToString();
            var mapName = match.GetType().GetProperty("mapName")?.GetValue(match)?.ToString() ?? "Unknown";
            var mapId = match.GetType().GetProperty("mapId")?.GetValue(match)?.ToString() ?? "";

            if (!string.IsNullOrEmpty(matchId))
            {
                matchToMap[matchId] = (mapName, mapId);

                // チーム情報の抽出
                var teamsProperty = match.GetType().GetProperty("teams");
                if (teamsProperty != null)
                {
                    var teamsValue = teamsProperty.GetValue(match);
                    if (teamsValue is System.Collections.IEnumerable teams)
                    {
                        var teamsList = new List<(string TeamId, bool WonMatch)>();

                        foreach (var team in teams)
                        {
                            var teamIdProp = team.GetType().GetProperty("teamId");
                            var wonMatchProp = team.GetType().GetProperty("wonMatch");

                            if (teamIdProp != null && wonMatchProp != null)
                            {
                                var teamId = teamIdProp.GetValue(team)?.ToString() ?? "";
                                var wonMatch = (bool)(wonMatchProp.GetValue(team) ?? false);

                                teamsList.Add((teamId, wonMatch));
                            }
                        }

                        if (teamsList.Any())
                        {
                            matchToTeams[matchId] = teamsList;
                        }
                    }
                }
            }
        }

        // パフォーマンスをマップ別にグループ化
        var mapPerformances = new Dictionary<string, List<(PlayerMatchPerformance Performance, bool WonMatch)>>();

        foreach (var perf in performances)
        {
            if (matchToMap.TryGetValue(perf.MatchId, out var mapInfo))
            {
                if (!mapPerformances.ContainsKey(mapInfo.MapName))
                {
                    mapPerformances[mapInfo.MapName] = new List<(PlayerMatchPerformance, bool)>();
                }

                // 勝利情報の取得
                bool wonMatch = false;
                if (matchToTeams.TryGetValue(perf.MatchId, out var teams))
                {
                    var team = teams.FirstOrDefault(t => t.TeamId == perf.TeamId);
                    wonMatch = team.WonMatch;
                }

                mapPerformances[mapInfo.MapName].Add((perf, wonMatch));
            }
        }

        // マップパフォーマンスの集計
        var result = new Dictionary<string, MapPerformance>();

        foreach (var mapEntry in mapPerformances)
        {
            string mapName = mapEntry.Key;
            var mapPerfs = mapEntry.Value;
            int gamesPlayed = mapPerfs.Count;

            // 最小ゲーム数でフィルタ
            if (gamesPlayed < minGames)
            {
                continue;
            }

            // 勝利数
            int wins = mapPerfs.Count(p => p.WonMatch);

            // 統計の集計
            int kills = mapPerfs.Sum(p => p.Performance.Kills);
            int deaths = mapPerfs.Sum(p => p.Performance.Deaths);
            int assists = mapPerfs.Sum(p => p.Performance.Assists);
            int damage = mapPerfs.Sum(p => p.Performance.DamageDealt);
            int headshots = mapPerfs.Sum(p => p.Performance.Headshots);
            int bodyshots = mapPerfs.Sum(p => p.Performance.Bodyshots);
            int legshots = mapPerfs.Sum(p => p.Performance.Legshots);

            // マップIDの取得
            string mapId = "";
            if (mapPerfs.Any())
            {
                var firstMatchId = mapPerfs.First().Performance.MatchId;
                if (matchToMap.TryGetValue(firstMatchId, out var mapInfo))
                {
                    mapId = mapInfo.MapId;
                }
            }

            // マップパフォーマンスを構築
            var mapPerf = new MapPerformance
            {
                MapName = mapName,
                MapId = mapId,
                GamesPlayed = gamesPlayed,
                GamesWon = wins,
                WinRate = gamesPlayed > 0 ? (float)wins / gamesPlayed : 0,
                TotalKills = kills,
                TotalDeaths = deaths,
                TotalAssists = assists,
                KdRatio = deaths > 0 ? (float)kills / deaths : kills,
                AverageKills = gamesPlayed > 0 ? (float)kills / gamesPlayed : 0,
                AverageDeaths = gamesPlayed > 0 ? (float)deaths / gamesPlayed : 0,
                AverageAssists = gamesPlayed > 0 ? (float)assists / gamesPlayed : 0,
                TotalDamage = damage,
                TotalHeadshots = headshots
            };

            // ヘッドショット率の計算
            int totalShots = headshots + bodyshots + legshots;
            float headshotPercentage = 0;
            if (totalShots > 0)
            {
                headshotPercentage = (float)headshots / totalShots;
            }

            // 更新されたマップパフォーマンスを作成
            mapPerf = new MapPerformance
            {
                MapName = mapPerf.MapName,
                MapId = mapPerf.MapId,
                GamesPlayed = mapPerf.GamesPlayed,
                GamesWon = mapPerf.GamesWon,
                WinRate = mapPerf.WinRate,
                TotalKills = mapPerf.TotalKills,
                TotalDeaths = mapPerf.TotalDeaths,
                TotalAssists = mapPerf.TotalAssists,
                KdRatio = mapPerf.KdRatio,
                AverageKills = mapPerf.AverageKills,
                AverageDeaths = mapPerf.AverageDeaths,
                AverageAssists = mapPerf.AverageAssists,
                TotalDamage = mapPerf.TotalDamage,
                TotalHeadshots = mapPerf.TotalHeadshots,
                TotalBodyshots = bodyshots,
                TotalLegshots = legshots,
                HeadshotPercentage = headshotPercentage
            };

            result[mapName] = mapPerf;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<List<(string Puuid, string PlayerName, string TagLine, float KdaRatio, int Kills, int Deaths, int Assists, int GamesPlayed)>>
        GetPlayersKdaRankingAsync(
            IEnumerable<string> puuids,
            DateTime from,
            DateTime to,
            string? gameMode = null,
            int minGames = 1,
            CancellationToken cancellationToken = default)
    {
        if (puuids == null || !puuids.Any())
        {
            return new List<(string, string, string, float, int, int, int, int)>();
        }

        // クエリ構築
        var searchDescriptor = new SearchDescriptor<PlayerMatchPerformance>()
            .Index(_performanceIndexName)
            .Query(q =>
            {
                var boolQuery = q.Bool(b => b
                    .Must(
                        m => m.Terms(t => t.Field(f => f.Puuid).Terms(puuids)),
                        m => m.DateRange(r => r
                            .Field(f => f.GameStartTimestamp)
                            .GreaterThanOrEquals(from)
                            .LessThanOrEquals(to)
                        )
                    )
                );

                // ゲームモードフィルタ（指定された場合）
                if (!string.IsNullOrEmpty(gameMode))
                {
                    boolQuery = q.Bool(b => b
                        .Must(
                            m => m.Terms(t => t.Field(f => f.Puuid).Terms(puuids)),
                            m => m.DateRange(r => r
                                .Field(f => f.GameStartTimestamp)
                                .GreaterThanOrEquals(from)
                                .LessThanOrEquals(to)
                            ),
                            m => m.Term(t => t.Field("gameMode").Value(gameMode.ToLowerInvariant()))
                        )
                    );
                }

                return boolQuery;
            })
            .Size(0) // 集計のみ
            .Aggregations(a => a
                .Terms("by_player", t => t
                    .Field(f => f.Puuid)
                    .Size(puuids.Count())
                    .MinimumDocumentCount(minGames) // 最小ゲーム数以上のプレイヤーのみ
                    .Aggregations(aa => aa
                        .Sum("kills", s => s.Field(f => f.Kills))
                        .Sum("deaths", s => s.Field(f => f.Deaths))
                        .Sum("assists", s => s.Field(f => f.Assists))
                        .Terms("names", nt => nt
                            .Field(f => f.PlayerName.Suffix("keyword"))
                            .Size(1)
                        )
                        .Terms("tags", tt => tt
                            .Field(f => f.TagLine.Suffix("keyword"))
                            .Size(1)
                        )
                    )
                )
            );

        var response = await _elasticClient.SearchAsync<PlayerMatchPerformance>(searchDescriptor, cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError("KDAランキングクエリエラー: {Error}", response.DebugInformation);
            return new List<(string, string, string, float, int, int, int, int)>();
        }

        var result = new List<(string Puuid, string PlayerName, string TagLine, float KdaRatio, int Kills, int Deaths, int Assists, int GamesPlayed)>();
        var playerTerms = response.Aggregations.Terms("by_player");

        if (playerTerms != null)
        {
            foreach (var playerBucket in playerTerms.Buckets)
            {
                string puuid = playerBucket.Key.ToString() ?? string.Empty;
                int gamesPlayed = (int)playerBucket.DocCount;

                // 最小ゲーム数チェック（不要かもしれないが念のため）
                if (gamesPlayed < minGames)
                {
                    continue;
                }

                // 集計値の取得
                int kills = (int)playerBucket.Sum("kills").Value;
                int deaths = (int)playerBucket.Sum("deaths").Value;
                int assists = (int)playerBucket.Sum("assists").Value;

                // KDA計算 (K+A)/D
                float kdaRatio = deaths > 0
                    ? (float)(kills + assists) / deaths
                    : kills + assists; // 0デスの場合

                // プレイヤー名とタグを取得
                string playerName = "Unknown";
                string tagLine = "";

                var nameTerms = playerBucket.Terms("names");
                if (nameTerms != null && nameTerms.Buckets.Any())
                {
                    playerName = nameTerms.Buckets.First().Key.ToString() ?? "Unknown";
                }

                var tagTerms = playerBucket.Terms("tags");
                if (tagTerms != null && tagTerms.Buckets.Any())
                {
                    tagLine = tagTerms.Buckets.First().Key.ToString() ?? "";
                }

                result.Add((puuid, playerName, tagLine, kdaRatio, kills, deaths, assists, gamesPlayed));
            }
        }

        // KDAで降順ソート
        return result.OrderByDescending(p => p.KdaRatio).ToList();
    }

    /// <summary>
    /// インデックスが存在することを確認し、なければ作成
    /// </summary>
    private async Task EnsureIndexCreatedAsync()
    {
        await CreateIndexAsync<PlayerMatchPerformance>(_performanceIndexName,
            settings => settings
                .Setting("index.mapping.total_fields.limit", 2000) // パフォーマンスで多くのフィールドを許可
                .Setting("index.max_result_window", 10000) // 検索結果の最大数を増やす
                .Setting("index.refresh_interval", "10s") // リフレッシュ間隔を長くして書き込みパフォーマンスを向上
                .Analysis(a => a
                    .Normalizers(n => n
                        .Custom("lowercase", cn => cn
                            .Filters("lowercase")
                        )
                    )
                ),
            mappings => mappings
                .AutoMap()
                .Properties(p => p
                    // 主要フィールドのマッピング
                    .Keyword(k => k.Name(n => n.Puuid).Index(true))
                    .Keyword(k => k.Name(n => n.MatchId).Index(true))
                    .Text(t => t.Name(n => n.PlayerName).Fields(f => f.Keyword(k => k.Name("keyword"))))
                    .Keyword(k => k.Name(n => n.TagLine).Fields(f => f.Keyword(k => k.Name("keyword"))))
                    .Keyword(k => k.Name(n => n.TeamId).Index(true))
                    .Keyword(k => k.Name(n => n.AgentName).Fields(f => f.Keyword(k => k.Name("keyword"))))
                    .Date(d => d.Name(n => n.GameStartTimestamp))

                    // 数値フィールドを最適化
                    .Number(n => n.Name(nm => nm.Kills).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.Deaths).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.Assists).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.Score).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.DamageDealt).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.Headshots).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.Bodyshots).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.Legshots).Type(NumberType.Integer))
                    // 勝利フラグ
                    .Boolean(b => b.Name("matchWon"))
                )
        );
    }
}