Write-Host "This script deletes all the connections for the custom connectors for all users" -ForegroundColor White
# Get the environment name that the user is currentl authenticated for the Power Apps CLI and check that they are happy with this

Install-Module -Name Microsoft.PowerApps.Administration.PowerShell -Scope CurrentUser
Install-Module -Name Microsoft.PowerApps.PowerShell -AllowClobber -Scope CurrentUser

# Prompt for the environment ID
$environmentID = Read-Host -Prompt "Please enter the Environment ID"


# Define a function to delete connections for a specific connector
function DeleteConnectorConnections {
    param (
        [string]$EnvironmentID,
        [string]$ConnectorSearch
    )

    # Delete all connections for a specific connector so that it can be deleted
    Get-AdminPowerAppConnection -EnvironmentName $EnvironmentID |
    Where-Object { $_.ConnectorName -like $ConnectorSearch } |
    ForEach-Object { 
        # Ask for confirmation before deleting the connection
        $confirmation = Read-Host "Are you sure you want to delete the connection '$($_.ConnectionName)' for connector: '$($_.ConnectorName)'? (Y/N)"
        if ($confirmation -eq "Y") {
            Write-Host "Deleting connection for connector: $($_.ConnectorName)" -ForegroundColor Yellow
            Remove-AdminPowerAppConnection -EnvironmentName $EnvironmentID -ConnectorName $_.ConnectorName -ConnectionName $_.ConnectionName > $null
        }
    }
}


DeleteConnectorConnections -EnvironmentID $environmentID -ConnectorSearch "*shared_payments-5f73e0190fb6721d27-5fb33b49e50c76a81d*"
DeleteConnectorConnections -EnvironmentID $environmentID -ConnectorSearch "*contoso-20stripe-20api-5f73e0190fb6721d27-5fb33b49e50c76a81d*"

Write-Host "All connections for the custom connectors have been deleted" -ForegroundColor Green
