using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Models.Player;
using KDalytics.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace KDalytics.Infrastructure.Repositories;

/// <summary>
/// Elasticsearchを使用したプレイヤーランクリポジトリの実装
/// </summary>
public class ElasticsearchPlayerRankRepository : ElasticsearchRepositoryBase, IPlayerRankRepository
{
    private readonly string _rankIndexName;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="elasticClient">Elasticsearchクライアント</param>
    /// <param name="options">Elasticsearch設定</param>
    /// <param name="logger">ロガー</param>
    public ElasticsearchPlayerRankRepository(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> options,
        ILogger<ElasticsearchPlayerRankRepository> logger)
        : base(elasticClient, options, logger)
    {
        _rankIndexName = GetIndexName(_settings.IndexSettings.PlayerRankIndex);

        // インデックスを初期化
        EnsureIndexCreatedAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<PlayerRankEntity> UpsertPlayerRankAsync(PlayerRankEntity rank, CancellationToken cancellationToken = default)
    {
        if (rank == null)
        {
            throw new ArgumentNullException(nameof(rank));
        }

        if (string.IsNullOrEmpty(rank.Puuid))
        {
            throw new ArgumentException("PUUIDは必須です", nameof(rank));
        }

        // タイムスタンプが設定されていない場合は現在時刻を設定
        if (rank.LastUpdated == default)
        {
            rank = rank with { LastUpdated = DateTime.UtcNow };
        }

        // ドキュメントIDはPUUID + タイムスタンプで一意に
        string id = $"{rank.Puuid}_{rank.LastUpdated.Ticks}";

        return await IndexDocumentAsync(_rankIndexName, rank, id, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlayerRankEntity?> GetLatestPlayerRankAsync(string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        return await GetFirstOrDefaultAsync<PlayerRankEntity>(_rankIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m.Term(t => t.Field(f => f.Puuid).Value(puuid)))
                )
            )
            .Sort(so => so.Descending(f => f.LastUpdated))
            .Size(1),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PlayerRankEntity>> GetPlayerRankHistoryAsync(string puuid, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        return await SearchDocumentsAsync<PlayerRankEntity>(_rankIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.Puuid).Value(puuid)),
                        m => m.DateRange(r => r
                            .Field(f => f.LastUpdated)
                            .GreaterThanOrEquals(from)
                            .LessThanOrEquals(to)
                        )
                    )
                )
            )
            .Sort(so => so.Ascending(f => f.LastUpdated))
            .Size(1000), // 十分大きな値（本番環境では適切なページングが必要）
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlayerRankEntity?> GetPlayerRankBySeasonAsync(string puuid, string seasonId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        if (string.IsNullOrEmpty(seasonId))
        {
            throw new ArgumentNullException(nameof(seasonId));
        }

        return await GetFirstOrDefaultAsync<PlayerRankEntity>(_rankIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.Puuid).Value(puuid)),
                        m => m.Term(t => t.Field(f => f.SeasonId).Value(seasonId))
                    )
                )
            )
            .Sort(so => so.Descending(f => f.LastUpdated))
            .Size(1),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(bool HasChanged, PlayerRankEntity? OldRank, PlayerRankEntity NewRank)> DetectRankChangeAsync(
        string puuid,
        PlayerRankEntity newRank,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        if (newRank == null)
        {
            throw new ArgumentNullException(nameof(newRank));
        }

        // 直近のランク情報を取得
        var latestRank = await GetLatestPlayerRankAsync(puuid, cancellationToken);

        // 初めてのランクデータの場合は変更があったとみなす
        if (latestRank == null)
        {
            return (true, null, newRank);
        }

        // ランクやティア、MMRに変更があるか確認
        bool hasChanged = latestRank.Rank != newRank.Rank ||
                         latestRank.CurrentTier != newRank.CurrentTier ||
                         latestRank.RankingInTier != newRank.RankingInTier ||
                         latestRank.Mmr != newRank.Mmr;

        return (hasChanged, latestRank, newRank);
    }

    /// <summary>
    /// インデックスが存在することを確認し、なければ作成
    /// </summary>
    private async Task EnsureIndexCreatedAsync()
    {
        await CreateIndexAsync<PlayerRankEntity>(_rankIndexName,
            null,
            mappings => mappings
                .AutoMap()
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Puuid).Index(true))
                    .Keyword(k => k.Name(n => n.SeasonId).Index(true))
                    .Number(n => n.Name(nm => nm.CurrentTier).Type(NumberType.Integer))
                    .Keyword(k => k.Name(n => n.CurrentTierPatched))
                    .Number(n => n.Name(nm => nm.RankingInTier).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.Mmr).Type(NumberType.Integer))
                    .Number(n => n.Name(nm => nm.MmrChangeToLastGame).Type(NumberType.Integer))
                    .Date(d => d.Name(n => n.LastUpdated))
                )
        );
    }
}