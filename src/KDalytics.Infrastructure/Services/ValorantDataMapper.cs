using System;
using System.Collections.Generic;
using System.Linq;
using KDalytics.Core.Enums;
using KDalytics.Core.Interfaces.Services;
using KDalytics.Core.Models.Match;
using KDalytics.Core.Models.Performance;
using KDalytics.Core.Models.Player;
using Microsoft.Extensions.Logging;
using static KDalytics.Core.Interfaces.Services.IHenrikApiClient;
using static KDalytics.Core.Interfaces.Services.ITrackerApiClient;

namespace KDalytics.Infrastructure.Services;

/// <summary>
/// APIデータをドメインモデルにマッピングする実装
/// </summary>
public class ValorantDataMapper : IValorantDataMapper
{
    private readonly ILogger<ValorantDataMapper> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    public ValorantDataMapper(ILogger<ValorantDataMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Henrik API マッピング

    /// <inheritdoc />
    public PlayerEntity MapToPlayerEntity(AccountInfoResponse playerInfo)
    {
        if (playerInfo?.Data == null)
        {
            throw new ArgumentException("プレイヤー情報のデータが不足しています", nameof(playerInfo));
        }

        return new PlayerEntity
        {
            Puuid = playerInfo.Data.Puuid,
            GameName = playerInfo.Data.Name,
            TagLine = playerInfo.Data.Tag,
            Region = playerInfo.Data.Region,
            AccountLevel = playerInfo.Data.AccountLevel,
            LastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(playerInfo.Data.LastUpdate).UtcDateTime,
            IsTracked = true // デフォルトでトラッキング対象
        };
    }

    /// <inheritdoc />
    public PlayerRankEntity MapToPlayerRankEntity(MmrInfoResponse mmrInfo)
    {
        if (mmrInfo?.Data == null)
        {
            throw new ArgumentException("ランク情報のデータが不足しています", nameof(mmrInfo));
        }

        return new PlayerRankEntity
        {
            Puuid = mmrInfo.Data.Puuid,
            CurrentTier = mmrInfo.Data.CurrentTier,
            CurrentTierPatched = mmrInfo.Data.CurrentTierPatched,
            RankingInTier = mmrInfo.Data.RankingInTier,
            MmrChangeToLastGame = mmrInfo.Data.MmrChangeToLastGame,
            SeasonId = GetCurrentSeasonId(), // 現在のシーズンを特定する処理を追加
            LastUpdated = DateTime.UtcNow // API呼び出し時の時刻を記録
        };
    }

    /// <inheritdoc />
    public MatchEntity MapToMatchEntity(MatchDetailsResponse matchDetails)
    {
        if (matchDetails?.Data?.Metadata == null)
        {
            throw new ArgumentException("試合詳細のデータが不足しています", nameof(matchDetails));
        }

        var metadata = matchDetails.Data.Metadata;
        var gameModeString = metadata.GameMode;

        return new MatchEntity
        {
            MatchId = metadata.MatchId,
            MapId = metadata.MapId,
            MapName = metadata.MapName,
            GameMode = gameModeString,
            GameLength = metadata.GameLength,
            StartTime = DateTimeOffset.FromUnixTimeMilliseconds(metadata.GameStart),
            SeasonId = metadata.SeasonId,
            Region = metadata.Region
        };
    }

    /// <inheritdoc />
    public List<PlayerMatchPerformance> MapToPlayerPerformances(MatchDetailsResponse matchDetails)
    {
        if (matchDetails?.Data?.Metadata == null || matchDetails.Data.Players == null)
        {
            throw new ArgumentException("試合詳細のデータが不足しています", nameof(matchDetails));
        }

        var performances = new List<PlayerMatchPerformance>();

        foreach (var player in matchDetails.Data.Players)
        {
            try
            {
                var gameStartTime = DateTimeOffset.FromUnixTimeMilliseconds(matchDetails.Data.Metadata.GameStart).UtcDateTime;

                var performance = new PlayerMatchPerformance
                {
                    Puuid = player.Puuid,
                    MatchId = matchDetails.Data.Metadata.MatchId,
                    PlayerName = player.Name,
                    TagLine = player.Tag,
                    TeamId = player.Team,
                    AgentName = player.Character,
                    Kills = player.Kills,
                    Deaths = player.Deaths,
                    Assists = player.Assists,
                    Score = player.Score,
                    DamageDealt = player.Damage,
                    Headshots = player.Headshots,
                    Bodyshots = player.Bodyshots,
                    Legshots = player.Legshots
                };

                performances.Add(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "プレイヤーパフォーマンスのマッピング中にエラーが発生しました: {Puuid}", player.Puuid);
                // エラーをスキップして次のプレイヤーに進む
            }
        }

        return performances;
    }

    /// <inheritdoc />
    public List<TeamData> MapToTeamData(MatchDetailsResponse matchDetails)
    {
        if (matchDetails?.Data?.Teams == null)
        {
            return new List<TeamData>();
        }

        var teams = new List<TeamData>();

        foreach (var team in matchDetails.Data.Teams)
        {
            teams.Add(new TeamData
            {
                TeamId = team.TeamId,
                HasWon = team.WonRounds,
                RoundsWon = team.RoundsWon
            });
        }

        return teams;
    }

    /// <inheritdoc />
    public List<RoundResult> MapToRoundResults(MatchDetailsResponse matchDetails)
    {
        if (matchDetails?.Data?.Rounds == null)
        {
            return new List<RoundResult>();
        }

        var rounds = new List<RoundResult>();

        foreach (var round in matchDetails.Data.Rounds)
        {
            rounds.Add(new RoundResult
            {
                RoundNum = round.RoundNumber,
                WinningTeam = round.WinningTeam,
                BombPlanted = round.BombPlanted,
                BombDefused = round.BombDefused
            });
        }

        return rounds;
    }

    #endregion

    #region Tracker Network API マッピング

    /// <inheritdoc />
    public PlayerEntity? MapToPlayerEntityFromTracker(TrackerProfileResponse profile)
    {
        if (profile?.Data?.PlatformInfo == null || profile.Data.UserInfo == null)
        {
            _logger.LogWarning("Tracker Networkからのプレイヤー情報が不足しています");
            return null;
        }

        try
        {
            // PlatformUserIdentifierからPUUIDを取得
            string puuid = profile.Data.PlatformInfo.PlatformUserIdentifier;

            // NameとTagの分離（形式: "Name#Tag"）
            string fullName = profile.Data.PlatformInfo.PlatformUserHandle;
            string[] nameParts = fullName.Split('#');

            string name = nameParts.Length > 0 ? nameParts[0] : fullName;
            string tag = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            return new PlayerEntity
            {
                Puuid = puuid,
                GameName = name,
                TagLine = tag,
                // 他の情報はHenrik APIから取得する方が正確なため、最小限の情報のみマッピング
                AccountLevel = 0, // Trackerからは取得できない
                LastUpdated = DateTime.UtcNow,
                IsTracked = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trackerからのプレイヤーエンティティのマッピング中にエラーが発生しました");
            return null;
        }
    }

    /// <inheritdoc />
    public PlayerRankEntity? MapToPlayerRankEntityFromTracker(TrackerProfileResponse profile)
    {
        if (profile?.Data?.UserInfo?.CurrentRank == null || profile.Data.PlatformInfo == null)
        {
            _logger.LogWarning("Tracker Networkからのランク情報が不足しています");
            return null;
        }

        try
        {
            var currentRank = profile.Data.UserInfo.CurrentRank;
            string puuid = profile.Data.PlatformInfo.PlatformUserIdentifier;

            // Rankの解析
            var rankInfo = ParseRankFromString(currentRank.RankName);

            return new PlayerRankEntity
            {
                Puuid = puuid,
                CurrentTier = (int)rankInfo.rank,
                CurrentTierPatched = currentRank.RankName,
                RankingInTier = currentRank.RankScore,
                SeasonId = GetCurrentSeasonId(), // 現在のシーズンを特定
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trackerからのランクエンティティのマッピング中にエラーが発生しました");
            return null;
        }
    }

    /// <inheritdoc />
    public List<string> ExtractRecentMatchIdsFromTracker(TrackerProfileResponse profile)
    {
        if (profile?.Data?.Matches == null || !profile.Data.Matches.Any())
        {
            _logger.LogWarning("Tracker Networkからの試合情報が見つかりません");
            return new List<string>();
        }

        try
        {
            return profile.Data.Matches
                .Where(m => !string.IsNullOrEmpty(m.Id))
                .Select(m => m.Id)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trackerからの試合IDの抽出中にエラーが発生しました");
            return new List<string>();
        }
    }

    /// <inheritdoc />
    public PerformanceStats? ExtractPerformanceStatsFromTracker(TrackerProfileResponse profile)
    {
        if (profile?.Data?.Stats == null || !profile.Data.Stats.Any())
        {
            _logger.LogWarning("Tracker Networkからの統計情報が見つかりません");
            return null;
        }

        try
        {
            // 統計情報の値を抽出
            float kdRatio = 0;
            float winRate = 0;
            int totalKills = 0;
            int totalDeaths = 0;
            int totalAssists = 0;
            float headshotPercentage = 0;
            int matchesPlayed = 0;
            int matchesWon = 0;

            // 主要な統計情報を抽出
            foreach (var stat in profile.Data.Stats)
            {
                switch (stat.Name.ToLowerInvariant())
                {
                    case "kd":
                    case "k/d":
                    case "kdratio":
                        kdRatio = (float)stat.Value;
                        break;
                    case "winpercentage":
                    case "winrate":
                        winRate = (float)stat.Value / 100; // パーセント値を0-1の範囲に変換
                        break;
                    case "kills":
                        totalKills = (int)stat.Value;
                        break;
                    case "deaths":
                        totalDeaths = (int)stat.Value;
                        break;
                    case "assists":
                        totalAssists = (int)stat.Value;
                        break;
                    case "headshots":
                    case "headshotpercentage":
                        headshotPercentage = (float)stat.Value / 100; // パーセント値を0-1の範囲に変換
                        break;
                    case "matchesplayed":
                    case "matches":
                        matchesPlayed = (int)stat.Value;
                        break;
                    case "matcheswon":
                    case "wins":
                        matchesWon = (int)stat.Value;
                        break;
                }
            }

            // 基本的な計算
            if (totalDeaths > 0 && kdRatio <= 0)
            {
                kdRatio = (float)totalKills / totalDeaths;
            }

            if (matchesPlayed > 0 && winRate <= 0)
            {
                winRate = (float)matchesWon / matchesPlayed;
            }

            // オブジェクト初期化子を使用して統計情報を作成
            return new PerformanceStats
            {
                Puuid = profile.Data.PlatformInfo?.PlatformUserIdentifier ?? string.Empty,
                StartDate = DateTime.UtcNow.AddDays(-90), // デフォルトで90日前からの統計とする
                EndDate = DateTime.UtcNow,
                KdRatio = kdRatio,
                WinRate = winRate,
                TotalKills = totalKills,
                TotalDeaths = totalDeaths,
                TotalAssists = totalAssists,
                HeadshotPercentage = headshotPercentage,
                MatchesPlayed = matchesPlayed,
                MatchesWon = matchesWon,
                Losses = matchesPlayed - matchesWon
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trackerからのパフォーマンス統計の抽出中にエラーが発生しました");
            return null;
        }
    }

    #endregion

    #region エンティティ変換・集計

    /// <inheritdoc />
    public PerformanceStats CalculatePerformanceStats(
        List<MatchEntity> matches,
        List<PlayerMatchPerformance> performances,
        string puuid,
        DateTime from,
        DateTime to)
    {
        // 指定期間内に絞り込む
        var filteredMatches = matches
            .Where(m => m.StartTime >= from && m.StartTime <= to)
            .ToList();

        var filteredPerformances = performances
            .Where(p => p.Puuid == puuid && p.MatchId != null &&
                 matches.Any(m => m.MatchId == p.MatchId && m.StartTime >= from && m.StartTime <= to))
            .ToList();

        // 試合数が0の場合は基本情報のみ返す
        if (filteredPerformances.Count == 0)
        {
            _logger.LogWarning("指定期間内のパフォーマンスデータが見つかりません: {Puuid}, {From}-{To}", puuid, from, to);
            return new PerformanceStats
            {
                Puuid = puuid,
                StartDate = from,
                EndDate = to,
                MatchesPlayed = 0
            };
        }

        // 勝利数をカウント
        int matchesWon = 0;
        foreach (var match in filteredMatches)
        {
            // この試合でのプレイヤーのパフォーマンスを検索
            var performance = filteredPerformances.FirstOrDefault(p => p.MatchId == match.MatchId);
            if (performance == null) continue;

            // チームデータがある場合のみ勝敗をカウント
            if (match.Teams != null && match.Teams.Any())
            {
                var team = match.Teams.FirstOrDefault(t => t.TeamId == performance.TeamId);
                if (team != null && team.HasWon)
                {
                    matchesWon++;
                }
            }
        }

        // 合計値の計算
        int totalKills = filteredPerformances.Sum(p => p.Kills);
        int totalDeaths = filteredPerformances.Sum(p => p.Deaths);
        int totalAssists = filteredPerformances.Sum(p => p.Assists);
        int totalScore = filteredPerformances.Sum(p => p.Score);
        int totalDamage = filteredPerformances.Sum(p => p.DamageDealt);
        int totalHeadshots = filteredPerformances.Sum(p => p.Headshots);
        int totalBodyshots = filteredPerformances.Sum(p => p.Bodyshots);
        int totalLegshots = filteredPerformances.Sum(p => p.Legshots);

        // 平均値の計算
        int matchCount = filteredPerformances.Count;
        float averageKills = (float)totalKills / matchCount;
        float averageDeaths = (float)totalDeaths / matchCount;
        float averageAssists = (float)totalAssists / matchCount;
        float averageScore = (float)totalScore / matchCount;
        float averageDamage = (float)totalDamage / matchCount;
        float averageHeadshots = (float)totalHeadshots / matchCount;
        float averageBodyshots = (float)totalBodyshots / matchCount;
        float averageLegshots = (float)totalLegshots / matchCount;

        // K/D比とHSPの計算
        float kdRatio = totalDeaths > 0 ? (float)totalKills / totalDeaths : totalKills;

        int totalShots = totalHeadshots + totalBodyshots + totalLegshots;
        float headshotPercentage = totalShots > 0 ? (float)totalHeadshots / totalShots : 0;

        // 勝率の計算
        float winRate = (float)matchesWon / matchCount;

        // 最も使用したエージェントの特定
        string mostPlayedAgent = string.Empty;
        if (filteredPerformances.Any())
        {
            var agentCounts = filteredPerformances
                .GroupBy(p => p.AgentName)
                .Select(g => new { Agent = g.Key, Count = g.Count() })
                .OrderByDescending(a => a.Count);

            mostPlayedAgent = agentCounts.First().Agent;
        }

        // オブジェクト初期化子を使用して統計情報を作成
        return new PerformanceStats
        {
            Puuid = puuid,
            StartDate = from,
            EndDate = to,
            MatchesPlayed = matchCount,
            MatchesWon = matchesWon,
            Losses = matchCount - matchesWon,
            WinRate = winRate,
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
            KdRatio = kdRatio,
            HeadshotPercentage = headshotPercentage,
            MostPlayedAgent = mostPlayedAgent
        };
    }

    /// <inheritdoc />
    public List<(string Puuid, string PlayerName, string TagLine, float KdaRatio, int Kills, int Deaths, int Assists, int GamesPlayed)>
        CreateKdaRanking(
            List<PlayerMatchPerformance> performances,
            Dictionary<string, string> playerNames,
            Dictionary<string, string> playerTags)
    {
        var ranking = performances
            .GroupBy(p => p.Puuid)
            .Select(g =>
            {
                string puuid = g.Key;
                string playerName = playerNames.TryGetValue(puuid, out var name) ? name : g.First().PlayerName;
                string tagLine = playerTags.TryGetValue(puuid, out var tag) ? tag : g.First().TagLine;

                int kills = g.Sum(p => p.Kills);
                int deaths = g.Sum(p => p.Deaths);
                int assists = g.Sum(p => p.Assists);
                int gamesPlayed = g.Count();

                // KDA計算 (K+A)/D
                float kdaRatio = deaths > 0
                    ? (float)(kills + assists) / deaths
                    : kills + assists; // 0デスの場合

                return (
                    Puuid: puuid,
                    PlayerName: playerName,
                    TagLine: tagLine,
                    KdaRatio: kdaRatio,
                    Kills: kills,
                    Deaths: deaths,
                    Assists: assists,
                    GamesPlayed: gamesPlayed
                );
            })
            .OrderByDescending(r => r.KdaRatio)
            .ToList();

        return ranking;
    }

    /// <inheritdoc />
    public Dictionary<string, AgentPerformance> CalculateAgentPerformance(
        List<PlayerMatchPerformance> performances,
        List<MatchEntity> matches,
        string puuid)
    {
        // プレイヤーのパフォーマンスのみフィルタリング
        var playerPerformances = performances
            .Where(p => p.Puuid == puuid)
            .ToList();

        // エージェント別にグループ化
        var agentGroups = playerPerformances
            .GroupBy(p => p.AgentName)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var agentPerformances = g.ToList();
                    var matchIds = agentPerformances.Select(p => p.MatchId).ToHashSet();

                    // エージェント別の試合データ
                    var agentMatches = matches
                        .Where(m => matchIds.Contains(m.MatchId))
                        .ToList();

                    // 勝利数のカウント
                    int wins = 0;
                    foreach (var perf in agentPerformances)
                    {
                        var match = agentMatches.FirstOrDefault(m => m.MatchId == perf.MatchId);
                        if (match?.Teams != null && match.Teams.Any())
                        {
                            var team = match.Teams.FirstOrDefault(t => t.TeamId == perf.TeamId);
                            if (team != null && team.HasWon)
                            {
                                wins++;
                            }
                        }
                    }

                    // 統計情報の計算
                    int gamesPlayed = agentPerformances.Count;
                    int kills = agentPerformances.Sum(p => p.Kills);
                    int deaths = agentPerformances.Sum(p => p.Deaths);
                    int assists = agentPerformances.Sum(p => p.Assists);

                    return new AgentPerformance
                    {
                        AgentName = g.Key,
                        GamesPlayed = gamesPlayed,
                        GamesWon = wins,
                        WinRate = gamesPlayed > 0 ? (float)wins / gamesPlayed : 0,
                        TotalKills = kills,
                        TotalDeaths = deaths,
                        TotalAssists = assists,
                        AverageKills = gamesPlayed > 0 ? (float)kills / gamesPlayed : 0,
                        AverageDeaths = gamesPlayed > 0 ? (float)deaths / gamesPlayed : 0,
                        AverageAssists = gamesPlayed > 0 ? (float)assists / gamesPlayed : 0,
                        KdRatio = deaths > 0 ? (float)kills / deaths : kills,
                        TotalDamage = agentPerformances.Sum(p => p.DamageDealt),
                        TotalHeadshots = agentPerformances.Sum(p => p.Headshots),
                        HeadshotPercentage = CalculateHeadshotPercentage(agentPerformances)
                    };
                });

        return agentGroups;
    }

    /// <inheritdoc />
    public Dictionary<string, MapPerformance> CalculateMapPerformance(
        List<PlayerMatchPerformance> performances,
        List<MatchEntity> matches,
        string puuid)
    {
        // プレイヤーのパフォーマンスのみフィルタリング
        var playerPerformances = performances
            .Where(p => p.Puuid == puuid)
            .ToList();

        // マップ別の統計を計算するためにマッチIDで試合をルックアップできるように
        var matchLookup = matches.ToDictionary(m => m.MatchId);

        // パフォーマンスをマップ別にグループ化
        var mapGroups = playerPerformances
            .Where(p => matchLookup.ContainsKey(p.MatchId)) // マッチデータがある場合のみ
            .GroupBy(p => matchLookup[p.MatchId].MapName)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var mapPerformances = g.ToList();

                    // マップ別の試合情報
                    var mapMatches = mapPerformances
                        .Select(p => matchLookup[p.MatchId])
                        .ToList();

                    // 勝利数のカウント
                    int wins = 0;
                    foreach (var perf in mapPerformances)
                    {
                        var match = matchLookup[perf.MatchId];
                        if (match.Teams != null && match.Teams.Any())
                        {
                            var team = match.Teams.FirstOrDefault(t => t.TeamId == perf.TeamId);
                            if (team != null && team.HasWon)
                            {
                                wins++;
                            }
                        }
                    }

                    // 統計情報の計算
                    int gamesPlayed = mapPerformances.Count;
                    int kills = mapPerformances.Sum(p => p.Kills);
                    int deaths = mapPerformances.Sum(p => p.Deaths);
                    int assists = mapPerformances.Sum(p => p.Assists);

                    return new MapPerformance
                    {
                        MapName = g.Key,
                        MapId = mapMatches.First().MapId,
                        GamesPlayed = gamesPlayed,
                        GamesWon = wins,
                        WinRate = gamesPlayed > 0 ? (float)wins / gamesPlayed : 0,
                        TotalKills = kills,
                        TotalDeaths = deaths,
                        TotalAssists = assists,
                        AverageKills = gamesPlayed > 0 ? (float)kills / gamesPlayed : 0,
                        AverageDeaths = gamesPlayed > 0 ? (float)deaths / gamesPlayed : 0,
                        AverageAssists = gamesPlayed > 0 ? (float)assists / gamesPlayed : 0,
                        KdRatio = deaths > 0 ? (float)kills / deaths : kills,
                        TotalDamage = mapPerformances.Sum(p => p.DamageDealt),
                        TotalHeadshots = mapPerformances.Sum(p => p.Headshots),
                        HeadshotPercentage = CalculateHeadshotPercentage(mapPerformances)
                    };
                });

        return mapGroups;
    }

