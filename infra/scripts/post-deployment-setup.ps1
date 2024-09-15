# This script runs the post deployment steps after running azd up
# -----------------------------------------------------------------------
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables
$envName = $envVars.AZURE_ENV_NAME

. "$PSScriptRoot\grant-access-to-payment-api.ps1" -azureEnv $envName
. "$PSScriptRoot\grant-access-to-sql.ps1" -azureEnv $envName
. "$PSScriptRoot\setup-stripe.ps1" -azureEnv $envName