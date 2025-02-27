using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KDalytics.Web.Services;

/// <summary>
/// API通信を行うための基本クライアント
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="httpClient">HTTPクライアント</param>
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// GETリクエストを送信
    /// </summary>
    /// <typeparam name="T">レスポンスの型</typeparam>
    /// <param name="url">URL</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>レスポンス</returns>
    public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"API GET エラー: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// POSTリクエストを送信
    /// </summary>
    /// <typeparam name="TRequest">リクエストの型</typeparam>
    /// <typeparam name="TResponse">レスポンスの型</typeparam>
    /// <param name="url">URL</param>
    /// <param name="request">リクエストデータ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>レスポンス</returns>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"API POST エラー: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// PUTリクエストを送信
    /// </summary>
    /// <typeparam name="TRequest">リクエストの型</typeparam>
    /// <typeparam name="TResponse">レスポンスの型</typeparam>
    /// <param name="url">URL</param>
    /// <param name="request">リクエストデータ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>レスポンス</returns>
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"API PUT エラー: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// DELETEリクエストを送信
    /// </summary>
    /// <typeparam name="T">レスポンスの型</typeparam>
    /// <param name="url">URL</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>レスポンス</returns>
    public async Task<T?> DeleteAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"API DELETE エラー: {ex.Message}");
            return default;
        }
    }
}