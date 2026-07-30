<#
.SYNOPSIS
Measures time to the first visible window for an unpackaged tray build.

.DESCRIPTION
Starts the selected product identity with a new temporary data directory,
waits until a window is available, emits a small JSON record, then closes only
the process it started. This is a local baseline, not a benchmark or SLO.

.PARAMETER Xiaoza
Measure the independently branded xiaozaclaw build. The requested build output
must already exist.

.PARAMETER Configuration
Build configuration to inspect. Defaults to Debug.

.PARAMETER TimeoutSeconds
Maximum time to wait for the first window. Defaults to 30 seconds.
#>
[CmdletBinding()]
param(
    [switch]$Xiaoza,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [ValidateRange(1, 120)]
    [int]$TimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\OpenClaw.Tray.WinUI\OpenClaw.Tray.WinUI.csproj"
[xml]$project = Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8
$targetFramework = ($project.Project.PropertyGroup | Where-Object { $_.TargetFramework } | Select-Object -First 1).TargetFramework
if (-not $targetFramework) {
    throw "Unable to determine TargetFramework from $projectPath."
}

$runtimeIdentifier = if ($env:PROCESSOR_ARCHITECTURE -eq "ARM64") { "win-arm64" } else { "win-x64" }
$outputDir = Join-Path $repoRoot "src\OpenClaw.Tray.WinUI\bin\$Configuration\$targetFramework\$runtimeIdentifier"
$executablePath = Join-Path $outputDir "OpenClaw.Tray.WinUI.exe"
if (-not (Test-Path -LiteralPath $executablePath)) {
    throw "Tray executable not found: $executablePath. Build the requested identity first."
}

$expectedIdentity = if ($Xiaoza) { "xiaozaclaw" } else { "release" }
$identityMarkerPath = Join-Path $outputDir "app-identity.txt"
if (-not (Test-Path -LiteralPath $identityMarkerPath)) {
    throw "App identity marker not found: $identityMarkerPath."
}
$actualIdentity = (Get-Content -LiteralPath $identityMarkerPath -Raw -Encoding UTF8).Trim()
if ($actualIdentity -ne $expectedIdentity) {
    throw "Build output identity '$actualIdentity' does not match '$expectedIdentity'. Rebuild before measuring."
}

$dataDirectory = Join-Path $env:TEMP "OpenClawTray\startup-measure-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $dataDirectory -Force | Out-Null
$process = $null
$measurement = $null

try {
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new($executablePath)
    $startInfo.UseShellExecute = $false
    $startInfo.WorkingDirectory = $outputDir
    $startInfo.Environment["OPENCLAW_TRAY_DATA_DIR"] = $dataDirectory
    $startInfo.Environment["OPENCLAW_SKIP_UPDATE_CHECK"] = "1"
    $startInfo.Environment["OPENCLAW_FORCE_ONBOARDING"] = "0"

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) {
        throw "Failed to start $executablePath."
    }

    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        $process.Refresh()
        if ($process.HasExited) {
            throw "The tray process exited before it showed a window. Exit code: $($process.ExitCode)."
        }
        if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
            $stopwatch.Stop()
            $measurement = [PSCustomObject]@{
                identity = $actualIdentity
                configuration = $Configuration
                timeToFirstWindowMs = [Math]::Round($stopwatch.Elapsed.TotalMilliseconds)
                workingSetMiB = [Math]::Round($process.WorkingSet64 / 1MB, 1)
                cpuMilliseconds = [Math]::Round($process.TotalProcessorTime.TotalMilliseconds)
                windowTitle = $process.MainWindowTitle
            }
            break
        }
        Start-Sleep -Milliseconds 100
    }

    if ($null -eq $measurement) {
        throw "Timed out after $TimeoutSeconds seconds waiting for the tray window."
    }
}
finally {
    if ($null -ne $process) {
        $process.Refresh()
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            $process.WaitForExit(10000) | Out-Null
        }
        $process.Dispose()
    }
    Remove-Item -LiteralPath $dataDirectory -Force -Recurse -ErrorAction SilentlyContinue
}

$measurement | ConvertTo-Json -Compress
