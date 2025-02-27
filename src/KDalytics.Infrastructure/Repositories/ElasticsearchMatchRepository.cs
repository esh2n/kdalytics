using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Models.Match;
using KDalytics.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace KDalytics.Infrastructure.Repositories;

/// <summary>
/// Elasticsearchを使用した試合リポジトリの実装
/// </summary>
public class ElasticsearchMatchRepository : ElasticsearchRepositoryBase, IMatchRepository
{
    private readonly string _matchIndexName;
    private readonly string _performanceIndexName;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="elasticClient">Elasticsearchクライアント</param>
    /// <param name="options">Elasticsearch設定</param>
    /// <param name="logger">ロガー</param>
    public ElasticsearchMatchRepository(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> options,
        ILogger<ElasticsearchMatchRepository> logger)
        : base(elasticClient, options, logger)
    {
        _matchIndexName = GetIndexName(_settings.IndexSettings.MatchIndex);
        _performanceIndexName = GetIndexName(_settings.IndexSettings.PlayerPerformanceIndex);

        // インデックスを初期化
        EnsureIndexCreatedAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<MatchEntity> UpsertMatchAsync(MatchEntity match, CancellationToken cancellationToken = default)
    {
        if (match == null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        if (string.IsNullOrEmpty(match.MatchId))
        {
            throw new ArgumentException("試合IDは必須です", nameof(match));
        }

        // チームデータとラウンドデータを親データとして一緒に保存
        return await IndexDocumentAsync(_matchIndexName, match, match.MatchId, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MatchEntity?> GetMatchByIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(matchId))
        {
            throw new ArgumentNullException(nameof(matchId));
        }

        return await GetDocumentAsync<MatchEntity>(_matchIndexName, matchId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<MatchEntity>> GetPlayerRecentMatchesAsync(string puuid, int count = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // まずパフォーマンスインデックスからプレイヤーが参加した試合IDを検索
        var matchIds = await GetPlayerMatchIdsAsync(puuid, count, cancellationToken);

        if (!matchIds.Any())
        {
            return new List<MatchEntity>();
        }

        // 試合IDのリストを使って試合情報を取得
        return await SearchDocumentsAsync<MatchEntity>(_matchIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Terms(t => t
                            .Field(f => f.MatchId)
                            .Terms(matchIds)
                        )
                    )
                )
            )
            .Size(matchIds.Count)
            .Sort(so => so.Descending(f => f.StartTime)),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<MatchEntity>> GetPlayerMatchesWithFilterAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        string? gameMode = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // まずパフォーマンスインデックスからプレイヤーが参加した試合IDをフィルタして検索
        var matchIds = await GetPlayerMatchIdsWithFilterAsync(puuid, from, to, gameMode, skip, take, cancellationToken);

        if (!matchIds.Any())
        {
            return new List<MatchEntity>();
        }

        // 試合IDのリストを使って試合情報を取得
        return await SearchDocumentsAsync<MatchEntity>(_matchIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Terms(t => t
                            .Field(f => f.MatchId)
                            .Terms(matchIds)
                        )
                    )
                )
            )
            .Size(matchIds.Count)
            .Sort(so => so.Descending(f => f.StartTime)),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string?> GetPlayerLatestMatchIdAsync(string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // パフォーマンスインデックスから最新の試合IDを1件だけ取得
        var searchResponse = await _elasticClient.SearchAsync<object>(s => s
            .Index(_performanceIndexName)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m.Term(t => t.Field("puuid").Value(puuid)))
                )
            )
            .Sort(so => so.Descending("gameStartTimestamp"))
            .Size(1)
            .Source(src => src.Includes(i => i.Field("matchId"))),
            cancellationToken);

        if (!searchResponse.IsValid || searchResponse.Documents.Count == 0)
        {
            return null;
        }

        // 検索結果から試合IDを抽出
        return searchResponse.Documents
            .First()
            .GetType()
            .GetProperty("matchId")?
            .GetValue(searchResponse.Documents.First())?
            .ToString();
    }

    /// <inheritdoc />
    public async Task<Dictionary<DateTime, int>> GetPlayerMatchCountByDayAsync(string puuid, int days = 30, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        var endDate = DateTime.UtcNow.Date.AddDays(1); // 今日の終わりまで

        // 日付集計クエリ
        var searchResponse = await AggregateAsync<object>(_performanceIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field("puuid").Value(puuid)),
                        m => m.DateRange(r => r
                            .Field("gameStartTimestamp")
                            .GreaterThanOrEquals(startDate)
                            .LessThan(endDate)
                        )
                    )
                )
            )
            .Size(0) // 集計のみで結果は不要
            .Aggregations(a => a
                .DateHistogram("matches_by_day", dh => dh
                    .Field("gameStartTimestamp")
                    .CalendarInterval(DateInterval.Day)
                    .Format("yyyy-MM-dd")
                    .MinimumDocumentCount(0) // 0件の日も含める
                    .ExtendedBounds(startDate, endDate.AddDays(-1))
                )
            ),
            cancellationToken);

        var result = new Dictionary<DateTime, int>();

        // 初期値として、全ての日付で0件を設定
        for (int i = 0; i < days; i++)
        {
            result[startDate.AddDays(i).Date] = 0;
        }

        // 集計結果から日付ごとの件数を抽出
        if (searchResponse.IsValid && searchResponse.Aggregations.ContainsKey("matches_by_day"))
        {
            // NESTのバージョンによって型が異なる可能性があるため、動的に処理
            dynamic dateHistogram = searchResponse.Aggregations["matches_by_day"];

            try
            {
                foreach (dynamic bucket in dateHistogram.Buckets)
                {
                    if (DateTime.TryParse(bucket.KeyAsString.ToString(), out DateTime date))
                    {
                        result[date.Date] = (int)bucket.DocCount;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("日付集計の処理中にエラーが発生しました: {Error}", ex.Message);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, int>> GetPlayerMatchCountByMapAsync(
        string puuid,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // パフォーマンスインデックスから参加した試合IDを取得
        var matchIds = await GetPlayerMatchIdsWithFilterAsync(puuid, from, to, null, null, null, cancellationToken);

        if (!matchIds.Any())
        {
            return new Dictionary<string, int>();
        }

        // 試合インデックスでマップごとに集計
        var searchResponse = await AggregateAsync<MatchEntity>(_matchIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Terms(t => t
                            .Field(f => f.MatchId)
                            .Terms(matchIds)
                        )
                    )
                )
            )
            .Size(0) // 集計のみで結果は不要
            .Aggregations(a => a
                .Terms("maps", t => t
                    .Field(f => f.MapName.Suffix("keyword"))
                    .Size(50) // 十分な数のマップを取得
                )
            ),
            cancellationToken);

        var result = new Dictionary<string, int>();

        // 集計結果からマップごとの件数を抽出
        if (searchResponse.IsValid && searchResponse.Aggregations.ContainsKey("maps"))
        {
            try
            {
                dynamic mapAgg = searchResponse.Aggregations["maps"];
                foreach (dynamic bucket in mapAgg.Buckets)
                {
                    var mapName = bucket.Key.ToString() ?? "Unknown";
                    result[mapName] = (int)bucket.DocCount;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("マップ集計の処理中にエラーが発生しました: {Error}", ex.Message);
            }
        }

        return result;
    }

    /// <summary>
    /// プレイヤーが参加した試合IDのリストを取得
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="count">取得する件数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合IDのリスト</returns>
    private async Task<List<string>> GetPlayerMatchIdsAsync(string puuid, int count, CancellationToken cancellationToken)
    {
        var searchResponse = await _elasticClient.SearchAsync<object>(s => s
            .Index(_performanceIndexName)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m.Term(t => t.Field("puuid").Value(puuid)))
                )
            )
            .Sort(so => so.Descending("gameStartTimestamp"))
            .Size(count)
            .Source(src => src.Includes(i => i.Field("matchId"))),
            cancellationToken);

        if (!searchResponse.IsValid)
        {
            _logger.LogError("プレイヤー参加試合検索エラー: {Error}", searchResponse.DebugInformation);
            return new List<string>();
        }

        // 検索結果から試合IDを抽出
        return searchResponse.Documents
            .Select(d => d.GetType().GetProperty("matchId")?.GetValue(d)?.ToString())
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .Cast<string>()
            .ToList();
    }

    /// <summary>
    /// フィルタ条件付きでプレイヤーが参加した試合IDのリストを取得
    /// </summary>
    /// <param name="puuid">プレイヤーのPUUID</param>
    /// <param name="from">開始日時</param>
    /// <param name="to">終了日時</param>
    /// <param name="gameMode">ゲームモード</param>
    /// <param name="skip">スキップする件数</param>
    /// <param name="take">取得する件数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>試合IDのリスト</returns>
    private async Task<List<string>> GetPlayerMatchIdsWithFilterAsync(
        string puuid,
        DateTime? from,
        DateTime? to,
        string? gameMode,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var searchDescriptor = new SearchDescriptor<object>()
            .Index(_performanceIndexName)
            .Query(q =>
            {
                var boolQuery = q.Bool(b => b.Must(m => m.Term(t => t.Field("puuid").Value(puuid))));

                // 日時フィルタは完全に別の方法で実装
                // NESTのバージョンによってAPIが異なるため、シンプルな実装に変更
                var query = q.Bool(b => b.Must(m => m.Term(t => t.Field("puuid").Value(puuid))));

                // 日時フィルタを追加
                if (from.HasValue || to.HasValue)
                {
                    // 元のクエリを保持
                    var originalQuery = query;

                    // 日時範囲クエリを作成
                    var dateRangeQuery = q.DateRange(r =>
                    {
                        r = r.Field("gameStartTimestamp");
                        if (from.HasValue)
                            r = r.GreaterThanOrEquals(DateMath.Anchored(from.Value));
                        if (to.HasValue)
                            r = r.LessThanOrEquals(DateMath.Anchored(to.Value));
                        return r;
                    });

                    // 両方のクエリを組み合わせる
                    query = q.Bool(b => b.Must(originalQuery, dateRangeQuery));
                }

                return query;
            })
            .Sort(so => so.Descending("gameStartTimestamp"))
            .Source(src => src.Includes(i => i.Field("matchId")));

        // ページング
        if (skip.HasValue || take.HasValue)
        {
            searchDescriptor = searchDescriptor
                .From(skip ?? 0)
                .Size(take ?? 50); // デフォルトは50件
        }
        else
        {
            searchDescriptor = searchDescriptor.Size(100); // デフォルトは最新100件
        }

        var searchResponse = await _elasticClient.SearchAsync<object>(searchDescriptor, cancellationToken);

        if (!searchResponse.IsValid)
        {
            _logger.LogError("フィルタ付きプレイヤー参加試合検索エラー: {Error}", searchResponse.DebugInformation);
            return new List<string>();
        }

        // 検索結果から試合IDを抽出
        var matchIds = searchResponse.Documents
            .Select(d => d.GetType().GetProperty("matchId")?.GetValue(d)?.ToString())
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .Cast<string>()
            .ToList();

        // ゲームモードのフィルタリングが必要な場合
        if (!string.IsNullOrEmpty(gameMode) && matchIds.Any())
        {
            // 試合情報を取得してゲームモードでフィルタリング
            var matches = await SearchDocumentsAsync<MatchEntity>(_matchIndexName, s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Terms(t => t.Field(f => f.MatchId).Terms(matchIds)),
                            m => m.Term(t => t.Field(f => f.GameMode.ToLowerInvariant()).Value(gameMode.ToLowerInvariant()))
                        )
                    )
                )
                .Size(matchIds.Count)
                .Source(src => src.Includes(i => i.Field(f => f.MatchId))),
                cancellationToken);

            return matches.Select(m => m.MatchId).ToList();
        }

        return matchIds;
    }

    /// <summary>
    /// インデックスが存在することを確認し、なければ作成
    /// </summary>
    private async Task EnsureIndexCreatedAsync()
    {
        try
        {
            // インデックスが存在するか確認
            var indexExists = await _elasticClient.Indices.ExistsAsync(_matchIndexName);
            if (indexExists.Exists)
            {
                _logger.LogInformation("インデックス {IndexName} は既に存在します", _matchIndexName);
                return;
            }

            // インデックスを作成
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(_matchIndexName, c => c
                .Map<MatchEntity>(m => m
                    .AutoMap()
                    .Properties(p => p
                        // 主要フィールドのマッピング
                        .Keyword(k => k.Name(n => n.MatchId).Index(true))
                        .Keyword(k => k.Name(n => n.MapId).Index(true))
                        .Text(t => t.Name(n => n.MapName).Fields(f => f.Keyword(k => k.Name("keyword"))))
                        .Keyword(k => k.Name(n => n.GameMode).Index(true))
                        .Number(n => n.Name(nm => nm.GameLength).Type(NumberType.Long))
                        .Date(d => d.Name(n => n.StartTime))
                        .Keyword(k => k.Name(n => n.SeasonId).Index(true))
                        .Keyword(k => k.Name(n => n.Region).Index(true))

                        // ネストされたオブジェクトのマッピング
                        .Nested<TeamData>(n => n
                            .Name(nn => nn.Teams)
                            .Properties(np => np
                                .Keyword(k => k.Name(tn => tn.TeamId))
                                .Boolean(b => b.Name(tn => tn.HasWon))
                                .Number(num => num.Name(tn => tn.RoundsWon).Type(NumberType.Integer))
                                .Nested<RoundResult>(nr => nr
                                    .Name(nrn => nrn.RoundResults)
                                    .Properties(nrp => nrp
                                        .Number(num => num.Name(rn => rn.RoundNum).Type(NumberType.Integer))
                                        .Keyword(k => k.Name(rn => rn.WinningTeam))
                                        .Boolean(b => b.Name(rn => rn.BombPlanted))
                                        .Boolean(b => b.Name(rn => rn.BombDefused))
                                    )
                                )
                            )
                        )
                    )
                )
            );

            if (!createIndexResponse.IsValid)
            {
                _logger.LogError("インデックス作成エラー: {Error}", createIndexResponse.DebugInformation);
                throw new Exception($"インデックス {_matchIndexName} の作成に失敗しました");
            }

            _logger.LogInformation("インデックス {IndexName} を作成しました", _matchIndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "インデックス作成中にエラーが発生しました: {Error}", ex.Message);
            throw;
        }
    }
}