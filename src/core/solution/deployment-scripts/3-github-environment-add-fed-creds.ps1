param (
    [string]$azureEnv,
    [bool]$skipLoginChecks
)

Write-Host "This script sets up a GitHub environment federated credentials for deployment to a specific environment" -ForegroundColor White
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables $azureEnv
$azureEnv = $envVars.AZURE_ENV_NAME

# Check the user is logged into AZ CLI and PAC
if (-not $skipLoginChecks) {
    CheckPACCLI
    CheckAZCLI
}

# Prompt for the name of the Github environment
$environmentName = Read-Host "Enter the name of the GitHub environment (e.g. development/test/production)"
$tenantId = az account show --query tenantId -o tsv
$environmentDetails = pac env who --json | ConvertFrom-Json
$environmentUrl = $environmentDetails.OrgUrl.TrimEnd('/')
$spnName = "cre-github-workflows-$environmentName"
Set-Location -Path $PSScriptRoot
$remoteUrl = git remote get-url origin
if ($remoteUrl -match "github\.com[:/](.+?)/(.+?)(\.git)?$") { $repoName = $matches[1] + "/" + $matches[2] }


if (-not (ConfirmPrompt -message "Are you sure you want to create federated credentials for GitHub '$environmentName' in the repo '$repoName' for the environment '$environmentUrl' ?")) {
    Write-Host "Exiting" -ForegroundColor Yellow
    exit
}

# Check if the spn already exist
$applicationId = az ad sp list --display-name $spnName --query "[0].appId" -o tsv

if ($applicationId)
{
    Write-Host "Adding existing SPN '$spnName' and adding to '$environmentUrl'" -ForegroundColor Green
    pac admin assign-user --environment $environmentUrl --application-user --user $applicationId --role "System Administrator"
}
else
{
    Write-Host "Creating SPN '$spnName' and adding to '$environmentUrl'" -ForegroundColor Green
    pac admin create-service-principal --name $spnName
    # Currently the pac admin create-service-principal or list-service-principal verbs do not support the --json command, so use az to get the application id
    $applicationId = az ad sp list --display-name $spnName --query "[0].appId" -o tsv
}

Write-Host "Adding federated credentials to $applicationId for environment $environmentName and repo $repoName" -ForegroundColor Green
az ad app federated-credential create --id $applicationId --parameters "{'name': '$spnName','issuer': 'https://token.actions.githubusercontent.com','subject': 'repo:$($repoName):environment:$environmentName','description': 'GitHub access for the environment $environmentName and repo $repoName ','audiences': ['api://AzureADTokenExchange']}" >> $null

Write-Host "Adding Key Vault access to the SPN so that Key Vault Environment Variables in Dataverse can be read" -ForegroundColor Green
# This prevents the solution import error 'The reason given was: User is not authorized to read secrets from '/subscriptions/.../resourceGroups/.../providers/Microsoft.KeyVault/vaults/..../secrets/...-development-payments-api-client-secret' resource.
az role assignment create --role "Key Vault Secrets User" --assignee $applicationId --scope /subscriptions/$($envVars.AZURE_SUBSCRIPTION_ID)/resourceGroups/$($envVars.AZURE_RESOURCE_GROUP)/providers/Microsoft.KeyVault/vaults/$($envVars.AZURE_KEY_VAULT_NAME) >> $null
az role assignment create --role "Key Vault Reader" --assignee $applicationId --scope /subscriptions/$($envVars.AZURE_SUBSCRIPTION_ID)/resourceGroups/$($envVars.AZURE_RESOURCE_GROUP)/providers/Microsoft.KeyVault/vaults/$($envVars.AZURE_KEY_VAULT_NAME) >> $null

Write-Host @"
Add the following GitHub environment secrets to the environment '$environmentName' in the repo '$repoName'

    PAC_DEPLOY_AZURE_TENANT_ID = $tenantId
    PAC_DEPLOY_CLIENT_ID = $applicationId
    PAC_DEPLOY_ENV_URL = $environmentUrl
"@ -ForegroundColor Cyan