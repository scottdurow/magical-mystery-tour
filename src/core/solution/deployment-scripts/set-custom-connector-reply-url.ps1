
param (
    [string]$azureEnv,
    [bool]$skipLoginChecks
)


function SetRedirectUrl {


    # Prompt for the redirect url from the custom connector
    $redirectUrl = Read-Host "Paste the redirect url from the connector (Right click to paste in the console)"

    $appId = $envVars.ENTRA_API_CLIENT_APP_ID

    # Get the current reply urls for the client application registration
    $currentRedirectUris = az ad app show --id $appId --query "web.redirectUris" -o json | ConvertFrom-Json

    # If $currentRedirectUris is a string, add it to an array
    if ($currentRedirectUris -is [string]) {
        $currentRedirectUris = @($currentRedirectUris)
    }

    Write-Host "Existing redirect URIs: $($currentRedirectUris -join ', ')" -ForegroundColor Green

    # Check if the $redirectUrl is already in the redirect URIs
    if ($currentRedirectUris -contains $redirectUrl) {
        Write-Host "The redirect URI '$redirectUrl' is already in the client application '$appId'" -ForegroundColor Yellow
        exit
    }

    # Append the new redirect URI
    $currentRedirectUris += $redirectUrl

    Write-Host "Adding the web reply urls  '$redirectUrl' to the client application '$appId'" -ForegroundColor Green

    az ad app update --id $appId --web-redirect-uris $currentRedirectUris

    Write-Host "Complete" -ForegroundColor Green
}

# -------------------------------------------------------------------------
function FixCustomConnector {
    param (
        [string]$connectorName,
        [string]$environmentId,
        [string]$apiAppId
    )
    Write-Host
    Write-Host "Fixing up Custom Connector '$connectorName'" -ForegroundColor Green
    $customConnectorUrl = "https://make.powerautomate.com/environments/$environmentId/connections/custom"

    # $customConnectorUrl = "https://make.powerautomate.com/environments/$environmentId/connections/available/custom/$connectorName/edit/security"
    Write-Host "1. Open the [$connectorName] custom connector "
    Write-Host $customConnectorUrl -ForegroundColor Blue
    Write-Host ""
    Write-host "2. Copy the 'Redirect URL'"
    #Write-Host "3. Select Edit below the 'Redirect Url' field. This is a work around because SPN authentication does not currently support environment variables."
    #Write-Host "4. Update the following fields"
    #Write-Host "    Client Secret = @environmentVariables(""contoso_PaymentsApiSecret"")" -ForegroundColor Blue
    #Write-Host "    Resource Url = api://$apiAppId" -ForegroundColor Blue
    #Write-Host "5. Click 'Update Connector'."
    SetRedirectUrl
    Write-Host ""
}

Write-Host "This script will setup your Payment API client application registration in Entra ID with the custom connector reply url" -ForegroundColor White
# -------------------------------------------------------------------------
# Import the environment variables
# -------------------------------------------------------------------------
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv

# Check the user is logged into AZ CLI and PAC
if (-not $skipLoginChecks) {
    CheckAZCLI
}

$environmentDetails = pac env who --json | ConvertFrom-Json
$environmentId = $environmentDetails.EnvironmentId

FixCustomConnector -connectorName "Contoso Payments API" -environmentId $environmentId -apiAppId $envVars.ENTRA_API_APP_ID
FixCustomConnector -connectorName "Contoso Stripe API" -environmentId $environmentId -apiAppId $envVars.ENTRA_API_APP_ID
