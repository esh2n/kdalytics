#!/bin/bash

# Elasticsearchインデックス作成スクリプト

# 色の設定
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Elasticsearchの接続情報
ES_HOST="localhost"
ES_PORT="9200"
ES_URL="http://${ES_HOST}:${ES_PORT}"
INDEX_PREFIX="kdalytics-"

# Elasticsearchの接続確認
check_elasticsearch() {
    echo -e "${CYAN}🔍 Elasticsearchの接続確認...${NC}"
    
    if ! curl -s "${ES_URL}/_cluster/health" > /dev/null; then
        echo -e "${RED}❌ Elasticsearchに接続できません。Elasticsearchが起動しているか確認してください。${NC}"
        echo -e "${YELLOW}💡 ヒント: docker-compose -f docker/docker-compose.yml up -d${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Elasticsearchに接続できました。${NC}"
}

# インデックスの作成
create_index() {
    local index_name="$1"
    local full_index_name="${INDEX_PREFIX}${index_name}"
    local mapping_file="$2"
    
    echo -e "${CYAN}🔧 インデックス '${full_index_name}' の作成...${NC}"
    
    # インデックスが既に存在するか確認
    if curl -s -o /dev/null -w "%{http_code}" "${ES_URL}/${full_index_name}" | grep -q "200"; then
        echo -e "${YELLOW}⚠️ インデックス '${full_index_name}' は既に存在します。${NC}"
        
        # 既存のインデックスを削除するか確認
        read -p "既存のインデックスを削除して再作成しますか？ (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo -e "${YELLOW}🗑️ インデックス '${full_index_name}' を削除します...${NC}"
            curl -X DELETE "${ES_URL}/${full_index_name}"
            echo
        else
            echo -e "${YELLOW}⏭️ インデックス '${full_index_name}' はスキップされました。${NC}"
            return
        fi
    fi
    
    # マッピングファイルが指定されている場合
    if [ -n "$mapping_file" ] && [ -f "$mapping_file" ]; then
        echo -e "${CYAN}📄 マッピングファイル '${mapping_file}' を使用してインデックスを作成します...${NC}"
        curl -X PUT "${ES_URL}/${full_index_name}" -H "Content-Type: application/json" -d @"$mapping_file"
    else
        # 基本的なインデックス設定
        echo -e "${CYAN}📄 基本設定でインデックスを作成します...${NC}"
        curl -X PUT "${ES_URL}/${full_index_name}" -H "Content-Type: application/json" -d '{
            "settings": {
                "number_of_shards": 1,
                "number_of_replicas": 0,
                "refresh_interval": "5s"
            }
        }'
    fi
    
    echo
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ インデックス '${full_index_name}' が正常に作成されました。${NC}"
    else
        echo -e "${RED}❌ インデックス '${full_index_name}' の作成に失敗しました。${NC}"
    fi
}

