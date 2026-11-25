<#
.SYNOPSIS
    HospiKeeper 本地环境运维脚本（MySQL 容器 + ASP.NET Core API + Avalonia 桌面端）。

.DESCRIPTION
    统一封装本地开发与运维操作：
      · Docker MySQL 的启动、停止、状态、日志、客户端与重置；
      · ASP.NET Core API 的构建、启动、停止、健康检查与日志；
      · EF Core 迁移的生成和应用；
      · 解决方案构建 / 测试；
      · Avalonia 桌面端启动。
    API 以本地进程方式运行（PID 写入 .run/，日志写入 logs/），MySQL 由 docker compose 管理。

.PARAMETER Action
    要执行的操作，默认 status：
      up         启动 MySQL，并构建 / 启动 API，等待就绪
      down       停止 API，并移除全部 compose 容器（保留 MySQL 数据卷）
      restart    重启目标服务（MySQL 容器 / 本地 API）
      reset      删除 MySQL 数据卷后重新启动整套环境（会清空数据库，需确认）
      build      构建整个解决方案
      test       运行全部测试
      logs       查看本地 API 或 MySQL 日志
      cli        进入 MySQL 命令行
      status     查看 MySQL / API 状态与访问地址
      init       删库重建，并重启 API 由 EF Core 迁移 + 种子数据恢复（需确认）
      migrate    应用 EF Core 迁移到当前数据库
      ef:add     新建 EF Core 迁移（需 -MigrationName）
      pull       拉取 MySQL 镜像
      desktop    启动 Avalonia 桌面端
      dev        启动整套环境后打开桌面端

.PARAMETER Target
    操作目标，默认 all（仅部分操作使用）：
      mysql  仅 MySQL
      api    仅 API（会自动确保 MySQL 已启动）
      all    MySQL + API

.PARAMETER Force
    reset / init 跳过二次确认；build 使用 --no-incremental。

.PARAMETER Follow
    logs 操作是否持续跟踪输出（建议配合 -Target api 或 mysql）。

.PARAMETER Tail
    logs 显示的最后行数，默认 200。

.PARAMETER TimeoutSeconds
    up / restart / reset / init 等待就绪的超时秒数，默认 90。

.PARAMETER NoWait
    up / restart / reset / init 启动后不等待健康检查。

.PARAMETER MigrationName
    ef:add 使用的迁移名称。

.EXAMPLE
    pwsh ./manage.ps1 up

.EXAMPLE
    pwsh ./manage.ps1 up -Target mysql

.EXAMPLE
    pwsh ./manage.ps1 dev

.EXAMPLE
    pwsh ./manage.ps1 logs -Target api -Follow -Tail 300

.EXAMPLE
    pwsh ./manage.ps1 reset -Force

.EXAMPLE
    pwsh ./manage.ps1 migrate

.EXAMPLE
    pwsh ./manage.ps1 ef:add -MigrationName AddRoomBalcony

.EXAMPLE
    pwsh ./manage.ps1 test
#>
#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet(
        'up', 'down', 'restart', 'reset', 'build', 'test', 'logs', 'cli', 'status',
        'init', 'migrate', 'ef:add', 'pull', 'desktop', 'dev'
    )]
    [string]$Action = 'status',

    [ValidateSet('mysql', 'api', 'all')]
    [string]$Target = 'all',

    [switch]$Force,
    [switch]$Follow,

    [ValidateRange(1, 10000)]
    [int]$Tail = 200,

    [ValidateRange(5, 600)]
    [int]$TimeoutSeconds = 90,

    [switch]$NoWait,

    [string]$MigrationName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# 统一以 UTF-8 输出，避免中文在重定向 / 管道中乱码
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$ScriptRoot = $PSScriptRoot
$ComposeFile = Join-Path $ScriptRoot 'Scripts/docker-compose.yml'
$SolutionFile = Join-Path $ScriptRoot 'HospiKeeper.slnx'
$ApiProject = Join-Path $ScriptRoot 'src/HospiKeeper.Api/HospiKeeper.Api.csproj'
$ApiProjectDir = Join-Path $ScriptRoot 'src/HospiKeeper.Api'
$ApiDll = Join-Path $ScriptRoot 'src/HospiKeeper.Api/bin/Debug/net10.0/HospiKeeper.Api.dll'
$DesktopProject = Join-Path $ScriptRoot 'src/HospiKeeper.Desktop/HospiKeeper.Desktop.csproj'
$ApiBaseUrl = 'http://localhost:5080'
$RunDir = Join-Path $ScriptRoot '.run'
$LogDir = Join-Path $ScriptRoot 'logs'
$ApiPidFile = Join-Path $RunDir 'api.pid'
$ApiOutLog = Join-Path $LogDir 'api.log'
$ApiErrLog = Join-Path $LogDir 'api.error.log'
$MysqlContainer = 'hospikeeper-mysql'

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host "    $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "    $Message" -ForegroundColor Yellow
}

