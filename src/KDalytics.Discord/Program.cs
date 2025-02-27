using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using KDalytics.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace KDalytics.Discord;

public class Program
{
    private DiscordSocketClient _client;
    private InteractionService _interactions;
    private IConfiguration _configuration;
    private IServiceProvider _services;

    // コンストラクタで初期化（警告を解消するため）
    public Program()
    {
        _client = new DiscordSocketClient();
        _interactions = new InteractionService(_client);
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
            // スラッシュコマンドに必要なインテント
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = false
        };

        _client = new DiscordSocketClient(config);
        _interactions = new InteractionService(_client);

        // 依存関係の設定
        _services = ConfigureServices();

        // イベントハンドラーの登録
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;

        // 従来のコマンドシステムのサポート（移行期間中）
        _client.MessageReceived += MessageReceivedAsync;

        // ボットの起動
        var token = _configuration["DiscordToken"];
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("DiscordToken が設定されていません。appsettings.json を確認してください。");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // インタラクションハンドラーの設定
        var interactionHandler = new InteractionHandler(_client, _interactions, _services, _configuration);
        await interactionHandler.InitializeAsync();

        // アプリケーションが終了しないようにする
        await Task.Delay(Timeout.Infinite);
    }

    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_interactions)
            .AddSingleton(_configuration);

        // APIクライアントの設定
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5000";
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // 各APIクライアントを登録
        services.AddSingleton<PlayerApiClient>()
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

    // 従来のコマンドシステムのサポート（移行期間中）
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

        try
        {
            Console.WriteLine($"従来のコマンドを受信: {commandText}");
            await message.ReplyAsync("このコマンドはスラッシュコマンドに移行しました。`/help` でスラッシュコマンドの一覧を確認してください。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"コマンド処理中にエラーが発生しました: {ex.Message}");
            await message.ReplyAsync($"エラーが発生しました: {ex.Message}");
        }
    }
}
