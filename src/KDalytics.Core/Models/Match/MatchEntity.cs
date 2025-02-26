using KDalytics.Core.Enums;
using KDalytics.Core.Models.Performance;

namespace KDalytics.Core.Models.Match;

/// <summary>
/// Valorantの試合情報を表すモデル
/// </summary>
public record MatchEntity
{
    /// <summary>
    /// 試合の一意識別子
    /// </summary>
    public string MatchId { get; init; } = string.Empty;

    /// <summary>
    /// マップID
    /// </summary>
    public string MapId { get; init; } = string.Empty;

    /// <summary>
    /// マップ名（人間可読形式）
    /// </summary>
    public string MapName { get; init; } = string.Empty;

    /// <summary>
    /// ゲームモード (Competitive, Unrated, Deathmatch など)
    /// </summary>
    public string GameMode { get; init; } = string.Empty;

    /// <summary>
    /// 試合開始時間
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// 試合の所要時間（ミリ秒）
    /// </summary>
    public long GameLength { get; init; }

    /// <summary>
    /// サーバーリージョン
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// シーズンID
    /// </summary>
    public string SeasonId { get; init; } = string.Empty;

    /// <summary>
    /// チーム情報
    /// </summary>
    public List<TeamData> Teams { get; init; } = new();

    /// <summary>
    /// プレイヤーのパフォーマンス情報
    /// </summary>
    public List<PlayerMatchPerformance> Players { get; init; } = new();

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 試合時間を人間可読形式で取得します
    /// </summary>
    /// <returns>「XX分XX秒」形式の試合時間</returns>
    public string GetFormattedGameLength()
    {
        TimeSpan duration = TimeSpan.FromMilliseconds(GameLength);
        return $"{(int)duration.TotalMinutes}分{duration.Seconds}秒";
    }

    /// <summary>
    /// ゲームモードを人間可読形式で取得します
    /// </summary>
    /// <returns>日本語表記のゲームモード名</returns>
    public string GetFormattedGameMode()
    {
        // ゲームモード文字列からEnum値への変換を試みる
        if (Enum.TryParse<GameMode>(GameMode, true, out var gameModeEnum))
        {
            return gameModeEnum switch
            {
                GameMode.Competitive => "コンペティティブ",
                GameMode.Unrated => "アンレーテッド",
                GameMode.SpikeRush => "スパイクラッシュ",
                GameMode.Deathmatch => "デスマッチ",
                GameMode.Escalation => "エスカレーション",
                GameMode.Custom => "カスタム",
                GameMode.Replication => "レプリケーション",
                GameMode.Snowball => "スノーボールファイト",
                GameMode.SwiftPlay => "スワットアットフライ",
                GameMode.TeamDeathmatch => "チームデスマッチ",
                GameMode.Premier => "プレミア",
                _ => GameMode
            };
        }

        // 文字列ベースでの変換（APIから直接来た値）
        return GameMode.ToLowerInvariant() switch
        {
            "competitive" => "コンペティティブ",
            "unrated" => "アンレーテッド",
            "spikerush" => "スパイクラッシュ",
            "deathmatch" => "デスマッチ",
            "escalation" => "エスカレーション",
            "custom" => "カスタム",
            "replication" => "レプリケーション",
            "snowball" => "スノーボールファイト",
            "swiftplay" => "スワットアットフライ",
            "teamdeathmatch" => "チームデスマッチ",
            "premier" => "プレミア",
            _ => GameMode
        };
    }

    /// <summary>
    /// 試合結果のスコア表示を取得します
    /// </summary>
    /// <returns>「13-7」のような形式のスコア</returns>
    public string GetScoreDisplay()
    {
        if (Teams.Count != 2)
            return "不明";

        return $"{Teams[0].RoundsWon}-{Teams[1].RoundsWon}";
    }

    /// <summary>
    /// 特定のプレイヤーのパフォーマンスを取得します
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <returns>プレイヤーのパフォーマンス情報、見つからない場合はnull</returns>
    public PlayerMatchPerformance? GetPlayerPerformance(string puuid)
    {
        return Players.FirstOrDefault(p => p.Puuid == puuid);
    }

    /// <summary>
    /// 特定のプレイヤーのチームを取得します
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <returns>プレイヤーが所属するチーム、見つからない場合はnull</returns>
    public TeamData? GetPlayerTeam(string puuid)
    {
        var performance = GetPlayerPerformance(puuid);
        if (performance == null)
            return null;

        return Teams.FirstOrDefault(t => t.TeamId == performance.TeamId);
    }

    /// <summary>
    /// 特定のプレイヤーが勝利したかどうかを判定します
    /// </summary>
    /// <param name="puuid">プレイヤーID</param>
    /// <returns>勝利した場合はtrue、それ以外はfalse</returns>
    public bool DidPlayerWin(string puuid)
    {
        var team = GetPlayerTeam(puuid);
        return team?.HasWon ?? false;
    }

    /// <summary>
    /// 試合情報の更新されたコピーを生成します
    /// </summary>
    /// <returns>新しいMatchEntityインスタンス（最終更新日時が現在時刻）</returns>
    public MatchEntity WithUpdatedTimestamp() => this with { LastUpdated = DateTime.UtcNow };
}