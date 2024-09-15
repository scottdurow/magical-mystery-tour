# This script will read the .env file in the selected environment folder and output the name value pairs
# then use the values to configure the Contoso Real Estate Power Platform environment that you are logged into using the Power Platform CLI
param (
    [string]$azureEnv
)

function GetUpsertEnvironmentVariablePfx($schemaName,$displayName,$type,$value) {
    Write-Host "Environment Variable '$schemaName' with value '$value'"
    $envVarCreateScript = @"
    // ----------------------------------------------------
    // Environment Variable ${$displayName}
    // ----------------------------------------------------

    // Create if it doesn't exist
    If(IsBlank(LookUp('Environment Variable Definitions','Schema Name'="${schemaName}")),
        Collect('Environment Variable Definitions',
            {
                'Schema Name':"${schemaName}",
                'Display Name':"${displayName}",
                'Is Required':'Is Required (Environment Variable Definitions)'.No,
                Type:'Type (Environment Variable Definitions)'.${type}
            }
        );
    );

    // Delete the current value
    If(!IsBlank(LookUp('Environment Variable Values', ThisRecord.'Environment Variable Definition'.'Environment Variable Definition' = LookUp('Environment Variable Definitions','Schema Name'="${schemaName}").'Environment Variable Definition')),
        Remove(
            'Environment Variable Values',
                LookUp('Environment Variable Values',
                ThisRecord.'Environment Variable Definition'.'Environment Variable Definition' = LookUp('Environment Variable Definitions','Schema Name'="${schemaName}").'Environment Variable Definition'
            )
        );
    );

    // Add the value
    Collect('Environment Variable Values',
        {
            'Environment Variable Definition':LookUp('Environment Variable Definitions','Schema Name'="${schemaName}"),
            'Schema Name':GUID(),
            'Value':"${value}",
            Owner:LookUp('Environment Variable Definitions','Schema Name'="${schemaName}").'Created By'
        }
    );
"@;
    return $envVarCreateScript;
}

# -----------------------------------------------------------------------
Write-Host "This script will setup the environment variables in your power platform environment for the selected Azure deployment" -ForegroundColor White
# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv


# Get Tenant ID, Application ID, OAuth 2.0 authorization endpoint (v2), OAuth 2.0 token endpoint (v2)
$solutionPrefix = 'contoso'
$apiAppName = 'PaymentsApi'
$tenantId = $envVars.AZURE_TENANT_ID
$subscriptionId = $envVars.AZURE_SUBSCRIPTION_ID
$appHostUrl = $envVars.SERVICE_API_URI.TrimStart("https://")
$keyVaultResourceGroup = $envVars.AZURE_RESOURCE_PREFIX
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

$pfxScript = "";

$pfxScript += GetUpsertEnvironmentVariablePfx `
    -schemaName:$tenantIdEnvVarName  `
    -displayName:"${apiAppName} Tenant Id" `
    -type:"String" `
    -value:$tenantId

$pfxScript += GetUpsertEnvironmentVariablePfx `
    -schemaName:$appIdEnvVarName `
    -displayName:"${apiAppName} AppId" `
    -type:"String" `
    -value:$app

$secretValue = "/subscriptions/${subscriptionId}/resourceGroups/${keyVaultResourceGroup}/providers/Microsoft.KeyVault/vaults/${keyVaultName}/secrets/${vaultSecretName}"
$pfxScript += GetUpsertEnvironmentVariablePfx `
    -schemaName:$secretEnvVarName `
    -displayName:"${apiAppName} Secret" `
    -type:"Secret" `
    -value:"${secretValue}"
    
$pfxScript += GetUpsertEnvironmentVariablePfx `
    -schemaName:$resourceUrlEnvVarName `
    -displayName:"${apiAppName} Resource Url"`
    -type:"String" `
    -value:$appResourceUri

$scope = "${appResourceUri}/${apiUserAccessScope}"
$pfxScript += GetUpsertEnvironmentVariablePfx `
    -schemaName:$scopeEnvVarName `
    -displayName:"${apiAppName} Scope" `
    -type:"String" `
    -value:"${scope}"

$pfxScript += GetUpsertEnvironmentVariablePfx `
    -schemaName:$hostEnvVarName `
    -displayName:"${apiAppName} Host" `
    -type:"String" `
    -value:$appHostUrl

Write-Host "Checking access via Power Platform CLI..." -ForegroundColor Green

# Get the environment name that the user is currently authenticated for the Power Apps CLI and check that they are happy with this
$environment = pac env who --json | ConvertFrom-Json
$environmentName = $environment.FriendlyName

Write-Host @"
You are currently authenticated to the Power Apps CLI for the environment '${environmentName}'

Do you want to update your Power Platform environment with these environment varibles? (Y/N)
"@ -ForegroundColor Yellow

$confirm = Read-Host 

if ($confirm.ToUpper() -ne 'Y') {
    Write-Host "Exiting"
    return
}

 Write-Host "Upserting Environment Variables..."
 Set-Content -Path .\temp-create-env-vars.pfx -Value $pfxScript
 pac pfx run --file "temp-create-env-vars.pfx" >> $null
 if ($? -eq $false) {
     throw $_.Exception
 }

Write-Host "Complete" -ForegroundColor Green