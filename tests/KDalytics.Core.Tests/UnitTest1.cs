using KDalytics.Core.Models.Player;
using KDalytics.Core.Models.Match;
using KDalytics.Core.Models.Performance;
using KDalytics.Core.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace KDalytics.Core.Tests;

public class PlayerEntityTests
{
    [Fact]
    public void PlayerEntity_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var puuid = "test-puuid";
        var gameName = "TestPlayer";
        var tagLine = "TEST";
        var region = "ap";
        var accountLevel = 100;
        var lastUpdated = DateTime.UtcNow;
        var isTracked = true;

        // Act
        var player = new PlayerEntity
        {
            Puuid = puuid,
            GameName = gameName,
            TagLine = tagLine,
            Region = region,
            AccountLevel = accountLevel,
            LastUpdated = lastUpdated,
            IsTracked = isTracked
        };

        // Assert
        Assert.Equal(puuid, player.Puuid);
        Assert.Equal(gameName, player.GameName);
        Assert.Equal(tagLine, player.TagLine);
        Assert.Equal(region, player.Region);
        Assert.Equal(accountLevel, player.AccountLevel);
        Assert.Equal(lastUpdated, player.LastUpdated);
        Assert.Equal(isTracked, player.IsTracked);
        Assert.Equal($"{gameName}#{tagLine}", player.GetRiotId());
    }

    [Fact]
    public void PlayerEntity_WithUpdatedTimestamp_ShouldCreateNewInstanceWithCurrentTime()
    {
        // Arrange
        var player = new PlayerEntity
        {
            Puuid = "test-puuid",
            LastUpdated = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var updatedPlayer = player.WithUpdatedTimestamp();

        // Assert
        Assert.NotEqual(player.LastUpdated, updatedPlayer.LastUpdated);
        Assert.True(updatedPlayer.LastUpdated > player.LastUpdated);
    }
}

public class PlayerRankEntityTests
{
    [Fact]
    public void PlayerRankEntity_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var puuid = "test-puuid";
        var currentTier = 24; // Immortal 3
        var currentTierPatched = "Immortal 3";
        var rankingInTier = 75;
        var mmr = 1800;
        var mmrChangeToLastGame = 15;
        var seasonId = "e7a1";
        var lastUpdated = DateTime.UtcNow;

        // Act
        var playerRank = new PlayerRankEntity
        {
            Puuid = puuid,
            CurrentTier = currentTier,
            CurrentTierPatched = currentTierPatched,
            RankingInTier = rankingInTier,
            Mmr = mmr,
            MmrChangeToLastGame = mmrChangeToLastGame,
            SeasonId = seasonId,
            LastUpdated = lastUpdated
        };

        // Assert
        Assert.Equal(puuid, playerRank.Puuid);
        Assert.Equal(currentTier, playerRank.CurrentTier);
        Assert.Equal(currentTierPatched, playerRank.CurrentTierPatched);
        Assert.Equal(rankingInTier, playerRank.RankingInTier);
        Assert.Equal(mmr, playerRank.Mmr);
        Assert.Equal(mmrChangeToLastGame, playerRank.MmrChangeToLastGame);
        Assert.Equal(seasonId, playerRank.SeasonId);
        Assert.Equal(lastUpdated, playerRank.LastUpdated);
    }

    [Fact]
    public void PlayerRankEntity_GetMmrChangeString_ShouldFormatCorrectly()
    {
        // Arrange
        var playerRankPositive = new PlayerRankEntity { MmrChangeToLastGame = 15 };
        var playerRankNegative = new PlayerRankEntity { MmrChangeToLastGame = -10 };

        // Act
        var positiveResult = playerRankPositive.GetMmrChangeString();
        var negativeResult = playerRankNegative.GetMmrChangeString();

        // Assert
        Assert.Equal("+15", positiveResult);
        Assert.Equal("-10", negativeResult);
    }
}

