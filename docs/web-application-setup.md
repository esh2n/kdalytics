# KDalytics Webアプリケーション起動ガイド

このガイドでは、KDalyticsのWebアプリケーションを起動する方法について説明します。

## 前提条件

- Docker Desktop がインストールされていること
- .NET 8.0 SDK がインストールされていること
- Elasticsearchが起動していること（[Elasticsearchセットアップガイド](./elasticsearch-setup.md)を参照）

## 起動方法

KDalyticsのWebアプリケーションを起動するには、以下の2つの方法があります。

### 方法1：個別のコマンドで起動

1. まず、Elasticsearchが起動していることを確認します：
   ```bash
   docker ps
   ```
   Elasticsearchコンテナ（kdalytics-elasticsearch）が起動していない場合は、以下のコマンドで起動します：
   ```bash
   cd docker
   docker-compose up -d
   ```

2. APIを起動します：
   ```bash
   dotnet run --project src/KDalytics.API/KDalytics.API.csproj
   ```

3. 別のターミナルでWebアプリケーションを起動します：
   ```bash
   dotnet run --project src/KDalytics.Web/KDalytics.Web.csproj
   ```

4. ブラウザで以下のURLにアクセスします：
   ```
   http://localhost:5047
   ```

### 方法2：セットアップスクリプトを使用

1. `setup-local.sh`スクリプトを実行して、ローカル環境をセットアップします：
   ```bash
   ./tools/setup-local.sh
   ```

   このスクリプトは以下の処理を行います：
   - .NET SDKとDockerがインストールされているか確認
   - NuGetパッケージの復元とソリューションのビルド
   - Dockerサービス（ElasticsearchとKibana）の起動
   - 各サービスの実行方法の表示

2. スクリプト実行後、表示される指示に従って各サービスを起動します：

   APIの実行：
   ```bash
   dotnet run --project src/KDalytics.API/KDalytics.API.csproj
   ```

   Webフロントエンドの実行：
   ```bash
   dotnet run --project src/KDalytics.Web/KDalytics.Web.csproj
   ```

3. ブラウザで以下のURLにアクセスします：
   ```
   http://localhost:5047
   ```

## 初回アクセス時の注意点

初回アクセス時は「トラッキング中のプレイヤーがいません」と表示されます。これは、Elasticsearchにプレイヤーデータがまだ登録されていないためです。

プレイヤーを追加するには、画面中央の検索フォームにプレイヤー名とタグを入力して「検索」ボタンをクリックします。APIを通じてRiot APIからプレイヤーデータが取得され、Elasticsearchに保存されます。

## テストデータの生成（オプション）

テストデータを生成するには、以下のスクリプトを実行します：
```bash
./tools/generate-test-data.sh
```

このスクリプトは、Elasticsearchの各インデックスにテストデータを追加します。

## トラブルシューティング

### Webアプリケーションにアクセスできない場合

1. APIとWebアプリケーションが起動しているか確認します。
2. ブラウザのコンソールでエラーメッセージを確認します。
3. Elasticsearchが起動しているか確認します。
   ```bash
   curl -X GET "http://localhost:9200/_cluster/health?pretty"
   ```

### APIがElasticsearchに接続できない場合

1. `appsettings.json` ファイルのElasticsearch設定を確認します。
   ```json
   "Elasticsearch": {
     "Urls": ["http://localhost:9200"],
     "Username": "",
     "Password": "",
     "ApiKey": "",
     "IndexPrefix": "kdalytics-",
     "MaxRetries": 3,
     "TimeoutSeconds": 30
   }
   ```

2. APIのログを確認します。

## 参考リンク

- [Elasticsearchセットアップガイド](./elasticsearch-setup.md)
- [KDalyticsプロジェクトのREADME](../README.md)