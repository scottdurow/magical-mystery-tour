# Use this script to set the secrets that are used for integration testing. 
# The script will prompt for the values and store them in the secrets.xml file in the user secrets folder.
function ReadHostWithDefault {
    param (
        [string]$Prompt,
        [string]$Default
    )

    if ($Default) {
        $Prompt = "$Prompt [$Default]"
    }

    $value = Read-Host $Prompt

    if ([string]::IsNullOrWhiteSpace($value)) {
        return $Default
    } else {
        return $value
    }
}


# Define the path to the user secrets folder (Note the GUID is the project user secrets GUID)
$userSecretsPath = [System.IO.Path]::Combine($env:APPDATA, "Microsoft", "UserSecrets", "a6b2ff45-9033-4727-946e-7f4ed05b50a2")

# Ensure the directory exists
if (-not (Test-Path -Path $userSecretsPath)) {
    New-Item -ItemType Directory -Path $userSecretsPath -Force
}

# Define the path to the secrets.xml file
$secretsFilePath = [System.IO.Path]::Combine($userSecretsPath, "secrets.xml")

Write-Host "Setting integration test secrets in $secretsFilePath"

$existingSecrets = @{}
# Read the existing values
# Check the $secretsFilePath exists
# If it does, read the existing secrets and store them in the $existingSecrets hashtable
if (Test-Path -Path $secretsFilePath) {
	$secretsXml = [xml](Get-Content -Path $secretsFilePath)
	$secretsXml.root.secrets.secret | ForEach-Object {
		$existingSecrets[$_.name] = $_.value
	}
}

# Prompt for each of the values
$secrets = @{
    "env_contoso_PaymentsApiTenantId" = ReadHostWithDefault -Prompt "Enter the tenant id for the payments client" -Default $existingSecrets["env_contoso_PaymentsApiTenantId"]
    "env_contoso_PaymentsApiAppId" = ReadHostWithDefault -Prompt "Enter the appid for the payments client" -Default $existingSecrets["env_contoso_PaymentsApiAppId"]
    "env_contoso_PaymentsApiSecret" = ReadHostWithDefault -Prompt "Enter the secret for the payments client" -Default $existingSecrets["env_contoso_PaymentsApiSecret"]
    "env_contoso_PaymentsApiResourceUrl" = ReadHostWithDefault -Prompt "Enter the resource url for the payments client" -Default $existingSecrets["env_contoso_PaymentsApiResourceUrl"]
    "env_contoso_PaymentsApiScope" = ReadHostWithDefault -Prompt "Enter the scope for the payments client" -Default $existingSecrets["env_contoso_PaymentsApiScope"]
    "env_contoso_PaymentsApiHost" = ReadHostWithDefault -Prompt "Enter the host for the payments API" -Default $existingSecrets["env_contoso_PaymentsApiHost"]
    "env_contoso_PaymentsApiBaseUrl" = ReadHostWithDefault -Prompt "Enter the base URL for the payments API" -Default $existingSecrets["env_contoso_PaymentsApiBaseUrl"]
    "IntegrationTestConnectionString" = ReadHostWithDefault -Prompt "Enter the integration testing Dataverse connection string" -Default $existingSecrets["IntegrationTestConnectionString"]
}

# Define the content of the secrets.xml file
$secretsContent = @"
<?xml version="1.0" encoding="utf-8" ?>
<root>
  <secrets ver="1.0">
    <secret name="env_contoso_PaymentsApiTenantId" value="$($secrets["env_contoso_PaymentsApiTenantId"])" />
    <secret name="env_contoso_PaymentsApiAppId" value="$($secrets["env_contoso_PaymentsApiAppId"])" />
    <secret name="env_contoso_PaymentsApiSecret" value="$($secrets["env_contoso_PaymentsApiSecret"])" />
    <secret name="env_contoso_PaymentsApiResourceUrl" value="$($secrets["env_contoso_PaymentsApiResourceUrl"])" />
    <secret name="env_contoso_PaymentsApiScope" value="$($secrets["env_contoso_PaymentsApiScope"])" />
    <secret name="env_contoso_PaymentsApiHost" value="$($secrets["env_contoso_PaymentsApiHost"])" />
    <secret name="env_contoso_PaymentsApiBaseUrl" value="$($secrets["env_contoso_PaymentsApiBaseUrl"])" />
    <secret name="env_EnvironmentWebApiEndpoint" value="" />
    <!-- This connection is only used to test operations that access resources in dataverse e.g. when testing the environment variables service -->
    <secret name="IntegrationTestConnectionString" value="$($secrets["IntegrationTestConnectionString"])"/>
  </secrets>
</root>
"@

# Write the content to the secrets.xml file
Set-Content -Path $secretsFilePath -Value $secretsContent -Force
