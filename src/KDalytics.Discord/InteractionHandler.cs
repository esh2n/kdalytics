using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace KDalytics.Discord;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;

    public InteractionHandler(
        DiscordSocketClient client,
        InteractionService commands,
        IServiceProvider services,
        IConfiguration configuration)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // スラッシュコマンドモジュールを登録
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // イベントハンドラーを登録
            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteractionAsync;
            _commands.SlashCommandExecuted += SlashCommandExecutedAsync;

            Console.WriteLine("インタラクションハンドラーを初期化しました");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"インタラクションハンドラーの初期化中にエラーが発生しました: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private async Task ReadyAsync()
    {
        try
        {
            // グローバルコマンドを登録（本番環境用）
            if (_configuration.GetValue<bool>("RegisterGlobalCommands", false))
            {
                Console.WriteLine("グローバルスラッシュコマンドを登録しています...");
                await _commands.RegisterCommandsGloballyAsync();
                Console.WriteLine("グローバルスラッシュコマンドの登録が完了しました");
            }
            // 特定のギルドにコマンドを登録（開発環境用、即時反映される）
            else
            {
                var testGuildId = _configuration.GetValue<ulong>("TestGuildId", 0);
                if (testGuildId != 0)
                {
                    Console.WriteLine($"テストギルド（ID: {testGuildId}）にスラッシュコマンドを登録しています...");
                    await _commands.RegisterCommandsToGuildAsync(testGuildId);
                    Console.WriteLine("テストギルドへのスラッシュコマンドの登録が完了しました");
                }
                else
                {
                    Console.WriteLine("TestGuildIdが設定されていないため、スラッシュコマンドは登録されません");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"スラッシュコマンドの登録中にエラーが発生しました: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            // インタラクションのコンテキストを作成
            var context = new SocketInteractionContext(_client, interaction);

            // インタラクションを処理
            await _commands.ExecuteCommandAsync(context, _services);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"インタラクションの処理中にエラーが発生しました: {ex.Message}");

            // すでに応答済みでない場合は、エラーメッセージを返す
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                var commandInteraction = (SocketSlashCommand)interaction;
                if (!commandInteraction.HasResponded)
                {
                    await commandInteraction.RespondAsync($"コマンドの実行中にエラーが発生しました: {ex.Message}", ephemeral: true);
                }
            }
        }
    }

    private Task SlashCommandExecutedAsync(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // 前提条件を満たしていない場合（権限不足など）
                    Console.WriteLine($"コマンド実行の前提条件を満たしていません: {result.ErrorReason}");
                    break;
                case InteractionCommandError.UnknownCommand:
                    // 不明なコマンド
                    Console.WriteLine($"不明なコマンドです: {commandInfo.Name}");
                    break;
                case InteractionCommandError.BadArgs:
                    // 引数が不正
                    Console.WriteLine($"コマンドの引数が不正です: {result.ErrorReason}");
                    break;
                case InteractionCommandError.Exception:
                    // 例外が発生
                    Console.WriteLine($"コマンド実行中に例外が発生しました: {result.ErrorReason}");
                    break;
                case InteractionCommandError.Unsuccessful:
                    // その他のエラー
                    Console.WriteLine($"コマンドの実行に失敗しました: {result.ErrorReason}");
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }
}