function Assert-DotNetPrerequisites {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw '未找到 dotnet 命令，请先安装 .NET SDK 并加入 PATH。'
    }

    if (-not (Test-Path -LiteralPath $SolutionFile)) {
        throw "未找到解决方案文件：$SolutionFile"
    }
}

function Assert-DockerPrerequisites {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw '未找到 docker 命令，请先安装并启动 Docker Desktop。'
    }

    if (-not (Test-Path -LiteralPath $ComposeFile)) {
        throw "未找到 compose 文件：$ComposeFile"
    }

    & docker compose version *> $null
    if ($LASTEXITCODE -ne 0) {
        throw '当前 docker 不支持 compose v2，请升级 Docker Desktop。'
    }
}

function Assert-EfTool {
    if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue) -and
        -not (Test-Path -LiteralPath (Join-Path $ScriptRoot '.config/dotnet-tools.json')) -and
        -not (Test-Path -LiteralPath (Join-Path $ScriptRoot 'dotnet-tools.json'))) {
        throw '未找到 dotnet-ef 工具，请先执行：dotnet tool restore'
    }
}

function Invoke-Compose {
    param([Parameter(Mandatory)][string[]]$ComposeArgs)

    & docker compose --file $ComposeFile @ComposeArgs
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose $($ComposeArgs -join ' ') 执行失败（退出码 $LASTEXITCODE）。"
    }
}

function Get-TargetIncludes {
    param([Parameter(Mandatory)][string]$Service)

    $services = switch ($Target) {
        'mysql' { @('mysql') }
        'api' { @('mysql', 'api') }
        default { @('mysql', 'api') }
    }

    return $services -contains $Service
}

function Ensure-RunDirs {
    foreach ($dir in @($RunDir, $LogDir)) {
        if (-not (Test-Path -LiteralPath $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
        }
    }
}

function Get-ApiProcess {
    if (-not (Test-Path -LiteralPath $ApiPidFile)) {
        return $null
    }

    $pidText = (Get-Content -LiteralPath $ApiPidFile -ErrorAction Stop | Select-Object -First 1)
    $pidValue = 0
    if (-not [int]::TryParse($pidText, [ref]$pidValue)) {
        Remove-Item -LiteralPath $ApiPidFile -Force -ErrorAction SilentlyContinue
        return $null
    }

    $process = Get-Process -Id $pidValue -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        Remove-Item -LiteralPath $ApiPidFile -Force -ErrorAction SilentlyContinue
        return $null
    }

    return $process
}

function Get-ApiProcessesByCommandLine {
    return @(
        Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue |
            Where-Object { $_.CommandLine -like '*HospiKeeper.Api.dll*' }
    )
}