public class MatchEntityTests
{
    [Fact]
    public void MatchEntity_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var matchId = "test-match-id";
        var mapId = "Ascent";
        var mapName = "Ascent";
        var gameMode = "Competitive";
        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var gameLength = 2400000; // 40分
        var seasonId = "e7a1";
        var region = "ap";

        var teams = new List<TeamData>
        {
            new TeamData
            {
                TeamId = "Blue",
                HasWon = true,
                RoundsWon = 13
            },
            new TeamData
            {
                TeamId = "Red",
                HasWon = false,
                RoundsWon = 11
            }
        };

        var players = new List<PlayerMatchPerformance>
        {
            new PlayerMatchPerformance
            {
                Puuid = "player-1",
                MatchId = matchId,
                TeamId = "Blue",
                Kills = 20
            }
        };

        // Act
        var match = new MatchEntity
        {
            MatchId = matchId,
            MapId = mapId,
            MapName = mapName,
            GameMode = gameMode,
            StartTime = startTime,
            GameLength = gameLength,
            SeasonId = seasonId,
            Region = region,
            Teams = teams,
            Players = players
        };

        // Assert
        Assert.Equal(matchId, match.MatchId);
        Assert.Equal(mapId, match.MapId);
        Assert.Equal(mapName, match.MapName);
        Assert.Equal(gameMode, match.GameMode);
        Assert.Equal(startTime, match.StartTime);
        Assert.Equal(gameLength, match.GameLength);
        Assert.Equal(seasonId, match.SeasonId);
        Assert.Equal(region, match.Region);
        Assert.Equal(teams, match.Teams);
        Assert.Equal(players, match.Players);
    }

    [Fact]
    public void MatchEntity_GetScoreDisplay_ShouldReturnCorrectFormat()
    {
        // Arrange
        var match = new MatchEntity
        {
            Teams = new List<TeamData>
            {
                new TeamData { RoundsWon = 13 },
                new TeamData { RoundsWon = 7 }
            }
        };

        // Act
        var scoreDisplay = match.GetScoreDisplay();

        // Assert
        Assert.Equal("13-7", scoreDisplay);
    }
}

public class TeamDataTests
{
    [Fact]
    public void TeamData_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var teamId = "Blue";
        var hasWon = true;
        var roundsWon = 13;
        var roundResults = new List<RoundResult>
        {
            new RoundResult { RoundNum = 1, WinningTeam = "Blue" }
        };

        // Act
        var team = new TeamData
        {
            TeamId = teamId,
            HasWon = hasWon,
            RoundsWon = roundsWon,
            RoundResults = roundResults
        };

        // Assert
        Assert.Equal(teamId, team.TeamId);
        Assert.Equal(hasWon, team.HasWon);
        Assert.Equal(roundsWon, team.RoundsWon);
        Assert.Equal(roundResults, team.RoundResults);
    }

    [Fact]
    public void TeamData_GetResultDisplayText_ShouldReturnCorrectText()
    {
        // Arrange
        var winningTeam = new TeamData { HasWon = true };
        var losingTeam = new TeamData { HasWon = false };

        // Act
        var winText = winningTeam.GetResultDisplayText();
        var loseText = losingTeam.GetResultDisplayText();

        // Assert
        Assert.Equal("勝利", winText);
        Assert.Equal("敗北", loseText);
    }
}

public class PlayerMatchPerformanceTests
{
    [Fact]
    public void PlayerMatchPerformance_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var matchId = "test-match-id";
        var puuid = "test-puuid";
        var teamId = "Blue";
        var playerName = "TestPlayer";
        var tagLine = "TEST";
        var agentId = "agent-jett";
        var agentName = "Jett";
        var gameStartTimestamp = DateTime.UtcNow;
        var kills = 20;
        var deaths = 10;
        var assists = 5;
        var score = 250;
        var headshots = 12;
        var bodyshots = 40;
        var legshots = 3;
        var damageDealt = 3500;