    #endregion

    #region ヘルパーメソッド

    /// <summary>
    /// ランク名からRankとティアを解析する
    /// </summary>
    /// <param name="rankString">ランク文字列（例: "Gold 2"）</param>
    /// <returns>Rankとティア</returns>
    private (Rank rank, int tier) ParseRankFromString(string rankString)
    {
        if (string.IsNullOrEmpty(rankString))
        {
            return (Rank.Unranked, 0);
        }

        // RankExtensionsのFromNameメソッドを使用
        Rank rank = RankExtensions.FromName(rankString);

        // ティアを抽出
        int tier = 0;
        var parts = rankString.Trim().Split(' ');
        if (parts.Length > 1 && int.TryParse(parts[1], out int t))
        {
            tier = t;
        }

        return (rank, tier);
    }

    /// <summary>
    /// ティアIDからRankを解析する
    /// </summary>
    /// <param name="tierId">ティアID</param>
    /// <returns>Rank</returns>
    private Rank ParseRankFromTierId(int tierId)
    {
        // RankExtensionsのFromIdメソッドを使用
        return RankExtensions.FromId(tierId);
    }

    /// <summary>
    /// ゲームモード文字列からGameModeを解析する
    /// </summary>
    /// <param name="gameModeString">ゲームモード文字列</param>
    /// <returns>GameMode</returns>
    private GameMode ParseGameMode(string gameModeString)
    {
        if (string.IsNullOrEmpty(gameModeString))
        {
            return GameMode.Unrated;
        }

        return gameModeString.ToLowerInvariant() switch
        {
            "competitive" => GameMode.Competitive,
            "unrated" => GameMode.Unrated,
            "spikerush" => GameMode.SpikeRush,
            "deathmatch" => GameMode.Deathmatch,
            "escalation" => GameMode.Escalation,
            "replication" => GameMode.Replication,
            "swiftplay" => GameMode.SwiftPlay,
            "teamdeathmatch" => GameMode.TeamDeathmatch,
            "custom" => GameMode.Custom,
            "premier" => GameMode.Premier,
            "snowball" => GameMode.Snowball,
            _ => GameMode.Unknown
        };
    }

    /// <summary>
    /// 現在のシーズンIDを取得する
    /// </summary>
    /// <returns>シーズンID</returns>
    private string GetCurrentSeasonId()
    {
        // 通常は実際のRiot APIからシーズン情報を取得するべきですが
        // 簡略化のため、現在のエピソードとアクトを推定
        // 本来はこれをキャッシュしたり、外部設定から取得するべき
        // 例: "e8a1" = エピソード8アクト1

        // このロジックは定期的に更新する必要があります
        return "e8a1"; // 例: エピソード8・アクト1とする（実際の状況に応じて変更要）
    }

    /// <summary>
    /// ヘッドショット率を計算する
    /// </summary>
    /// <param name="performances">パフォーマンスリスト</param>
    /// <returns>ヘッドショット率（0-1）</returns>
    private float CalculateHeadshotPercentage(List<PlayerMatchPerformance> performances)
    {
        int headshots = performances.Sum(p => p.Headshots);
        int bodyshots = performances.Sum(p => p.Bodyshots);
        int legshots = performances.Sum(p => p.Legshots);

        int totalShots = headshots + bodyshots + legshots;
        return totalShots > 0 ? (float)headshots / totalShots : 0;
    }

    #endregion
}