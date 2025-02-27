Henrik VALORANT API 詳細仕様書
概要
Henrik VALORANT APIは、非公式のVALORANT統計データAPIです。プレイヤーのプロフィール情報やマッチ履歴、ランク情報、ストア情報など、様々なゲームデータを取得できます。本APIを利用するにはAPIキーが必要であり、全てのリクエストでHTTPヘッダーのAuthorizationにAPIキーを指定します（例：Authorization: YOUR_API_KEY）。エンドポイントのベースURLは https://api.henrikdev.xyz です。リージョン指定: リージョンを指定するパラメータは多くのエンドポイントで必要です。リージョンは次から選択します: na（北米）、eu（ヨーロッパ）、ap（アジア太平洋）、kr（韓国）、latam（ラテンアメリカ）、br（ブラジル）。なお、一部エンドポイントではlatamやbrを内部でnaに統合する処理が行われます​
DOCS.HENRIKDEV.XYZ
。プラットフォーム指定: 一部のエンドポイント（v3やv4と表記される最新バージョン）ではpcまたはconsoleといったプラットフォーム指定が必要です​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。v1やv2の旧バージョンではプラットフォーム指定がなくPCのみ対応でしたが、v3以降ではコンソールのデータ取得にも対応しています。戻り値: APIは通常JSON形式のデータを返します（クロスヘア画像生成APIを除く）。成功時のJSONにはstatusフィールド（例: status: 1やstatus: 200）とdataフィールドが含まれます。エラー時にはHTTPステータスコードに応じたJSONが返り、statusにエラーコード、errors配列に詳細なエラーメッセージ等が入ります。例えば、存在しないプレイヤー名を指定するとHTTP 404となり、レスポンスボディには以下のような情報が含まれます（例）:
json
コピーする
編集する
{
  "status": 404,
  "errors": [
    {
      "message": "Player not found",
      "code": 404,
      "details": "The requested player does not exist."
    }
  ]
}
共通して発生しうるエラーコードと意味は以下の通りです​
DOCS.HENRIKDEV.XYZ
:
400 Bad Request: リクエスト内容に誤り（必須パラメータの欠落など）がある​
DOCS.HENRIKDEV.XYZ
。
403 Forbidden: Riot公式API側がメンテナンス中、またはHenrikDev API側でアクセスがブロックされた場合。
404 Not Found: リソースが見つからない（例: 存在しないプレイヤー名やマッチID）。
408 Request Timeout: データ取得がタイムアウトした場合。
410 Gone: エンドポイントが廃止され利用できない場合​
DOCS.HENRIKDEV.XYZ
。
429 Too Many Requests: レートリミット超過。Basicキーでは1分間に30リクエスト、Advancedキーでは90リクエストの上限があります。
501 Not Implemented: エンドポイントのバージョンが未実装の場合​
DOCS.HENRIKDEV.XYZ
。
503 Service Unavailable: Riot API自体がダウンしているなどでデータ取得不可。
以下、カテゴリ別にすべてのエンドポイントの詳細な仕様を説明します。
アカウント情報API
アカウント情報APIでは、プレイヤーの名前（ゲーム内表示名）からプライヤーID（PUUID）やレベルなどの基本情報を取得できます。またPUUIDから逆引きで名前を取得するエンドポイントも提供されています。バージョン2（v2）はコンソール版を含むための改良版です（v1はPCのみ対応）。
GET /valorant/v1/account/{name}/{tag}
指定したプレイヤーのアカウント詳細を取得します（v1版、PCのみ）。プレイヤーのゲーム内名 {name} とタグライン {tag}（例：「PlayerName#TAG」の「PlayerName」と「TAG」部分）をパスパラメータで指定します。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/account/{name}/{tag}
パスパラメータ:
name (string, 必須) – プレイヤー名
tag (string, 必須) – タグライン（#以降のプレイヤー識別子）
クエリパラメータ:
force (boolean, オプション) – trueにすると強制的に最新データを取得します（キャッシュを無視してRiot APIから再取得）。
レスポンス: アカウントのプロフィール情報を含むJSONオブジェクト。
レスポンス例: 成功時（HTTP 200）の例を示します。
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "puuid": "123e4567-e89b-12d3-a456-426614174000",
    "region": "eu",
    "account_level": 100,
    "name": "PlayerName",
    "tag": "TAG",
    "card": {
      "small": "https://images.example.com/smallcard.png",
      "large": "https://images.example.com/largecard.png",
      "wide": "https://images.example.com/widecard.png",
      "id": "faf239d5-43b2-cable-8fe0-bf8e78934567"
    },
    "last_update": "2025-02-26T13:07:00.687Z",
    "last_update_raw": 1677414420
  }
}
上記の例では、account_levelはアカウントレベル、puuidはプレイヤー固有ID、cardは現在設定されているプレイヤーカードの画像URLやIDなどの情報です。last_updateはこのデータの最終更新日時です。
エラーレスポンス:
404エラー（プレイヤーが存在しない場合）の例:
json
コピーする
編集する
{
  "status": 404,
  "errors": [ { "message": "Player not found", "code": 404 } ]
}
その他、共通エラーコードに従ったJSONが返ります（詳細は前述の共通エラーを参照）。
GET /valorant/v2/account/{name}/{tag}
v1の改良版で、コンソール版でのプレイも反映されるアカウント詳細（v2）です。基本的な使い方はv1と同じですが、v2ではレスポンスにタイトル（称号）やプラットフォーム情報が追加されています。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v2/account/{name}/{tag}
パスパラメータ: name, tag（どちらもv1と同様）
クエリパラメータ: force (boolean, 任意) – 強制データ更新フラグ（同上）。
レスポンス: v1と同様のアカウント情報に加え、コンソール/PC情報などが含まれます。
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "puuid": "123e4567-e89b-12d3-a456-426614174000",
    "region": "na",
    "account_level": 150,
    "name": "SampleUser",
    "tag": "GAME",
    "card": {
      "small": "https://images.example.com/card_small.png",
      "large": "https://images.example.com/card_large.png",
      "wide": "https://images.example.com/card_wide.png",
      "id": "47f1ab3c-4120-4fe7-be56-12345abcdef"
    },
    "title": "The Unstoppable", 
    "platforms": [ "pc" ],
    "last_update": "2025-02-27T10:00:00.000Z",
    "last_update_raw": 1677498000
  }
}
ここでtitleはプレイヤーがプロフィールに設定しているタイトル（称号）で、platformsは最近プレイしたプラットフォームのリストです（例では"pc"）。
備考: 基本的にv2の使用が推奨されます。v1も使用できますが、新データ取得やコンソール対応を考慮するとv2を使う方が良いでしょう。
GET /valorant/v1/by-puuid/account/{puuid}
プレイヤーの**PUUID（固有ID）**からアカウント情報を取得します。例えばマッチ情報から得たPUUIDを元に、そのプレイヤーの名前やプロフィールを知りたい場合に利用できます。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/by-puuid/account/{puuid}
パスパラメータ:
puuid (string, 必須) – プレイヤーのPUUID
クエリパラメータ:
force (boolean, 任意) – 強制データ更新フラグ（前述）。
レスポンス: 指定PUUIDのアカウント情報（v1形式）がJSONで返ります。
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "puuid": "123e4567-e89b-12d3-a456-426614174000",
    "region": "ap",
    "account_level": 42,
    "name": "AnotherPlayer",
    "tag": "XYZ",
    "card": {
      "small": "https://images.example.com/card2_small.png",
      "large": "https://images.example.com/card2_large.png",
      "wide": "https://images.example.com/card2_wide.png",
      "id": "9ab2cd34-5678-90ef-gh12-34567ijklmno"
    },
    "last_update": "2025-02-20T08:30:00.000Z",
    "last_update_raw": 1676872200
  }
}
備考: 取得できる内容は/account/{name}/{tag}と同様ですが、こちらは名前ではなくPUUIDで検索できる点が異なります。
GET /valorant/v2/by-puuid/account/{puuid}
PUUIDからのアカウント情報取得のv2版です。使い方はv1と同様で、コンソール対応などの拡張が含まれます。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v2/by-puuid/account/{puuid}
パスパラメータ: puuid (必須)
クエリパラメータ: force (任意)
レスポンス: /valorant/v2/account/{name}/{tag}と同様のフォーマットで、該当PUUIDの情報を返します。
レスポンス例: （省略：内容は「アカウント情報API v2」の例と同様のフィールドを持ちます）
備考: 特定のPUUIDのプレイヤー名やタグがわからない場合でも、このエンドポイントを使えば名前とタグを取得可能です。
コンテンツ情報API
コンテンツAPIではゲーム内の静的コンテンツ情報（エージェント、マップ、武器スキンなど）を取得します。主にIDや名前、各言語へのローカライズ名を含むデータをまとめて返します。
GET /valorant/v1/content
ゲームの最新コンテンツ一覧を取得します。キャラクター（エージェント）、マップ、武器スキン、スプレーなどの情報が一度に得られます。基本的にゲームデータのマスタ情報と考えてください。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/content
クエリパラメータ:
locale (string, オプション) – 言語ロケールを指定できます。指定しない場合はデフォルト（英語）の名称を返します。例: ja-JP を指定すると日本語名を含むデータが得られます。対応言語は en-US、ja-JP、fr-FR など多数。
レスポンス: ゲーム内コンテンツ情報を包括するJSON。主なフィールド:
version: データバージョン（ゲームバージョンに対応）。
characters: エージェント一覧。
maps: マップ一覧。
chromas: 武器スキンのカラーバリエーション一覧。
skins: 武器スキン一覧。
skinLevels: スキンの各レベル情報一覧。
equips: 装備品一覧（近接武器など）。
gameModes: ゲームモード一覧。
sprays: スプレー一覧。
sprayLevels: スプレーのレベル一覧。
charms: 武器チャーム一覧。
charmLevels: チャームのレベル一覧。
playerCards: プレイヤーカード一覧。
playerTitles: プレイヤータイトル（称号）一覧。
acts: 現行および過去のエピソード・アクト情報。
レスポンス例: （一部抜粋）
json
コピーする
編集する
{
  "version": "release-04.08",
  "characters": [
    {
      "name": "Jett",
      "id": "add6443a-41bd-e414-f6ad-e58d267f4e95",
      "assetName": "Default__Jett_PrimaryAsset_C",
      "assetPath": ".../ShooterGame/Content/Characters/Jett/Jett_PrimaryAsset.uasset",
      "localizedNames": {
        "ja-JP": "ジェット",
        "en-US": "Jett",
        "fr-FR": "Jett",
        "...": "..."
      }
    },
    ... （エージェント一覧 他の要素） 
  ],
  "maps": [
    {
      "name": "Ascent",
      "id": "7eaeadd1-4e01-4f3d-a6d1-94444750ce14",
      "assetName": "Default__Ascent_MapAsset_C",
      "assetPath": ".../Maps/Ascent/Ascent_MapAsset.uasset",
      "localizedNames": {
        "ja-JP": "アセント",
        "en-US": "Ascent",
        "fr-FR": "Ascent",
        "...": "..."
      }
    },
    ... （マップ一覧 続く）
  ],
  ... （その他フィールド省略） ...
  "acts": [
    {
      "id": "97b6e739-44cc-ffa7-49ad-398ba502ceb0",
      "name": "Episode 1: Ignition",
      "localizedNames": {
        "ja-JP": "エピソード1: イグニッション",
        "en-US": "Episode 1: Ignition",
        "...": "..."
      },
      "isActive": false
    },
    ... （複数のAct情報）
  ]
}
上記のように、各種コンテンツのIDや名称が網羅されています。例えばエージェント「ジェット」のデータからは、その内部ID（UUID）やゲーム内資産名（assetName）、日本語名や英語名などを取得できます。注: 画像など実際のアセットデータが必要な場合、Henrik APIではなく公式のstatic Valorant API（valorant-api.comなど）を参照することが推奨されています。
エラーハンドリング: 基本的にこのエンドポイントではエラーは発生しにくいですが、クエリに不正なロケールを指定した場合などは400エラーとなる可能性があります。レートリミット超過時は429、サーバエラー時は500が返る場合があります。
クロスヘア生成API
クロスヘアAPIでは、ゲーム内のクロスヘア設定コードからクロスヘア画像を生成できます​
DOCS.HENRIKDEV.XYZ
。指定したクロスヘアID（ゲーム内の共有コード）に対応する**PNG画像（1024x1024）**を返すエンドポイントです​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v1/crosshair/generate
指定したクロスヘアコードの画像を生成します​
DOCS.HENRIKDEV.XYZ
。このエンドポイントは他のエンドポイントと異なり画像データ（PNG）が直接レスポンスとして返ります​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/crosshair/generate
クエリパラメータ:
id (string, 必須) – クロスヘアの共有コードID​
DOCS.HENRIKDEV.XYZ
。ゲーム内のクロスヘア設定で取得できる文字列です。例: 0;P;h;0;0t;0;0l;0;0o;1;0a;1;0f;0;1b;0 のような形式。
レスポンス: image/png 形式の画像データ（1024x1024ピクセル）​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。ブラウザで直接アクセスするとPNG画像が表示されます。プログラムから呼び出す場合はバイナリデータとして取得してください。
使用例:
リクエスト: GET /valorant/v1/crosshair/generate?id=0%3BP%3Bh%3B0%3B0t%3B0%3B0l%3B0%3B0o%3B1%3B0a%3B1%3B0f%3B0%3B1b%3B0
レスポンス: クロスヘア画像（PNGバイナリ）
エラー:
必須パラメータidが欠如している場合、HTTP 400 （Bad Request）が返り、JSON形式のエラーメッセージを含みます​
DOCS.HENRIKDEV.XYZ
。
存在しない/不正なクロスヘアIDの場合は404エラーとなる可能性があります​
DOCS.HENRIKDEV.XYZ
。
それ以外のエラー（レート制限やサーバ障害）は他エンドポイントと同様のHTTPステータスコード（429, 503等）が返されます​
DOCS.HENRIKDEV.XYZ
。
補足: このエンドポイントの実装上の都合により、将来的にURLや仕様が変更される可能性がある旨が公式ドキュメントに記載されています​
DOCS.HENRIKDEV.XYZ
。利用の際は最新情報に注意してください。
eスポーツ関連API
eスポーツAPIでは、公式大会の試合スケジュール情報など**競技シーン（プロシーン）**に関するデータを取得できます。現在は主に大会スケジュールの取得が可能です。
GET /valorant/v1/esports/schedule
直近および今後予定されている公式大会のスケジュール情報を取得します。地域やリーグでフィルタすることも可能です。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/esports/schedule
クエリパラメータ:
region (string, オプション) – 地域フィルタ。国際大会ならinternational、地域別ならnorth americaやjapan等を指定できます。
league (string, オプション) – リーグフィルタ。例: vct_americas, challengers_jpn, game_changers_emea 等。リーグ名は大会シリーズに応じて変わるため動的です。複数リーグをまたぐ場合は該当リーグの組み合わせを指定します。
レスポンス: スケジュール情報のリストがJSONで返ります。日付、試合の状態、種類、動画リンク、所属リーグ/大会名、対戦チーム情報などが含まれます。
レスポンス例: （一部省略）
json
コピーする
編集する
{
  "status": 1,
  "data": [
    {
      "date": "2023-01-17T20:00:00Z",
      "state": "completed",
      "type": "match",
      "vod": "https://youtu.be/PrQ-LBZ4W-E",
      "league": {
        "name": "Challengers DACH",
        "identifier": "vrl_dach",
        "icon": "https://static.lolesports.com/leagues/1672932144616_DACH_ICON_400_400.png",
        "region": "EMEA"
      },
      "tournament": {
        "name": "challengers_emea_leagues_split_1",
        "season": "2023"
      },
      "match": {
        "id": "109625073196211557",
        "game_type": { "count": 2, "type": "playAll" },
        "teams": [
          {
            "name": "Angry Titans",
            "code": "AT",
            "icon": "https://static.lolesports.com/teams/1644488801867_AT_red_icon2x.png",
            "has_won": false,
            "game_wins": 0,
            "record": { "wins": 0, "losses": 0 }
          },
          {
            "name": "Wave Esports",
            "code": "WAVE",
            "icon": "https://static.lolesports.com/teams/1644510664057_WAVE_Icon_Fullcolor.png",
            "has_won": true,
            "game_wins": 2,
            "record": { "wins": 1, "losses": 0 }
          }
        ]
      }
    },
    ... （他の試合データ）
  ]
}
この例では、2023年1月17日に行われたChallengers DACHの試合の情報が含まれています。各試合には開始日時 (date)、試合状態 (state – completedは終了)、動画リンク (vod)、所属リーグ (league)および大会 (tournament)、対戦カードの詳細 (match)が含まれています。teams配列内にチーム名や勝敗結果、戦績が示されます。
エラーハンドリング: 地域やリーグ名が不正な場合は400エラーが返ることがあります。データが存在しない組み合わせの場合は404となる可能性があります。その他、共通のエラーコードが適用されます（429, 503等）。
ランキングAPI（リーダーボード）
**ランキングAPI（Leaderboard）では、コンペティティブモードの地域別上位ランク（レーディング）**のリーダーボードを取得できます。各地域の上位プレイヤーの順位やレーティングポイント、勝利数などを確認可能です。注意: v1エンドポイントは古く非推奨となっており、v3エンドポイントへの移行が推奨されています。v3ではプラットフォーム（PC/コンソール）別のランキング取得に対応しています。
GET /valorant/v3/leaderboard/{region}/{platform}
指定したリージョン・プラットフォームにおける現在シーズンのトップランカー一覧（リーダーボード）を取得します​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v3/leaderboard/{region}/{platform}
パスパラメータ:
region (string, 必須) – リージョン。例: eu, na, ap など。
platform (string, 必須) – プラットフォーム​
DOCS.HENRIKDEV.XYZ
。pc または console を指定。
クエリパラメータ:
puuid (string, オプション) – 特定のプレイヤーのPUUID。これを指定すると、そのプレイヤーを含むページのみ取得します（※nameとtagと同時指定は不可）。
name (string, オプション) – 特定のプレイヤー名。tagとセットで指定し、そのプレイヤーの順位周辺を取得します（※puuidと同時指定不可）。
tag (string, オプション) – 特定のプレイヤータグ。上記nameと組み合わせて使用。
season (string, オプション) – シーズンを指定。指定するとそのシーズンの最終リーダーボードを取得します。シーズンはエピソードとアクトの短縮形で指定（例: e4a3 はエピソード4・アクト3）。指定しない場合は現在進行中のシーズンのリアルタイム順位を取得します。
レスポンス: ランキングデータのJSON。players配列に各プレイヤーの情報が含まれます。その中の主なフィールド:
rankまたはleaderboardRank: 順位。
gameNameまたはname: プレイヤー名。
tagLineまたはtag: タグライン。
rankedRatingまたはelo: レートポイント（RR、エローレーティング値）。
numberOfWins: 勝利数。
competitiveTier: 現在のランクTier（数値。例えば24ならImmortal、25ならRadiant等）。
mmr (存在する場合): 内部MMR値。
レスポンス例: （上位1名のみ抜粋）
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "players": [
      {
        "leaderboardRank": 1,
        "rankedRating": 100,
        "numberOfWins": 200,
        "competitiveTier": 25,
        "gameName": "TopPlayer",
        "tagLine": "AAA"
      },
      {
        "leaderboardRank": 2,
        "rankedRating": 98,
        "numberOfWins": 198,
        "competitiveTier": 25,
        "gameName": "SecondBest",
        "tagLine": "BBB"
      },
      ... （以下略）
    ],
    "totalPlayers": 500,
    "immortalStartingPage": 1,
    "immortalStartingIndex": 1,
    "topTierRRThreshold": 450
  }
}
上記例では、地域内1位のプレイヤー「TopPlayer#AAA」のレートが100RR、勝利数200、ランクTierが25（Radiant）であることが分かります。totalPlayersはリーダーボード掲載対象の総プレイヤー数です。
備考: seasonを指定すると終了シーズンの固定結果を取得できますが、現在シーズンに対しては常にリアルタイムで変動する順位を取得します。
エラーハンドリング: リージョンやプラットフォームが無効な場合は400が返ります。指定したプレイヤーやシーズンが見つからなければ404です。基本的なエラーコードは共通です（429のレート制限など）​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v1/leaderboard/{region} (非推奨)
v3とほぼ同様の機能を持つ旧エンドポイントです​
DOCS.HENRIKDEV.XYZ
。プラットフォーム指定がなくPCのみ対象であり、データ形式も若干異なる可能性があります。新規開発では原則として使用せず、上記v3エンドポイントを使用してください。v1特有のパラメータとしてstartIndexやsizeを指定してページネーションを行う仕様がありましたが、現在は廃止されています。
マッチ履歴API（Matchlist）
マッチ履歴APIでは、指定したプレイヤーの過去のマッチリスト（マッチID一覧および各試合の概略データ）を取得できます​
DOCS.HENRIKDEV.XYZ
。プレイヤーを特定する方法として、プレイヤー名+タグ、またはPUUIDを使用できます。最新バージョンではコンソール/PCの切替が可能です。エンドポイントのバージョン: v3およびv4が存在します。v3はPCのみ、v4はプラットフォーム指定が追加された最新版です​
DOCS.HENRIKDEV.XYZ
。以下では最新のv4を中心に説明し、適宜v3との差異に触れます。
GET /valorant/v4/matches/{region}/{platform}/{name}/{tag}
指定したプレイヤー（名前+タグ）の最近のマッチ履歴を取得します​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。取得できる試合数は最大10件で、モードやマップでのフィルタリングも可能です。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v4/matches/{region}/{platform}/{name}/{tag}
パスパラメータ:
region (string, 必須) – リージョン（前述のリージョン指定を参照）​
DOCS.HENRIKDEV.XYZ
platform (string, 必須) – プラットフォーム（pc または console）​
DOCS.HENRIKDEV.XYZ
name (string, 必須) – プレイヤー名​
DOCS.HENRIKDEV.XYZ
tag (string, 必須) – プレイヤータグ​
DOCS.HENRIKDEV.XYZ
クエリパラメータ:
mode (string, オプション) – ゲームモードでフィルタ​
DOCS.HENRIKDEV.XYZ
。例: competitive（ランクマ）, unrated（アンレート）, deathmatch（デスマッチ）等​
DOCS.HENRIKDEV.XYZ
。指定するとそのモードの試合のみ取得。
map (string, オプション) – マップ名でフィルタ​
DOCS.HENRIKDEV.XYZ
。例: Ascent, Icebox, Lotus 等​
DOCS.HENRIKDEV.XYZ
。
size (integer, オプション) – 取得する試合数​
DOCS.HENRIKDEV.XYZ
。1～10まで指定可（デフォルト10）。
start (integer, オプション) – ページネーション用の開始インデックス​
DOCS.HENRIKDEV.XYZ
。過去の試合を遡る場合に使用。
レスポンス: dataフィールドにマッチの配列を含むJSON。各試合ごとに以下のような概要情報を含みます:
metadata: マッチIDやマップ、モード、開始時間など試合メタ情報。
players: 試合に参加したプレイヤーの基本情報とスタッツ（キル/デスなどの成績サマリ）。
teams: チームごとのスコア情報（総ラウンド獲得数等）。
rounds（場合により存在）: ラウンドごとの結果要約。
注: このエンドポイントのレスポンスには各マッチの詳細データ（ラウンド毎のキルなど）は含まれません。それら詳細は後述の「マッチ詳細API」で取得します。本エンドポイントはあくまで履歴一覧と各試合の概要です。
レスポンス例: （data配列内の単一マッチ例）
json
コピーする
編集する
{
  "status": 1,
  "data": [
    {
      "metadata": {
        "matchid": "a1b2c3d4-5678-90ab-cdef-111213141516",
        "map": "Icebox",
        "game_version": "release-04.08",
        "game_length": 2160000,
        "game_start": "2025-02-25T12:00:00Z",
        "queue": "competitive",
        "mode": "Standard"
      },
      "teams": [
        { "teamId": "Red", "won": true, "rounds_won": 13, "rounds_lost": 8 },
        { "teamId": "Blue", "won": false, "rounds_won": 8, "rounds_lost": 13 }
      ],
      "players": [
        {
          "puuid": "123e4567-e89b-12d3-a456-426614174000",
          "name": "PlayerOne",
          "tag": "AAA",
          "team": "Red",
          "character": "Sova",
          "currenttier": 18,
          "currenttier_patched": "Diamond 3",
          "stats": {
            "kills": 20,
            "deaths": 15,
            "assists": 5,
            "score": 2700
          }
        },
        {
          "puuid": "...",
          "name": "PlayerTwo",
          "tag": "BBB",
          "team": "Blue",
          "character": "Jett",
          "currenttier": 17,
          "currenttier_patched": "Diamond 2",
          "stats": {
            "kills": 18,
            "deaths": 18,
            "assists": 5,
            "score": 2500
          }
        },
        ... （他のプレイヤー7名省略）
      ]
    },
    ... （他の試合項目が続く 最大指定数分）
  ]
}
この例では、Iceboxで行われたコンペティティブの試合概要が示されています。teamsでRedとBlueチームのラウンドスコアが分かり、playersリストで各参加者のエージェント（character）やランク（currenttier_patched）、キル/デス/アシスト数などが確認できます。
備考: currenttier_patchedはランクを人間可読な名前で示したものです（例: 18 -> "Diamond 3"）。
エラーハンドリング: プレイヤー名/タグに誤りがある場合や履歴が存在しない場合は404エラーとなります​
DOCS.HENRIKDEV.XYZ
。パラメータの指定が不正な場合は400、レート超過時は429が返ります​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。v4エンドポイントが未実装な地域の場合、501エラーが返る可能性があります​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v4/by-puuid/matches/{region}/{platform}/{puuid}
PUUIDを用いてマッチ履歴を取得するエンドポイントです​
DOCS.HENRIKDEV.XYZ
。基本仕様は前述の/matches/{name}/{tag}と同様ですが、プレイヤー識別にPUUIDを使います。例えばプレイヤーネームが不明な場合やPUUIDしか保持していない場合に有用です。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v4/by-puuid/matches/{region}/{platform}/{puuid}
パスパラメータ: region, platform, puuid（すべて必須）。
クエリパラメータ: mode, map, size, start（機能は前述と同じ）。
レスポンス: 指定PUUIDに紐づくプレイヤーのマッチ履歴（JSON形式、内容は/matches/{name}/{tag}と同様）。
その他: 名前ではなくPUUIDで検索する点を除き機能は同等です。
GET /valorant/v3/matches/{region}/{name}/{tag} (旧バージョン)
プラットフォーム指定のない旧バージョンです​
DOCS.HENRIKDEV.XYZ
。基本的にPC版の履歴のみ取得します。v4とレスポンス構造は似ていますが、platformパスがない点と、コンソールプレイヤーのデータが含まれない点が異なります。新規実装では通常v4を使用し、このv3は互換用と考えてください。
GET /valorant/v3/by-puuid/matches/{region}/{puuid} (旧バージョン)
同様に、PUUID指定の旧バージョンです。機能はv4版のPUUIDエンドポイントに準じますが、PCのみ対応です。
マッチ詳細API
マッチ詳細APIでは、特定のマッチIDに対する詳細な試合データを取得できます​
DOCS.HENRIKDEV.XYZ
。マッチ履歴APIが試合の概要一覧を返すのに対し、マッチ詳細APIは個別試合のラウンド毎のイベントや各プレイヤーの細かな統計を含む完全な試合レコードを返します。エンドポイントのバージョン: v2およびv4が存在します。v2はリージョン指定不要でしたが、v4ではregionをパスに含める点が異なります​
DOCS.HENRIKDEV.XYZ
。ここでは最新のv4を説明します。
GET /valorant/v4/match/{region}/{matchid}
指定したマッチIDの試合詳細データを取得します​
DOCS.HENRIKDEV.XYZ
。マッチIDはマッチ履歴APIなどから取得できる一意の試合識別子です。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v4/match/{region}/{matchid}
パスパラメータ:
region (string, 必須) – 試合が行われたリージョン​
DOCS.HENRIKDEV.XYZ
。例えば、マッチIDだけではリージョンが判別できない場合があるため、この指定が必要です。
matchid (string, 必須) – 取得したい試合のID（UUID形式）​
DOCS.HENRIKDEV.XYZ
。
クエリパラメータ: なし。
レスポンス: 試合詳細データのJSON。主な内容:
metadata: 試合情報（マッチID、サーバ地域、モード、マップ、開始時間、所要時間など）。
players: 両チーム全プレイヤーの詳細統計（エージェント、武器使用状況、各ラウンドごとのパフォーマンスなど）。
teams: チーム情報（最終スコア、勝利チーム）。
rounds: 全ラウンドの詳細。一つ一つのラウンドについて、開始・終了状態、各キルイベント（誰が誰を倒したか、ヘッドショット/ボディショット/レッグショット数）、スパイク設置/解除などのイベントが含まれる。
その他、ラウンド間経済情報など必要に応じて追加のデータが入ります。
レスポンス例: （概要部分のみ抜粋）
json
コピーする
編集する
{
  "status": 200,
  "data": {
    "metadata": {
      "matchid": "a1b2c3d4-5678-90ab-cdef-111213141516",
      "region": "AP",
      "map": "Icebox",
      "game_length": 2187453,
      "game_start": "2025-02-25T12:00:00Z",
      "game_version": "release-04.08",
      "mode": "Standard",
      "queue": "competitive"
    },
    "teams": [
      { "teamId": "Red", "won": true, "rounds_won": 13, "rounds_lost": 11 },
      { "teamId": "Blue", "won": false, "rounds_won": 11, "rounds_lost": 13 }
    ],
    "players": [
      {
        "puuid": "123e4567-e89b-12d3-a456-426614174000",
        "name": "PlayerOne",
        "tag": "AAA",
        "team": "Red",
        "character": "Sova",
        "stats": {
          "kills": 18,
          "deaths": 15,
          "assists": 7,
          "headshots": 10,
          "bodyshots": 40,
          "legshots": 2,
          "damage_made": 3400,
          "ability_casts": { "grenadeCasts": 2, "ability1Casts": 8, "ability2Casts": 12, "ultimateCasts": 2 }
        },
        "economy": {
          "spent": 49000,
          "loadoutValue": 3900
        },
        "weapons": [
          { "name": "Vandal", "kills": 10, "headshots": 6, "bodyshots": 20, "legshots": 1 },
          { "name": "Ghost", "kills": 2, "headshots": 1, "bodyshots": 5, "legshots": 0 },
          ... （武器毎の成績）
        ]
      },
      ... （他プレイヤーのデータ）
    ],
    "rounds": [
      {
        "round": 1,
        "winningTeam": "Blue",
        "endType": "Eliminated",
        "bombPlanted": false,
        "bombDefused": false,
        "playerStats": [
          {
            "puuid": "....",
            "kills": [
              {
                "killer": "PlayerTwo",
                "victim": "PlayerOne",
                "weapon": "Ghost",
                "headshot": true,
                "roundTime": 43163
              }
            ],
            "damage": [ ... ],
            "score": 430,
            "economy": { "loadoutValue": 800, "remaining": 100, "spent": 800 }
          },
          ... （各プレイヤーごとのラウンド成績）
        ]
      },
      ... （全24ラウンド分のデータ）
    ]
  }
}
この例はCompetitive（ランク）モードの24ラウンドに及ぶ試合の詳細です。各プレイヤーごとに、総合KDA（キル18、デス15など）だけでなく、使用武器ごとのキル数や、各ラウンドごとの行動（どのラウンドで誰を倒したかなど）まで含まれていることがわかります。roundsセクションではラウンド単位で結果とイベントがネストしており、非常に詳細な構造になっています。
エラーハンドリング: 存在しないmatchidを指定すると404エラーが返ります​
DOCS.HENRIKDEV.XYZ
。regionが誤っている場合も404になる可能性があります。v4の場合、リージョン指定がないなどURLが間違っていると501エラー（未実装のAPIバージョン）となることもあります​
DOCS.HENRIKDEV.XYZ
。他の一般的なエラー（403, 408, 429, 503等）は共通仕様通りです​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。
備考: マッチ詳細データは非常に大きなJSONになることがあります。必要な部分（例えば各プレイヤーの合計K/Dなど）を抽出して使うようにしましょう。また、プライベートマッチ等は取得できない場合があります（404扱い）。
MMR（ランクレーティング）API
MMR APIでは、プレイヤーのランクやレーティング（RR: Ranked Rating）情報を取得できます。一般に「現在のランクとレートポイントを知るAPI」として利用されます。Henrik APIではシーズン別のランクデータや推定MMRも含めた詳細情報を提供します。バージョン: v1は非推奨で、v2およびv3が提供されています​
DOCS.HENRIKDEV.XYZ
。v3が最新でコンソール対応や詳細情報の拡充がされています。ここではv3をメインに説明します。
GET /valorant/v3/mmr/{region}/{platform}/{name}/{tag}
指定プレイヤーの現在のランク/MMR詳細を取得します​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。コンペティティブ（ランクマッチ）のランクTierやランクRR、シーズン成績などの情報が得られます。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v3/mmr/{region}/{platform}/{name}/{tag}
パスパラメータ:
region (string, 必須) – リージョン​
DOCS.HENRIKDEV.XYZ
platform (string, 必須) – プラットフォーム（pc または console）​
DOCS.HENRIKDEV.XYZ
name (string, 必須) – プレイヤー名
tag (string, 必須) – プレイヤータグ
クエリパラメータ:
season (string, オプション) – シーズンID​
DOCS.HENRIKDEV.XYZ
。指定するとそのシーズン終了時点のランクを取得できます（例: e5a3）。指定しない場合は現在シーズンのリアルタイムランク情報。
レスポンス: ランク（MMR）情報の詳細JSON。主な内容:
currenttier / currenttierpatched: 現在のランクTier数値とその名称（例: 24 と "Immortal 1"）。
ranking_in_tier: 現在Tier内でのRR（0～100の値）。
mmr_change_to_last_game: 最後の試合で増減したRR値。
elo: 内部的なMMR数値（非公式の総合レートポイント、具体的な最大値は不公開ですが、Radiant到達者はおおよそ**>= 400**程度になる）。
games_needed_for_rankup: 次のランク昇格までに必要なゲーム数（推定）。
old: データが古いかどうかのフラグ（過去シーズンを参照した場合true）。
act_rank_wins: そのエピソードのActランクで勝利したTier一覧（Act Rankバッジ用）。
players_in_queue: Radiantランクの場合のみ表示。キュー中の上位プレイヤー人数など。
レスポンス例:
json
コピーする
編集する
{
  "status": 200,
  "data": {
    "name": "PlayerOne",
    "tag": "AAA",
    "puuid": "123e4567-e89b-12d3-a456-426614174000",
    "current_data": {
      "currenttier": 18,
      "currenttierpatched": "Diamond 3",
      "ranking_in_tier": 75,
      "mmr_change_to_last_game": +20,
      "elo": 1575
    },
    "highest_rank": {
      "patched_tier": "Immortal 1",
      "tier": 21,
      "ranked_rating": 80,
      "displayed_rank_since": "2025-01-15"
    },
    "by_season": {
      "e5a3": {
        "currenttier": 18,
        "ranking_in_tier": 75,
        "competitiveTier": 18,
        "wins": 60,
        "games_played": 110
      },
      "...": { ... }
    }
  }
}
この例では、PlayerOne#AAAの現在ランクはDiamond 3（Tier18）で、同ティア内RRが75、直近の試合で+20ポイント変動し、内部MMR (elo) が1575であることがわかります。またhighest_rankは今シーズン到達した最高ランクがImmortal 1(21)のRR80であったことを示しています。by_seasonには各シーズンの最終ランクと戦績要約が入っています。
備考: コンソール版のプレイヤーについても、当該プラットフォームのランクが取得できます（ただし2023年時点でVALORANTコンソール版は存在せず、将来対応のためのパラメータです）。seasonを指定した場合、data内の情報は指定シーズン終了時点の値となります。指定しない場合はcurrent_dataフィールドで現在の値が返ります。
エラーハンドリング: プレイヤーが見つからない場合404、パラメータ不備時は400となります。seasonが不正な場合も404もしくは400となるでしょう。レートリミットやその他エラーは共通仕様に準じます​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v2/mmr/{region}/{name}/{tag} (旧バージョン)
v3と比較してプラットフォーム指定がなくPCのみ対象です​
DOCS.HENRIKDEV.XYZ
。seasonパラメータはv2でも使用可能​
DOCS.HENRIKDEV.XYZ
ですが、レスポンス形式が若干異なる部分があります。例えばcurrent_dataのフィールドが直接data直下に展開されて返るなどです。現在はv3への移行が推奨されています​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v2/by-puuid/mmr/{region}/{puuid} / /valorant/v3/by-puuid/mmr/{region}/{platform}/{puuid}
PUUID指定でMMRを取得するエンドポイントです​
DOCS.HENRIKDEV.XYZ
。指定のプレイヤーのランク情報をPUUIDで取得できます。機能は名前指定のものと同等で、v3版ではプラットフォームも指定します。PUUIDしか持っていない状況（例: データベースにPUUIDだけ保存している場合）で、そのプレイヤーの現在ランクを取得したい場合に有用です。
ランク履歴API（MMR History）
MMR履歴APIでは、プレイヤーの過去数試合分のランクレーティング変動履歴を取得できます。直近のマッチ結果ごとのRR増減や、その時点のランクを一覧できます。最大約数十試合分（内部的な保存件数分のみ）のデータが返ります。エンドポイント: v1はDeprecated（lifetimeという名称でした）が、v2エンドポイントが提供されています​
DOCS.HENRIKDEV.XYZ
。v2ではPUUID対応も追加されています。
GET /valorant/v2/mmr-history/{region}/{platform}/{name}/{tag}
指定プレイヤーの直近マッチにおけるランク変動履歴を取得します​
DOCS.HENRIKDEV.XYZ
。各試合ごとのRR増減値やマッチID、マップ、シーズン情報などが得られます。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v2/mmr-history/{region}/{platform}/{name}/{tag}
パスパラメータ:
region (string, 必須) – リージョン。
platform (string, 必須) – プラットフォーム（pc / console）。
name (string, 必須) – プレイヤー名。
tag (string, 必須) – タグ。
クエリパラメータ: なし。
レスポンス: JSON形式の履歴データ。historyフィールドに試合履歴の配列が入ります。各要素:
match_id: 試合ID。
tier: その試合終了時点のランクTier情報（IDと名称）。
map: 試合が行われたマップ。
season: 試合が属するシーズン（Act）のIDと短縮名。
rr: 試合後のランクRR（0～100）。
last_change: その試合で増減したRR値（プラスは昇格、マイナスは降格幅）。
elo: 試合後の内部MMR値（非公開指標なので参考程度）。
date: 試合日時（ISO8601 UTC）。
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "account": {
      "puuid": "123e4567-e89b-12d3-a456-426614174000",
      "name": "PlayerOne",
      "tag": "AAA"
    },
    "history": [
      {
        "match_id": "00000001-0002-0003-0004-000000000005",
        "tier": { "id": 18, "name": "Diamond 3" },
        "map": { "id": "7eaeadd1-4e01-4f3d-a6d1-94444750ce14", "name": "Ascent" },
        "season": { "id": "52e9749a-429b-7060-99fe-4595426a0cf7", "short": "e5a3" },
        "rr": 90,
        "last_change": +18,
        "elo": 1490,
        "date": "2025-02-27T08:10:20.797Z"
      },
      {
        "match_id": "00000001-0002-0003-0004-000000000004",
        "tier": { "id": 17, "name": "Diamond 2" },
        "map": { "id": "2c9d57ec-4431-e424-65ab-519519cd8c24", "name": "Fracture" },
        "season": { "id": "52e9749a-429b-7060-99fe-4595426a0cf7", "short": "e5a3" },
        "rr": 72,
        "last_change": -20,
        "elo": 1372,
        "date": "2025-02-26T14:05:10.123Z"
      },
      ... （過去試合のエントリが続く）
    ]
  }
}
この例では、PlayerOne#AAAの直近2試合のランク変動が示されています。最直近の試合（Ascent）ではDiamond 3の状態で+18RR（90RRに到達）、一つ前の試合（Fracture）ではDiamond 2から-20RRして72RRになったことがわかります。
備考: history配列の順序は新しい試合が前に来ます（時系列降順）。通常は最大数十件程度の履歴が取得できます（Henrik API内部でどの程度保存しているかによる）。この履歴はHenrik APIが取得した試合データに基づいて構築されているため、APIが一度も取得していない過去の試合は含まれない点に注意してください。
GET /valorant/v2/by-puuid/mmr-history/{region}/{platform}/{puuid}
PUUIDベースでMMR履歴を取得するエンドポイントです​
DOCS.HENRIKDEV.XYZ
。内容および使い方は名前指定のものと同様です。取得できるデータも同じ構造で、PUUIDでプレイヤーを指定する以外の違いはありません。
GET /valorant/v1/mmr-history/{region}/{name}/{tag} (非推奨)
v1エンドポイントは「lifetime」等とも呼ばれていた旧バージョンですが、現在は上記v2に置き換わっています​
DOCS.HENRIKDEV.XYZ
。新規利用は避け、v2を使用してください。v1ではプラットフォーム指定がない点と、レスポンスのラップが異なる程度です。
Premier（チーム競技）API
Premier APIは、ゲーム内の競技シーン機能「Premier（プレミア）」に関するデータを取得します。Premierではプレイヤーがチームを組み、シーズン制で大会に参加できます。このAPIではPremierチーム情報や大会成績、マッチ履歴、リーグ順位などを取得できます​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v1/premier/{team_name}/{team_tag}
指定したPremierチームの詳細情報を取得します​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/premier/{team_name}/{team_tag}
パスパラメータ:
team_name (string, 必須) – チーム名（ゲーム内で設定したチームの名前）​
DOCS.HENRIKDEV.XYZ
team_tag (string, 必須) – チームタグ（数文字のタグ）​
DOCS.HENRIKDEV.XYZ
レスポンス: チーム情報のJSON。主なフィールド:
id: チームのID。
name: チーム名。
tag: チームタグ。
enrolled: シーズン登録済みかどうかのフラグ。
stats: チームの戦績統計（勝ち数、負け数、総試合数）。
placement: 現在のリーグ内でのポイントやカンファレンス/ディビジョン、順位。
customization: チームのアイコンやカスタムカラー設定。
member: チームメンバー一覧（各メンバーのPUUID、名前、タグなど）。
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Sample Team",
    "tag": "STM",
    "enrolled": true,
    "stats": {
      "wins": 5,
      "matches": 7,
      "losses": 2
    },
    "placement": {
      "points": 15,
      "conference": "Alpha",
      "division": 3,
      "place": 1
    },
    "customization": {
      "icon": "12345",
      "image": "12345.png",
      "primary": "#ff0000",
      "secondary": "#00ff00",
      "tertiary": "#0000ff"
    },
    "member": [
      {
        "puuid": "abcdef01-2345-6789-abcd-ef0123456789",
        "name": "PlayerOne",
        "tag": "AAA"
      },
      {
        "puuid": "01234567-89ab-cdef-0123-456789abcdef",
        "name": "PlayerTwo",
        "tag": "BBB"
      }
      ... （他メンバー）
    ]
  }
}
上記の例では、Sample Team (STM)がAlphaカンファレンスのDivision 3で現在1位（15ポイント）であること、戦績が5勝2敗（7試合）であることがわかります。メンバーリストにはチーム所属プレイヤーの名前とタグが含まれます。
エラーハンドリング: 存在しないチーム名/タグを指定すると404が返ります​
DOCS.HENRIKDEV.XYZ
。team_nameやteam_tagが不正な形式の場合は400となります​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v1/premier/{team_name}/{team_tag}/history
指定したPremierチームの試合履歴を取得します​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/premier/{team_name}/{team_tag}/history
パスパラメータ: team_name, team_tag（いずれもチーム名とタグ、前述と同じ）。
レスポンス: チームのリーグ戦試合結果のJSON。league_matches配列に各試合の結果が含まれます:
id: 試合ID。
points_before: 試合前のチームポイント。
points_after: 試合後のチームポイント。
started_at: 試合開始日時。
（今後拡張で詳細な試合結果が入る可能性もあります）
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "league_matches": [
      {
        "id": "98765432-aaaa-bbbb-cccc-210987654321",
        "points_before": 12,
        "points_after": 15,
        "started_at": "2025-02-27T08:09:45.083Z"
      },
      {
        "id": "12345678-aaaa-bbbb-cccc-123456789012",
        "points_before": 9,
        "points_after": 12,
        "started_at": "2025-02-20T10:30:00.000Z"
      }
      ... （それ以前の試合）
    ]
  }
}
例では、直近の試合でポイントが12から15に増えている（勝利した）ことがわかります。idは各試合のIDで、必要であればこのIDを用いてマッチ詳細APIから詳細を取得できる可能性があります（ただしPremierの試合も通常のカスタムマッチ扱いでマッチ詳細取得が可能かは限定的です）。
エラーハンドリング: チームが見つからなければ404、パラメータ不備は400などとなります。共通エラーも同様です​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v1/premier/{team_id} / /valorant/v1/premier/{team_id}/history
チーム名ではなくチームIDで詳細や履歴を取得することもできます​
DOCS.HENRIKDEV.XYZ
。team_idはチーム詳細情報内にあるUUID形式のIDです。エンドポイントの機能はそれぞれ{team_name}/{team_tag}版と同様ですので、IDがわかっている場合に利用できます。
GET /valorant/v1/premier/search
Premierのチーム検索を行うエンドポイントです​
DOCS.HENRIKDEV.XYZ
。具体的な使い方はドキュメント上詳細がありませんが、おそらくクエリパラメータでチーム名の部分一致などを渡すことで該当チームを検索するものと思われます。認証や公開範囲の制約がある可能性があります。
（詳細仕様は将来的な更新に委ねます）
GET /valorant/v1/premier/conferences
Premierで使用されているカンファレンス一覧を取得します​
DOCS.HENRIKDEV.XYZ
。例えば「Alpha」「Bravo」など地域ごとに存在するカンファレンス名のリストを得られると思われます。
GET /valorant/v1/premier/seasons/{region}
指定リージョンのPremierシーズン情報を取得します​
DOCS.HENRIKDEV.XYZ
。現在進行中のシーズンや登録期間、プレイオフ情報などが含まれる可能性があります。
GET /valorant/v1/premier/leaderboard/{region}
指定リージョン内の**Premierリーグ順位表（総合）**を取得します​
DOCS.HENRIKDEV.XYZ
。各カンファレンス混合の総合順位、またはAlpha/Bravo統合の全体順位を返すと推測されます。
パスパラメータ: region (string, 必須) – リージョン。
レスポンス: 地域内全体のトップチームリスト。各チームのname, tag, pointsなど。
GET /valorant/v1/premier/leaderboard/{region}/{conference}
特定カンファレンス内の順位表を取得します​
DOCS.HENRIKDEV.XYZ
。
パスパラメータ:
region – リージョン
conference – カンファレンス名（文字列、例: Alpha, Bravo）
レスポンス: そのカンファレンス内の順位表。
GET /valorant/v1/premier/leaderboard/{region}/{conference}/{division}
特定カンファレンス内のディビジョン別順位表を取得します​
DOCS.HENRIKDEV.XYZ
。
パスパラメータ:
region – リージョン
conference – カンファレンス名
division – ディビジョン番号 (1〜?)
レスポンス: 指定したDivision内の順位表。
Premier APIの補足: Premier関連はまだ新しい機能のため、エンドポイントが頻繁に更新されたり、追加パラメータが必要になる可能性があります。最新のHenrikDev APIドキュメントを確認してください。
キュー状態API（Queue Status）
Queue Status APIでは、ゲーム内マッチングのキュー状況を取得できます​
DOCS.HENRIKDEV.XYZ
。現在マッチング可能なモード（キュー）の一覧や稼働状況を知ることができます。
GET /valorant/v1/queue-status/{region}
指定したリージョンにおける全キューの稼働状況を取得します​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/queue-status/{region}
パスパラメータ:
region (string, 必須) – リージョン（eu, na, ap, kr, latam, br）​
DOCS.HENRIKDEV.XYZ
。
レスポンス: 取得したリージョンで利用可能なゲームキュー（ゲームモード）の一覧と、それぞれのステータスを返します​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。ステータスには通常「UP」(稼働中) や「DOWN」(停止中)、メンテナンス情報などが含まれます。また各キューのIDや説明テキスト等のメタデータも併せて返されます。
レスポンス例: （概略例）
json
コピーする
編集する
{
  "status": 1,
  "data": [
    {
      "queueId": "competitive",
      "queueName": "Competitive",
      "isAvailable": true,
      "description": "Ranked matchmaking",
      "currentState": "UP"
    },
    {
      "queueId": "deathmatch",
      "queueName": "Deathmatch",
      "isAvailable": false,
      "description": "Free-for-all deathmatch",
      "currentState": "DOWN (Scheduled Maintenance)"
    },
    ... （他のキュー）
  ]
}
例では、Competitiveは利用可能（UP）だがDeathmatchはメンテ中（DOWN）であることが分かります。descriptionにはモード説明が入ります。
エラーハンドリング: リージョン指定が不正な場合は400エラーです​
DOCS.HENRIKDEV.XYZ
。提供されていない地域だと404の可能性があります​
DOCS.HENRIKDEV.XYZ
。エンドポイント自体の将来的な廃止時には410 Goneが返る設計になっています​
DOCS.HENRIKDEV.XYZ
。429や503等の共通エラーも起こり得ます​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。
備考: このエンドポイントはゲーム内モードの稼働有無を知るのに使えます。例えば大型パッチ直後にランク戦が一時停止されているか確認する、といった用途があります。
RawデータAPI（Raw）
Raw APIは、HenrikDev側で特別な加工を行わずRiotの非公開APIへ直接クエリを投げてそのレスポンスを取得するためのエンドポイントです​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。上級者向けで、公式非公開APIのレスポンス形式をそのまま取得できます。通常の利用では必要ありませんが、HenrikDev APIにないデータを取得したい場合に使います。
POST /valorant/v1/raw
Riotの内部APIに対する直接リクエストを行います​
DOCS.HENRIKDEV.XYZ
。POSTリクエストのボディにどのデータを取得したいかを指定します​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: POST
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/raw
リクエストボディ (JSON):
type (string or array, 必須) – 取得したいデータ種別を表すキー​
DOCS.HENRIKDEV.XYZ
。例: "matchdetails", "competitiveupdates", "mmr" など。複数のタイプを配列で指定可能（その場合、一度に複数のエンドポイントを取得）。
value (string, 必須) – クエリ値​
DOCS.HENRIKDEV.XYZ
。例えばtypeがmatchdetailsなら取得したいマッチID、competitiveupdatesならプレイヤー名/タグの指定など、内部APIエンドポイントが期待する値を渡します。
region (string, 必須) – クエリ対象のリージョン​
DOCS.HENRIKDEV.XYZ
。Riot内部API上のルーティングに使われます（例: eu）。
queries (string, オプション) – 追加のクエリストリング。内部APIに追加のクエリパラメータが必要な場合に指定（なければnull可）。
使用例:
Riot内部の「matchdetails」（試合詳細API）から直接データを取得する例:
json
コピーする
編集する
{
  "type": "matchdetails",
  "value": "a1b2c3d4-5678-90ab-cdef-111213141516",
  "region": "eu",
  "queries": null
}
これにより、HenrikDev APIで通常/matchエンドポイントを使わずにRiotの生データを取得できます。
レスポンス: 成功時はHTTP 200で、その内部APIが返すJSONデータがそのままレスポンスとして返ってきます​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。HenrikDev独自のstatusラップなどは基本的になく、純粋なRiot APIのレスポンスです。たとえばtype: matchdetailsの場合、Riotのmatchdetails APIが返すJSON（HenrikDevの/valorant/v2/matchと類似の構造）がそのまま得られます。
エラーハンドリング:
リクエストボディの形式が不正な場合は400エラーとなり、HenrikDev側からエラーメッセージが返ります​
DOCS.HENRIKDEV.XYZ
。
typeやvalueが間違っていてRiot側で404になる場合、そのまま404が返るかHenrikDev側で404エラー処理されます​
DOCS.HENRIKDEV.XYZ
。
その他403, 408, 429, 503なども基本共通ですが、注意: Rawでは内部API呼び出しなので、例えばRiot側が503を返した場合もそのまま503となるなど、通常エンドポイント以上に状況によります。
備考: Raw APIは想定された用途以外には推奨されません。たとえばtypeやvalueの指定にはRiot非公開APIの知識が必要です。HenrikDevのドキュメントでは詳細な使用例はあまり載っておらず、GitHub上のtechchrism/valorant-api-docs（RiotクライアントAPIのドキュメント）への参照が記載されています​
DOCS.HENRIKDEV.XYZ
。一般開発者は、まずHenrikDev APIが提供する高レベルエンドポイント（前述の各種）で目的が達成できないか確認し、それでも必要な場合のみRawを使うようにしてください。
サーバステータスAPI
ステータスAPIでは、指定した地域のVALORANT公式サーバの稼働状態やメンテナンス情報を取得できます​
DOCS.HENRIKDEV.XYZ
。公式のサービスステータス（https://status.riotgames.comなどに出る情報）を得るものです。
GET /valorant/v1/status/{region}
リージョンのサーバステータスおよび告知情報を取得します​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/status/{region}
パスパラメータ:
region (string, 必須) – リージョン（eu, na, ap, kr, latam, br）​
DOCS.HENRIKDEV.XYZ
。
レスポンス: サーバステータス情報のJSON。主な内容:
maintenance_status: サーバメンテナンス状態（例: "Scheduled", "in_progress", "none"等）。
incident_severity: インシデント（障害）の深刻度（例: "info", "warning", "critical"）。
platforms: 影響を受けているプラットフォーム（例: ["windows"]）。
issues: 現在発生している問題のリスト。それぞれにタイトルや説明、発生時刻など。
maintenances: 予定または進行中のメンテナンス情報のリスト。各メンテナンスに開始予定・終了予定時刻、告知内容等が含まれます。
locales: 利用可能な言語ロケール（お知らせを取得できる言語一覧）。
レスポンス例: （一部抜粋）
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "maintenance_status": "in_progress",
    "incident_severity": "warning",
    "platforms": [ "windows" ],
    "maintenances": [
      {
        "maintenance_id": "1234",
        "maintenance_status": "in_progress",
        "titles": [
          { "locale": "en_US", "content": "Scheduled Maintenance" },
          { "locale": "ja_JP", "content": "計画メンテナンス中" }
        ],
        "updates": [],
        "maintenance_start": "2025-02-28T01:00:00Z",
        "maintenance_end": "2025-02-28T03:00:00Z"
      }
    ],
    "incidents": []
  }
}
この例では、指定地域で計画メンテナンスが進行中であることが示されています。メンテナンス情報には多言語のタイトルや開始/終了予定時刻が含まれています。
エラーハンドリング: リージョン指定が無効な場合は400エラーです​
DOCS.HENRIKDEV.XYZ
。データ取得に失敗した場合（Riot側のAPI障害など）は503が返ることがあります​
DOCS.HENRIKDEV.XYZ
。
備考: ステータス情報はリージョンごとに分かれているため、必要に応じて全リージョンを確認する必要があります。また、プラットフォームとしてPCだけでなくモバイル等も将来的に含まれる可能性がありますが、現状VALORANTはPCのみサービスなのでplatformsは常に["windows"]になることが多いです​
DOCS.HENRIKDEV.XYZ
。
保存データAPI（Stored Data）
保存データAPIでは、HenrikDev APIサーバにキャッシュ/保存されているデータにアクセスできます。具体的には、過去に取得されたマッチ一覧やMMR履歴を無条件にすべて取得するエンドポイントです。新規取得ではなく、サーバ側に蓄積された履歴へのアクセスである点が特徴です。HenrikDev APIは一度取得したプレイヤーのマッチやMMRデータを保存しており、これらの「Stored（保存された）」データを用いてユーザーのライフタイム統計に近い情報を提供します​
DOCS.HENRIKDEV.XYZ
。v2以降でstored-matchesおよびstored-mmr-historyというエンドポイント名に統一されています​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v1/stored-matches/{region}/{name}/{tag}
指定したプレイヤーのHenrikDevサーバに保存されている全マッチ履歴を取得します​
DOCS.HENRIKDEV.XYZ
。通常のマッチ履歴APIは最新最大10試合程度ですが、このStored APIではHenrikDevが過去に取得したそのプレイヤーの全試合IDを返します。新規取得は行わず、あくまで保存済みデータを返す点に注意してください。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/stored-matches/{region}/{name}/{tag}
パスパラメータ:
region (string, 必須) – リージョン​
DOCS.HENRIKDEV.XYZ
name (string, 必須) – プレイヤー名​
DOCS.HENRIKDEV.XYZ
tag (string, 必須) – プレイヤータグ​
DOCS.HENRIKDEV.XYZ
クエリパラメータ:
mode (string, オプション) – モードフィルタ（competitive等）​
DOCS.HENRIKDEV.XYZ
map (string, オプション) – マップフィルタ​
DOCS.HENRIKDEV.XYZ
page (integer, オプション) – ページ番号​
DOCS.HENRIKDEV.XYZ
。データが多い場合のページ指定（sizeと併用）。
size (integer, オプション) – 1ページあたり取得試合数​
DOCS.HENRIKDEV.XYZ
。
レスポンス: JSON形式で、data内にマッチIDリストまたは簡易マッチ情報リストが入ります。レスポンス構造は通常のマッチ履歴APIと似ていますが、より多くの試合が含まれる点が異なります。HenrikDev APIの仕様上、保存された試合ごとの詳しいデータ（勝敗やスコア）は持っていないかもしれないため、各エントリはマッチIDと日付程度になる可能性があります。
備考: このエンドポイントはlifetimeという旧名称でも呼ばれていましたが、現在はstored-matchesに統一されています​
DOCS.HENRIKDEV.XYZ
。HenrikDev APIが取得したことのない古い試合は含まれないため、例えばAPIを使い始める前の試合履歴は全て網羅されない可能性があります。
エラーハンドリング: 404（データなし）、400（パラメータ不正）、他共通エラーコード。
GET /valorant/v1/by-puuid/stored-matches/{region}/{puuid}
PUUID指定版のStoredマッチリスト取得です​
DOCS.HENRIKDEV.XYZ
。機能は名前指定と同様です。
GET /valorant/v2/stored-mmr-history/{region}/{platform}/{name}/{tag}
指定プレイヤーの保存された全MMR履歴を取得します​
DOCS.HENRIKDEV.XYZ
。直近のMMR履歴ではなく、HenrikDev APIサーバ上に蓄積されている限りの長期のランク履歴をページング付きで取得できます。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v2/stored-mmr-history/{region}/{platform}/{name}/{tag}
パスパラメータ: region, platform, name, tag（いずれも前述どおり）​
DOCS.HENRIKDEV.XYZ
。
クエリパラメータ:
page (integer, オプション) – ページ番号​
DOCS.HENRIKDEV.XYZ
。指定する場合sizeも指定。
size (integer, オプション) – 1ページあたり件数​
DOCS.HENRIKDEV.XYZ
。ページ指定時は必須。
レスポンス: data.historyにMMR履歴のリストを返します。構造は前述のMMR History API (/mmr-history) と同じフォーマットですが、件数が増えています。
備考: ページングを使わずに呼び出した場合、おそらくデフォルトサイズで1ページ目を返します。全件を取得したい場合はページを切り替えながら取得してください。
エラーハンドリング: パラメータが足りない場合400、データが無ければ404等。基本は共通。
その他 Stored Data エンドポイント
旧エンドポイント名として/valorant/v1/lifetime/matchesや/valorant/v1/lifetime/mmrなどが存在しましたが、現在は廃止され/stored-に統一されています​
DOCS.HENRIKDEV.XYZ
。今後はstored-系エンドポイントを使用してください。
ストア情報API
ストアAPIでは、ゲーム内ストアに関する情報を取得できます。ただし個々のプレイヤーのデイリーストア（ナイトマーケット等）は取得できません​
DOCS.HENRIKDEV.XYZ
。このAPIで提供されるのは現在開催中のストアバンドル（期間限定セット）や恒常オファーの一覧です​
DOCS.HENRIKDEV.XYZ
。
GET /valorant/v1/store-featured
現在ストアで販売中の**バンドル（Featured Bundle）**情報を取得します​
DOCS.HENRIKDEV.XYZ
。通常、期間限定で販売されるスキンのセット情報です。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/store-featured
クエリパラメータ: なし。
レスポンス: ストアにおける現在のFeatured Bundle情報。data内にバンドル名、構成アイテム、価格などが含まれます。また、バンドル更新までの残り時間等も取れる場合があります。
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "FeaturedBundle": {
      "BundleID": "1234abcd-5678-efgh-ijkl-000000000000",
      "BundleName": "Give Back Bundle",
      "Items": [
        { "ItemTypeID": "weapon_skin", "ItemID": "skin123", "BasePrice": 4350 },
        { "ItemTypeID": "weapon_skin", "ItemID": "skin456", "BasePrice": 4350 },
        ... （バンドル内の全アイテム）
      ],
      "BundlePrice": 8700,
      "Currency": "VP",
      "DurationRemainingInSeconds": 172800
    }
  }
}
ここでは「Give Back Bundle」というバンドルが表示され、武器スキン2つを含み合計8700VPで販売されている例です。DurationRemainingInSecondsはあと2日（172800秒）でバンドル変更となることを示しています。
エラーハンドリング: 特になし。エンドポイントが無効化されていない限り常にデータを返します。万一この機能が停止している場合は500エラー等になるかもしれません。
GET /valorant/v1/store-offers
ゲーム内ストアで購入可能なオファー一覧を取得します​
DOCS.HENRIKDEV.XYZ
。武器スキン個別や契約、バトルパス購入など、ストアに恒常的に存在する商品のリストです。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/store-offers
クエリパラメータ: なし。
レスポンス: 全ストアオファーの一覧。各オファーの詳細（OfferID、価格、リワード内容など）が含まれます。
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "Offers": [
      {
        "OfferID": "offer_aaa111...",
        "IsDirectPurchase": true,
        "Cost": { "ValorantPoints": 875 },
        "Rewards": [
          { "ItemTypeID": "weapon_skin", "ItemID": "skin789", "Quantity": 1 }
        ]
      },
      {
        "OfferID": "offer_bbb222...",
        "IsDirectPurchase": true,
        "Cost": { "ValorantPoints": 1775 },
        "Rewards": [
          { "ItemTypeID": "weapon_skin", "ItemID": "skin101112", "Quantity": 1 }
        ]
      }
      ... （多数のオファーが続く）
    ]
  }
}
例として、武器スキンが2種オファーとしてリストされています。それぞれValorant Pointsでの価格と、得られるアイテム（Rewards）が示されています。オファーIDは内部識別子です。
備考: /store-offersには大量のアイテムが含まれるため、実際のレスポンスは非常に大きくなります。例えば全ての武器スキン、契約アイテムなどが一覧に入る可能性があります。特定のアイテム情報（名前や画像）は含まれずIDのみ返るため、必要に応じてContent API等と突合させる必要があります。
GET /valorant/v2/store-offers
/store-offersの改良版です​
DOCS.HENRIKDEV.XYZ
。返却データの構造に改善があるか、もしくは画像URLなど追加情報が含まれる可能性があります。利用方法はv1と同様です。HenrikDev公式では詳細な違いが記載されていませんが、基本的にはv2の利用が推奨されます。
エラーハンドリング: 基本同じ。v2が未実装の場合501エラーになる可能性がありますが、現在では実装済みです。
注: 個人のデイリーストア（4つのスキンのローテーション）やナイトマーケットの情報は、プライバシーおよび規約上の理由からHenrikDev APIでは提供されません​
DOCS.HENRIKDEV.XYZ
。
バージョン情報API
Version APIでは、現在のVALORANTクライアントバージョンなどの情報を取得できます​
DOCS.HENRIKDEV.XYZ
。ゲームのアップデートチェックなどに利用できます。
GET /valorant/v1/version/{region}
指定地域のVALORANTクライアントバージョン情報を取得します​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/version/{region}
パスパラメータ: region (string, 必須) – リージョン​
DOCS.HENRIKDEV.XYZ
。
レスポンス: バージョン情報のJSON。フィールド例:
manifestId: マニフェストID（ゲーム更新用ID）。
branch: バージョンブランチ名（例: "release-04.00"）​
DOCS.HENRIKDEV.XYZ
。
version: クライアントバージョン番号（例: "4.08.0.1234567.789012" のようなフォーマット）。
buildVersion: ビルド番号。
region: 対象リージョン。
レスポンス例:
json
コピーする
編集する
{
  "status": 1,
  "data": {
    "manifestId": "eff3cfd8-4a7c-11ec-81d3-0242ac130003",
    "branch": "release-04.08",
    "version": "4.08.0.1234567.789012",
    "buildVersion": "1234567",
    "region": "NA"
  }
}
例では、NAリージョンのクライアントが"release-04.08"ブランチのバージョン4.08.0.xxxであることを示しています。
エラーハンドリング: リージョンが不正な場合は400、データ取得失敗時は503等が返る可能性があります​
DOCS.HENRIKDEV.XYZ
​
DOCS.HENRIKDEV.XYZ
。
備考: 通常、この情報は全リージョン共通であることが多いですが、リージョンによってアップデートのタイミングが異なる場合には差異が出る可能性があります。
ニュース（お知らせ）API
Website APIでは、VALORANT公式サイトのニュースページから記事一覧を取得できます​
DOCS.HENRIKDEV.XYZ
。ゲーム内から見られるニュース（新マップ紹介やパッチノートへのリンク等）情報を言語別に取得するものです。
GET /valorant/v1/website/{countrycode}
公式サイトのニュース記事を取得します​
DOCS.HENRIKDEV.XYZ
。
HTTPメソッド: GET
エンドポイントURL: https://api.henrikdev.xyz/valorant/v1/website/{countrycode}
パスパラメータ:
countrycode (string, 必須) – ニュース記事を取得する言語/国コード​
DOCS.HENRIKDEV.XYZ
。例: ja-jp（日本語）, en-us（米国英語）, fr-fr（フランス語）など​
DOCS.HENRIKDEV.XYZ
。
レスポンス: ニュース記事の一覧JSON。data配列内に各記事が含まれます。主なフィールド:
title: 記事タイトル。
url: 記事のWebページURL。
banner_url: 記事サムネイル画像URL。
publish_date: 公開日時。
category: カテゴリ（例: パッチノート, ニュース）。
slug: 記事スラッグ（URL末尾）。
そのほか記事の概要などが含まれる場合があります。
レスポンス例: （日本語ja-jpの例）
json
コピーする
編集する
{
  "status": 1,
  "data": [
    {
      "title": "パッチノート4.08",
      "url": "https://playvalorant.com/ja-jp/news/game-updates/valorant-patch-notes-4-08/",
      "banner_url": "https://images.contentstack.io/v3/assets/.../header.jpg",
      "publish_date": "2025-02-15T02:00:00Z",
      "category": "Game Updates",
      "slug": "valorant-patch-notes-4-08"
    },
    {
      "title": "新エージェント「仮称」公開",
      "url": "https://playvalorant.com/ja-jp/news/agents/agent-teaser-2025/",
      "banner_url": "https://images.contentstack.io/v3/assets/.../teaser.jpg",
      "publish_date": "2025-02-10T10:00:00Z",
      "category": "Agents",
      "slug": "agent-teaser-2025"
    }
    ... （他の記事）
  ]
}
エラーハンドリング: 対応していない言語コードの場合でも、近い言語にフォールバックするか、データがなければ空配列が返ることがあります（詳細なエラーコードは特記なし）。明らかに不正なコードの場合は400になり得ます。
備考: 記事データはRiot公式サイトからパースして取得しているため、記事の増減や内容の更新はリアルタイムではない可能性があります。定期的に更新される想定です。
