#!/bin/bash

# テストデータ生成スクリプト

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

# インデックスの存在確認
check_index() {
    local index_name="${INDEX_PREFIX}$1"
    
    echo -e "${CYAN}🔍 インデックス '${index_name}' の確認...${NC}"
    
    if ! curl -s -o /dev/null -w "%{http_code}" "${ES_URL}/${index_name}" | grep -q "200"; then
        echo -e "${RED}❌ インデックス '${index_name}' が存在しません。${NC}"
        echo -e "${YELLOW}💡 ヒント: tools/setup-elasticsearch-indices.sh を実行してインデックスを作成してください。${NC}"
        return 1
    fi
    
    echo -e "${GREEN}✅ インデックス '${index_name}' が存在します。${NC}"
    return 0
}

# ドキュメントの追加
add_document() {
    local index_name="${INDEX_PREFIX}$1"
    local document="$2"
    local id="$3"
    
    echo -e "${CYAN}📄 ドキュメントを '${index_name}' に追加...${NC}"
    
    local url="${ES_URL}/${index_name}/_doc"
    if [ -n "$id" ]; then
        url="${url}/${id}"
    fi
    
    curl -X POST "${url}" -H "Content-Type: application/json" -d "${document}"
    echo
}

# プレイヤーデータの生成
generate_player_data() {
    echo -e "${CYAN}👤 プレイヤーデータの生成...${NC}"
    
    # プレイヤー1
    add_document "players" '{
        "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "name": "esh2n",
        "tag": "JP1",
        "region": "ap",
        "accountLevel": 150,
        "lastUpdated": "2025-02-27T00:00:00Z",
        "cardId": "card-1",
        "titleId": "title-1"
    }' "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    
    # プレイヤー2
    add_document "players" '{
        "puuid": "b2c3d4e5-f6g7-8901-hijk-lm2345678901",
        "name": "Valorant",
        "tag": "PRO",
        "region": "ap",
        "accountLevel": 200,
        "lastUpdated": "2025-02-27T00:00:00Z",
        "cardId": "card-2",
        "titleId": "title-2"
    }' "b2c3d4e5-f6g7-8901-hijk-lm2345678901"
    
    # プレイヤー3
    add_document "players" '{
        "puuid": "c3d4e5f6-g7h8-9012-nopq-rs3456789012",
        "name": "Shunya",
        "tag": "DEV",
        "region": "ap",
        "accountLevel": 120,
        "lastUpdated": "2025-02-27T00:00:00Z",
        "cardId": "card-3",
        "titleId": "title-3"
    }' "c3d4e5f6-g7h8-9012-nopq-rs3456789012"
}

# プレイヤーランクデータの生成
generate_player_rank_data() {
    echo -e "${CYAN}🏆 プレイヤーランクデータの生成...${NC}"
    
    # プレイヤー1のランク
    add_document "player-ranks" '{
        "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "season": "e7a1",
        "rank": "Diamond",
        "rankTier": 2,
        "mmr": 1850,
        "rrChangeToLastGame": 15,
        "games": 50,
        "wins": 30,
        "timestamp": "2025-02-27T00:00:00Z"
    }' "a1b2c3d4-e5f6-7890-abcd-ef1234567890_e7a1"
    
    # プレイヤー2のランク
    add_document "player-ranks" '{
        "puuid": "b2c3d4e5-f6g7-8901-hijk-lm2345678901",
        "season": "e7a1",
        "rank": "Immortal",
        "rankTier": 1,
        "mmr": 2100,
        "rrChangeToLastGame": 10,
        "games": 80,
        "wins": 55,
        "timestamp": "2025-02-27T00:00:00Z"
    }' "b2c3d4e5-f6g7-8901-hijk-lm2345678901_e7a1"
    
    # プレイヤー3のランク
    add_document "player-ranks" '{
        "puuid": "c3d4e5f6-g7h8-9012-nopq-rs3456789012",
        "season": "e7a1",
        "rank": "Platinum",
        "rankTier": 3,
        "mmr": 1650,
        "rrChangeToLastGame": 20,
        "games": 40,
        "wins": 22,
        "timestamp": "2025-02-27T00:00:00Z"
    }' "c3d4e5f6-g7h8-9012-nopq-rs3456789012_e7a1"
}