function Test-ApiHealth {
    try {
        $response = Invoke-WebRequest -Uri "$ApiBaseUrl/health" -TimeoutSec 2 -UseBasicParsing
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

function Stop-Api {
    $process = Get-ApiProcess
    if ($null -ne $process) {
        Write-Step "停止 API（PID $($process.Id)）..."
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 500
        Remove-Item -LiteralPath $ApiPidFile -Force -ErrorAction SilentlyContinue
        Write-Ok 'API 已停止。'
        return
    }

    foreach ($item in (Get-ApiProcessesByCommandLine)) {
        Write-Step "停止遗留 API 进程（PID $($item.ProcessId)）..."
        Stop-Process -Id $item.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

function Wait-MySqlHealthy {
    if ($NoWait) {
        Write-Warn '已跳过健康检查等待。'
        return
    }

    Write-Step "等待 MySQL 就绪（最长 ${TimeoutSeconds}s）..."
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $status = & docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' $MysqlContainer 2>$null
        if ($LASTEXITCODE -eq 0) {
            switch ($status | Select-Object -First 1) {
                'healthy' {
                    Write-Ok 'MySQL 已就绪。'
                    return
                }
                'unhealthy' {
                    throw 'MySQL 健康检查失败，请执行：pwsh ./manage.ps1 logs -Target mysql -Follow'
                }
            }
        }

        Start-Sleep -Seconds 2
    }

    throw "等待 MySQL 就绪超时（${TimeoutSeconds}s）。"
}

function Wait-ApiHealthy {
    if ($NoWait) {
        Write-Warn '已跳过健康检查等待。'
        return
    }

    Write-Step "等待 API 就绪（最长 ${TimeoutSeconds}s）..."
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-ApiHealth) {
            Write-Ok "API 已就绪：$ApiBaseUrl/swagger"
            return
        }

        Start-Sleep -Seconds 1
    }

    Write-Warn 'API 未在超时时间内就绪，最近日志如下：'
    Show-ApiLogs
    throw "等待 API 就绪超时（${TimeoutSeconds}s）。"
}

function Start-MySql {
    Write-Step '启动 MySQL 容器...'
    Invoke-Compose -ComposeArgs @('up', '-d', 'mysql')
    Wait-MySqlHealthy
}

function Build-Api {
    Write-Step '构建 API 与依赖...'
    & dotnet build $ApiProject -c Debug --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "API 构建失败（退出码 $LASTEXITCODE）。"
    }
    Write-Ok 'API 构建完成。'
}

function Build-Solution {
    Write-Step '构建解决方案...'
    $buildArgs = @('build', $SolutionFile, '--nologo')
    if ($Force) {
        $buildArgs += '--no-incremental'
    }
    & dotnet @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "解决方案构建失败（退出码 $LASTEXITCODE）。"
    }
    Write-Ok '解决方案构建完成。'
}

function Start-Api {
    Ensure-RunDirs

    if (Test-ApiHealth) {
        Write-Warn "检测到 API 已在运行：$ApiBaseUrl"
        return
    }

    if (-not (Test-Path -LiteralPath $ApiDll)) {
        throw "未找到 API 构建产物：$ApiDll；请先执行 pwsh ./manage.ps1 build"
    }

    Write-Step '启动 API...'
    $process = Start-Process -FilePath 'dotnet' `
        -ArgumentList @($ApiDll) `
        -WorkingDirectory $ApiProjectDir `
        -RedirectStandardOutput $ApiOutLog `
        -RedirectStandardError $ApiErrLog `
        -WindowStyle Hidden `
        -PassThru

    Set-Content -LiteralPath $ApiPidFile -Value $process.Id -Encoding utf8
    Write-Ok "API 进程 PID $($process.Id)，日志：$ApiOutLog"
    Wait-ApiHealthy
}

function Show-ApiLogs {
    if (-not (Test-Path -LiteralPath $ApiOutLog) -and -not (Test-Path -LiteralPath $ApiErrLog)) {
        Write-Warn "暂无 API 日志，日志目录：$LogDir"
        return
    }

    foreach ($file in @($ApiOutLog, $ApiErrLog)) {
        if (Test-Path -LiteralPath $file) {
            Write-Host "--- $file ---" -ForegroundColor DarkGray
            Get-Content -LiteralPath $file -Tail $Tail
        }
    }
}

