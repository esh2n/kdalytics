using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KDalytics.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KDalytics.Discord;

public class Program
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IConfiguration _configuration;
    private IServiceProvider _services;

    // コンストラクタで初期化（警告を解消するため）
    public Program()
    {
        _client = new DiscordSocketClient();
        _commands = new CommandService();
        _configuration = new ConfigurationBuilder().Build(); // 一時的な空の設定
        _services = new ServiceCollection().BuildServiceProvider(); // 一時的な空のサービスプロバイダー
    }

    public static async Task Main(string[] args)
    {
        await new Program().RunAsync();
    }

    public async Task RunAsync()
    {
        // 設定ファイルの読み込み
        try
        {
            // 実行ファイルのディレクトリを取得
            var executablePath = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(executablePath, "appsettings.json");
            var devConfigPath = Path.Combine(executablePath, "appsettings.Development.json");

            Console.WriteLine($"設定ファイルのパス: {configPath}");
            Console.WriteLine($"開発用設定ファイルのパス: {devConfigPath}");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: true);

            // 開発環境の設定ファイルを追加（存在する場合）
            if (File.Exists(devConfigPath))
            {
                Console.WriteLine("開発用設定ファイルが見つかりました。読み込みます。");
                builder.AddJsonFile(devConfigPath, optional: false);
            }

            _configuration = builder.Build();

            Console.WriteLine("設定ファイルの読み込みに成功しました。");

            // トークンの確認（デバッグ用）
            var discordToken = _configuration["DiscordToken"];
            if (!string.IsNullOrEmpty(discordToken))
            {
                Console.WriteLine($"DiscordToken が設定されています: {discordToken.Substring(0, 10)}...");
            }
            else
            {
                Console.WriteLine("DiscordToken が設定されていません。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"設定ファイルの読み込みに失敗しました: {ex.Message}");
            // 空の設定を使用
            _configuration = new ConfigurationBuilder().Build();
        }

        // Discordクライアントの設定
        var config = new DiscordSocketConfig
        {
            // MessageContent インテントを使用しない
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages,
            AlwaysDownloadUsers = false // GuildMembers インテントが必要ないように変更
        };

        _client = new DiscordSocketClient(config);
        _commands = new CommandService();

        // 依存関係の設定
        _services = ConfigureServices();

        // イベントハンドラーの登録
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;

        // ボットの起動
        var token = _configuration["DiscordToken"]; // 正しい設定キーを使用
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("DiscordToken が設定されていません。appsettings.json を確認してください。");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // コマンドハンドラーの設定
        var commandHandler = new CommandHandler(_client, _commands, _services);
        await commandHandler.InstallCommandsAsync();

        // アプリケーションが終了しないようにする
        await Task.Delay(Timeout.Infinite);
    }

    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_configuration)
            .AddSingleton<PlayerApiClient>()
            .AddSingleton<MatchApiClient>()
            .AddSingleton<PerformanceApiClient>();

        return services.BuildServiceProvider();
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser} が起動しました！");
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage messageParam)
    {
        // ボットからのメッセージは無視
        if (messageParam is not SocketUserMessage message) return;
        if (message.Author.IsBot) return;

        // コマンドの先頭文字（!）を確認
        int argPos = 0;
        if (!message.HasCharPrefix('!', ref argPos)) return;

        // コマンドの内容を取得
        var commandText = message.Content.Substring(argPos).Trim();
        var context = new SocketCommandContext(_client, message);

        try
        {
            Console.WriteLine($"コマンドを受信: {commandText}");

            // 簡易的なコマンド処理
            if (commandText.StartsWith("channel"))
            {
                await HandleChannelCommandAsync(context, commandText);
            }
            else
            {
                await message.ReplyAsync("未知のコマンドです。`!channel help` でヘルプを表示できます。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"コマンド処理中にエラーが発生しました: {ex.Message}");
            await message.ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }

    private async Task HandleChannelCommandAsync(SocketCommandContext context, string commandText)
    {
        var parts = commandText.Split(' ', 2);
        var subCommand = parts.Length > 1 ? parts[1] : "help";

        var playerApi = _services.GetService<PlayerApiClient>();

        switch (subCommand)
        {
            case "register":
                await context.Channel.TriggerTypingAsync();
                try
                {
                    // チャンネル情報を取得
                    var channelId = context.Channel.Id;
                    var guildId = context.Guild?.Id ?? 0;
                    var channelName = context.Channel.Name;
                    var guildName = context.Guild?.Name ?? "DMチャンネル";

                    // 簡易的なアクセスコード生成
                    var accessCode = Convert.ToBase64String(
                        System.Security.Cryptography.SHA256.Create()
                        .ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{channelId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 86400}"))
                    ).Substring(0, 8).Replace("/", "X").Replace("+", "Y");

                    var dashboardUrl = _configuration["WebAppUrl"] ?? "http://localhost:5173";

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

                    await context.Channel.SendMessageAsync(embed: embed);
                }
                catch (Exception ex)
                {
                    await context.Channel.SendMessageAsync($"エラーが発生しました: {ex.Message}");
                }
                break;

            case "help":
            default:
                var helpEmbed = new EmbedBuilder()
                    .WithTitle("チャンネルコマンドのヘルプ")
                    .WithColor(Color.Blue)
                    .WithDescription("以下のコマンドが利用可能です：")
                    .AddField("!channel register", "現在のチャンネルをKDalyticsに登録します。")
                    .AddField("!channel players", "このチャンネルで登録されているプレイヤー一覧を表示します。")
                    .AddField("!channel dashboard", "このチャンネルのダッシュボードURLを表示します。")
                    .WithFooter(footer => footer.Text = "KDalytics Bot")
                    .WithCurrentTimestamp()
                    .Build();

                await context.Channel.SendMessageAsync(embed: helpEmbed);
                break;
        }
    }
}
