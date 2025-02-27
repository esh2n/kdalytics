# KDalytics - Valorant 戦績トラッカー + Discord連携

Valorantの戦績データを取得し、プレイヤーの統計情報を可視化するWebアプリケーションです。Discord Botを通じて試合結果や戦績の要約を通知する機能も提供します。さらに、Discordチャンネルごとに独自のダッシュボードを持つことができ、チームやフレンドグループでの戦績共有が容易になります。

## 機能

### Webアプリケーション
- プレイヤーの戦績データの取得と表示
- 試合履歴の詳細表示と分析
- エージェント別・マップ別のパフォーマンス統計
- チャンネル専用ダッシュボード（Discord連携）
- ランク変動の追跡

### Discord Bot
- プレイヤー検索と登録
- 試合結果の通知と詳細表示
- KDAランキングなどの統計情報
- チャンネル専用ダッシュボードへのアクセス提供

## 技術スタック

- **バックエンド**: ASP.NET Core, C#
- **フロントエンド**: Blazor
- **データベース**: Elasticsearch
- **API連携**: Henrik API, Tracker Network API
- **通知**: Discord Bot (Discord.Net)
- **デプロイ**: Azure

## 使用方法

### Discord Botの使用

1. Botをサーバーに招待
2. チャンネルの登録
   ```
   !channel register
   ```
3. プレイヤーの登録
   ```
   !player track <名前> <タグ>
   ```
4. 統計情報の確認
   ```
   !player stats <名前> <タグ> [期間]
   ```
5. 試合履歴の確認
   ```
   !match recent <名前> <タグ> [件数]
   ```
6. ランキングの確認
   ```
   !ranking kda [期間]
   ```
7. ダッシュボードへのアクセス
   ```
   !channel dashboard
   ```

### Webダッシュボードの使用

1. Discord Botから取得したURLにアクセス
2. アクセスコードを入力してログイン
3. チャンネルに登録されたプレイヤーの統計情報を閲覧
4. 各プレイヤーの詳細ページや試合詳細ページを確認

## 開発環境のセットアップ

### 前提条件

- .NET 9.0 SDK
- Docker Desktop
- Visual Studio 2022 または Visual Studio Code
- Discord Bot Token（Discord Developer Portalから取得）

### 手順

1. リポジトリをクローン

```bash
git clone https://github.com/esh2n/kdalytics.git
cd kdalytics
```

2. 依存関係のインストール

```bash
dotnet restore
```

3. Elasticsearchの起動

```bash
cd docker
docker-compose up -d
```

4. Elasticsearchのインデックスを作成

```bash
./tools/setup-elasticsearch-indices.sh
```

5. テストデータの生成（オプション）

```bash
./tools/generate-test-data.sh
```

6. Discord Bot設定

Discord Botのトークンを`src/KDalytics.Discord/appsettings.json`に設定します：

```json
{
  "ApiBaseUrl": "http://localhost:5000",
  "WebAppUrl": "http://localhost:5173",
  "DiscordToken": "YOUR_DISCORD_BOT_TOKEN_HERE",
  "AccessCodeSecret": "YOUR_SECRET_KEY_HERE"
}
```

7. APIの起動

```bash
dotnet run --project src/KDalytics.API/KDalytics.API.csproj
```

8. Webフロントエンドの起動

```bash
dotnet run --project src/KDalytics.Web/KDalytics.Web.csproj
```

9. Discord Botの起動

```bash
dotnet run --project src/KDalytics.Discord/KDalytics.Discord.csproj
```

## APIエンドポイント

APIの詳細なドキュメントは、APIを起動した後に以下のURLでSwagger UIを通じて確認できます：

```
http://localhost:5167/swagger
```

主要なエンドポイント：

- `GET /api/players/{puuid}` - プレイヤー情報の取得
- `POST /api/players/search` - 名前とタグでプレイヤーを検索
- `GET /api/matches/player/{puuid}/recent` - プレイヤーの最近の試合を取得
- `GET /api/performances/player/{puuid}/agents` - エージェント別パフォーマンスを取得

## Elasticsearchの設定

Elasticsearchの詳細な設定方法については、[Elasticsearchセットアップガイド](docs/elasticsearch-setup.md)を参照してください。

## テスト

```bash
dotnet test
```

## ライセンス

このプロジェクトはMITライセンスの下で公開されています。詳細は[LICENSE](LICENSE)ファイルを参照してください。

## 貢献

プルリクエストは歓迎します。大きな変更を加える前には、まずissueを開いて変更内容について議論してください。

## 謝辞

- [Henrik-3/unofficial-valorant-api](https://github.com/Henrik-3/unofficial-valorant-api) - 非公式Valorant APIの提供
- [Tracker Network](https://tracker.gg/) - 戦績データの提供