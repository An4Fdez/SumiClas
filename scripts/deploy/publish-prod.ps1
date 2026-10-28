<#
.SYNOPSIS
    Publishes the API together with the web build to the production App Service.

.DESCRIPTION
    1. Checks the tools and the Azure session.
    2. Runs `dotnet publish -c Release` (which also builds the web with pnpm and copies it to wwwroot).
    3. Zips the result and deploys it with `az webapp deploy`.
    4. Waits until the health check answers.

    The app settings (connection string, JWT key...) are NOT touched: they live in the App Service.

.PARAMETER ResourceGroup
    Resource group of the App Service.

.PARAMETER AppName
    App Service name. When omitted it is read from `terraform output app_name` (terraform folder).

.PARAMETER SkipConfirmation
    Does not ask before deploying.

.PARAMETER BuildOnly
    Builds and zips but does not deploy; prints where the zip is.

.EXAMPLE
    .\scripts\deploy\publish-prod.ps1
    .\scripts\deploy\publish-prod.ps1 -AppName app-sumiclas-prod-vetju -SkipConfirmation
#>
[CmdletBinding()]
param(
    [string]$ResourceGroup = 'Dev-Resource-Group-Ana',
    [string]$AppName,
    [switch]$SkipConfirmation,
    [switch]$BuildOnly
)

$ErrorActionPreference = 'Stop'

$HealthPath = '/healthy'
$HealthMaxAttempts = 30
$HealthDelaySeconds = 10
$HealthRequestTimeoutSeconds = 30
$ApiProject = 'apps/backend/api/SumiClas.Api.csproj'
$ApiProcessName = 'SumiClas.Api'

function Write-Step([string]$Message) {
    Write-Host ''
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Invoke-Native([string]$Description, [scriptblock]$Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Description falló (código $LASTEXITCODE)."
    }
}

# Windows PowerShell 5.1 turns the stderr of a redirected native command into a terminating error under 'Stop'.
function Invoke-Quiet([scriptblock]$Command) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        & $Command 2>$null
    } finally {
        $ErrorActionPreference = $previous
    }
}

function Test-Tool([string]$Name, [string]$Hint) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "No encuentro '$Name' en el PATH. $Hint"
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$workDir = Join-Path ([System.IO.Path]::GetTempPath()) ("sumiclas-publish-" + [Guid]::NewGuid().ToString('N'))
$publishDir = Join-Path $workDir 'publish'
$zipPath = Join-Path $workDir 'publish.zip'

try {
    Write-Step 'Comprobando herramientas'
    Test-Tool 'dotnet' 'Instala el SDK de .NET 10.'
    Test-Tool 'node' 'Instala Node.js: la publicación construye la web.'
    Test-Tool 'pnpm' 'Instala pnpm: la publicación construye la web.'
    if (-not $BuildOnly) {
        Test-Tool 'az' 'Instala la CLI de Azure (winget install Microsoft.AzureCLI).'
    }

    if (Get-Process -Name $ApiProcessName -ErrorAction SilentlyContinue) {
        throw "La API ($ApiProcessName) está en ejecución y bloquea los ficheros. Párala (también desde el IDE) y vuelve a lanzar."
    }

    $subscriptionName = $null
    if (-not $BuildOnly) {
        Write-Step 'Comprobando la sesión de Azure'
        $account = Invoke-Quiet { az account show --output json } | ConvertFrom-Json
        if (-not $account) {
            throw "No hay sesión de Azure. Ejecuta 'az login' y vuelve a lanzar."
        }
        $subscriptionName = "$($account.name) ($($account.id))"
        Write-Host "Suscripción: $subscriptionName"

        if (-not $AppName) {
            Test-Tool 'terraform' "Indica el nombre con -AppName o instala Terraform para leerlo de 'terraform output'."
            Push-Location (Join-Path $repoRoot 'terraform')
            try {
                $AppName = Invoke-Quiet { terraform output -raw app_name }
            } finally {
                Pop-Location
            }
            if ($LASTEXITCODE -ne 0 -or -not $AppName) {
                throw "No he podido leer 'app_name' de Terraform. Indica el nombre con -AppName."
            }
        }

        Invoke-Quiet { az webapp show --resource-group $ResourceGroup --name $AppName --query name --output tsv } | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "No existe el App Service '$AppName' en el grupo '$ResourceGroup' de esa suscripción."
        }
    }

    Write-Step 'Compilando y publicando (API + web)'
    New-Item -ItemType Directory -Path $workDir -Force | Out-Null
    Push-Location $repoRoot
    try {
        Invoke-Native 'dotnet publish' { dotnet publish $ApiProject -c Release -o $publishDir }
    } finally {
        Pop-Location
    }

    $indexPath = Join-Path $publishDir 'wwwroot\index.html'
    if (-not (Test-Path $indexPath)) {
        throw "La publicación no contiene wwwroot\index.html: la web no se ha construido. Revisa la salida de pnpm."
    }

    Write-Step 'Empaquetando'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)
    $zipMegabytes = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
    Write-Host "Paquete: $zipPath ($zipMegabytes MB)"

    if ($BuildOnly) {
        $keptZip = Join-Path ([System.IO.Path]::GetTempPath()) 'sumiclas-publish.zip'
        Copy-Item $zipPath $keptZip -Force
        Write-Host ''
        Write-Host "Solo compilación: el paquete queda en $keptZip" -ForegroundColor Green
        return
    }

    if (-not $SkipConfirmation) {
        Write-Host ''
        Write-Host "Se va a desplegar en PRODUCCIÓN:" -ForegroundColor Yellow
        Write-Host "  App Service: $AppName"
        Write-Host "  Grupo:       $ResourceGroup"
        Write-Host "  Suscripción: $subscriptionName"
        $answer = Read-Host 'Escribe "si" para continuar'
        if ($answer -ne 'si') {
            Write-Host 'Cancelado, no se ha desplegado nada.'
            return
        }
    }

    Write-Step 'Subiendo al App Service'
    Invoke-Native 'az webapp deploy' {
        az webapp deploy --resource-group $ResourceGroup --name $AppName --src-path $zipPath --type zip
    }

    Write-Step 'Esperando a que la aplicación responda'
    $hostName = az webapp show --resource-group $ResourceGroup --name $AppName --query defaultHostName --output tsv
    $healthUrl = "https://$hostName$HealthPath"
    $healthy = $false
    for ($attempt = 1; $attempt -le $HealthMaxAttempts; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec $HealthRequestTimeoutSeconds
            if ($response.StatusCode -eq 200) {
                $healthy = $true
                break
            }
        } catch {
            Write-Host "  intento $attempt/${HealthMaxAttempts}: todavía no responde"
        }
        Start-Sleep -Seconds $HealthDelaySeconds
    }

    if (-not $healthy) {
        throw "La aplicación no respondió 200 en $healthUrl. Mira el registro: az webapp log tail --resource-group $ResourceGroup --name $AppName"
    }

    Write-Host ''
    Write-Host "Publicado. $healthUrl responde 200." -ForegroundColor Green
    Write-Host "Abre: https://$hostName"
} finally {
    if (Test-Path $workDir) {
        Remove-Item $workDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
