namespace KDalytics.Core.Models.Player;

/// <summary>
/// Valorantプレイヤーの基本情報を表すモデル
/// </summary>
public record PlayerEntity
{
    /// <summary>
    /// Riot APIが発行するプレイヤーの一意識別子
    /// </summary>
    public string Puuid { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤーのゲーム内表示名
    /// </summary>
    public string GameName { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤーのタグライン (#以降の部分)
    /// </summary>
    public string TagLine { get; init; } = string.Empty;

    /// <summary>
    /// プレイヤーが所属するリージョン (例: ap, eu, na など)
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// アカウントレベル
    /// </summary>
    public int AccountLevel { get; init; }

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// トラッキング対象かどうか
    /// </summary>
    public bool IsTracked { get; init; } = false;

    /// <summary>
    /// プレイヤーのRiot ID（表示用）を取得します
    /// </summary>
    /// <returns>GameName#TagLine 形式の文字列</returns>
    public string GetRiotId() => $"{GameName}#{TagLine}";

    /// <summary>
    /// プレイヤーの更新されたコピーを生成します
    /// </summary>
    /// <returns>新しいPlayerEntityインスタンス（最終更新日時が現在時刻）</returns>
    public PlayerEntity WithUpdatedTimestamp() => this with { LastUpdated = DateTime.UtcNow };

    /// <summary>
    /// プレイヤーのトラッキング状態を変更したコピーを生成します
    /// </summary>
    /// <param name="isTracked">トラッキング対象とするかどうか</param>
    /// <returns>新しいPlayerEntityインスタンス（トラッキング状態が変更済み）</returns>
    public PlayerEntity WithTracking(bool isTracked) => this with { IsTracked = isTracked };
}