# マッピングディレクトリの作成
create_mapping_directory() {
    local mapping_dir="./tools/elasticsearch-mappings"
    
    if [ ! -d "$mapping_dir" ]; then
        echo -e "${CYAN}📁 マッピングディレクトリを作成します...${NC}"
        mkdir -p "$mapping_dir"
    fi
    
    # プレイヤーインデックスのマッピング
    cat > "$mapping_dir/players.json" << 'EOF'
{
    "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "refresh_interval": "5s"
    },
    "mappings": {
        "properties": {
            "puuid": { "type": "keyword" },
            "name": { "type": "keyword" },
            "tag": { "type": "keyword" },
            "region": { "type": "keyword" },
            "accountLevel": { "type": "integer" },
            "lastUpdated": { "type": "date" },
            "cardId": { "type": "keyword" },
            "titleId": { "type": "keyword" }
        }
    }
}
EOF

    # プレイヤーランクインデックスのマッピング
    cat > "$mapping_dir/player-ranks.json" << 'EOF'
{
    "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "refresh_interval": "5s"
    },
    "mappings": {
        "properties": {
            "puuid": { "type": "keyword" },
            "season": { "type": "keyword" },
            "rank": { "type": "keyword" },
            "rankTier": { "type": "integer" },
            "mmr": { "type": "integer" },
            "rrChangeToLastGame": { "type": "integer" },
            "games": { "type": "integer" },
            "wins": { "type": "integer" },
            "timestamp": { "type": "date" }
        }
    }
}
EOF

    # 試合インデックスのマッピング
    cat > "$mapping_dir/matches.json" << 'EOF'
{
    "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "refresh_interval": "5s"
    },
    "mappings": {
        "properties": {
            "matchId": { "type": "keyword" },
            "mapId": { "type": "keyword" },
            "gameMode": { "type": "keyword" },
            "startTime": { "type": "date" },
            "endTime": { "type": "date" },
            "seasonId": { "type": "keyword" },
            "region": { "type": "keyword" },
            "teams": {
                "properties": {
                    "teamId": { "type": "keyword" },
                    "won": { "type": "boolean" },
                    "roundsWon": { "type": "integer" },
                    "roundsPlayed": { "type": "integer" }
                }
            },
            "rounds": {
                "properties": {
                    "roundNum": { "type": "integer" },
                    "winningTeam": { "type": "keyword" },
                    "bombPlanted": { "type": "boolean" },
                    "bombDefused": { "type": "boolean" }
                }
            }
        }
    }
}
EOF

    # プレイヤーパフォーマンスインデックスのマッピング
    cat > "$mapping_dir/player-performances.json" << 'EOF'
{
    "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "refresh_interval": "5s"
    },
    "mappings": {
        "properties": {
            "matchId": { "type": "keyword" },
            "puuid": { "type": "keyword" },
            "teamId": { "type": "keyword" },
            "agent": { "type": "keyword" },
            "kills": { "type": "integer" },
            "deaths": { "type": "integer" },
            "assists": { "type": "integer" },
            "score": { "type": "integer" },
            "headshots": { "type": "integer" },
            "bodyshots": { "type": "integer" },
            "legshots": { "type": "integer" },
            "damageDealt": { "type": "integer" },
            "damageTaken": { "type": "integer" },
            "econRating": { "type": "float" },
            "firstBloods": { "type": "integer" },
            "plants": { "type": "integer" },
            "defuses": { "type": "integer" },
            "won": { "type": "boolean" },
            "startTime": { "type": "date" }
        }
    }
}
EOF

    # トラッキング設定インデックスのマッピング
    cat > "$mapping_dir/tracking-configs.json" << 'EOF'
{
    "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "refresh_interval": "5s"
    },
    "mappings": {
        "properties": {
            "puuid": { "type": "keyword" },
            "track": { "type": "boolean" },
            "discordUserId": { "type": "keyword" },
            "discordChannelId": { "type": "keyword" },
            "notifyOnMatchEnd": { "type": "boolean" },
            "notifyOnRankChange": { "type": "boolean" },
            "lastUpdated": { "type": "date" }
        }
    }
}
EOF

    echo -e "${GREEN}✅ マッピングファイルが作成されました。${NC}"
}

# メイン実行ブロック
main() {
    echo -e "${CYAN}🚀 Elasticsearchインデックスのセットアップを開始します${NC}"
    
    check_elasticsearch
    create_mapping_directory
    
    # インデックスの作成
    create_index "players" "./tools/elasticsearch-mappings/players.json"
    create_index "player-ranks" "./tools/elasticsearch-mappings/player-ranks.json"
    create_index "matches" "./tools/elasticsearch-mappings/matches.json"
    create_index "player-performances" "./tools/elasticsearch-mappings/player-performances.json"
    create_index "tracking-configs" "./tools/elasticsearch-mappings/tracking-configs.json"
    
    echo -e "\n${GREEN}✅ Elasticsearchインデックスのセットアップが完了しました！${NC}"
    echo -e "\n${YELLOW}💡 Kibanaでインデックスを確認: http://localhost:5601${NC}"
}

# スクリプトの実行
main