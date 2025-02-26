#!/bin/bash

# ローカル開発環境セットアップスクリプト

# 色の設定
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# 必要なパッケージの確認とインストール
ensure_dotnet_sdk() {
    echo -e "${CYAN}🔍 .NET SDKの確認...${NC}"
    
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}❌ .NET SDKが見つかりません。インストールしてください。${NC}"
        echo -e "${YELLOW}📥 https://dotnet.microsoft.com/download${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ .NET SDKが見つかりました。${NC}"
}

ensure_docker() {
    echo -e "${CYAN}🔍 Dockerの確認...${NC}"
    
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}❌ Dockerが見つかりません。インストールしてください。${NC}"
        echo -e "${YELLOW}📥 https://www.docker.com/products/docker-desktop${NC}"
        exit 1
    fi
    
    docker_version=$(docker --version)
    echo -e "${GREEN}✅ Dockerが見つかりました: $docker_version${NC}"
}

setup_project() {
    echo -e "${CYAN}🔧 プロジェクトのセットアップを開始...${NC}"
    
    # NuGetパッケージの復元
    echo -e "${CYAN}📦 NuGetパッケージの復元...${NC}"
    dotnet restore
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ パッケージの復元に失敗しました。${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ パッケージが正常に復元されました。${NC}"
    
    # ビルド
    echo -e "${CYAN}🔨 ソリューションのビルド...${NC}"
    dotnet build
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ ビルドに失敗しました。${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ ビルドが成功しました。${NC}"
}

start_docker_services() {
    echo -e "${CYAN}🐳 Dockerサービスの起動...${NC}"
    
    # カレントディレクトリをリポジトリルートに設定
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
    REPO_ROOT="$(dirname "$SCRIPT_DIR")"
    cd "$REPO_ROOT"
    
    # docker-composeファイルの存在確認
    DOCKER_COMPOSE_FILE="$REPO_ROOT/docker/docker-compose.yml"
    
    if [ ! -f "$DOCKER_COMPOSE_FILE" ]; then
        echo -e "${RED}❌ docker-compose.ymlが見つかりません: $DOCKER_COMPOSE_FILE${NC}"
        exit 1
    fi
    
    # Dockerサービスの起動
    echo -e "${CYAN}🚀 Elasticsearchとその他サービスを起動中...${NC}"
    docker-compose -f "$DOCKER_COMPOSE_FILE" up -d
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Dockerサービスの起動に失敗しました。${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Dockerサービスが正常に起動しました。${NC}"
    echo -e "${GREEN}  - Elasticsearch: http://localhost:9200${NC}"
    echo -e "${GREEN}  - Kibana: http://localhost:5601${NC}"
}

show_instructions() {
    echo -e "\n${CYAN}🚀 KDalyticsセットアップが完了しました！${NC}"
    echo -e "\n以下のコマンドで各サービスを実行できます:"
    
    echo -e "\n${YELLOW}📊 APIの実行:${NC}"
    echo -e "  dotnet run --project src/KDalytics.API/KDalytics.API.csproj"
    
    echo -e "\n${YELLOW}🌐 Webフロントエンドの実行:${NC}"
    echo -e "  dotnet run --project src/KDalytics.Web/KDalytics.Web.csproj"
    
    echo -e "\n${YELLOW}⚙️ Functionsのローカル実行:${NC}"
    echo -e "  dotnet run --project src/KDalytics.Functions/KDalytics.Functions.csproj"
    
    echo -e "\n${YELLOW}🤖 Discord Botの実行:${NC}"
    echo -e "  dotnet run --project src/KDalytics.Discord/KDalytics.Discord.csproj"
    
    echo -e "\n${YELLOW}🧪 テストの実行:${NC}"
    echo -e "  dotnet test"
    
    echo -e "\n${YELLOW}🛑 Dockerサービスの停止:${NC}"
    echo -e "  docker-compose -f docker/docker-compose.yml down"
    
    echo -e "\n${CYAN}👋 Happy coding!${NC}"
}

# メイン実行ブロック
{
    echo -e "${CYAN}🚀 KDalytics ローカル開発環境セットアップを開始します${NC}"
    
    ensure_dotnet_sdk
    ensure_docker
    setup_project
    start_docker_services
    show_instructions
    
    echo -e "\n${GREEN}✅ セットアップが正常に完了しました！${NC}"
} || {
    echo -e "\n${RED}❌ エラーが発生しました${NC}"
    exit 1
}