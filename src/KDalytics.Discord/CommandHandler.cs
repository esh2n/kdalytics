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
        // コマンドモジュールの登録
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // 登録されたコマンドの一覧を表示
        Console.WriteLine("登録されたコマンド:");
        foreach (var command in _commands.Commands)
        {
            Console.WriteLine($"- !{command.Name}: {command.Summary}");
        }
    }
}