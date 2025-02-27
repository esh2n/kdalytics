using Discord;
using Discord.Commands;
using KDalytics.Web.Models;
using KDalytics.Web.Services;

namespace KDalytics.Discord.Modules;

[Group("match")]
[Summary("試合関連のコマンド")]
public class MatchModule : ModuleBase<SocketCommandContext>
{
    private readonly PlayerApiClient _playerApi;
    private readonly MatchApiClient _matchApi;
    private readonly PerformanceApiClient _performanceApi;

    public MatchModule(PlayerApiClient playerApi, MatchApiClient matchApi, PerformanceApiClient performanceApi)
    {
        _playerApi = playerApi;
        _matchApi = matchApi;
        _performanceApi = performanceApi;
    }

    [Command("recent")]
    [Summary("プレイヤーの最近の試合を表示します")]
    public async Task GetRecentMatchesAsync(string name, string tag, [Remainder] int count = 3)
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            var player = await _playerApi.SearchPlayerAsync(name, tag);
            if (player == null)
            {
                await ReplyAsync($"プレイヤー '{name}#{tag}' が見つかりませんでした。");
                return;
            }

            var matches = await _matchApi.GetPlayerRecentMatchesAsync(player.Puuid, Math.Min(count, 5));
            if (matches == null || matches.Count == 0)
            {
                await ReplyAsync($"プレイヤー '{player.DisplayName}' の最近の試合が見つかりませんでした。");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"{player.DisplayName} の最近の試合")
                .WithColor(Color.Blue)
                .WithFooter(footer => footer.Text = $"PUUID: {player.Puuid}")
                .WithCurrentTimestamp();

            foreach (var match in matches)
            {
                var performance = match.Players.FirstOrDefault(p => p.Puuid == player.Puuid);
                var team = match.Teams.FirstOrDefault(t => t.TeamId == performance?.TeamId);
                var isWin = team?.HasWon ?? false;

                embed.AddField(
                    $"{match.MapName} ({match.GameMode}) - {(isWin ? "勝利" : "敗北")}",
                    $"日時: {match.FormattedStartTime}\n" +
                    $"試合時間: {match.FormattedGameLength}\n" +
                    (performance != null ? $"エージェント: {performance.AgentName}\n" +
                    $"KDA: {performance.KdaDisplay}\n" +
                    $"スコア: {performance.Score}" : "パフォーマンス情報なし")
                );
            }

            await ReplyAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("detail")]
    [Summary("試合の詳細情報を表示します")]
    public async Task GetMatchDetailAsync(string matchId)
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            var match = await _matchApi.GetMatchByIdAsync(matchId);
            if (match == null)
            {
                await ReplyAsync($"試合ID '{matchId}' が見つかりませんでした。");
                return;
            }

            var team1 = match.Teams.FirstOrDefault(t => t.TeamId == "Blue");
            var team2 = match.Teams.FirstOrDefault(t => t.TeamId == "Red");
            var team1Score = team1?.RoundsWon ?? 0;
            var team2Score = team2?.RoundsWon ?? 0;

            var embed = new EmbedBuilder()
                .WithTitle($"試合詳細: {match.MapName}")
                .WithDescription($"ゲームモード: {match.GameMode}\n" +
                                $"日時: {match.FormattedStartTime}\n" +
                                $"試合時間: {match.FormattedGameLength}\n" +
                                $"スコア: Blue {team1Score} - {team2Score} Red")
                .WithColor(Color.DarkGreen)
                .WithFooter(footer => footer.Text = $"MatchID: {match.MatchId}")
                .WithCurrentTimestamp();

            // Blue Team
            var blueTeamPlayers = match.Players.Where(p => p.TeamId == "Blue").OrderByDescending(p => p.Score).ToList();
            if (blueTeamPlayers.Any())
            {
                var blueTeamField = "```\n";
                blueTeamField += "プレイヤー名          | エージェント | スコア | KDA\n";
                blueTeamField += "-------------------|------------|-------|--------\n";

                foreach (var player in blueTeamPlayers)
                {
                    var playerName = player.DisplayName.Length > 18 ? player.DisplayName.Substring(0, 15) + "..." : player.DisplayName.PadRight(18);
                    blueTeamField += $"{playerName} | {player.AgentName.PadRight(10)} | {player.Score.ToString().PadRight(5)} | {player.KdaDisplay}\n";
                }

                blueTeamField += "```";
                embed.AddField($"Blue Team {(team1?.HasWon == true ? "(勝利)" : "")}", blueTeamField);
            }

            // Red Team
            var redTeamPlayers = match.Players.Where(p => p.TeamId == "Red").OrderByDescending(p => p.Score).ToList();
            if (redTeamPlayers.Any())
            {
                var redTeamField = "```\n";
                redTeamField += "プレイヤー名          | エージェント | スコア | KDA\n";
                redTeamField += "-------------------|------------|-------|--------\n";

                foreach (var player in redTeamPlayers)
                {
                    var playerName = player.DisplayName.Length > 18 ? player.DisplayName.Substring(0, 15) + "..." : player.DisplayName.PadRight(18);
                    redTeamField += $"{playerName} | {player.AgentName.PadRight(10)} | {player.Score.ToString().PadRight(5)} | {player.KdaDisplay}\n";
                }

                redTeamField += "```";
                embed.AddField($"Red Team {(team2?.HasWon == true ? "(勝利)" : "")}", redTeamField);
            }

            await ReplyAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("count")]
    [Summary("プレイヤーの日別試合数を表示します")]
    public async Task GetMatchCountByDayAsync(string name, string tag, [Remainder] int days = 7)
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            var player = await _playerApi.SearchPlayerAsync(name, tag);
            if (player == null)
            {
                await ReplyAsync($"プレイヤー '{name}#{tag}' が見つかりませんでした。");
                return;
            }

            var matchCounts = await _matchApi.GetPlayerMatchCountByDayAsync(player.Puuid, Math.Min(days, 30));
            if (matchCounts == null || matchCounts.Count == 0)
            {
                await ReplyAsync($"プレイヤー '{player.DisplayName}' の試合数情報が見つかりませんでした。");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"{player.DisplayName} の日別試合数")
                .WithColor(Color.Orange)
                .WithFooter(footer => footer.Text = $"PUUID: {player.Puuid}")
                .WithCurrentTimestamp();

            var description = "```\n";
            description += "日付       | 試合数\n";
            description += "-----------|------\n";

            foreach (var item in matchCounts.OrderByDescending(kv => kv.Key))
            {
                description += $"{item.Key.ToLocalTime():yyyy/MM/dd} | {item.Value}\n";
            }

            description += "```";
            embed.WithDescription(description);

            await ReplyAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }
}