[CmdletBinding()]
param(
    [switch]$Check
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$sourcePath = Join-Path $repoRoot 'src\OpenClaw.Tray.WinUI\Strings\zh-cn\Resources.resw'
$englishPath = Join-Path $repoRoot 'src\OpenClaw.Tray.WinUI\Strings\en-us\Resources.resw'
$overridePath = Join-Path $repoRoot 'src\OpenClaw.Tray.WinUI\XiaozaclawStrings\zh-cn\Overrides.json'
$outputPath = Join-Path $repoRoot 'src\OpenClaw.Tray.WinUI\XiaozaclawStrings\Resources.resw'

function Read-ResourceMap([string]$Path) {
    [xml]$document = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $map = [ordered]@{}
    foreach ($data in $document.root.data) {
        $map[[string]$data.name] = [string]$data.value
    }
    return $map
}

[xml]$document = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8
$english = Read-ResourceMap $englishPath
$configuration = Get-Content -LiteralPath $overridePath -Raw -Encoding UTF8 | ConvertFrom-Json
$translations = @{}
foreach ($property in $configuration.translations.PSObject.Properties) {
    $translations[$property.Name] = [string]$property.Value
}
$invariants = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($key in $configuration.invariantKeys) {
    [void]$invariants.Add([string]$key)
}

$knownKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($data in $document.root.data) {
    $key = [string]$data.name
    [void]$knownKeys.Add($key)
    $value = ([string]$data.value).Replace('OpenClawGateway', 'xiaozaclawGateway').Replace('OpenClaw', 'xiaozaclaw')
    if ($translations.ContainsKey($key)) {
        $value = $translations[$key]
    }
    $data.value = $value
}

$unknownOverrides = @($translations.Keys | Where-Object { -not $knownKeys.Contains($_) } | Sort-Object)
if ($unknownOverrides.Count -gt 0) {
    throw "Unknown xiaozaclaw translation keys: $($unknownOverrides -join ', ')"
}
$unknownInvariants = @($invariants | Where-Object { -not $knownKeys.Contains($_) } | Sort-Object)
if ($unknownInvariants.Count -gt 0) {
    throw "Unknown xiaozaclaw invariant keys: $($unknownInvariants -join ', ')"
}

$remainingEnglish = [System.Collections.Generic.List[string]]::new()
$remainingBrand = [System.Collections.Generic.List[string]]::new()
foreach ($data in $document.root.data) {
    $key = [string]$data.name
    $value = [string]$data.value
    if ($value.IndexOf('OpenClaw', [System.StringComparison]::Ordinal) -ge 0) {
        $remainingBrand.Add("$key = $value")
    }
    if ($english.Contains($key) -and
        $value -eq $english[$key] -and
        $value -match '[A-Za-z]{3}' -and
        -not $invariants.Contains($key)) {
        $remainingEnglish.Add("$key = $value")
    }
}
if ($remainingBrand.Count -gt 0) {
    throw "OpenClaw display-brand remnants found:`n$($remainingBrand -join "`n")"
}
if ($remainingEnglish.Count -gt 0) {
    throw "Untranslated xiaozaclaw resources found:`n$($remainingEnglish -join "`n")"
}

$settings = [System.Xml.XmlWriterSettings]::new()
$settings.Encoding = [System.Text.UTF8Encoding]::new($false)
$settings.Indent = $true
$settings.NewLineChars = "`n"
$settings.NewLineHandling = [System.Xml.NewLineHandling]::Entitize
$settings.OmitXmlDeclaration = $false
$builder = [System.Text.StringBuilder]::new()
$writer = [System.Xml.XmlWriter]::Create($builder, $settings)
$document.Save($writer)
$writer.Dispose()
$generated = $builder.ToString() + "`n"

if ($Check) {
    if (-not (Test-Path -LiteralPath $outputPath)) {
        throw "Generated xiaozaclaw resource file is missing. Run scripts\Generate-XiaozaclawResources.ps1."
    }
    $current = Get-Content -LiteralPath $outputPath -Raw -Encoding UTF8
    if ($current -cne $generated) {
        throw "Generated xiaozaclaw resources are stale. Run scripts\Generate-XiaozaclawResources.ps1."
    }
    Write-Host "xiaozaclaw resources are complete and current."
    return
}

$outputDirectory = Split-Path -Parent $outputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
[System.IO.File]::WriteAllText($outputPath, $generated, [System.Text.UTF8Encoding]::new($false))
Write-Host "Generated $outputPath"
