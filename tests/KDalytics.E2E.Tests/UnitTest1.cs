using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using KDalytics.API.Models.DTOs;
using Xunit;

namespace KDalytics.E2E.Tests;

public class ApiEndpointsTests
{
    [Fact(Skip = "Elasticsearchが必要なため、CI環境では実行しない")]
    public async Task GetPlayerByPuuid_ReturnsNotFound_WhenPlayerDoesNotExist()
    {
        // このテストはElasticsearchが必要なため、実際の実行時にはSkipされます
        // 実際の実装では、WebApplicationFactoryを使用してテスト用のHTTPクライアントを作成します

        // Arrange
        var puuid = "non-existent-puuid";
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5167");

        // Act
        var response = await client.GetAsync($"/api/players/{puuid}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(Skip = "Elasticsearchが必要なため、CI環境では実行しない")]
    public async Task SearchPlayer_ReturnsBadRequest_WhenNameOrTagIsEmpty()
    {
        // このテストはElasticsearchが必要なため、実際の実行時にはSkipされます
        // 実際の実装では、WebApplicationFactoryを使用してテスト用のHTTPクライアントを作成します

        // Arrange
        var searchRequest = new PlayerSearchRequestDto
        {
            Name = "",
            Tag = "TEST"
        };
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5167");

        // Act
        var response = await client.PostAsJsonAsync("/api/players/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "Elasticsearchが必要なため、CI環境では実行しない")]
    public async Task GetTrackedPlayers_ReturnsOkResult()
    {
        // このテストはElasticsearchが必要なため、実際の実行時にはSkipされます
        // 実際の実装では、WebApplicationFactoryを使用してテスト用のHTTPクライアントを作成します

        // Arrange
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5167");

        // Act
        var response = await client.GetAsync("/api/players/tracked");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
