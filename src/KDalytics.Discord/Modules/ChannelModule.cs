using Discord;
using Discord.Commands;
using KDalytics.Web.Models;
using KDalytics.Web.Services;
using System.Text;

namespace KDalytics.Discord.Modules;

[Group("channel")]
[Summary("チャンネル関連のコマンド")]
public class ChannelModule : ModuleBase<SocketCommandContext>
{
    private readonly PlayerApiClient _playerApi;
    private readonly IConfiguration _configuration;

    public ChannelModule(PlayerApiClient playerApi, IConfiguration configuration)
    {
        _playerApi = playerApi;
        _configuration = configuration;
    }

    [Command("register")]
    [Summary("現在のチャンネルをKDalyticsに登録します")]
    public async Task RegisterChannelAsync()
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            // チャンネル情報を取得
            var channelId = Context.Channel.Id;
            var guildId = Context.Guild?.Id ?? 0;
            var channelName = Context.Channel.Name;
            var guildName = Context.Guild?.Name ?? "DMチャンネル";

            // TODO: APIを通じてチャンネル情報を登録する処理を実装
            // 現在はモックレスポンスを返す
            var dashboardUrl = _configuration["WebAppUrl"] ?? "http://localhost:5173";
            var accessCode = GenerateAccessCode(channelId);

            var embed = new EmbedBuilder()
                .WithTitle("チャンネル登録完了")
                .WithDescription($"このチャンネル「{channelName}」をKDalyticsに登録しました。")
                .WithColor(Color.Green)
                .AddField("ダッシュボードURL", $"{dashboardUrl}/channel/{channelId}")
                .AddField("アクセスコード", $"`{accessCode}`")
                .AddField("使い方", "1. 上記URLにアクセスしてください\n2. アクセスコードを入力してログインしてください\n3. このチャンネルで登録したプレイヤーの統計が表示されます")
                .WithFooter(footer => footer.Text = $"チャンネルID: {channelId}")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("players")]
    [Summary("このチャンネルで登録されているプレイヤー一覧を表示します")]
    public async Task ListChannelPlayersAsync()
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            var channelId = Context.Channel.Id;

            // TODO: APIを通じてチャンネルに登録されているプレイヤー一覧を取得する処理を実装
            // 現在はトラッキング中のプレイヤー一覧を表示
            var players = await _playerApi.GetTrackedPlayersAsync();
            if (players == null || players.Count == 0)
            {
                await ReplyAsync("このチャンネルに登録されているプレイヤーはいません。\n`!player track <名前> <タグ>`コマンドでプレイヤーを登録してください。");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("登録プレイヤー一覧")
                .WithColor(Color.Blue)
                .WithFooter(footer => footer.Text = $"チャンネルID: {channelId}")
                .WithCurrentTimestamp();

            var description = new StringBuilder();
            description.AppendLine("```");
            description.AppendLine("プレイヤー名          | リージョン | レベル");
            description.AppendLine("-------------------|----------|-------");

            foreach (var player in players.OrderBy(p => p.GameName))
            {
                var playerName = player.DisplayName.Length > 18 ? player.DisplayName.Substring(0, 15) + "..." : player.DisplayName.PadRight(18);
                description.AppendLine($"{playerName} | {player.Region.PadRight(8)} | {player.AccountLevel,5}");
            }

            description.AppendLine("```");
            embed.WithDescription(description.ToString());

            await ReplyAsync(embed: embed);
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("dashboard")]
    [Summary("このチャンネルのダッシュボードURLを表示します")]
    public async Task GetDashboardUrlAsync()
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            var channelId = Context.Channel.Id;
            var channelName = Context.Channel.Name;
            var dashboardUrl = _configuration["WebAppUrl"] ?? "http://localhost:5173";
            var accessCode = GenerateAccessCode(channelId);

            var embed = new EmbedBuilder()
                .WithTitle("KDalyticsダッシュボード")
                .WithDescription($"チャンネル「{channelName}」のダッシュボードにアクセスするには、以下のURLとアクセスコードを使用してください。")
                .WithColor(Color.Blue)
                .AddField("ダッシュボードURL", $"{dashboardUrl}/channel/{channelId}")
                .AddField("アクセスコード", $"`{accessCode}`")
                .WithFooter(footer => footer.Text = $"チャンネルID: {channelId}")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);
        }
        catch (Exception ex)
        {
            await ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [Command("help")]
    [Summary("チャンネルコマンドのヘルプを表示します")]
    public async Task HelpAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("チャンネルコマンドのヘルプ")
            .WithColor(Color.Blue)
            .WithDescription("以下のコマンドが利用可能です：")
            .AddField("!channel register", "現在のチャンネルをKDalyticsに登録します。")
            .AddField("!channel players", "このチャンネルで登録されているプレイヤー一覧を表示します。")
            .AddField("!channel dashboard", "このチャンネルのダッシュボードURLを表示します。")
            .WithFooter(footer => footer.Text = "KDalytics Bot")
            .WithCurrentTimestamp()
            .Build();

        await ReplyAsync(embed: embed);
    }

    // チャンネルIDからアクセスコードを生成するヘルパーメソッド
    private string GenerateAccessCode(ulong channelId)
    {
        // 簡易的なアクセスコード生成（実際の実装ではより安全な方法を使用すべき）
        var secret = _configuration["AccessCodeSecret"] ?? "KDalyticsSecret";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var input = $"{channelId}-{secret}-{timestamp / 86400}"; // 日単位で変更

        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash).Substring(0, 8).Replace("/", "X").Replace("+", "Y");
    }
}