using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KDalytics.API.Controllers;
using KDalytics.API.Models.DTOs;
using KDalytics.Core.Interfaces.Repository;
using KDalytics.Core.Interfaces.Services;
using KDalytics.Core.Models.Player;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KDalytics.API.Tests;

public class PlayersControllerTests
{
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<IPlayerRankRepository> _mockPlayerRankRepository;
    private readonly Mock<IHenrikApiClient> _mockHenrikApiClient;
    private readonly Mock<IValorantDataMapper> _mockDataMapper;
    private readonly Mock<ILogger<PlayersController>> _mockLogger;
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockPlayerRankRepository = new Mock<IPlayerRankRepository>();
        _mockHenrikApiClient = new Mock<IHenrikApiClient>();
        _mockDataMapper = new Mock<IValorantDataMapper>();
        _mockLogger = new Mock<ILogger<PlayersController>>();
        _controller = new PlayersController(
            _mockPlayerRepository.Object,
            _mockPlayerRankRepository.Object,
            _mockHenrikApiClient.Object,
            _mockDataMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetPlayerByPuuid_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var puuid = "test-puuid";
        var player = new PlayerEntity
        {
            Puuid = puuid,
            GameName = "TestPlayer",
            TagLine = "TEST",
            Region = "ap",
            AccountLevel = 100,
            LastUpdated = DateTime.UtcNow
        };

        _mockPlayerRepository.Setup(repo => repo.GetPlayerByPuuidAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _controller.GetPlayerByPuuid(puuid, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<PlayerResponseDto>(okResult.Value);
        Assert.Equal(puuid, returnValue.Puuid);
        Assert.Equal(player.GameName, returnValue.GameName);
        Assert.Equal(player.TagLine, returnValue.TagLine);
    }

    [Fact]
    public async Task GetPlayerByPuuid_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var puuid = "invalid-puuid";
        _mockPlayerRepository.Setup(repo => repo.GetPlayerByPuuidAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerEntity)null);

        // Act
        var result = await _controller.GetPlayerByPuuid(puuid, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task SearchPlayer_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var searchRequest = new PlayerSearchRequestDto
        {
            Name = "TestPlayer",
            Tag = "TEST"
        };

        var player = new PlayerEntity
        {
            Puuid = "test-puuid",
            GameName = searchRequest.Name,
            TagLine = searchRequest.Tag,
            Region = "ap",
            AccountLevel = 100,
            LastUpdated = DateTime.UtcNow
        };

        _mockPlayerRepository.Setup(repo => repo.GetPlayerByNameTagAsync(
                searchRequest.Name, searchRequest.Tag, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _controller.SearchPlayer(searchRequest, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<PlayerResponseDto>(okResult.Value);
        Assert.Equal(player.Puuid, returnValue.Puuid);
        Assert.Equal(player.GameName, returnValue.GameName);
        Assert.Equal(player.TagLine, returnValue.TagLine);
    }

    [Fact]
    public async Task SearchPlayer_WithInvalidData_ReturnsNotFound()
    {
        // Arrange
        var searchRequest = new PlayerSearchRequestDto
        {
            Name = "InvalidPlayer",
            Tag = "INVALID"
        };

        _mockPlayerRepository.Setup(repo => repo.GetPlayerByNameTagAsync(
                searchRequest.Name, searchRequest.Tag, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerEntity)null);

        // API呼び出しも失敗するように設定
        _mockHenrikApiClient.Setup(client => client.GetPlayerInfoAsync(
                searchRequest.Name, searchRequest.Tag, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountInfoResponse { Status = 404 });

        // Act
        var result = await _controller.SearchPlayer(searchRequest, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTrackedPlayers_ReturnsOkResult()
    {
        // Arrange
        var players = new List<PlayerEntity>
        {
            new PlayerEntity
            {
                Puuid = "player-1",
                GameName = "Player1",
                TagLine = "TAG1",
                IsTracked = true
            },
            new PlayerEntity
            {
                Puuid = "player-2",
                GameName = "Player2",
                TagLine = "TAG2",
                IsTracked = true
            }
        };

        _mockPlayerRepository.Setup(repo => repo.GetTrackedPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        // Act
        var result = await _controller.GetTrackedPlayers(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<PlayerResponseDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
        Assert.Equal(players[0].Puuid, returnValue[0].Puuid);
        Assert.Equal(players[1].Puuid, returnValue[1].Puuid);
    }
}