# 試合データの生成
generate_match_data() {
    echo -e "${CYAN}🎮 試合データの生成...${NC}"
    
    # 試合1
    add_document "matches" '{
        "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
        "mapId": "Ascent",
        "gameMode": "Competitive",
        "startTime": "2025-02-26T18:00:00Z",
        "endTime": "2025-02-26T18:45:00Z",
        "seasonId": "e7a1",
        "region": "ap",
        "teams": [
            {
                "teamId": "Blue",
                "won": true,
                "roundsWon": 13,
                "roundsPlayed": 24
            },
            {
                "teamId": "Red",
                "won": false,
                "roundsWon": 11,
                "roundsPlayed": 24
            }
        ],
        "rounds": [
            {
                "roundNum": 1,
                "winningTeam": "Blue",
                "bombPlanted": true,
                "bombDefused": false
            },
            {
                "roundNum": 2,
                "winningTeam": "Red",
                "bombPlanted": false,
                "bombDefused": false
            }
        ]
    }' "m1n2o3p4-q5r6-7890-stuv-wx1234567890"
    
    # 試合2
    add_document "matches" '{
        "matchId": "n2o3p4q5-r6s7-8901-tuvw-xy2345678901",
        "mapId": "Bind",
        "gameMode": "Competitive",
        "startTime": "2025-02-26T19:00:00Z",
        "endTime": "2025-02-26T19:40:00Z",
        "seasonId": "e7a1",
        "region": "ap",
        "teams": [
            {
                "teamId": "Blue",
                "won": false,
                "roundsWon": 7,
                "roundsPlayed": 20
            },
            {
                "teamId": "Red",
                "won": true,
                "roundsWon": 13,
                "roundsPlayed": 20
            }
        ],
        "rounds": [
            {
                "roundNum": 1,
                "winningTeam": "Red",
                "bombPlanted": true,
                "bombDefused": false
            },
            {
                "roundNum": 2,
                "winningTeam": "Blue",
                "bombPlanted": false,
                "bombDefused": false
            }
        ]
    }' "n2o3p4q5-r6s7-8901-tuvw-xy2345678901"
}

# プレイヤーパフォーマンスデータの生成
generate_player_performance_data() {
    echo -e "${CYAN}📊 プレイヤーパフォーマンスデータの生成...${NC}"
    
    # 試合1のプレイヤー1のパフォーマンス
    add_document "player-performances" '{
        "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
        "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "teamId": "Blue",
        "agent": "Jett",
        "kills": 22,
        "deaths": 15,
        "assists": 3,
        "score": 280,
        "headshots": 12,
        "bodyshots": 45,
        "legshots": 5,
        "damageDealt": 3500,
        "damageTaken": 2800,
        "econRating": 85.5,
        "firstBloods": 3,
        "plants": 2,
        "defuses": 0,
        "won": true,
        "startTime": "2025-02-26T18:00:00Z"
    }' "m1n2o3p4-q5r6-7890-stuv-wx1234567890_a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    
    # 試合1のプレイヤー2のパフォーマンス
    add_document "player-performances" '{
        "matchId": "m1n2o3p4-q5r6-7890-stuv-wx1234567890",
        "puuid": "b2c3d4e5-f6g7-8901-hijk-lm2345678901",
        "teamId": "Red",
        "agent": "Reyna",
        "kills": 25,
        "deaths": 18,
        "assists": 2,
        "score": 290,
        "headshots": 15,
        "bodyshots": 40,
        "legshots": 3,
        "damageDealt": 3800,
        "damageTaken": 2500,
        "econRating": 90.2,
        "firstBloods": 4,
        "plants": 0,
        "defuses": 1,
        "won": false,
        "startTime": "2025-02-26T18:00:00Z"
    }' "m1n2o3p4-q5r6-7890-stuv-wx1234567890_b2c3d4e5-f6g7-8901-hijk-lm2345678901"
    
    # 試合2のプレイヤー1のパフォーマンス
    add_document "player-performances" '{
        "matchId": "n2o3p4q5-r6s7-8901-tuvw-xy2345678901",
        "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "teamId": "Blue",
        "agent": "Sage",
        "kills": 15,
        "deaths": 16,
        "assists": 8,
        "score": 220,
        "headshots": 8,
        "bodyshots": 35,
        "legshots": 4,
        "damageDealt": 2800,
        "damageTaken": 2900,
        "econRating": 75.0,
        "firstBloods": 1,
        "plants": 1,
        "defuses": 2,
        "won": false,
        "startTime": "2025-02-26T19:00:00Z"
    }' "n2o3p4q5-r6s7-8901-tuvw-xy2345678901_a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    
    # 試合2のプレイヤー3のパフォーマンス
    add_document "player-performances" '{
        "matchId": "n2o3p4q5-r6s7-8901-tuvw-xy2345678901",
        "puuid": "c3d4e5f6-g7h8-9012-nopq-rs3456789012",
        "teamId": "Red",
        "agent": "Omen",
        "kills": 18,
        "deaths": 12,
        "assists": 10,
        "score": 250,
        "headshots": 10,
        "bodyshots": 38,
        "legshots": 6,
        "damageDealt": 3200,
        "damageTaken": 2400,
        "econRating": 82.5,
        "firstBloods": 2,
        "plants": 3,
        "defuses": 0,
        "won": true,
        "startTime": "2025-02-26T19:00:00Z"
    }' "n2o3p4q5-r6s7-8901-tuvw-xy2345678901_c3d4e5f6-g7h8-9012-nopq-rs3456789012"
}

