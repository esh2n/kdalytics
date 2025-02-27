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
/// Elasticsearchを使用したプレイヤーリポジトリの実装
/// </summary>
public class ElasticsearchPlayerRepository : ElasticsearchRepositoryBase, IPlayerRepository
{
    private readonly string _playerIndexName;
    private readonly string _trackingConfigIndexName;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="elasticClient">Elasticsearchクライアント</param>
    /// <param name="options">Elasticsearch設定</param>
    /// <param name="logger">ロガー</param>
    public ElasticsearchPlayerRepository(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> options,
        ILogger<ElasticsearchPlayerRepository> logger)
        : base(elasticClient, options, logger)
    {
        _playerIndexName = GetIndexName(_settings.IndexSettings.PlayerIndex);
        _trackingConfigIndexName = GetIndexName(_settings.IndexSettings.TrackingConfigIndex);

        // インデックスを初期化
        EnsureIndexesCreatedAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<PlayerEntity> UpsertPlayerAsync(PlayerEntity player, CancellationToken cancellationToken = default)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (string.IsNullOrEmpty(player.Puuid))
        {
            throw new ArgumentException("PUUIDは必須です", nameof(player));
        }

        // 最終更新日時を設定
        if (player.LastUpdated == default)
        {
            player = player with { LastUpdated = DateTime.UtcNow };
        }

        return await IndexDocumentAsync(_playerIndexName, player, player.Puuid, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlayerEntity?> GetPlayerByPuuidAsync(string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        return await GetDocumentAsync<PlayerEntity>(_playerIndexName, puuid, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlayerEntity?> GetPlayerByNameTagAsync(string name, string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrEmpty(tag))
        {
            throw new ArgumentNullException(nameof(tag));
        }

        return await GetFirstOrDefaultAsync<PlayerEntity>(_playerIndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Field(f => f.GameName).Value(name.ToLowerInvariant())),
                        m => m.Term(t => t.Field(f => f.TagLine).Value(tag.ToLowerInvariant()))
                    )
                )
            ), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PlayerEntity>> GetTrackedPlayersAsync(CancellationToken cancellationToken = default)
    {
        return await SearchDocumentsAsync<PlayerEntity>(_playerIndexName, s => s
            .Query(q => q
                .Term(t => t.Field(f => f.IsTracked).Value(true))
            )
            .Size(10000) // 十分大きな値（本番環境では適切なページングが必要）
            .Sort(so => so.Descending(f => f.LastUpdated)),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlayerTrackingConfig> GetOrCreateTrackingConfigAsync(string puuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(puuid))
        {
            throw new ArgumentNullException(nameof(puuid));
        }

        // 既存の設定を検索
        var config = await GetDocumentAsync<PlayerTrackingConfig>(_trackingConfigIndexName, puuid, cancellationToken);

        // 設定が存在しない場合はデフォルト設定で作成
        if (config == null)
        {
            config = new PlayerTrackingConfig
            {
                Puuid = puuid,
                IsTracked = true,
                TrackingInterval = TimeSpan.FromHours(1), // デフォルトは1時間おき
                LastTrackedAt = null, // まだ一度も追跡していない
                NextTrackingAt = DateTime.UtcNow, // すぐに追跡開始
                IsActive = true,
                DiscordNotificationsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await IndexDocumentAsync(_trackingConfigIndexName, config, puuid, true, cancellationToken);
        }

        return config;
    }

    /// <inheritdoc />
    public async Task<PlayerTrackingConfig> UpdateTrackingConfigAsync(PlayerTrackingConfig config, CancellationToken cancellationToken = default)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (string.IsNullOrEmpty(config.Puuid))
        {
            throw new ArgumentException("PUUIDは必須です", nameof(config));
        }

        // 更新日時を設定
        config = config with { UpdatedAt = DateTime.UtcNow };

        return await IndexDocumentAsync(_trackingConfigIndexName, config, config.Puuid, true, cancellationToken);
    }

    /// <summary>
    /// 必要なインデックスが存在することを確認し、なければ作成
    /// </summary>
    private async Task EnsureIndexesCreatedAsync()
    {
        // プレイヤーインデックスの作成
        await CreateIndexAsync<PlayerEntity>(_playerIndexName,
            settings => settings
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("my_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "trim")
                        )
                    )
                ),
            mappings => mappings
                .AutoMap() // 基本的なマッピングは自動生成
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Puuid).Index(true))
                    .Text(t => t.Name(n => n.GameName).Analyzer("my_analyzer").Fields(f => f.Keyword(k => k.Name("keyword"))))
                    .Keyword(k => k.Name(n => n.TagLine).Index(true))
                    .Keyword(k => k.Name(n => n.Region).Index(true))
                    .Number(n => n.Name(nm => nm.AccountLevel).Type(NumberType.Integer))
                    .Date(d => d.Name(n => n.LastUpdated))
                    .Boolean(b => b.Name(n => n.IsTracked))
                )
        );

        // トラッキング設定インデックスの作成
        await CreateIndexAsync<PlayerTrackingConfig>(_trackingConfigIndexName,
            settings => settings
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("default_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "trim")
                        )
                    )
                ),
            mappings => mappings
                .AutoMap()
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Puuid).Index(true))
                    .Boolean(b => b.Name(n => n.IsTracked))
                    .Boolean(b => b.Name(n => n.IsActive))
                    .Date(d => d.Name(n => n.LastTrackedAt))
                    .Date(d => d.Name(n => n.NextTrackingAt))
                    .Date(d => d.Name(n => n.CreatedAt))
                    .Date(d => d.Name(n => n.UpdatedAt))
                )
        );
    }
}