using System;
using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Interfaces.Services;
using KDalytics.Infrastructure.Configuration;
using KDalytics.Infrastructure.Repositories;
using KDalytics.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;

namespace KDalytics.Infrastructure.Extensions;

/// <summary>
/// サービスコレクション拡張メソッド
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// インフラストラクチャサービスの登録
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="configuration">構成</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 設定を登録
        services.Configure<ApiSettings>(configuration.GetSection("Api"));
        services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

        // APIクライアント登録
        services.AddHttpClient<IHenrikApiClient, HenrikApiClient>();
        services.AddHttpClient<ITrackerApiClient, TrackerApiClient>();
        services.AddSingleton<IValorantDataMapper, ValorantDataMapper>();

        // Elasticsearchリポジトリを登録
        services.AddElasticsearchRepositories(configuration);

        return services;
    }

    /// <summary>
    /// Henrik API クライアントの登録
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="configuration">構成</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddHenrikApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiSettings>(configuration.GetSection("Api"));
        services.AddHttpClient<IHenrikApiClient, HenrikApiClient>();
        return services;
    }

    /// <summary>
    /// Tracker Network API クライアントの登録
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="configuration">構成</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddTrackerApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiSettings>(configuration.GetSection("Api"));
        services.AddHttpClient<ITrackerApiClient, TrackerApiClient>();
        return services;
    }

    /// <summary>
    /// データマッパーの登録
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddValorantDataMapper(
        this IServiceCollection services)
    {
        services.AddSingleton<IValorantDataMapper, ValorantDataMapper>();
        return services;
    }

    /// <summary>
    /// Elasticsearchリポジトリの登録
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="configuration">構成</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddElasticsearchRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

        // Elasticsearchクライアント登録
        services.AddSingleton<IElasticClient>(provider =>
        {
            var settings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>()
                ?? new ElasticsearchSettings
                {
                    Urls = new[] { "http://localhost:9200" }
                };

            var logger = provider.GetRequiredService<ILogger<ElasticsearchRepositoryBase>>();

            try
            {
                var connectionSettings = new ConnectionSettings(new Uri(settings.Urls[0]))
                    .DisableDirectStreaming()
                    .EnableApiVersioningHeader();

                // 基本認証設定
                if (!string.IsNullOrEmpty(settings.Username) && !string.IsNullOrEmpty(settings.Password))
                {
                    connectionSettings = connectionSettings.BasicAuthentication(settings.Username, settings.Password);
                }
                // APIキー認証設定
                else if (!string.IsNullOrEmpty(settings.ApiKey))
                {
                    // APIキー認証メソッドが見つからないため、警告をログに出力
                    logger.LogWarning("APIキー認証が設定されていますが、現在のNESTバージョンではサポートされていません。基本認証を使用するか、NESTを更新してください。");

                    // 注: NESTのバージョンによって以下のいずれかの方法が使用できる可能性があります
                    // connectionSettings = connectionSettings.ApiKey(settings.ApiKey);
                    // connectionSettings = connectionSettings.Authentication(new ApiKeyAuthenticationCredentials(settings.ApiKey));
                    // connectionSettings = connectionSettings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
                }

                // 高度な設定
                connectionSettings = connectionSettings
                    .MaximumRetries(settings.MaxRetries)
                    .RequestTimeout(TimeSpan.FromSeconds(settings.TimeoutSeconds))
                    .DefaultMappingFor<object>(m => m.IndexName(settings.IndexPrefix + "matches"));

                return new ElasticClient(connectionSettings);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Elasticsearchクライアント初期化中にエラーが発生しました");
                throw;
            }
        });

        // リポジトリ登録
        services.AddSingleton<IPlayerRepository, ElasticsearchPlayerRepository>();
        services.AddSingleton<IPlayerRankRepository, ElasticsearchPlayerRankRepository>();
        services.AddSingleton<IMatchRepository, ElasticsearchMatchRepository>();
        services.AddSingleton<IPlayerPerformanceRepository, ElasticsearchPlayerPerformanceRepository>();

        return services;
    }
}