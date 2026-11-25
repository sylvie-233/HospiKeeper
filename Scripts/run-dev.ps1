$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "启动 MySQL（docker compose）..."
docker compose -f Scripts/docker-compose.yml up -d --wait

Write-Host "启动 HospiKeeper.Api ..."
Start-Process -FilePath "dotnet" -ArgumentList "run --project src/HospiKeeper.Api" -WorkingDirectory $root

Write-Host "等待 API 就绪 ..."
Start-Sleep -Seconds 8

Write-Host "启动 HospiKeeper.Desktop ..."
dotnet run --project src/HospiKeeper.Desktop
