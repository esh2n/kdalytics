# KDalytics API ドキュメント

KDalytics APIは、Valorantの戦績データを取得・管理するためのRESTful APIです。このドキュメントでは、利用可能なエンドポイント、リクエスト・レスポンスの形式、認証方法などについて説明します。

## 目次

- [基本情報](#基本情報)
- [認証](#認証)
- [プレイヤー関連API](#プレイヤー関連api)
- [試合関連API](#試合関連api)
- [パフォーマンス関連API](#パフォーマンス関連api)
- [エラーレスポンス](#エラーレスポンス)
- [データモデル](#データモデル)

## 基本情報

- **ベースURL**: `http://localhost:5167/api`（開発環境）
- **コンテンツタイプ**: `application/json`
- **文字コード**: UTF-8

## 認証

現在、APIは認証なしで利用可能です。将来的にはAPIキーまたはOAuth2認証が実装される予定です。

## プレイヤー関連API

プレイヤー情報の取得や管理を行うAPIです。

### プレイヤーIDでプレイヤー情報を取得

```
GET /players/{puuid}
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |

#### レスポンス

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gameName": "esh2n",
  "tagLine": "JP1",
  "region": "ap",
  "accountLevel": 150,
  "isTracked": true,
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない
- `500 Internal Server Error`: サーバー内部エラー

### 名前とタグでプレイヤー情報を検索

```
POST /players/search
```

#### リクエスト本文

```json
{
  "name": "esh2n",
  "tag": "JP1"
}
```

#### レスポンス

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gameName": "esh2n",
  "tagLine": "JP1",
  "region": "ap",
  "accountLevel": 150,
  "isTracked": true,
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `400 Bad Request`: リクエストパラメータが不正
- `404 Not Found`: 指定された名前とタグのプレイヤーが見つからない
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーのトラッキング設定を更新

```
POST /players/tracking
```

#### リクエスト本文

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "track": true
}
```

#### レスポンス

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gameName": "esh2n",
  "tagLine": "JP1",
  "region": "ap",
  "accountLevel": 150,
  "isTracked": true,
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `400 Bad Request`: リクエストパラメータが不正
- `404 Not Found`: 指定されたプレイヤーIDが見つからない
- `500 Internal Server Error`: サーバー内部エラー

### トラッキング対象のプレイヤー一覧を取得

```
GET /players/tracked
```

#### レスポンス

```json
[
  {
    "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "gameName": "esh2n",
    "tagLine": "JP1",
    "region": "ap",
    "accountLevel": 150,
    "isTracked": true,
    "lastUpdated": "2025-02-27T05:34:44Z"
  },
  {
    "puuid": "b2c3d4e5-f6g7-8901-hijk-lm2345678901",
    "gameName": "player2",
    "tagLine": "JP2",
    "region": "ap",
    "accountLevel": 120,
    "isTracked": true,
    "lastUpdated": "2025-02-27T05:34:44Z"
  }
]
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーのランク情報を取得

```
GET /players/{puuid}/rank
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |

#### レスポンス

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gameName": "esh2n",
  "tagLine": "JP1",
  "currentTier": 21,
  "currentTierName": "Diamond 1",
  "rankingInTier": 67,
  "mmr": 1750,
  "mmrChange": 15,
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない、またはランク情報が見つからない
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーのランク履歴を取得

```
GET /players/{puuid}/rank/history
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |
| from | string (optional) | 開始日時（ISO 8601形式、デフォルト: 30日前） |
| to | string (optional) | 終了日時（ISO 8601形式、デフォルト: 現在） |

#### レスポンス

```json
[
  {
    "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "gameName": "esh2n",
    "tagLine": "JP1",
    "currentTier": 21,
    "currentTierName": "Diamond 1",
    "rankingInTier": 67,
    "mmr": 1750,
    "mmrChange": 15,
    "lastUpdated": "2025-02-27T05:34:44Z"
  },
  {
    "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "gameName": "esh2n",
    "tagLine": "JP1",
    "currentTier": 20,
    "currentTierName": "Platinum 3",
    "rankingInTier": 95,
    "mmr": 1735,
    "mmrChange": 20,
    "lastUpdated": "2025-02-26T05:34:44Z"
  }
]
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない、または指定期間内のランク履歴が見つからない
- `500 Internal Server Error`: サーバー内部エラー

## 試合関連API

試合情報の取得や管理を行うAPIです。

### 試合IDで試合情報を取得

```
GET /matches/{matchId}
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| matchId | string | 試合の一意識別子 |

#### レスポンス

```json
{
  "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
  "mapId": "ascent",
  "mapName": "Ascent",
  "gameMode": "Competitive",
  "startTime": "2025-02-27T03:30:00Z",
  "gameLength": 2400000,
  "region": "ap",
  "seasonId": "act1-episode7",
  "teams": [
    {
      "teamId": "Blue",
      "hasWon": true,
      "roundsWon": 13
    },
    {
      "teamId": "Red",
      "hasWon": false,
      "roundsWon": 7
    }
  ],
  "players": [
    {
      "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
      "teamId": "Blue",
      "playerName": "esh2n",
      "tagLine": "JP1",
      "agentName": "Jett",
      "score": 320,
      "kills": 22,
      "deaths": 10,
      "assists": 5,
      "damageDealt": 4500,
      "headshotPercentage": 35.5
    }
    // 他のプレイヤー情報...
  ],
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定された試合IDが見つからない
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーの最近の試合を取得

```
GET /matches/player/{puuid}/recent
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |
| count | integer (optional) | 取得する試合数（デフォルト: 5） |

#### レスポンス

```json
[
  {
    "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
    "mapId": "ascent",
    "mapName": "Ascent",
    "gameMode": "Competitive",
    "startTime": "2025-02-27T03:30:00Z",
    "gameLength": 2400000,
    "region": "ap",
    "seasonId": "act1-episode7",
    "teams": [
      {
        "teamId": "Blue",
        "hasWon": true,
        "roundsWon": 13
      },
      {
        "teamId": "Red",
        "hasWon": false,
        "roundsWon": 7
      }
    ],
    "players": [
      // プレイヤー情報...
    ],
    "lastUpdated": "2025-02-27T05:34:44Z"
  }
  // 他の試合情報...
]
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない、または最近の試合が見つからない
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーの試合をフィルタして取得

```
POST /matches/filter
```

#### リクエスト本文

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "from": "2025-01-01T00:00:00Z",
  "to": "2025-02-27T00:00:00Z",
  "gameMode": "Competitive",
  "skip": 0,
  "take": 10
}
```

#### レスポンス

```json
[
  {
    "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
    "mapId": "ascent",
    "mapName": "Ascent",
    "gameMode": "Competitive",
    "startTime": "2025-02-27T03:30:00Z",
    "gameLength": 2400000,
    "region": "ap",
    "seasonId": "act1-episode7",
    "teams": [
      // チーム情報...
    ],
    "players": [
      // プレイヤー情報...
    ],
    "lastUpdated": "2025-02-27T05:34:44Z"
  }
  // 他の試合情報...
]
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `400 Bad Request`: リクエストパラメータが不正
- `404 Not Found`: 指定されたプレイヤーIDが見つからない、または条件に一致する試合が見つからない
- `500 Internal Server Error`: サーバー内部エラー

### 試合内のプレイヤーパフォーマンスを取得

```
GET /matches/{matchId}/performances
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| matchId | string | 試合の一意識別子 |

#### レスポンス

```json
[
  {
    "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
    "teamId": "Blue",
    "playerName": "esh2n",
    "tagLine": "JP1",
    "agentName": "Jett",
    "score": 320,
    "kills": 22,
    "deaths": 10,
    "assists": 5,
    "damageDealt": 4500,
    "headshotPercentage": 35.5
  }
  // 他のプレイヤーパフォーマンス情報...
]
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定された試合IDのパフォーマンス情報が見つからない
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーの特定試合でのパフォーマンスを取得

```
GET /matches/{matchId}/player/{puuid}
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| matchId | string | 試合の一意識別子 |
| puuid | string | プレイヤーの一意識別子 |

#### レスポンス

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
  "teamId": "Blue",
  "playerName": "esh2n",
  "tagLine": "JP1",
  "agentName": "Jett",
  "score": 320,
  "kills": 22,
  "deaths": 10,
  "assists": 5,
  "damageDealt": 4500,
  "headshotPercentage": 35.5
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定された試合IDとプレイヤーIDの組み合わせのパフォーマンス情報が見つからない
- `500 Internal Server Error`: サーバー内部エラー

### 日別の試合件数を取得

```
GET /matches/player/{puuid}/count-by-day
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |
| days | integer (optional) | 過去何日分を取得するか（デフォルト: 30） |

#### レスポンス

```json
{
  "2025-02-27": 5,
  "2025-02-26": 3,
  "2025-02-25": 7
  // 他の日付と試合数...
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない
- `500 Internal Server Error`: サーバー内部エラー

### マップごとの試合件数を取得

```
GET /matches/player/{puuid}/count-by-map
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |
| from | string (optional) | 開始日時（ISO 8601形式、デフォルト: 30日前） |
| to | string (optional) | 終了日時（ISO 8601形式、デフォルト: 現在） |

#### レスポンス

```json
{
  "Ascent": 10,
  "Bind": 8,
  "Haven": 12,
  "Split": 7,
  "Icebox": 5,
  "Breeze": 3,
  "Fracture": 2,
  "Pearl": 4,
  "Lotus": 6
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない
- `500 Internal Server Error`: サーバー内部エラー

## パフォーマンス関連API

プレイヤーのパフォーマンス統計情報を取得するAPIです。

### プレイヤーのパフォーマンス統計を取得

```
POST /performances/stats
```

#### リクエスト本文

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "from": "2025-01-01T00:00:00Z",
  "to": "2025-02-27T00:00:00Z",
  "gameMode": "Competitive"
}
```

#### レスポンス

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-02-27T00:00:00Z",
  "matchesPlayed": 50,
  "matchesWon": 30,
  "losses": 20,
  "winRate": 60.0,
  "totalKills": 950,
  "totalDeaths": 500,
  "totalAssists": 300,
  "averageKills": 19.0,
  "averageDeaths": 10.0,
  "averageAssists": 6.0,
  "kdRatio": 1.9,
  "kdaRatio": 2.5,
  "headshotPercentage": 32.5,
  "mostPlayedAgent": "Jett",
  "agentStats": {
    "Jett": {
      "agentName": "Jett",
      "gamesPlayed": 25,
      "gamesWon": 15,
      "losses": 10,
      "winRate": 60.0,
      "averageKills": 20.5,
      "averageDeaths": 9.5,
      "averageAssists": 4.5,
      "kdRatio": 2.16,
      "kdaRatio": 2.63,
      "headshotPercentage": 35.0
    },
    "Reyna": {
      "agentName": "Reyna",
      "gamesPlayed": 15,
      "gamesWon": 10,
      "losses": 5,
      "winRate": 66.7,
      "averageKills": 22.0,
      "averageDeaths": 11.0,
      "averageAssists": 3.0,
      "kdRatio": 2.0,
      "kdaRatio": 2.27,
      "headshotPercentage": 33.0
    }
    // 他のエージェント統計...
  },
  "mapStats": {
    "Ascent": {
      "mapName": "Ascent",
      "mapId": "ascent",
      "gamesPlayed": 12,
      "gamesWon": 8,
      "losses": 4,
      "winRate": 66.7,
      "averageKills": 18.5,
      "averageDeaths": 9.8,
      "averageAssists": 6.2,
      "kdRatio": 1.89,
      "kdaRatio": 2.52
    }
    // 他のマップ統計...
  }
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `400 Bad Request`: リクエストパラメータが不正
- `404 Not Found`: 指定されたプレイヤーIDが見つからない
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーのエージェント別パフォーマンスを取得

```
GET /performances/player/{puuid}/agents
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |
| from | string (optional) | 開始日時（ISO 8601形式、デフォルト: 30日前） |
| to | string (optional) | 終了日時（ISO 8601形式、デフォルト: 現在） |
| minGames | integer (optional) | 最低試合数（デフォルト: 0） |

#### レスポンス

```json
{
  "Jett": {
    "agentName": "Jett",
    "gamesPlayed": 25,
    "gamesWon": 15,
    "losses": 10,
    "winRate": 60.0,
    "averageKills": 20.5,
    "averageDeaths": 9.5,
    "averageAssists": 4.5,
    "kdRatio": 2.16,
    "kdaRatio": 2.63,
    "headshotPercentage": 35.0
  },
  "Reyna": {
    "agentName": "Reyna",
    "gamesPlayed": 15,
    "gamesWon": 10,
    "losses": 5,
    "winRate": 66.7,
    "averageKills": 22.0,
    "averageDeaths": 11.0,
    "averageAssists": 3.0,
    "kdRatio": 2.0,
    "kdaRatio": 2.27,
    "headshotPercentage": 33.0
  }
  // 他のエージェント統計...
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない、またはエージェント別パフォーマンス情報が見つからない
- `500 Internal Server Error`: サーバー内部エラー

### プレイヤーのマップ別パフォーマンスを取得

```
GET /performances/player/{puuid}/maps
```

#### パラメータ

| 名前 | 型 | 説明 |
|------|------|------|
| puuid | string | プレイヤーの一意識別子 |
| from | string (optional) | 開始日時（ISO 8601形式、デフォルト: 30日前） |
| to | string (optional) | 終了日時（ISO 8601形式、デフォルト: 現在） |
| minGames | integer (optional) | 最低試合数（デフォルト: 0） |

#### レスポンス

```json
{
  "Ascent": {
    "mapName": "Ascent",
    "mapId": "ascent",
    "gamesPlayed": 12,
    "gamesWon": 8,
    "losses": 4,
    "winRate": 66.7,
    "averageKills": 18.5,
    "averageDeaths": 9.8,
    "averageAssists": 6.2,
    "kdRatio": 1.89,
    "kdaRatio": 2.52
  },
  "Bind": {
    "mapName": "Bind",
    "mapId": "bind",
    "gamesPlayed": 10,
    "gamesWon": 6,
    "losses": 4,
    "winRate": 60.0,
    "averageKills": 17.2,
    "averageDeaths": 10.5,
    "averageAssists": 5.8,
    "kdRatio": 1.64,
    "kdaRatio": 2.19
  }
  // 他のマップ統計...
}
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `404 Not Found`: 指定されたプレイヤーIDが見つからない、またはマップ別パフォーマンス情報が見つからない
- `500 Internal Server Error`: サーバー内部エラー

### 複数プレイヤーのKDAランキングを取得

```
POST /performances/kda-ranking
```

#### リクエスト本文

```json
{
  "puuids": [
    "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "b2c3d4e5-f6g7-8901-hijk-lm2345678901",
    "c3d4e5f6-g7h8-9012-nopq-rs3456789012"
  ],
  "from": "2025-01-01T00:00:00Z",
  "to": "2025-02-27T00:00:00Z",
  "gameMode": "Competitive",
  "minGames": 5
}
```

#### レスポンス

```json
[
  {
    "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "playerName": "esh2n",
    "tagLine": "JP1",
    "kdaRatio": 2.5,
    "kills": 950,
    "deaths": 500,
    "assists": 300,
    "gamesPlayed": 50
  },
  {
    "puuid": "b2c3d4e5-f6g7-8901-hijk-lm2345678901",
    "playerName": "player2",
    "tagLine": "JP2",
    "kdaRatio": 2.2,
    "kills": 800,
    "deaths": 450,
    "assists": 250,
    "gamesPlayed": 45
  },
  {
    "puuid": "c3d4e5f6-g7h8-9012-nopq-rs3456789012",
    "playerName": "player3",
    "tagLine": "JP3",
    "kdaRatio": 1.9,
    "kills": 700,
    "deaths": 480,
    "assists": 200,
    "gamesPlayed": 40
  }
]
```

#### ステータスコード

- `200 OK`: リクエスト成功
- `400 Bad Request`: リクエストパラメータが不正
- `500 Internal Server Error`: サーバー内部エラー

## エラーレスポンス

エラーが発生した場合、APIは以下の形式でエラー情報を返します。

```json
{
  "status": 404,
  "message": "プレイヤーID 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' が見つかりません。"
}
```

## データモデル

### プレイヤー情報

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gameName": "esh2n",
  "tagLine": "JP1",
  "region": "ap",
  "accountLevel": 150,
  "isTracked": true,
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

### プレイヤーランク情報

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "gameName": "esh2n",
  "tagLine": "JP1",
  "currentTier": 21,
  "currentTierName": "Diamond 1",
  "rankingInTier": 67,
  "mmr": 1750,
  "mmrChange": 15,
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

### 試合情報

```json
{
  "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
  "mapId": "ascent",
  "mapName": "Ascent",
  "gameMode": "Competitive",
  "startTime": "2025-02-27T03:30:00Z",
  "gameLength": 2400000,
  "region": "ap",
  "seasonId": "act1-episode7",
  "teams": [
    {
      "teamId": "Blue",
      "hasWon": true,
      "roundsWon": 13
    },
    {
      "teamId": "Red",
      "hasWon": false,
      "roundsWon": 7
    }
  ],
  "players": [
    {
      "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
      "teamId": "Blue",
      "playerName": "esh2n",
      "tagLine": "JP1",
      "agentName": "Jett",
      "score": 320,
      "kills": 22,
      "deaths": 10,
      "assists": 5,
      "damageDealt": 4500,
      "headshotPercentage": 35.5
    }
    // 他のプレイヤー情報...
  ],
  "lastUpdated": "2025-02-27T05:34:44Z"
}
```

### プレイヤーパフォーマンス情報

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
  "teamId": "Blue",
  "playerName": "esh2n",
  "tagLine": "JP1",
  "agentName": "Jett",
  "score": 320,
  "kills": 22,
  "deaths": 10,
  "assists": 5,
  "damageDealt": 4500,
  "headshotPercentage": 35.5
}
```

### パフォーマンス統計情報

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-02-27T00:00:00Z",
  "matchesPlayed": 50,
  "matchesWon": 30,
  "losses": 20,
  "winRate": 60.0,
  "totalKills": 950,
  "totalDeaths": 500,
  "totalAssists": 300,
  "averageKills": 19.0,
  "averageDeaths": 10.0,
  "averageAssists": 6.0,
  "kdRatio": 1.9,
  "kdaRatio": 2.5,
  "headshotPercentage": 32.5,
  "mostPlayedAgent": "Jett",
  "agentStats": {
    // エージェント別統計...
  },
  "mapStats": {
    // マップ別統計...
  }
}
```

### エージェント別パフォーマンス情報

```json
{
  "agentName": "Jett",
  "gamesPlayed": 25,
  "gamesWon": 15,
  "losses": 10,
  "winRate": 60.0,
  "averageKills": 20.5,
  "averageDeaths": 9.5,
  "averageAssists": 4.5,
  "kdRatio": 2.16,
  "kdaRatio": 2.63,
  "headshotPercentage": 35.0
}
```

### マップ別パフォーマンス情報

```json
{
  "mapName": "Ascent",
  "mapId": "ascent",
  "gamesPlayed": 12,
  "gamesWon": 8,
  "losses": 4,
  "winRate": 66.7,
  "averageKills": 18.5,
  "averageDeaths": 9.8,
  "averageAssists": 6.2,
  "kdRatio": 1.89,
  "kdaRatio": 2.52
}
```

### KDAランキング情報

```json
{
  "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "playerName": "esh2n",
  "tagLine": "JP1",
  "kdaRatio": 2.5,
  "kills": 950,
  "deaths": 500,
  "assists": 300,
  "gamesPlayed": 50
}