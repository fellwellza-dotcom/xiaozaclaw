[CmdletBinding()]
param(
    [ValidateRange(1, 65535)]
    [int]$Port = 3000,
    [switch]$Stop
)

$ErrorActionPreference = 'Stop'
$deploymentRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$composePath = Join-Path $deploymentRoot 'compose.yaml'
$environmentPath = Join-Path $deploymentRoot '.env'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker Desktop is not installed. Install and start Docker Desktop, then run this script again.'
}

docker info *> $null
if ($LASTEXITCODE -ne 0) {
    throw 'Docker Desktop is not running. Start Docker Desktop, then run this script again.'
}

if ($Stop) {
    docker compose --env-file $environmentPath -f $composePath down
    exit $LASTEXITCODE
}

if (-not (Test-Path -LiteralPath $environmentPath)) {
    $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        function New-Secret([int]$length = 32) {
            $bytes = New-Object byte[] $length
            $random.GetBytes($bytes)
            return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
        }

        $content = @(
            "XIAOZACLAW_PORT=$Port"
            'POSTGRES_USER=xiaozaclaw'
            'POSTGRES_DB=new-api'
            "POSTGRES_PASSWORD=$(New-Secret)"
            "REDIS_PASSWORD=$(New-Secret)"
            "SESSION_SECRET=$(New-Secret 48)"
            'TZ=Asia/Shanghai'
            'SESSION_COOKIE_SECURE=false'
        ) -join [Environment]::NewLine
        [IO.File]::WriteAllText($environmentPath, "$content$([Environment]::NewLine)", [Text.UTF8Encoding]::new($false))
        Write-Host 'Created local secrets in xiaozaclaw/.env.'
    }
    finally {
        $random.Dispose()
    }
}

docker compose --env-file $environmentPath -f $composePath up --build --detach
if ($LASTEXITCODE -ne 0) {
    throw 'xiaozaclaw could not start. Run docker compose --env-file .env -f compose.yaml logs gateway for details.'
}

$portLine = Get-Content -LiteralPath $environmentPath |
    Where-Object { $_ -like 'XIAOZACLAW_PORT=*' } |
    Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($portLine)) {
    throw 'The deployment environment is missing XIAOZACLAW_PORT.'
}

$configuredPort = $portLine.Split('=', 2)[1]
Write-Host "xiaozaclaw is running at http://localhost:$configuredPort. Complete the administrator setup in your browser."
Write-Host 'After initialization, configure HTTPS, a trusted origin, employee accounts, model channels, permissions, and quotas.'
