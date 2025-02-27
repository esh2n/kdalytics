using Discord;
using Discord.Interactions;

namespace KDalytics.Discord.Modules.SlashCommands;

public class HelpCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("help", "利用可能なコマンドの一覧を表示します")]
    public async Task HelpAsync()
    {
        await DeferAsync();

        var embed = new EmbedBuilder()
            .WithTitle("KDalytics Bot コマンド一覧")
            .WithDescription("以下のスラッシュコマンドが利用可能です。各コマンドの詳細は `/` を入力すると表示されます。")
            .WithColor(Color.Blue)
            .WithFooter(footer => footer.Text = "KDalytics Bot")
            .WithCurrentTimestamp();

        // チャンネル関連コマンド
        embed.AddField("チャンネル関連",
            "`/channel register` - 現在のチャンネルをKDalyticsに登録します\n" +
            "`/channel players` - チャンネルで登録されているプレイヤー一覧を表示します\n" +
            "`/channel dashboard` - チャンネルのダッシュボードURLを表示します");

        // プレイヤー関連コマンド
        embed.AddField("プレイヤー関連",
            "`/player search <name> <tag>` - プレイヤーを検索します\n" +
            "`/player track <name> <tag>` - プレイヤーのトラッキングを開始します\n" +
            "`/player untrack <name> <tag>` - プレイヤーのトラッキングを停止します\n" +
            "`/player stats <name> <tag> [period]` - プレイヤーの統計情報を表示します");

        // 試合関連コマンド
        embed.AddField("試合関連",
            "`/match recent <name> <tag> [count]` - プレイヤーの最近の試合を表示します\n" +
            "`/match detail <match_id>` - 試合の詳細情報を表示します\n" +
            "`/match count <name> <tag> [days]` - プレイヤーの日別試合数を表示します");

        // ランキング関連コマンド
        embed.AddField("ランキング関連",
            "`/ranking kda [period]` - トラッキング中のプレイヤーのKDAランキングを表示します\n" +
            "`/ranking tracked` - トラッキング中のプレイヤー一覧を表示します");

        // 期間パラメータの説明
        embed.AddField("期間パラメータ",
            "`week` - 1週間\n" +
            "`month` - 1ヶ月（デフォルト）\n" +
            "`season` - シーズン（約3ヶ月）");

        await FollowupAsync(embed: embed.Build());
    }
}