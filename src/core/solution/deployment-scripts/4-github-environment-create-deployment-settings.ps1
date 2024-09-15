param (
    [string]$azureEnv,
    [bool]$skipLoginChecks
)

Write-Host "This script creates deployment settings to add to a GitHub deployment environment" -ForegroundColor White


# -----------------------------------------------------------------------
# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv

# Generate the deployment settings for a chose azure deployment for the core solution
. "$PSScriptRoot\generate-deployment-settings.ps1" $envVars.AZURE_ENV_NAME

# Load the deployment settings file created at 'temp_deploymentSettings_$($envVars.AZURE_ENV_NAME).json'
$deploymentSettingsFile = "$PSScriptRoot\temp_deploymentSettings_$($envVars.AZURE_ENV_NAME).json"

# Check if the file exists
if (Test-Path -Path $deploymentSettingsFile) {
    # Load the content of the file
    $coreDeploymentSettingsContent = Get-Content -Path $deploymentSettingsFile -Raw

} else {
    Write-Host "Deployment settings file not found: $deploymentSettingsFile" -ForegroundColor Red
    exit 1
}

# Load the environment-settings.json
$environmentSettingsPath = Join-Path -Path $PSScriptRoot -ChildPath "/../environment-settings.json"
# Check if the file exists and then load
if (Test-Path -Path $environmentSettingsPath) {
    $environmentSettings = Get-Content -Path $environmentSettingsPath 
} else {
    Write-Host "Environment settings file not found: $environmentSettingsPath" -ForegroundColor Red
    exit 1
}

# Prompt for the Power Pages Portal Url
$portalUrl = Read-Host "Enter the Url for the Power Pages Portal deployment on this environment (E.g. https://cre-portal-dev.powerappsportals.com)"

# There is somewhat of a chicken and egg here, because we can't create the connections until the deployment has been done
$deploymentSettings = @"
{
"ContosoRealEstateCore": {
    "data": [
        "reference-data.zip",
        "sample-data.zip"
    ],
    "deploymentSettings": $coreDeploymentSettingsContent,
    "environmentSettings": $environmentSettings
},
"ContosoRealEstatePortal": {
    "EnvironmentVariables": [
        {
        "SchemaName":  "contoso_ContosoRealEstatePortalUr",
        "Value":  "$portalUrl"
        }
        ],
        "ConnectionReferences": [
        {
        "LogicalName": "contoso_PortalBotQueries",
        "ConnectionId": "",
        "ConnectorId": "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps"
        },
        {
        "LogicalName": "contoso_StripeAPI",
        "ConnectionId": "",
        "ConnectorId": "/providers/Microsoft.PowerApps/apis/shared_contoso-5fcontoso-20stripe-20api-5f6a4f91c8025d1333"
        }
    ]
}
}
"@

# Format $deploymentSettings as pretty json (With tab size of 4 characters)
$deploymentSettings = ConvertFrom-Json $deploymentSettings | ConvertTo-Json -Depth 100 -Compress

Write-Host
Write-Host "Add the a new Environment variable name PAC_DEPLOY_CONFIG:" -ForegroundColor Green
Write-Host $deploymentSettings

Write-Host
Write-Host "Add a new Environment secret variable" -ForegroundColor Green
Write-Host "PLUGIN_MANAGED_IDENTITY_APP_ID = $($envVars.ENTRA_API_CLIENT_APP_ID)"