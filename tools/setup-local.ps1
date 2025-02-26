# ローカル開発環境セットアップスクリプト

# 必要なパッケージの確認とインストール
function Ensure-DotNetSDK {
    Write-Host "🔍 .NET SDKの確認..." -ForegroundColor Cyan
    $dotnetInfo = dotnet --info
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ .NET SDKが見つかりません。インストールしてください。" -ForegroundColor Red
        Write-Host "📥 https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "✅ .NET SDKが見つかりました。" -ForegroundColor Green
}

function Ensure-Docker {
    Write-Host "🔍 Dockerの確認..." -ForegroundColor Cyan
    $dockerRunning = $false
    
    try {
        $dockerVersion = docker --version
        $dockerRunning = $true
        Write-Host "✅ Dockerが見つかりました: $dockerVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Dockerが見つからないか、実行されていません。" -ForegroundColor Red
        Write-Host "📥 https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
        exit 1
    }
}

function Setup-Project {
    Write-Host "🔧 プロジェクトのセットアップを開始..." -ForegroundColor Cyan
    
    # NuGetパッケージの復元
    Write-Host "📦 NuGetパッケージの復元..." -ForegroundColor Cyan
    dotnet restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ パッケージの復元に失敗しました。" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ パッケージが正常に復元されました。" -ForegroundColor Green
    
    # ビルド
    Write-Host "🔨 ソリューションのビルド..." -ForegroundColor Cyan
    dotnet build
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ ビルドに失敗しました。" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ ビルドが成功しました。" -ForegroundColor Green
}

function Start-DockerServices {
    Write-Host "🐳 Dockerサービスの起動..." -ForegroundColor Cyan
    
    # カレントディレクトリをリポジトリルートに設定
    $repoRoot = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
    Set-Location $repoRoot
    
    # docker-composeファイルの存在確認
    $dockerComposeFile = Join-Path $repoRoot "docker" "docker-compose.yml"
    
    if (-not (Test-Path $dockerComposeFile)) {
        Write-Host "❌ docker-compose.ymlが見つかりません: $dockerComposeFile" -ForegroundColor Red
        exit 1
    }
    
    # Dockerサービスの起動
    Write-Host "🚀 Elasticsearchとその他サービスを起動中..." -ForegroundColor Cyan
    docker-compose -f $dockerComposeFile up -d
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Dockerサービスの起動に失敗しました。" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Dockerサービスが正常に起動しました。" -ForegroundColor Green
    Write-Host "  - Elasticsearch: http://localhost:9200" -ForegroundColor Green
    Write-Host "  - Kibana: http://localhost:5601" -ForegroundColor Green
}

function Show-Instructions {
    Write-Host "`n🚀 KDalyticsセットアップが完了しました！" -ForegroundColor Cyan
    Write-Host "`n以下のコマンドで各サービスを実行できます:" -ForegroundColor White
    
    Write-Host "`n📊 APIの実行:" -ForegroundColor Yellow
    Write-Host "  dotnet run --project src/KDalytics.API/KDalytics.API.csproj" -ForegroundColor White
    
    Write-Host "`n🌐 Webフロントエンドの実行:" -ForegroundColor Yellow
    Write-Host "  dotnet run --project src/KDalytics.Web/KDalytics.Web.csproj" -ForegroundColor White
    
    Write-Host "`n⚙️ Functionsのローカル実行:" -ForegroundColor Yellow
    Write-Host "  dotnet run --project src/KDalytics.Functions/KDalytics.Functions.csproj" -ForegroundColor White
    
    Write-Host "`n🤖 Discord Botの実行:" -ForegroundColor Yellow
    Write-Host "  dotnet run --project src/KDalytics.Discord/KDalytics.Discord.csproj" -ForegroundColor White
    
    Write-Host "`n🧪 テストの実行:" -ForegroundColor Yellow
    Write-Host "  dotnet test" -ForegroundColor White
    
    Write-Host "`n🛑 Dockerサービスの停止:" -ForegroundColor Yellow
    Write-Host "  docker-compose -f docker/docker-compose.yml down" -ForegroundColor White
    
    Write-Host "`n👋 Happy coding!" -ForegroundColor Cyan
}

# メイン実行ブロック
try {
    Write-Host "🚀 KDalytics ローカル開発環境セットアップを開始します" -ForegroundColor Cyan
    
    Ensure-DotNetSDK
    Ensure-Docker
    Setup-Project
    Start-DockerServices
    Show-Instructions
    
    Write-Host "`n✅ セットアップが正常に完了しました！" -ForegroundColor Green
}
catch {
    Write-Host "`n❌ エラーが発生しました: $_" -ForegroundColor Red
    exit 1
}