using Discord;
using Discord.Interactions;
using KDalytics.Web.Models;
using KDalytics.Web.Services;

namespace KDalytics.Discord.Modules.SlashCommands;

[Group("ranking", "ランキング関連のコマンド")]
public class RankingCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly PlayerApiClient _playerApi;
    private readonly PerformanceApiClient _performanceApi;

    public RankingCommands(PlayerApiClient playerApi, PerformanceApiClient performanceApi)
    {
        _playerApi = playerApi;
        _performanceApi = performanceApi;
    }

    [SlashCommand("kda", "トラッキング中のプレイヤーのKDAランキングを表示します")]
    public async Task GetKdaRankingAsync(
        [Summary("period", "期間（week/month/season）")] string period = "month")
    {
        await DeferAsync();

        try
        {
            var players = await _playerApi.GetTrackedPlayersAsync();
            if (players == null || players.Count == 0)
            {
                await FollowupAsync("トラッキング中のプレイヤーが見つかりませんでした。");
                return;
            }

            DateTime from = DateTime.UtcNow;
            string periodName;

            switch (period.ToLower())
            {
                case "week":
                    from = DateTime.UtcNow.AddDays(-7);
                    periodName = "1週間";
                    break;
                case "season":
                    from = DateTime.UtcNow.AddDays(-90);
                    periodName = "シーズン";
                    break;
                case "month":
                default:
                    from = DateTime.UtcNow.AddDays(-30);
                    periodName = "1ヶ月";
                    break;
            }

            var puuids = players.Select(p => p.Puuid).ToList();
            var ranking = await _performanceApi.GetPlayersKdaRankingAsync(puuids, from);

            if (ranking == null || ranking.Count == 0)
            {
                await FollowupAsync("ランキング情報が見つかりませんでした。");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"KDAランキング（{periodName}）")
                .WithColor(Color.Gold)
                .WithFooter(footer => footer.Text = $"対象プレイヤー数: {players.Count}")
                .WithCurrentTimestamp();

            var description = "```\n";
            description += "順位 | プレイヤー名          | KDA比  | KDA        | 試合数\n";
            description += "-----|-------------------|-------|------------|------\n";

            int rank = 1;
            foreach (var player in ranking.OrderByDescending(p => p.KdaRatio).Take(10))
            {
                var playerName = player.DisplayName.Length > 18 ? player.DisplayName.Substring(0, 15) + "..." : player.DisplayName.PadRight(18);
                description += $"{rank,4} | {playerName} | {player.KdaRatioDisplay,5} | {player.KdaDisplay,-10} | {player.GamesPlayed,5}\n";
                rank++;
            }

            description += "```";
            embed.WithDescription(description);

            await FollowupAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await FollowupAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [SlashCommand("tracked", "トラッキング中のプレイヤー一覧を表示します")]
    public async Task GetTrackedPlayersAsync()
    {
        await DeferAsync();

        try
        {
            var players = await _playerApi.GetTrackedPlayersAsync();
            if (players == null || players.Count == 0)
            {
                await FollowupAsync("トラッキング中のプレイヤーが見つかりませんでした。");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("トラッキング中のプレイヤー一覧")
                .WithColor(Color.Purple)
                .WithFooter(footer => footer.Text = $"合計: {players.Count}人")
                .WithCurrentTimestamp();

            var description = "```\n";
            description += "プレイヤー名          | リージョン | レベル | 最終更新\n";
            description += "-------------------|----------|-------|------------\n";

            foreach (var player in players.OrderBy(p => p.GameName))
            {
                var playerName = player.DisplayName.Length > 18 ? player.DisplayName.Substring(0, 15) + "..." : player.DisplayName.PadRight(18);
                description += $"{playerName} | {player.Region.PadRight(8)} | {player.AccountLevel,5} | {player.LastUpdated.ToLocalTime():yyyy/MM/dd}\n";
            }

            description += "```";
            embed.WithDescription(description);

            await FollowupAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await FollowupAsync($"エラーが発生しました: {ex.Message}");
        }
    }
}