function Invoke-StackUp {
    if (Get-TargetIncludes 'mysql') {
        Start-MySql
    }

    if (Get-TargetIncludes 'api') {
        if (Test-ApiHealth -or $null -ne (Get-ApiProcess)) {
            Write-Warn 'API 已在运行，跳过构建与启动；如需应用最新代码请执行 restart。'
        }
        else {
            Build-Api
            Start-Api
        }
    }
}

function Confirm-Destructive {
    param([Parameter(Mandatory)][string]$Message)

    if ($Force) {
        return
    }

    $answer = Read-Host "$Message 输入 YES 继续"
    if ($answer -ne 'YES') {
        Write-Host '已取消。' -ForegroundColor Yellow
        exit 0
    }
}

function Reset-DatabaseInsideContainer {
    Write-Step '重建数据库（DROP / CREATE）...'
    Invoke-Compose -ComposeArgs @(
        'exec', '-T', 'mysql', 'sh', '-c',
        'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" mysql -uroot -e "DROP DATABASE IF EXISTS $MYSQL_DATABASE; CREATE DATABASE $MYSQL_DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;"'
    )
    Write-Ok '数据库已重建，重启 API 后会执行 EF Core 迁移与种子数据。'
}

function Invoke-MigrationUpdate {
    Assert-EfTool
    Write-Step '应用 EF Core 迁移...'
    & dotnet ef database update `
        --project (Join-Path $ScriptRoot 'src/HospiKeeper.Infrastructure/HospiKeeper.Infrastructure.csproj') `
        --startup-project $ApiProject
    if ($LASTEXITCODE -ne 0) {
        throw "EF Core 迁移失败（退出码 $LASTEXITCODE）。"
    }
    Write-Ok '迁移完成。'
}

function Show-Status {
    Write-Step '容器状态'
    Invoke-Compose -ComposeArgs @('ps')

    $health = & docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' $MysqlContainer 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "MySQL 容器（$MysqlContainer）尚未创建，执行 pwsh ./manage.ps1 up -Target mysql"
    }
    else {
        $mysqlStatus = $health | Select-Object -First 1
        $color = if ($mysqlStatus -in @('healthy', 'running')) { 'Green' } elseif ($mysqlStatus -eq 'unhealthy') { 'Red' } else { 'Yellow' }
        Write-Host "    MySQL：$mysqlStatus" -ForegroundColor $color
    }

    $apiProcess = Get-ApiProcess
    if (Test-ApiHealth) {
        Write-Host '    API  ：healthy' -ForegroundColor Green
    }
    elseif ($null -ne $apiProcess) {
        Write-Host "    API  ：进程 $($apiProcess.Id) 已启动，但健康检查未通过" -ForegroundColor Yellow
    }
    else {
        Write-Host '    API  ：未运行' -ForegroundColor Yellow
    }

    Write-Host ''
    Write-Host '访问地址：' -ForegroundColor Cyan
    Write-Host "    API        $ApiBaseUrl"
    Write-Host "    Swagger    $ApiBaseUrl/swagger"
    Write-Host "    MySQL      127.0.0.1:3306/hospikeeper"
    Write-Host "    API 日志   $ApiOutLog"
}

function Open-Desktop {
    if (-not (Test-ApiHealth)) {
        Write-Warn "API 未就绪，桌面端登录可能失败。先执行：pwsh ./manage.ps1 up"
    }

    Write-Step '启动 Avalonia 桌面端...'
    & dotnet run --project $DesktopProject
    if ($LASTEXITCODE -ne 0) {
        throw "桌面端退出（退出码 $LASTEXITCODE）。"
    }
}

Assert-DotNetPrerequisites

$dockerActions = @('up', 'down', 'restart', 'reset', 'logs', 'cli', 'status', 'init', 'migrate', 'pull', 'dev')
if ($dockerActions -contains $Action) {
    Assert-DockerPrerequisites
}