# トラッキング設定データの生成
generate_tracking_config_data() {
    echo -e "${CYAN}⚙️ トラッキング設定データの生成...${NC}"
    
    # プレイヤー1のトラッキング設定
    add_document "tracking-configs" '{
        "puuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "track": true,
        "discordUserId": "discord-user-1",
        "discordChannelId": "discord-channel-1",
        "notifyOnMatchEnd": true,
        "notifyOnRankChange": true,
        "lastUpdated": "2025-02-27T00:00:00Z"
    }' "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    
    # プレイヤー2のトラッキング設定
    add_document "tracking-configs" '{
        "puuid": "b2c3d4e5-f6g7-8901-hijk-lm2345678901",
        "track": true,
        "discordUserId": "discord-user-2",
        "discordChannelId": "discord-channel-1",
        "notifyOnMatchEnd": false,
        "notifyOnRankChange": true,
        "lastUpdated": "2025-02-27T00:00:00Z"
    }' "b2c3d4e5-f6g7-8901-hijk-lm2345678901"
    
    # プレイヤー3のトラッキング設定
    add_document "tracking-configs" '{
        "puuid": "c3d4e5f6-g7h8-9012-nopq-rs3456789012",
        "track": true,
        "discordUserId": "discord-user-3",
        "discordChannelId": "discord-channel-2",
        "notifyOnMatchEnd": true,
        "notifyOnRankChange": false,
        "lastUpdated": "2025-02-27T00:00:00Z"
    }' "c3d4e5f6-g7h8-9012-nopq-rs3456789012"
}

# メイン実行ブロック
main() {
    echo -e "${CYAN}🚀 テストデータの生成を開始します${NC}"
    
    check_elasticsearch
    
    # インデックスの確認
    check_index "players" && check_index "player-ranks" && check_index "matches" && check_index "player-performances" && check_index "tracking-configs" || {
        echo -e "${RED}❌ 必要なインデックスが存在しません。${NC}"
        echo -e "${YELLOW}💡 ヒント: tools/setup-elasticsearch-indices.sh を実行してインデックスを作成してください。${NC}"
        exit 1
    }
    
    # テストデータの生成
    generate_player_data
    generate_player_rank_data
    generate_match_data
    generate_player_performance_data
    generate_tracking_config_data
    
    echo -e "\n${GREEN}✅ テストデータの生成が完了しました！${NC}"
    echo -e "\n${YELLOW}💡 Kibanaでデータを確認: http://localhost:5601${NC}"
    echo -e "${YELLOW}💡 APIでデータを確認: http://localhost:5167/swagger${NC}"
}

# スクリプトの実行
main