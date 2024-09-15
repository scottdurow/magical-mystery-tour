# This script adds the api managed identity to the sql database to allow it to access the payments data
param (
    [string]$azureEnv
)

Write-Host "This script adds the api managed identity to the sql database to allow it to access the payments data" -ForegroundColor White
# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv
$APPMANAGEDIDENTITY = $envVars.SERVICE_API_NAME

$sqlScript = @"
DROP USER IF EXISTS [$APPMANAGEDIDENTITY]
-- Create a user in the database for the managed identity
CREATE USER [$APPMANAGEDIDENTITY] FROM EXTERNAL PROVIDER

-- The object ID of the managed identity
EXEC sp_addrolemember 'db_datareader', '$APPMANAGEDIDENTITY'
EXEC sp_addrolemember 'db_datawriter', '$APPMANAGEDIDENTITY'
"@

Write-Host "Granting the Api identity access to the SQL Payments database" -ForegroundColor Green
# login to SQL using the current user's identity
$serverInstance = "$($envVars.AZURE_RESOURCE_PREFIX)-sql.database.windows.net"
$databaseName = "$($envVars.AZURE_RESOURCE_PREFIX)-payments-sql-db"
$connectionString = "Server=$serverInstance;Database=$databaseName;"

$token=$(az account get-access-token --resource=https://database.windows.net/ --query accessToken --output tsv)

# Load the .NET SQL Client assembly
Add-Type -AssemblyName "System.Data"

# Create and open the SQL connection
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.AccessToken = $token
$connection.Open()

# Create the SQL command
$command = $connection.CreateCommand()
$command.CommandText = $sqlScript

# Execute the SQL command
try {
    $command.ExecuteNonQuery()  | Out-Null
    Write-Host "SQL query executed successfully." -ForegroundColor Green
} catch {
    Write-Error "An error occurred while executing the SQL query: $_"
} finally {
    # Close the connection
    $connection.Close()
}

Write-Host "Complete" -ForegroundColor Green