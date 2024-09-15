param (
    [string]$azureEnv,
    [bool]$skipLoginChecks
)

Write-Host "This script updates the Plugin Managed Identity configuration for your environment" -ForegroundColor White
# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv

# The Application ID of the API Client
# In this sample we are not using a User Assigned Managed Identity (UAMI)
# But if we were, then this would be the Object ID of the UAMI
# And the UAMI would have App Roles assigned using the script src\core\plugins\payments-virtual-table-provider\ManagedIdentity\grant-managed-identity-api-app-roles.ps1
$ManagedIdentityPrincipal = $envVars.ENTRA_API_CLIENT_APP_ID

# Read the certificate thumbprint from the file src\core\plugins\payments-virtual-table-provider\ManagedIdentity\thumbprint.txt
$thumbprintPath = Join-Path -Path $PSScriptRoot -ChildPath "/../../plugins/payments-virtual-table-provider/ManagedIdentity/thumbprint.txt"

$thumbprint = Get-Content -Path $thumbprintPath

# Strip any carriage returns or line feeds
$thumbprint = $thumbprint -replace "`r|`n"

# Check the user is logged into AZ CLI and PAC
if (-not $skipLoginChecks) {
    CheckPACCLI
    CheckAZCLI
}

# Get the environment name that the user is currently authenticated for the Power Apps CLI and check that they are happy with this
$environment = pac env who --json | ConvertFrom-Json
$environmentName = $environment.FriendlyName

if (-not (ConfirmPrompt -message "Do you want to add the federated credentials for identity '$ManagedIdentityPrincipal' for environment '${environmentName}' (change this using pac auth)?")) {
    Write-Host "Exiting" -ForegroundColor Yellow
    exit
}

$TenantId = $envVars.AZURE_TENANT_ID
# remove - from the $environment.EnvironmentId
$environmentId = $environment.EnvironmentId
$environmenStrippedId = $environment.EnvironmentId -replace "-",""
# first 30 characters of the $environmentId
$environmentSegment1 = $environmenStrippedId.Substring(0,30)
# last 2 characters of the $environmentId
$environmentSegment2 = $environmenStrippedId.Substring($environmenStrippedId.Length - 2)

Write-Host "Adding Federated Credentials for the API Client Application $ManagedIdentityPrincipal in the environment $environmentId..." -ForegroundColor Green
# NOTE: The audience (including case) api://azureadtokenexchange might need to be updated to match the audience that Dataverse uses if that changes once GA
az ad app federated-credential create --id $ManagedIdentityPrincipal --parameters "{'name': 'Dataverse-$environmenStrippedId','issuer': 'https://$environmentSegment1.$environmentSegment2.environment.api.powerplatform.com/sts','subject': 'component:pluginassembly,thumbprint:$thumbprint,environment:$environmentId','description': 'Federated credentials for the Payments Virtual Entity Provider plugin','audiences': ['api://azureadtokenexchange']}" >> $null

# Update the Managed Identity record in the solution to point to the Application Id of the API Client or Managed Identity Object Id
# The ManagedIdentity Id is the GUID of the Managed Identity record in the solution - this will always be the same
# NOTE: This creates an unmanaged layer on top of the managed layer
Write-Host @"
Do you want to update the Managed Identity Record in environment '${environmentName}'
 (only do this for development environments because it will create am unmanaged layer if the solution is managed)
"@ -ForegroundColor Yellow

if ( ConfirmPrompt -message "Update Managed Identity?") {
    $pfxScript = @"
Patch('Managed Identities', LookUp('Managed Identities','ManagedIdentity Id' = GUID("fcdb7ca6-8d5e-ef11-bfe2-002248083aae")),
    {
        ApplicationId:GUID("$ManagedIdentityPrincipal"),
        TenantId:GUID("$TenantId")
    }
)
"@
    <#
    You can create a new managed identity using
    Collect('Managed Identities',
        {
            'Name':"Payments Virtual Entity Provider (unmanaged workaround)",
            ApplicationId:GUID("fd8ec592-ed17-4244-9e3c-47dc268e90e2"),
            TenantId:GUID("57872548-46bd-458e-8ae4-c2a541ea6d1b"),
            'Credential Source':'Credential Source (Managed Identities)'.IsManaged,
            'Subject Scope':'Subject Scope (Managed Identities)'.EnviornmentScope
        }
    )
    #>
    
    Write-Host "Updating the Managed Identity record in the solution..." -ForegroundColor Green
    $pfxName = "temp-update-managed-identity.pfx"
    $tempFilePath = Join-Path -Path $PSScriptRoot -ChildPath $pfxName
    Set-Content -Path $tempFilePath -Value $pfxScript
    pac pfx run --file $tempFilePath --echo #>> $null
    if ($? -eq $false) {
        throw $_.Exception
    }
}