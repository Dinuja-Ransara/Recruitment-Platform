<#
    Publishes Meridian.Api to MonsterASP.NET over Web Deploy.

    Nothing secret lives in this script. It reads the connection string from
    deploy/connectionstring.txt and the hosting credentials from the
    .publishSettings file, both of which are gitignored. The production
    appsettings file it generates is gitignored too.

    Usage, from the repository root:
        pwsh -File deploy/publish.ps1
#>

$ErrorActionPreference = 'Stop'

$repoRoot   = Split-Path -Parent $PSScriptRoot
$deployDir  = Join-Path $repoRoot 'deploy'
$apiProject = Join-Path $repoRoot 'src/Meridian.Api/Meridian.Api.csproj'
$publishDir = Join-Path $deployDir 'publish-output'

# --- Inputs -----------------------------------------------------------------

$profilePath = Get-ChildItem -Path $deployDir -Filter '*.publishSettings' |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $profilePath) {
    throw "No .publishSettings file found in $deployDir."
}

$connectionStringPath = Join-Path $deployDir 'connectionstring.txt'
if (-not (Test-Path $connectionStringPath)) {
    throw "Missing $connectionStringPath. Paste the MSSQL connection string from the MonsterASP control panel into that file."
}

$connectionString = (Get-Content $connectionStringPath -Raw).Trim()

[xml]$profileXml = Get-Content $profilePath
$node = $profileXml.publishData.publishProfile | Where-Object { $_.publishMethod -eq 'MSDeploy' } | Select-Object -First 1

$publishUrl   = $node.publishUrl
$siteName     = $node.msdeploySite
$userName     = $node.userName
$password     = $node.userPWD
$destinationUrl = $node.destinationAppUrl

Write-Host "Publishing to $siteName at $destinationUrl" -ForegroundColor Cyan

# --- Production configuration ----------------------------------------------
# A fresh signing key is generated on every publish. The application refuses to
# start outside development if the committed placeholder key is still in place.

$keyBytes = New-Object 'System.Byte[]' 48
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($keyBytes)
$signingKey = [Convert]::ToBase64String($keyBytes)

$productionSettings = [ordered]@{
    ConnectionStrings = [ordered]@{ Default = $connectionString }
    Jwt = [ordered]@{
        Issuer             = 'Meridian.Api'
        Audience           = 'Meridian.Web'
        SigningKey         = $signingKey
        AccessTokenMinutes = 120
    }
    Cors = [ordered]@{
        AllowedOrigins         = @('https://meridian-talent.pages.dev')
        AllowedOriginSuffixes  = @('meridian-talent.pages.dev')
    }
}

$settingsPath = Join-Path $repoRoot 'src/Meridian.Api/appsettings.Production.json'
$productionSettings | ConvertTo-Json -Depth 6 | Set-Content -Path $settingsPath -Encoding utf8
Write-Host "Wrote $settingsPath (gitignored)" -ForegroundColor DarkGray

# --- Build ------------------------------------------------------------------

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

Write-Host 'Building release output...' -ForegroundColor Cyan
dotnet publish $apiProject -c Release -o $publishDir --nologo
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

# ASPNETCORE_ENVIRONMENT is set on the server through web.config so that the
# production settings file is the one that loads.
$webConfigPath = Join-Path $publishDir 'web.config'
if (Test-Path $webConfigPath) {
    [xml]$webConfig = Get-Content $webConfigPath

    # The aspNetCore element sits under <location><system.webServer>, not directly
    # under <configuration>, so it has to be selected by XPath rather than by
    # dotted property access, which silently returns nothing for the wrong path.
    $handler = $webConfig.SelectSingleNode('//aspNetCore')
    if (-not $handler) { throw 'Could not find the aspNetCore element in web.config.' }

    # Startup failures on a shared host are otherwise invisible: IIS returns a
    # generic 500.30 with no detail. stdout logging is what makes them readable.
    $handler.SetAttribute('stdoutLogEnabled', 'true')
    $handler.SetAttribute('stdoutLogFile', '.\logs\stdout')

    $envNode = $webConfig.CreateElement('environmentVariables')
    foreach ($pair in @(
        @{ name = 'ASPNETCORE_ENVIRONMENT';   value = 'Production' },
        @{ name = 'ASPNETCORE_DETAILEDERRORS'; value = 'true' }
    )) {
        $var = $webConfig.CreateElement('environmentVariable')
        $var.SetAttribute('name',  $pair.name)
        $var.SetAttribute('value', $pair.value)
        $envNode.AppendChild($var) | Out-Null
    }

    $handler.AppendChild($envNode) | Out-Null
    $webConfig.Save($webConfigPath)
    Write-Host 'web.config: environment pinned, stdout logging enabled' -ForegroundColor DarkGray
}

# --- Deploy -----------------------------------------------------------------

$msdeploy = 'C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe'
if (-not (Test-Path $msdeploy)) { throw "msdeploy.exe not found at $msdeploy" }

# Port 8172 is the Web Management Service listener. The publishSettings file
# gives the host without a scheme or port, and the endpoint is not served on 443.
$endpoint = "https://${publishUrl}:8172/msdeploy.axd?site=$siteName"
$dest = "contentPath=$siteName,computerName=$endpoint,userName=$userName,password=$password,authType=Basic"

Write-Host 'Syncing to host...' -ForegroundColor Cyan
& $msdeploy `
    '-verb:sync' `
    "-source:contentPath=$publishDir" `
    "-dest:$dest" `
    '-enableRule:AppOffline' `
    '-allowUntrusted' `
    '-retryAttempts:3'

if ($LASTEXITCODE -ne 0) { throw "msdeploy failed with exit code $LASTEXITCODE." }

Write-Host "Deployed. Site: $destinationUrl" -ForegroundColor Green
Write-Host "Swagger: ${destinationUrl}swagger" -ForegroundColor Green
