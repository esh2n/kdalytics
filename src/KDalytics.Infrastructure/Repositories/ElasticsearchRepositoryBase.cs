using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KDalytics.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace KDalytics.Infrastructure.Repositories;

/// <summary>
/// Elasticsearchリポジトリの基底クラス
/// </summary>
public abstract class ElasticsearchRepositoryBase
{
    protected readonly IElasticClient _elasticClient;
    protected readonly ElasticsearchSettings _settings;
    protected readonly ILogger _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="elasticClient">Elasticsearchクライアント</param>
    /// <param name="options">Elasticsearch設定</param>
    /// <param name="logger">ロガー</param>
    protected ElasticsearchRepositoryBase(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> options,
        ILogger logger)
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// インデックス名を取得
    /// </summary>
    /// <param name="baseIndexName">ベースインデックス名</param>
    /// <returns>完全なインデックス名</returns>
    protected string GetIndexName(string baseIndexName)
    {
        return $"{_settings.IndexPrefix}{baseIndexName}";
    }

    /// <summary>
    /// インデックスが存在するか確認
    /// </summary>
    /// <param name="indexName">インデックス名</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>存在する場合はtrue</returns>
    protected async Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        var existsResponse = await _elasticClient.Indices.ExistsAsync(indexName, ct: cancellationToken);
        return existsResponse.Exists;
    }

    /// <summary>
    /// インデックスを作成
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="configureSettings">インデックス設定のカスタマイズ</param>
    /// <param name="configureMappings">マッピング設定のカスタマイズ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>作成結果</returns>
    protected async Task<bool> CreateIndexAsync<T>(
        string indexName,
        Action<IndexSettingsDescriptor>? configureSettings = null,
        Action<TypeMappingDescriptor<T>>? configureMappings = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            // インデックスが既に存在するか確認
            if (await IndexExistsAsync(indexName, cancellationToken))
            {
                _logger.LogInformation("インデックスは既に存在します: {IndexName}", indexName);
                return true;
            }

            // 基本設定
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(indexName, c =>
            {
                c.Settings(s =>
                {
                    s.NumberOfShards(_settings.IndexSettings.NumberOfShards)
                     .NumberOfReplicas(_settings.IndexSettings.NumberOfReplicas)
                     .RefreshInterval(_settings.IndexSettings.RefreshInterval);

                    // カスタム設定を適用
                    configureSettings?.Invoke(s);

                    return s;
                });

                // マッピングを設定
                if (configureMappings != null)
                {
                    c.Map<T>(m =>
                    {
                        configureMappings(m);
                        return m;
                    });
                }
                else
                {
                    // デフォルトのマッピング
                    c.Map<T>(m => m.AutoMap());
                }

                return c;
            }, cancellationToken);

            if (!createIndexResponse.IsValid)
            {
                _logger.LogError("インデックス作成エラー: {IndexName}, {Error}", indexName, createIndexResponse.DebugInformation);
                return false;
            }

            _logger.LogInformation("インデックスを作成しました: {IndexName}", indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "インデックス作成中にエラーが発生しました: {IndexName}", indexName);
            return false;
        }
    }

    /// <summary>
    /// ドキュメントを取得
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="id">ドキュメントID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ドキュメント、存在しない場合はnull</returns>
    protected async Task<T?> GetDocumentAsync<T>(string indexName, string id, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var response = await _elasticClient.GetAsync<T>(id, g =>
                g.Index(indexName), cancellationToken);

            if (!response.IsValid)
            {
                if (response.ApiCall.HttpStatusCode == 404)
                {
                    // ドキュメントが見つからない場合
                    return null;
                }

                _logger.LogWarning("ドキュメント取得エラー: {Id}, {Error}", id, response.DebugInformation);
                return null;
            }

            return response.Source;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ドキュメント取得中にエラーが発生しました: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// ドキュメントを索引付け（挿入または更新）
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="document">ドキュメント</param>
    /// <param name="id">ドキュメントID</param>
    /// <param name="refresh">即時リフレッシュするか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>索引付けされたドキュメント</returns>
    protected async Task<T> IndexDocumentAsync<T>(
        string indexName,
        T document,
        string id,
        bool refresh = false,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            // インデックスが存在しない場合は作成
            if (!await IndexExistsAsync(indexName, cancellationToken))
            {
                await CreateIndexAsync<T>(indexName, null, null, cancellationToken);
            }

            var response = await _elasticClient.IndexAsync(document, i =>
                i.Index(indexName)
                 .Id(id)
                 .Refresh(refresh ? Elasticsearch.Net.Refresh.True : Elasticsearch.Net.Refresh.False),
                cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("ドキュメント索引付けエラー: {Id}, {Error}", id, response.DebugInformation);
                throw new Exception($"ドキュメント索引付けに失敗しました: {response.DebugInformation}");
            }

            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ドキュメント索引付け中にエラーが発生しました: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// 複数のドキュメントを一括で索引付け
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="documents">ドキュメントとIDのタプルリスト</param>
    /// <param name="refresh">即時リフレッシュするか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>成功したか</returns>
    protected async Task<bool> BulkIndexAsync<T>(
        string indexName,
        IEnumerable<(T Document, string Id)> documents,
        bool refresh = false,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            // インデックスが存在しない場合は作成
            if (!await IndexExistsAsync(indexName, cancellationToken))
            {
                await CreateIndexAsync<T>(indexName, null, null, cancellationToken);
            }

            var bulkRequest = new BulkRequest(indexName)
            {
                Operations = new List<IBulkOperation>(),
                Refresh = refresh ? Elasticsearch.Net.Refresh.True : Elasticsearch.Net.Refresh.False
            };

            foreach (var (document, id) in documents)
            {
                bulkRequest.Operations.Add(new BulkIndexOperation<T>(document)
                {
                    Id = id
                });
            }

            var response = await _elasticClient.BulkAsync(bulkRequest, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("バルク索引付けエラー: {Error}", response.DebugInformation);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "バルク索引付け中にエラーが発生しました");
            return false;
        }
    }

    /// <summary>
    /// ドキュメントを検索
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="configureSearch">検索条件の設定</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検索結果のリスト</returns>
    protected async Task<List<T>> SearchDocumentsAsync<T>(
        string indexName,
        Action<SearchDescriptor<T>> configureSearch,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var searchResponse = await _elasticClient.SearchAsync<T>(s =>
            {
                s.Index(indexName);
                configureSearch(s);
                return s;
            }, cancellationToken);

            if (!searchResponse.IsValid)
            {
                _logger.LogError("ドキュメント検索エラー: {Error}", searchResponse.DebugInformation);
                return new List<T>();
            }

            return searchResponse.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ドキュメント検索中にエラーが発生しました");
            return new List<T>();
        }
    }

    /// <summary>
    /// 集計クエリを実行
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="configureSearch">検索と集計の設定</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検索レスポンス</returns>
    protected async Task<ISearchResponse<T>> AggregateAsync<T>(
        string indexName,
        Action<SearchDescriptor<T>> configureSearch,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var searchResponse = await _elasticClient.SearchAsync<T>(s =>
            {
                s.Index(indexName);
                configureSearch(s);
                return s;
            }, cancellationToken);

            if (!searchResponse.IsValid)
            {
                _logger.LogError("集計クエリエラー: {Error}", searchResponse.DebugInformation);
                throw new Exception($"集計クエリに失敗しました: {searchResponse.DebugInformation}");
            }

            return searchResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "集計クエリ実行中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// クエリを実行して最初の結果を取得
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="configureSearch">検索条件の設定</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>最初の結果、存在しない場合はnull</returns>
    protected async Task<T?> GetFirstOrDefaultAsync<T>(
        string indexName,
        Action<SearchDescriptor<T>> configureSearch,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var searchResponse = await _elasticClient.SearchAsync<T>(s =>
            {
                s.Index(indexName);
                configureSearch(s);
                s.Size(1); // 最初の1件のみ取得
                return s;
            }, cancellationToken);

            if (!searchResponse.IsValid)
            {
                _logger.LogError("ドキュメント検索エラー: {Error}", searchResponse.DebugInformation);
                return null;
            }

            return searchResponse.Documents.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ドキュメント検索中にエラーが発生しました");
            return null;
        }
    }

    /// <summary>
    /// カウントクエリを実行
    /// </summary>
    /// <typeparam name="T">ドキュメント型</typeparam>
    /// <param name="indexName">インデックス名</param>
    /// <param name="configureCount">クエリ条件の設定</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>件数</returns>
    protected async Task<long> CountAsync<T>(
        string indexName,
        Action<CountDescriptor<T>> configureCount,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var countResponse = await _elasticClient.CountAsync<T>(c =>
            {
                c.Index(indexName);
                configureCount(c);
                return c;
            }, cancellationToken);

            if (!countResponse.IsValid)
            {
                _logger.LogError("カウントクエリエラー: {Error}", countResponse.DebugInformation);
                return 0;
            }

            return countResponse.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カウントクエリ実行中にエラーが発生しました");
            return 0;
        }
    }
}