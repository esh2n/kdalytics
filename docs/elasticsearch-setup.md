# Elasticsearchローカル開発環境セットアップガイド

このガイドでは、KDalyticsプロジェクトのElasticsearchローカル開発環境のセットアップ方法について説明します。

## 前提条件

- Docker Desktop がインストールされていること
- .NET 8.0 SDK がインストールされていること

## セットアップ手順

### 1. Dockerコンテナの起動

以下のコマンドを実行して、ElasticsearchとKibanaのDockerコンテナを起動します。

```bash
# リポジトリのルートディレクトリで実行
cd docker
docker-compose up -d
```

これにより、以下のサービスが起動します：

- Elasticsearch: `http://localhost:9200`
- Kibana: `http://localhost:5601`

### 2. Elasticsearchインデックスの作成

プロジェクトで使用するElasticsearchインデックスを作成するために、以下のスクリプトを実行します。

```bash
# リポジトリのルートディレクトリで実行
./tools/setup-elasticsearch-indices.sh
```

このスクリプトは以下のインデックスを作成します：

- `kdalytics-players`: プレイヤー情報を格納
- `kdalytics-player-ranks`: プレイヤーのランク情報を格納
- `kdalytics-matches`: 試合情報を格納
- `kdalytics-player-performances`: プレイヤーのパフォーマンス情報を格納
- `kdalytics-tracking-configs`: プレイヤーのトラッキング設定を格納

### 3. テストデータの生成（オプション）

APIのテストに使用するサンプルデータを生成するには、以下のスクリプトを実行します。

```bash
# リポジトリのルートディレクトリで実行
./tools/generate-test-data.sh
```

このスクリプトは、各インデックスにテストデータを追加します。

### 4. Kibanaでのデータ確認

Kibanaを使用してElasticsearchのデータを確認するには、ブラウザで `http://localhost:5601` にアクセスします。

1. サイドメニューから「Management」→「Stack Management」を選択
2. 「Kibana」セクションの「Data Views」を選択
3. 「Create data view」ボタンをクリック
4. 「Name」に「kdalytics-*」と入力
5. 「Index pattern」に「kdalytics-*」と入力
6. 「Timestamp field」は「I don't want to use a time field」を選択
7. 「Create data view」ボタンをクリック

これで、「Discover」タブでデータを確認できるようになります。

### 5. APIの起動

APIを起動するには、以下のコマンドを実行します。

```bash
# リポジトリのルートディレクトリで実行
dotnet run --project src/KDalytics.API/KDalytics.API.csproj
```

APIが起動したら、ブラウザで `http://localhost:5167/swagger` にアクセスして、Swagger UIでAPIエンドポイントを確認できます。

## トラブルシューティング

### Elasticsearchに接続できない場合

1. Dockerコンテナが起動しているか確認します。

```bash
docker ps
```

2. Elasticsearchのログを確認します。

```bash
docker logs kdalytics-elasticsearch
```

3. メモリ設定を確認します。Dockerに割り当てられているメモリが少ない場合、Elasticsearchが起動しない可能性があります。Docker Desktopの設定で、少なくとも4GBのメモリを割り当ててください。

### インデックスが作成できない場合

1. Elasticsearchのヘルスチェックを実行します。

```bash
curl -X GET "http://localhost:9200/_cluster/health?pretty"
```

2. インデックスの一覧を確認します。

```bash
curl -X GET "http://localhost:9200/_cat/indices?v"
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

- [Elasticsearch公式ドキュメント](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [Kibana公式ドキュメント](https://www.elastic.co/guide/en/kibana/current/index.html)
- [NEST (Elasticsearch .NET Client)ドキュメント](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html)