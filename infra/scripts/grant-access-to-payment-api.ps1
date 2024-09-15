# This script assigns all the app roles of the payments api to the current user for testing
# -----------------------------------------------------------------------
param (
    [string]$azureEnv
)

function AssignRolesToPrincipal {
    param (
        [string]$roleNames,
        [string]$principalId,
        [object]$appId
    )

    # Convert the comma-separated list to an array
    $roleNamesArray = $roleNames -split ','

    
    $appRoles = $(az ad app show --id $appId --query "appRoles" --output json)
    $appRoles = $appRoles | ConvertFrom-Json

    # Get the service principal id
    $servicePrincipalId = az ad sp list --filter "appId eq '$appId'" --query "[0].id" --output tsv

    # Loop through each object and get the id if the value is in the list
    foreach ($item in $appRoles) {
        if ($roleNamesArray -contains $item.value) {
            Write-Host "Assigning role $($item.value)[$($item.id)] to principal [$principalId]" -ForegroundColor Green
            # See https://learn.microsoft.com/en-us/graph/api/serviceprincipal-post-approleassignedto?view=graph-rest-1.0&tabs=http
            az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$servicePrincipalId/appRoleAssignedTo" --body $body "{'principalId': '$principalId','resourceId': '$servicePrincipalId','appRoleId': '$($item.id)'}"
        }
    }
}
# -----------------------------------------------------------------------
# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv

# -----------------------------------------------------------------------
Write-Host "This script and assigns all the app roles of the payments api to the current user for testing" -ForegroundColor White

$appId = $envVars.ENTRA_API_APP_ID

# Assign the roles to the current user for testing
Write-Host "Granting access to the Payment API for the current user" -ForegroundColor Green
$currentUserPrincipalId=$(az ad signed-in-user show --query id -o tsv)
AssignRolesToPrincipal -roleNames "CanAddPayments,CanQueryPayments,CanCreateStripeSessions" -principalId $currentUserPrincipalId -appId $appId

# The Client for Contoso Real Estate Payments API needs admin consent if it's used as a service principal to access the API
Write-Host "Granting access to the Payment API for the SPN used in connections" -ForegroundColor Green
az ad app permission admin-consent --id $envVars.ENTRA_API_CLIENT_APP_ID

Write-Host "Complete" -ForegroundColor Green

