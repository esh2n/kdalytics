using Discord;
using Discord.Interactions;
using KDalytics.Web.Models;
using KDalytics.Web.Services;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace KDalytics.Discord.Modules.SlashCommands;

[Group("channel", "チャンネル関連のコマンド")]
public class ChannelCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly PlayerApiClient _playerApi;
    private readonly IConfiguration _configuration;

    public ChannelCommands(PlayerApiClient playerApi, IConfiguration configuration)
    {
        _playerApi = playerApi;
        _configuration = configuration;
    }

    [SlashCommand("register", "現在のチャンネルをKDalyticsに登録します")]
    public async Task RegisterChannelAsync()
    {
        await DeferAsync();

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

            await FollowupAsync(embed: embed);
        }
        catch (Exception ex)
        {
            await FollowupAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [SlashCommand("players", "このチャンネルで登録されているプレイヤー一覧を表示します")]
    public async Task ListChannelPlayersAsync()
    {
        await DeferAsync();

        try
        {
            var channelId = Context.Channel.Id;

            // TODO: APIを通じてチャンネルに登録されているプレイヤー一覧を取得する処理を実装
            // 現在はトラッキング中のプレイヤー一覧を表示
            var players = await _playerApi.GetTrackedPlayersAsync();
            if (players == null || players.Count == 0)
            {
                await FollowupAsync("このチャンネルに登録されているプレイヤーはいません。\n`/player track`コマンドでプレイヤーを登録してください。");
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

            await FollowupAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await FollowupAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    [SlashCommand("dashboard", "このチャンネルのダッシュボードURLを表示します")]
    public async Task GetDashboardUrlAsync()
    {
        await DeferAsync();

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

            await FollowupAsync(embed: embed);
        }
        catch (Exception ex)
        {
            await FollowupAsync($"エラーが発生しました: {ex.Message}");
        }
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