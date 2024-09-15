param (
    [string]$applicationId,
    [string]$tenantId,
    [bool]$silent
)
# MANAGED_IDENTITY_IMPORT_BUG
Write-Host "This script updates the Plugin Managed Identity configuration for your environment" -ForegroundColor White

# If applicationId is not set, then prompt for it
if (-not $applicationId) {
    $applicationId = Read-Host "Enter the Application Id of the Managed Identity"
}
#If tenantId is not set, then prompt for it
if (-not $tenantId) {
    $tenantId = Read-Host "Enter the Tenant Id of the Managed Identity"
}

# Check the user is logged into AZ CLI and PAC
if (-not $silent) {
    . "$PSScriptRoot\..\function-get-environment-variables.ps1"

    CheckPACCLI

    # Get the environment name that the user is currently authenticated for the Power Apps CLI and check that they are happy with this
    $environment = pac env who --json | ConvertFrom-Json
    $environmentName = $environment.FriendlyName


    if (-not (ConfirmPrompt -message "Do you want to add the managed identity '$applicationId' for environment '${environmentName}' (change this using pac auth)?")) {
        Write-Host "Exiting" -ForegroundColor Yellow
        exit
    }
}

# Create a new GUID for the Managed Identity
$ManagedIdentityId = [guid]::NewGuid().ToString()

$pfxScript = @"
Collect(
    'Managed Identities',
    {
        Name: "Contoso Real Estate Plugins $ManagedIdentityId",
        ApplicationId:GUID("$applicationId"),
        TenantId:GUID("$tenantId"),
        'Credential Source':'Credential Source (Managed Identities)'.IsManaged,
        'Subject Scope':'Subject Scope (Managed Identities)'.EnviornmentScope
    }
).'ManagedIdentity Id'
"@
    
Write-Host "Creating a new Managed Identity record in the solution '$ManagedIdentityId'..." -ForegroundColor Green
$tempFilePath = Join-Path -Path $PSScriptRoot -ChildPath "temp-create-managed-identity.pfx"
Set-Content -Path $tempFilePath -Value $pfxScript
pac pfx run --file $tempFilePath --echo
if ($? -eq $false) {
    throw $_.Exception
}




