using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace KDalytics.Discord;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
    }

    public async Task InstallCommandsAsync()
    {
        try
        {
            // モジュールの登録をスキップ
            Console.WriteLine("コマンドハンドラーを設定しました");
            Console.WriteLine("モジュールの登録はスキップします");

            // 代わりに、Program.cs の MessageReceived イベントで直接コマンドを処理します
        }
        catch (Exception ex)
        {
            Console.WriteLine($"コマンドのインストール中にエラーが発生しました: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}