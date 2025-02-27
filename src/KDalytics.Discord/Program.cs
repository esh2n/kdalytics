using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KDalytics.Web.Models;
using KDalytics.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KDalytics.Discord;

class Program
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IServiceProvider _services;
    private IConfiguration _configuration;

    static async Task Main(string[] args)
    {
        await new Program().RunAsync();
    }

    public Program()
    {
        // 設定ファイルの読み込み
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Discordクライアントの設定
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 100
        };

        _client = new DiscordSocketClient(config);
        _commands = new CommandService();

        // サービスの設定
        _services = ConfigureServices();

        // イベントハンドラの登録
        _client.Log += LogAsync;
        _commands.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += HandleCommandAsync;
    }

    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_configuration)
            .AddSingleton<HttpClient>(provider => new HttpClient
            {
                BaseAddress = new Uri(_configuration["ApiBaseUrl"] ?? "http://localhost:5000")
            })
            .AddSingleton<ApiClient>()
            .AddSingleton<PlayerApiClient>()
            .AddSingleton<MatchApiClient>()
            .AddSingleton<PerformanceApiClient>()
            .AddSingleton<CommandHandler>();

        return services.BuildServiceProvider();
    }

    private async Task RunAsync()
    {
        // Botトークンの取得
        var token = _configuration["DiscordToken"];
        if (string.IsNullOrEmpty(token))
        {
            Console.Error.WriteLine("Discord Botトークンが設定されていません。appsettings.jsonを確認してください。");
            return;
        }

        // Botにログイン
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // コマンドの登録
        await _services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

        // Botが終了するまで待機
        await Task.Delay(Timeout.Infinite);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser} is connected!");
        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // メッセージがユーザーからのものでない場合は無視
        if (messageParam is not SocketUserMessage message) return;

        // 自分自身のメッセージは無視
        if (message.Author.Id == _client.CurrentUser.Id) return;

        // コマンドの先頭文字位置を特定
        int argPos = 0;

        // メッセージがコマンドプレフィックスで始まるか、Botにメンションされているか確認
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            return;

        // コマンドの実行
        var context = new SocketCommandContext(_client, message);
        await _commands.ExecuteAsync(context, argPos, _services);
    }
}
