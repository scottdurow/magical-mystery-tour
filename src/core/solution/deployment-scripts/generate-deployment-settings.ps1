# This script will use the Azure deployment environment variables to create a deploymentSettings.json file for deployment of the ContosoRealEstateCore solution.
param (
    [string]$azureEnv
)


# -----------------------------------------------------------------------
# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv


# Get Tenant ID, Application ID, OAuth 2.0 authorization endpoint (v2), OAuth 2.0 token endpoint (v2)
$solutionPrefix = 'contoso'
$apiAppName = 'PaymentsApi'
$tenantId = $envVars.AZURE_TENANT_ID
$subscriptionId = $envVars.AZURE_SUBSCRIPTION_ID
$appHostUrl = $envVars.SERVICE_API_URI.TrimStart("https://")
$keyVaultResourceGroup = $envVars.AZURE_RESOURCE_GROUP
$keyVaultName = $envVars.AZURE_KEY_VAULT_NAME
$vaultSecretName = $envVars.AZURE_KEY_VAULT_ENTRA_API_SECRET_NAME
$app = $envVars.ENTRA_API_CLIENT_APP_ID
$appResourceUri = $envVars.SERVICE_API_RESOURCE_URI
$apiUserAccessScope = "user_impersonation"

# Environment variable names
$tenantIdEnvVarName = "${solutionPrefix}_${apiAppName}TenantId";
$appIdEnvVarName = "${solutionPrefix}_${apiAppName}AppId";
$secretEnvVarName = "${solutionPrefix}_${apiAppName}Secret";
$resourceUrlEnvVarName = "${solutionPrefix}_${apiAppName}ResourceUrl";
$scopeEnvVarName = "${solutionPrefix}_${apiAppName}Scope";
$hostEnvVarName = "${solutionPrefix}_${apiAppName}Host";
$hostBaseUrlVarName = "${solutionPrefix}_${apiAppName}BaseUrl";
$deploymentSettingsEnvironmentVariables = "";

$secretValue = "/subscriptions/${subscriptionId}/resourceGroups/${keyVaultResourceGroup}/providers/Microsoft.KeyVault/vaults/${keyVaultName}/secrets/${vaultSecretName}"
$scope = "${appResourceUri}/${apiUserAccessScope}"

function EnvironmentVariableJson($schemaName, $value, [bool]$isLast = $false) {
    $json = @{
        "SchemaName" = $schemaName
        "Value" = $value
    } | ConvertTo-Json

    if (-not $isLast) {
        $json += "," + [Environment]::NewLine
    }

    return $json
}

$deploymentSettingsEnvironmentVariables += EnvironmentVariableJson $appIdEnvVarName $app
    
$deploymentSettingsEnvironmentVariables += EnvironmentVariableJson $hostBaseUrlVarName "/api"

$deploymentSettingsEnvironmentVariables += EnvironmentVariableJson $hostEnvVarName $appHostUrl

$deploymentSettingsEnvironmentVariables += EnvironmentVariableJson $resourceUrlEnvVarName $appResourceUri

$deploymentSettingsEnvironmentVariables += EnvironmentVariableJson $scopeEnvVarName $scope

$deploymentSettingsEnvironmentVariables += EnvironmentVariableJson $secretEnvVarName $secretValue

$deploymentSettingsEnvironmentVariables += EnvironmentVariableJson $tenantIdEnvVarName $tenantId $true


$deploymentSettings = @"
{
"EnvironmentVariables": [
$deploymentSettingsEnvironmentVariables
],
"ConnectionReferences": []
}
"@

# Output the deployment settings to the deploymentSettings_AZURE_ENV_NAM.json file
$azureEnv = $envVars.AZURE_ENV_NAME
$deploymentSettingsFilePath = "$PSScriptRoot\temp_deploymentSettings_${azureEnv}.json"

Write-Host "Generating deployment settings file at $deploymentSettingsFilePath" -ForegroundColor Green
Set-Content -Path $deploymentSettingsFilePath -Value $deploymentSettings
