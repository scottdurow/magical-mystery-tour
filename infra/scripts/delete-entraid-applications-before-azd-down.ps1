# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables

# Get the env variable ENTRA_APP_API_OBJECT_ID from the $envVars
$ENTRA_API_OBJECT_ID = $envVars.ENTRA_API_OBJECT_ID
Write-Host "Deleting the application with object id $ENTRA_API_OBJECT_ID"
az rest --method delete --url "https://graph.microsoft.com/v1.0/applications/$ENTRA_API_OBJECT_ID"
az rest --method delete --url "https://graph.microsoft.com/v1.0/directory/deletedItems/$ENTRA_API_OBJECT_ID"

# Get the env variable ENTRA_API_CLIENT_APP_ID from the $envVars
$ENTRA_API_CLIENT_OBJECT_ID = $envVars.ENTRA_API_CLIENT_OBJECT_ID
Write-Host "Deleting the application with object id $ENTRA_API_CLIENT_OBJECT_ID"
az rest --method delete --url "https://graph.microsoft.com/v1.0/applications/$ENTRA_API_CLIENT_OBJECT_ID"
az rest --method delete --url "https://graph.microsoft.com/v1.0/directory/deletedItems/$ENTRA_API_CLIENT_OBJECT_ID"

Write-Host "Complete" -ForegroundColor Green