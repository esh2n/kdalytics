# KDalytics - Valorant Stats Tracker + Discord Integration

A web application that retrieves Valorant performance data and visualizes player statistics. It also provides notifications of match results and performance summaries through a Discord Bot. Additionally, each Discord channel can have its own dashboard, making it easy to share stats among teams and friend groups.

## Features

### Web Application
- Retrieval and display of player performance data
- Detailed match history display and analysis
- Agent-specific and map-specific performance statistics
- Channel-specific dashboards (Discord integration)
- Rank progression tracking

### Discord Bot
- Player search and registration
- Match result notifications and detailed display
- Statistical information such as KDA rankings
- Access to channel-specific dashboards

## Technology Stack

- **Backend**: ASP.NET Core, C#
- **Frontend**: Blazor
- **Database**: Elasticsearch
- **API Integration**: Henrik API, Tracker Network API
- **Notifications**: Discord Bot (Discord.Net)
- **Deployment**: Azure

## Usage

### Discord Bot Usage

1. Invite the Bot to your server
2. Register a channel
   ```
   !channel register
   ```
3. Register a player
   ```
   !player track <name> <tag>
   ```
4. Check statistics
   ```
   !player stats <name> <tag> [period]
   ```
5. Check match history
   ```
   !match recent <name> <tag> [count]
   ```
6. Check rankings
   ```
   !ranking kda [period]
   ```
7. Access the dashboard
   ```
   !channel dashboard
   ```

### Web Dashboard Usage

1. Access the URL obtained from the Discord Bot
2. Enter the access code to log in
3. View statistics for players registered in the channel
4. Check detailed pages for each player and match details

## Development Environment Setup

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- Visual Studio 2022 or Visual Studio Code
- Discord Bot Token (obtained from Discord Developer Portal)

### Steps

1. Clone the repository

```bash
git clone https://github.com/esh2n/kdalytics.git
cd kdalytics
```

2. Install dependencies

```bash
dotnet restore
```

3. Start Elasticsearch

```bash
cd docker
docker-compose up -d
```

4. Create Elasticsearch indices

```bash
./tools/setup-elasticsearch-indices.sh
```

5. Generate test data (optional)

```bash
./tools/generate-test-data.sh
```

6. Discord Bot configuration

Set the Discord Bot token in `src/KDalytics.Discord/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://localhost:5000",
  "WebAppUrl": "http://localhost:5173",
  "DiscordToken": "YOUR_DISCORD_BOT_TOKEN_HERE",
  "AccessCodeSecret": "YOUR_SECRET_KEY_HERE"
}
```

7. Start the API

```bash
dotnet run --project src/KDalytics.API/KDalytics.API.csproj
```

8. Start the Web frontend

```bash
dotnet run --project src/KDalytics.Web/KDalytics.Web.csproj
```

9. Start the Discord Bot

```bash
dotnet run --project src/KDalytics.Discord/KDalytics.Discord.csproj
```

## API Endpoints

Detailed API documentation can be viewed through Swagger UI after starting the API:

```
http://localhost:5167/swagger
```

Main endpoints:

- `GET /api/players/{puuid}` - Get player information
- `POST /api/players/search` - Search for a player by name and tag
- `GET /api/matches/player/{puuid}/recent` - Get a player's recent matches
- `GET /api/performances/player/{puuid}/agents` - Get agent-specific performance

## Elasticsearch Configuration

For detailed information on configuring Elasticsearch, refer to the [Elasticsearch Setup Guide](docs/elasticsearch-setup.md).

## Testing

```bash
dotnet test
```

## License

This project is released under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## Acknowledgements

- [Henrik-3/unofficial-valorant-api](https://github.com/Henrik-3/unofficial-valorant-api) - Providing the unofficial Valorant API
- [Tracker Network](https://tracker.gg/) - Providing performance data