# KDalytics - Valorant戦績トラッカー + Discord連携

KDalyticsは、Valorantの戦績データを取得・分析し、プレイヤー統計情報を可視化するWebアプリケーションと、Discord Bot連携を組み合わせたプロジェクトです。

## プロジェクト概要

Riot APIからValorantの戦績データを収集し、プレイヤーの統計情報をWeb上で可視化します。また、Discord Bot連携により、試合結果や戦績情報をDiscordサーバーに通知する機能を提供します。

### 主要機能

- **戦績取得**: Riot APIからプレイヤー戦績データを自動収集
- **データ分析**: エージェント別、マップ別の成績分析と可視化
- **Webダッシュボード**: 戦績の詳細な可視化と分析
- **Discord連携**: 試合結果の自動通知、戦績検索コマンド

## システム構成

- **バックエンド**: ASP.NET Core 9.0
- **フロントエンド**: Blazor WebAssembly
- **データ処理**: Azure Functions
- **データストア**: Elasticsearch
- **Discord連携**: DSharpPlus
- **CI/CD**: GitHub Actions

## プロジェクト構造

```
/
├── src/                          # ソースコード
│   ├── KDalytics.API/            # ASP.NET Core API
│   ├── KDalytics.Core/           # ドメインモデルとビジネスロジック
│   ├── KDalytics.Infrastructure/ # 外部サービス統合
│   ├── KDalytics.Functions/      # Azure Functions
│   ├── KDalytics.Discord/        # Discord Bot実装
│   └── KDalytics.Web/            # Blazorフロントエンド
│
├── tests/                        # テスト
│   ├── KDalytics.Core.Tests/     # コアロジックのユニットテスト
│   ├── KDalytics.API.Tests/      # API統合テスト
│   └── KDalytics.E2E.Tests/      # エンドツーエンドテスト
│
├── tools/                        # ユーティリティスクリプト
│   ├── setup-local.ps1           # Windows環境セットアップ
│   └── setup-local.sh            # macOS/Linux環境セットアップ
│
├── docker/                       # Docker設定
│   └── docker-compose.yml        # 開発環境用Docker Compose
│
└── .github/                      # GitHub設定
    └── workflows/                # GitHub Actions設定
```

## 開発環境のセットアップ

### 前提条件

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) または [Visual Studio Code](https://code.visualstudio.com/)

### セットアップ手順

#### Windows環境

PowerShellを管理者権限で実行し、以下のコマンドを実行します：

```powershell
.\tools\setup-local.ps1
```

#### macOS/Linux環境

ターミナルで以下のコマンドを実行します：

```bash
./tools/setup-local.sh
```

これにより、以下の処理が自動的に行われます：
- 必要なNuGetパッケージの復元
- ソリューションのビルド
- Elasticsearchなどのサービスを含むDockerコンテナの起動

### 開発サーバーの起動

#### APIサーバー

```bash
dotnet run --project src/KDalytics.API/KDalytics.API.csproj
```

#### Webフロントエンド

```bash
dotnet run --project src/KDalytics.Web/KDalytics.Web.csproj
```

### テストの実行

```bash
dotnet test
```

## 環境変数の設定

開発環境で使用する環境変数は、`src/KDalytics.API/appsettings.Development.json`ファイルに設定します。

必要な環境変数：
- `RiotApi:ApiKey`: Riot API開発者キー
- `Discord:BotToken`: Discord Bot Token
- `Elasticsearch:Url`: Elasticsearchのエンドポイント

## 貢献方法

1. このリポジトリをフォークする
2. 新しいブランチを作成する (`git checkout -b feature/amazing-feature`)
3. 変更をコミットする (`git commit -m 'Add some amazing feature'`)
4. ブランチにプッシュする (`git push origin feature/amazing-feature`)
5. Pull Requestを作成する

## ライセンス

このプロジェクトはMITライセンスの下で公開されています。

## 謝辞

- [Riot Games API](https://developer.riotgames.com/) - ゲームデータの提供
- [Discord API](https://discord.com/developers/docs/intro) - Discord統合のためのAPI