switch ($Action) {
    'up' {
        Invoke-StackUp
        Show-Status
    }

    'down' {
        Stop-Api
        Write-Step '停止 compose 容器（保留数据卷）...'
        Invoke-Compose -ComposeArgs @('down')
        Write-Ok '环境已停止。'
    }

    'restart' {
        if (Get-TargetIncludes 'api') {
            Stop-Api
        }

        if (Get-TargetIncludes 'mysql') {
            Write-Step '重启 MySQL 容器...'
            Invoke-Compose -ComposeArgs @('restart', 'mysql')
            Wait-MySqlHealthy
        }

        if (Get-TargetIncludes 'api') {
            Build-Api
            Start-Api
        }

        Show-Status
    }

    'reset' {
        Confirm-Destructive -Message '将删除 MySQL 数据卷，数据库与全部业务数据都会丢失。'
        Stop-Api
        Write-Step '移除容器与数据卷...'
        Invoke-Compose -ComposeArgs @('down', '-v')
        Invoke-StackUp
        Show-Status
    }

    'build' {
        Build-Solution
    }

    'test' {
        Write-Step '运行测试...'
        & dotnet test $SolutionFile --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "测试失败（退出码 $LASTEXITCODE）。"
        }
        Write-Ok '测试通过。'
    }

    'logs' {
        switch ($Target) {
            'api' {
                if ($Follow) {
                    Write-Step "持续跟踪 API 日志（Ctrl+C 退出）：$ApiOutLog"
                    if (Test-Path -LiteralPath $ApiOutLog) {
                        Get-Content -LiteralPath $ApiOutLog -Tail $Tail -Wait
                    }
                    else {
                        Show-ApiLogs
                    }
                }
                else {
                    Show-ApiLogs
                }
            }
            'mysql' {
                $logArgs = @('logs', '--tail', "$Tail")
                if ($Follow) { $logArgs += '-f' }
                $logArgs += 'mysql'
                Invoke-Compose -ComposeArgs $logArgs
            }
            default {
                Show-ApiLogs
                Write-Step 'MySQL 最近日志'
                Invoke-Compose -ComposeArgs @('logs', '--tail', "$Tail", 'mysql')
            }
        }
    }

    'cli' {
        Write-Step '进入 MySQL 命令行（输入 exit 退出）...'
        Invoke-Compose -ComposeArgs @(
            'exec', 'mysql', 'sh', '-c',
            'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" mysql -uroot "$MYSQL_DATABASE"'
        )
    }

    'status' {
        Show-Status
    }

    'init' {
        Confirm-Destructive -Message '将删除并重建数据库，随后由 API 重新执行 EF Core 迁移与演示数据。'
        Stop-Api
        Start-MySql
        Reset-DatabaseInsideContainer
        Build-Api
        Start-Api
        Show-Status
    }

    'migrate' {
        Start-MySql
        Invoke-MigrationUpdate
    }

    'ef:add' {
        if ([string]::IsNullOrWhiteSpace($MigrationName)) {
            throw '请提供迁移名称，例如：pwsh ./manage.ps1 ef:add -MigrationName AddRoomBalcony'
        }

        Assert-EfTool
        Write-Step "生成 EF Core 迁移：$MigrationName"
        & dotnet ef migrations add $MigrationName `
            --project (Join-Path $ScriptRoot 'src/HospiKeeper.Infrastructure/HospiKeeper.Infrastructure.csproj') `
            --startup-project $ApiProject `
            --output-dir Persistence/Migrations
        if ($LASTEXITCODE -ne 0) {
            throw "生成迁移失败（退出码 $LASTEXITCODE）。"
        }
        Write-Ok '迁移已生成。'
    }

    'pull' {
        Write-Step '拉取 MySQL 镜像...'
        Invoke-Compose -ComposeArgs @('pull', 'mysql')
        Write-Ok '镜像已更新。'
    }

    'desktop' {
        Open-Desktop
    }

    'dev' {
        Invoke-StackUp
        Open-Desktop
    }
}
