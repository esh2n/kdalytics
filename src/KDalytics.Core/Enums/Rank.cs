namespace KDalytics.Core.Enums;

/// <summary>
/// Valorantのランクを表す列挙型
/// </summary>
public enum Rank
{
    /// <summary>
    /// ランクなし
    /// </summary>
    Unranked = 0,

    /// <summary>
    /// アイアン 1
    /// </summary>
    Iron1 = 3,

    /// <summary>
    /// アイアン 2
    /// </summary>
    Iron2 = 4,

    /// <summary>
    /// アイアン 3
    /// </summary>
    Iron3 = 5,

    /// <summary>
    /// ブロンズ 1
    /// </summary>
    Bronze1 = 6,

    /// <summary>
    /// ブロンズ 2
    /// </summary>
    Bronze2 = 7,

    /// <summary>
    /// ブロンズ 3
    /// </summary>
    Bronze3 = 8,

    /// <summary>
    /// シルバー 1
    /// </summary>
    Silver1 = 9,

    /// <summary>
    /// シルバー 2
    /// </summary>
    Silver2 = 10,

    /// <summary>
    /// シルバー 3
    /// </summary>
    Silver3 = 11,

    /// <summary>
    /// ゴールド 1
    /// </summary>
    Gold1 = 12,

    /// <summary>
    /// ゴールド 2
    /// </summary>
    Gold2 = 13,

    /// <summary>
    /// ゴールド 3
    /// </summary>
    Gold3 = 14,

    /// <summary>
    /// プラチナ 1
    /// </summary>
    Platinum1 = 15,

    /// <summary>
    /// プラチナ 2
    /// </summary>
    Platinum2 = 16,

    /// <summary>
    /// プラチナ 3
    /// </summary>
    Platinum3 = 17,

    /// <summary>
    /// ダイアモンド 1
    /// </summary>
    Diamond1 = 18,

    /// <summary>
    /// ダイアモンド 2
    /// </summary>
    Diamond2 = 19,

    /// <summary>
    /// ダイアモンド 3
    /// </summary>
    Diamond3 = 20,

    /// <summary>
    /// アセンダント 1
    /// </summary>
    Ascendant1 = 21,

    /// <summary>
    /// アセンダント 2
    /// </summary>
    Ascendant2 = 22,

    /// <summary>
    /// アセンダント 3
    /// </summary>
    Ascendant3 = 23,

    /// <summary>
    /// イモータル 1
    /// </summary>
    Immortal1 = 24,

    /// <summary>
    /// イモータル 2
    /// </summary>
    Immortal2 = 25,

    /// <summary>
    /// イモータル 3
    /// </summary>
    Immortal3 = 26,

    /// <summary>
    /// レディアント
    /// </summary>
    Radiant = 27
}

/// <summary>
/// ランク関連のユーティリティメソッドを提供します
/// </summary>
public static class RankExtensions
{
    /// <summary>
    /// ランクIDからRank列挙型に変換します
    /// </summary>
    /// <param name="rankId">Riot APIから取得したランクID</param>
    /// <returns>対応するRank列挙値、一致しない場合はUnranked</returns>
    public static Rank FromId(int rankId)
    {
        if (Enum.IsDefined(typeof(Rank), rankId))
        {
            return (Rank)rankId;
        }
        return Rank.Unranked;
    }

    /// <summary>
    /// ランク名からRank列挙型に変換します
    /// </summary>
    /// <param name="rankName">ランク名（例: "Gold 1", "Platinum 3"）</param>
    /// <returns>対応するRank列挙値、一致しない場合はUnranked</returns>
    public static Rank FromName(string rankName)
    {
        if (string.IsNullOrWhiteSpace(rankName))
            return Rank.Unranked;

        // 空白で分割
        string[] parts = rankName.Trim().Split(' ');
        if (parts.Length != 2)
            return Rank.Unranked;

        string tier = parts[0].ToLowerInvariant();
        if (!int.TryParse(parts[1], out int level) || level < 1 || level > 3)
        {
            // Radiantの場合は特別処理
            if (tier == "radiant")
                return Rank.Radiant;
            return Rank.Unranked;
        }

        return tier switch
        {
            "iron" => level == 1 ? Rank.Iron1 : level == 2 ? Rank.Iron2 : Rank.Iron3,
            "bronze" => level == 1 ? Rank.Bronze1 : level == 2 ? Rank.Bronze2 : Rank.Bronze3,
            "silver" => level == 1 ? Rank.Silver1 : level == 2 ? Rank.Silver2 : Rank.Silver3,
            "gold" => level == 1 ? Rank.Gold1 : level == 2 ? Rank.Gold2 : Rank.Gold3,
            "platinum" => level == 1 ? Rank.Platinum1 : level == 2 ? Rank.Platinum2 : Rank.Platinum3,
            "diamond" => level == 1 ? Rank.Diamond1 : level == 2 ? Rank.Diamond2 : Rank.Diamond3,
            "ascendant" => level == 1 ? Rank.Ascendant1 : level == 2 ? Rank.Ascendant2 : Rank.Ascendant3,
            "immortal" => level == 1 ? Rank.Immortal1 : level == 2 ? Rank.Immortal2 : Rank.Immortal3,
            _ => Rank.Unranked
        };
    }

    /// <summary>
    /// ランクを表示用の文字列に変換します
    /// </summary>
    /// <param name="rank">ランク</param>
    /// <returns>表示用の文字列</returns>
    public static string ToDisplayString(this Rank rank)
    {
        return rank switch
        {
            Rank.Unranked => "ランクなし",
            Rank.Iron1 => "アイアン 1",
            Rank.Iron2 => "アイアン 2",
            Rank.Iron3 => "アイアン 3",
            Rank.Bronze1 => "ブロンズ 1",
            Rank.Bronze2 => "ブロンズ 2",
            Rank.Bronze3 => "ブロンズ 3",
            Rank.Silver1 => "シルバー 1",
            Rank.Silver2 => "シルバー 2",
            Rank.Silver3 => "シルバー 3",
            Rank.Gold1 => "ゴールド 1",
            Rank.Gold2 => "ゴールド 2",
            Rank.Gold3 => "ゴールド 3",
            Rank.Platinum1 => "プラチナ 1",
            Rank.Platinum2 => "プラチナ 2",
            Rank.Platinum3 => "プラチナ 3",
            Rank.Diamond1 => "ダイアモンド 1",
            Rank.Diamond2 => "ダイアモンド 2",
            Rank.Diamond3 => "ダイアモンド 3",
            Rank.Ascendant1 => "アセンダント 1",
            Rank.Ascendant2 => "アセンダント 2",
            Rank.Ascendant3 => "アセンダント 3",
            Rank.Immortal1 => "イモータル 1",
            Rank.Immortal2 => "イモータル 2",
            Rank.Immortal3 => "イモータル 3",
            Rank.Radiant => "レディアント",
            _ => "不明"
        };
    }
}