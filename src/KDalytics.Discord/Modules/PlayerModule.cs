using Discord;
using Discord.Commands;
using KDalytics.Web.Models;
using KDalytics.Web.Services;

namespace KDalytics.Discord.Modules;

[Group("player")]
[Summary("プレイヤー関連のコマンド")]
public class PlayerModule : ModuleBase<SocketCommandContext>
{
    private readonly PlayerApiClient _playerApi;
    private readonly MatchApiClient _matchApi;
    private readonly PerformanceApiClient _performanceApi;

    public PlayerModule(PlayerApiClient playerApi, MatchApiClient matchApi, PerformanceApiClient performanceApi)
    {
        _playerApi = playerApi;
        _matchApi = matchApi;
        _performanceApi = performanceApi;
    }

    [Command("search")]
    [Summary("プレイヤーを検索します")]
    public async Task SearchPlayerAsync(string name, string tag)
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

            var embed = new EmbedBuilder()
                .WithTitle($"{player.DisplayName}")
                .WithDescription($"リージョン: {player.Region}")
                .WithColor(Color.Blue)
                .WithThumbnailUrl("https://static.wikia.nocookie.net/valorant/images/9/91/VALORANT_Logo_-_Render.png")
                .AddField("アカウントレベル", player.AccountLevel, true)
                .AddField("トラッキング", player.IsTracked ? "有効" : "無効", true)
                .AddField("最終更新", player.LastUpdated.ToLocalTime().ToString("yyyy/MM/dd HH:mm"), true)
                .WithFooter(footer => footer.Text = $"PUUID: {player.Puuid}")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);

            // ランク情報を取得
            var rank = await _playerApi.GetPlayerRankAsync(player.Puuid);
            if (rank != null)
            {
                var rankEmbed = new EmbedBuilder()
                    .WithTitle($"{player.DisplayName} のランク情報")
                    .WithColor(Color.Gold)
                    .AddField("ランク", rank.CurrentTierName, true)
                    .AddField("MMR", rank.Mmr, true)
                    .AddField("最終変動", $"{(rank.MmrChange >= 0 ? "+" : "")}{rank.MmrChange}", true)
                    .WithFooter(footer => footer.Text = $"最終更新: {rank.LastUpdated.ToLocalTime():yyyy/MM/dd HH:mm}")
                    .WithCurrentTimestamp()
                    .Build();

                await ReplyAsync(embed: rankEmbed);
            }
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("track")]
    [Summary("プレイヤーのトラッキングを開始し、現在のチャンネルに関連付けます")]
    public async Task TrackPlayerAsync(string name, string tag)
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

            // チャンネル情報を取得
            var channelId = Context.Channel.Id;
            var guildId = Context.Guild?.Id ?? 0;
            var channelName = Context.Channel.Name;
            var guildName = Context.Guild?.Name ?? "DMチャンネル";

            if (player.IsTracked)
            {
                // TODO: チャンネルとプレイヤーの関連付けを行うAPIを呼び出す
                // 現在はモックレスポンスを返す
                await ReplyAsync($"プレイヤー '{player.DisplayName}' をチャンネル '{channelName}' に関連付けました。");
                return;
            }

            var updatedPlayer = await _playerApi.UpdateTrackingAsync(player.Puuid, true);
            if (updatedPlayer != null)
            {
                // TODO: チャンネルとプレイヤーの関連付けを行うAPIを呼び出す
                // 現在はモックレスポンスを返す
                await ReplyAsync($"プレイヤー '{updatedPlayer.DisplayName}' のトラッキングを開始し、チャンネル '{channelName}' に関連付けました。");
            }
            else
            {
                await ReplyAsync($"プレイヤー '{player.DisplayName}' のトラッキング設定に失敗しました。");
            }
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("untrack")]
    [Summary("プレイヤーのトラッキングを停止し、現在のチャンネルとの関連付けを解除します")]
    public async Task UntrackPlayerAsync(string name, string tag)
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

            // チャンネル情報を取得
            var channelId = Context.Channel.Id;
            var channelName = Context.Channel.Name;

            if (!player.IsTracked)
            {
                await ReplyAsync($"プレイヤー '{player.DisplayName}' は既にトラッキングが停止されています。");
                return;
            }

            // TODO: チャンネルとプレイヤーの関連付けを解除するAPIを呼び出す
            // 現在はモックレスポンスを返す
            await ReplyAsync($"プレイヤー '{player.DisplayName}' をチャンネル '{channelName}' から削除しました。");

            // 他のチャンネルでトラッキングされているか確認
            // TODO: 実際にはAPIを呼び出して確認する
            bool trackedInOtherChannels = false;

            if (!trackedInOtherChannels)
            {
                var updatedPlayer = await _playerApi.UpdateTrackingAsync(player.Puuid, false);
                if (updatedPlayer != null)
                {
                    await ReplyAsync($"プレイヤー '{updatedPlayer.DisplayName}' のトラッキングを停止しました（どのチャンネルでもトラッキングされていません）。");
                }
                else
                {
                    await ReplyAsync($"プレイヤー '{player.DisplayName}' のトラッキング設定に失敗しました。");
                }
            }
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("stats")]
    [Summary("プレイヤーの統計情報を表示します")]
    public async Task GetPlayerStatsAsync(string name, string tag, [Remainder] string period = "month")
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

            var stats = await _performanceApi.GetPlayerPerformanceStatsAsync(player.Puuid, from);
            if (stats == null)
            {
                await ReplyAsync($"プレイヤー '{player.DisplayName}' の統計情報が見つかりませんでした。");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"{player.DisplayName} の統計情報（{periodName}）")
                .WithDescription($"期間: {stats.StartDate.ToLocalTime():yyyy/MM/dd} - {stats.EndDate.ToLocalTime():yyyy/MM/dd}")
                .WithColor(Color.DarkPurple)
                .AddField("試合数", $"{stats.MatchesPlayed}試合（{stats.MatchesWon}勝 {stats.Losses}敗）", true)
                .AddField("勝率", stats.WinRateDisplay, true)
                .AddField("KDA", stats.KdaDisplay, true)
                .AddField("K/D", stats.KdRatioDisplay, true)
                .AddField("KDA比", stats.KdaRatioDisplay, true)
                .AddField("HS率", stats.HeadshotPercentageDisplay, true)
                .AddField("最も使用したエージェント", stats.MostPlayedAgent, true)
                .WithFooter(footer => footer.Text = $"PUUID: {player.Puuid}")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);

            // エージェント別統計
            if (stats.AgentStats.Count > 0)
            {
                var agentStatsEmbed = new EmbedBuilder()
                    .WithTitle($"{player.DisplayName} のエージェント別統計")
                    .WithColor(Color.Teal);

                foreach (var agent in stats.AgentStats.OrderByDescending(a => a.Value.GamesPlayed).Take(5))
                {
                    agentStatsEmbed.AddField(
                        $"{agent.Key} ({agent.Value.GamesPlayed}試合)",
                        $"勝率: {agent.Value.WinRateDisplay}\n" +
                        $"KDA: {agent.Value.KdaDisplay}\n" +
                        $"K/D: {agent.Value.KdRatioDisplay}",
                        true);
                }

                await ReplyAsync(embed: agentStatsEmbed.Build());
            }
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }
}