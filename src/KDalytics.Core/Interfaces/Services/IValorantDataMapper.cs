using KDalytics.Core.Models.Match;
using KDalytics.Core.Models.Performance;
using KDalytics.Core.Models.Player;

namespace KDalytics.Core.Interfaces.Services;

/// <summary>
/// API応答からエンティティへのマッピングを行うインターフェイス
/// </summary>
public interface IValorantDataMapper
{
    #region Henrik API マッピング

    /// <summary>
    /// Henrik APIのプレイヤー情報からPlayerEntityへ変換
    /// </summary>
    /// <param name="playerInfo">Henrik APIのプレイヤー情報</param>
    /// <returns>プレイヤーエンティティ</returns>
    PlayerEntity MapToPlayerEntity(AccountInfoResponse playerInfo);

    /// <summary>
    /// Henrik APIのMMR情報からPlayerRankEntityへ変換
    /// </summary>
    /// <param name="mmrInfo">Henrik APIのMMR情報</param>
    /// <returns>プレイヤーランク情報エンティティ</returns>
    PlayerRankEntity MapToPlayerRankEntity(MmrInfoResponse mmrInfo);

    /// <summary>
    /// Henrik APIの試合詳細からMatchEntityへ変換
    /// </summary>
    /// <param name="matchDetails">Henrik APIの試合詳細</param>
    /// <returns>試合情報エンティティ</returns>
    MatchEntity MapToMatchEntity(MatchDetailsResponse matchDetails);

    /// <summary>
    /// Henrik APIの試合詳細からPlayerMatchPerformanceのリストへ変換
    /// </summary>
    /// <param name="matchDetails">Henrik APIの試合詳細</param>
    /// <returns>プレイヤーパフォーマンス情報のリスト</returns>
    List<PlayerMatchPerformance> MapToPlayerPerformances(MatchDetailsResponse matchDetails);

    /// <summary>
    /// Henrik APIの試合詳細からTeamDataのリストへ変換
    /// </summary>
    /// <param name="matchDetails">Henrik APIの試合詳細</param>
    /// <returns>チーム情報のリスト</returns>
    List<TeamData> MapToTeamData(MatchDetailsResponse matchDetails);

    /// <summary>
    /// Henrik APIの試合詳細からRoundResultのリストへ変換
    /// </summary>
    /// <param name="matchDetails">Henrik APIの試合詳細</param>
    /// <returns>ラウンド結果のリスト</returns>
    List<RoundResult> MapToRoundResults(MatchDetailsResponse matchDetails);

    #endregion

    #region Tracker Network API マッピング

    /// <summary>
    /// Tracker NetworkのプロフィールデータからPlayerEntityへ変換
    /// </summary>
    /// <param name="profile">Tracker Networkのプロフィールデータ</param>
    /// <returns>プレイヤーエンティティ</returns>
    PlayerEntity? MapToPlayerEntityFromTracker(TrackerProfileResponse profile);

    /// <summary>
    /// Tracker NetworkのプロフィールデータからPlayerRankEntityへ変換
    /// </summary>
    /// <param name="profile">Tracker Networkのプロフィールデータ</param>
    /// <returns>プレイヤーランク情報エンティティ、取得できない場合はnull</returns>
    PlayerRankEntity? MapToPlayerRankEntityFromTracker(TrackerProfileResponse profile);

    /// <summary>
    /// Tracker Networkのプロフィールデータから過去の試合情報へ変換
    /// </summary>
    /// <param name="profile">Tracker Networkのプロフィールデータ</param>
    /// <returns>試合IDのリスト（最新のものが先頭）</returns>
    List<string> ExtractRecentMatchIdsFromTracker(TrackerProfileResponse profile);

    /// <summary>
    /// Tracker Networkのプロフィールデータから統計情報へ変換
    /// </summary>
    /// <param name="profile">Tracker Networkのプロフィールデータ</param>
    /// <returns>統計情報、取得できない場合はnull</returns>
    PerformanceStats? ExtractPerformanceStatsFromTracker(TrackerProfileResponse profile);

    #endregion

    #region エンティティ変換・集計

    /// <summary>
    /// 試合データから特定プレイヤーのパフォーマンス統計を集計
    /// </summary>
    /// <param name="matches">試合データのリスト</param>
    /// <param name="performances">パフォーマンスデータのリスト</param>
    /// <param name="puuid">プレイヤーID</param>
    /// <param name="from">集計開始日時</param>
    /// <param name="to">集計終了日時</param>
    /// <returns>パフォーマンス統計情報</returns>
    PerformanceStats CalculatePerformanceStats(
        List<MatchEntity> matches,
        List<PlayerMatchPerformance> performances,
        string puuid,
        DateTime from,
        DateTime to);

    /// <summary>
    /// 複数プレイヤーのパフォーマンスをランキング化
    /// </summary>
    /// <param name="performances">プレイヤーパフォーマンスのリスト</param>
    /// <param name="playerNames">プレイヤー名のマップ（PUUID => 名前）</param>
    /// <param name="playerTags">プレイヤータグのマップ（PUUID => タグ）</param>
    /// <returns>ランキングデータ（KDA降順）</returns>
    List<(string Puuid, string PlayerName, string TagLine, float KdaRatio, int Kills, int Deaths, int Assists, int GamesPlayed)>
        CreateKdaRanking(
            List<PlayerMatchPerformance> performances,
            Dictionary<string, string> playerNames,
            Dictionary<string, string> playerTags);

    /// <summary>
    /// プレイヤーのエージェント別パフォーマンスを集計
    /// </summary>
    /// <param name="performances">プレイヤーパフォーマンスのリスト</param>
    /// <param name="matches">試合データのリスト</param>
    /// <param name="puuid">プレイヤーID</param>
    /// <returns>エージェント別パフォーマンスのマップ</returns>
    Dictionary<string, AgentPerformance> CalculateAgentPerformance(
        List<PlayerMatchPerformance> performances,
        List<MatchEntity> matches,
        string puuid);

    /// <summary>
    /// プレイヤーのマップ別パフォーマンスを集計
    /// </summary>
    /// <param name="performances">プレイヤーパフォーマンスのリスト</param>
    /// <param name="matches">試合データのリスト</param>
    /// <param name="puuid">プレイヤーID</param>
    /// <returns>マップ別パフォーマンスのマップ</returns>
    Dictionary<string, MapPerformance> CalculateMapPerformance(
        List<PlayerMatchPerformance> performances,
        List<MatchEntity> matches,
        string puuid);

    #endregion
}