        // Act
        var performance = new PlayerMatchPerformance
        {
            MatchId = matchId,
            Puuid = puuid,
            TeamId = teamId,
            PlayerName = playerName,
            TagLine = tagLine,
            AgentId = agentId,
            AgentName = agentName,
            GameStartTimestamp = gameStartTimestamp,
            Kills = kills,
            Deaths = deaths,
            Assists = assists,
            Score = score,
            Headshots = headshots,
            Bodyshots = bodyshots,
            Legshots = legshots,
            DamageDealt = damageDealt
        };

        // Assert
        Assert.Equal(matchId, performance.MatchId);
        Assert.Equal(puuid, performance.Puuid);
        Assert.Equal(teamId, performance.TeamId);
        Assert.Equal(playerName, performance.PlayerName);
        Assert.Equal(tagLine, performance.TagLine);
        Assert.Equal(agentId, performance.AgentId);
        Assert.Equal(agentName, performance.AgentName);
        Assert.Equal(gameStartTimestamp, performance.GameStartTimestamp);
        Assert.Equal(kills, performance.Kills);
        Assert.Equal(deaths, performance.Deaths);
        Assert.Equal(assists, performance.Assists);
        Assert.Equal(score, performance.Score);
        Assert.Equal(headshots, performance.Headshots);
        Assert.Equal(bodyshots, performance.Bodyshots);
        Assert.Equal(legshots, performance.Legshots);
        Assert.Equal(damageDealt, performance.DamageDealt);
    }

    [Fact]
    public void PlayerMatchPerformance_GetKdaRatio_ShouldCalculateCorrectly()
    {
        // Arrange
        var performance = new PlayerMatchPerformance
        {
            Kills = 20,
            Deaths = 10,
            Assists = 5
        };

        // Act
        var kda = performance.GetKdaRatio();

        // Assert
        Assert.Equal(2.5f, kda);
    }

    [Fact]
    public void PlayerMatchPerformance_GetKdaRatio_ShouldReturnKillsAndAssistsWhenDeathsIsZero()
    {
        // Arrange
        var performance = new PlayerMatchPerformance
        {
            Kills = 20,
            Deaths = 0,
            Assists = 5
        };

        // Act
        var kda = performance.GetKdaRatio();

        // Assert
        Assert.Equal(25.0f, kda);
    }

    [Fact]
    public void PlayerMatchPerformance_GetHeadshotPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var performance = new PlayerMatchPerformance
        {
            Headshots = 12,
            Bodyshots = 40,
            Legshots = 8
        };

        // Act
        var headshotPercentage = performance.GetHeadshotPercentage();

        // Assert
        Assert.Equal(20.0f, headshotPercentage);
    }

    [Fact]
    public void PlayerMatchPerformance_GetHeadshotPercentage_ShouldReturnZeroWhenNoShots()
    {
        // Arrange
        var performance = new PlayerMatchPerformance
        {
            Headshots = 0,
            Bodyshots = 0,
            Legshots = 0
        };

        // Act
        var headshotPercentage = performance.GetHeadshotPercentage();

        // Assert
        Assert.Equal(0.0f, headshotPercentage);
    }
}

public class RoundResultTests
{
    [Fact]
    public void RoundResult_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var roundNum = 1;
        var winningTeam = "Blue";
        var bombPlanted = true;
        var bombDefused = false;

        // Act
        var roundResult = new RoundResult
        {
            RoundNum = roundNum,
            WinningTeam = winningTeam,
            BombPlanted = bombPlanted,
            BombDefused = bombDefused
        };

        // Assert
        Assert.Equal(roundNum, roundResult.RoundNum);
        Assert.Equal(winningTeam, roundResult.WinningTeam);
        Assert.Equal(bombPlanted, roundResult.BombPlanted);
        Assert.Equal(bombDefused, roundResult.BombDefused);
    }
}
