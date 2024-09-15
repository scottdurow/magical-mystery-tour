# Some environment specific variables cannot be updated using Power Platform Environment Variables
# This script injects those variables into the Solution Zip file
param (
    [string]$solutionFilePath,
    [string]$pluginManagedIdentityAppId,
    [string]$tenantId,
    [string]$azureEnv,
    # This is used to change the managed identity association on a specific environment
    # Works around a bug in Managed Identity support, where you cannot update a existing plugin with a managed identity unless it's unmanaged
    [string]$overridePluginManagedIdentityId 
)

# ---------------------------------------------------------------------------------------------
# Function to patch an XML file
function PatchXmlFile {
    param (
        [string]$FilePath,
        [string]$XPath,
        [string]$NewValue
    )

    try {
        Write-Host "Patching $FilePath with $XPath = $NewValue" -ForegroundColor Green
        
        $xmlContent = Get-Content -Path $FilePath
        Write-Host "File loaded: $xmlContent"
        $xml = [xml]$xmlContent

        $node = $xml.SelectSingleNode($XPath)
        # Check the node is found
        if (-not $node) {
            Write-Host "The XPath '$XPath' was not found in the XML file. Exiting..." -ForegroundColor Red
            exit 1
        }
        Write-Host "Existing value = $($node.InnerText)"
        $node.InnerText = $NewValue

        # Save the updated XML content
        $xml.Save($FilePath)
    }
    catch {
        Write-Host "An error occurred while patching the XML file. Exiting..." -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

# ---------------------------------------------------------------------------------------------
# Function to patch a JSON file
function PatchJsonFile {
    param (
        [string]$FilePath,
        [string]$JsonPath,
        [string]$NewValue
    )

    try {
        Write-Host "Patching $FilePath with $JsonPath = $NewValue" -ForegroundColor Green
        
        $JsonContent = Get-Content -Path $FilePath -Raw

        Write-host  $JsonContent
        $JsonObject = $JsonContent | ConvertFrom-Json

        $Path = "`$JsonObject.$JsonPath"
        Invoke-Expression "$Path = '$NewValue'"

        # Convert the updated object back to JSON
        $updatedJsonContent = $JsonObject | ConvertTo-Json -Compress -Depth 20
        Write-Host $updatedJsonContent
        Set-Content -Path $FilePath -Value $UpdatedJsonContent
    }
    catch {
        Write-Host "An error occurred while patching the JSON file. Exiting..." -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

# ---------------------------------------------------------------------------------------------
# Function to unzip the solution
function UnzipSolution {
    param (
        [string]$solutionFilePath
    )

    try {
        # Get a temporary folder
        # Create a temp unique ID
        $guid = [guid]::NewGuid().ToString()
        $tempFolder = [System.IO.Path]::GetTempPath()
        $tempFolder = Join-Path -Path $tempFolder -ChildPath "ContosoRealEstateCore_$guid"
        if (-not (Test-Path -Path $tempFolder)) {
            New-Item -ItemType Directory -Path $tempFolder > $null
        }

        # Unzip the core solution
        Write-Host "Unzipping the solution '$solutionFilePath' to '$tempFolder'" -ForegroundColor Green
        Expand-Archive -Path $solutionFilePath -DestinationPath $tempFolder
    }
    catch {
        Write-Host "An error occurred while unzipping the solution. Exiting..." -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }

    return $tempFolder
}

# ---------------------------------------------------------------------------------------------
# Function to rezip the solution
function RezipSolution {
    param (
        [string]$tempFolder,
        [string]$solutionFilePath
    )

    try {
        Write-Host "Rezipping the solution to $solutionFilePath" -ForegroundColor Green
        Compress-Archive -Path $tempFolder/* -DestinationPath $solutionFilePath -Force
    }
    catch {
        Write-Host "An error occurred while rezipping the solution. Exiting..." -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

# ---------------------------------------------------------------------------------------------
# If the $apiAppId is not specified, load from a local .azure deployment
if (-not $pluginManagedIdentityAppId) {
    . "$PSScriptRoot\function-get-environment-variables.ps1" 
    $envVars = GetEnvironmentVariables $azureEnv
    $pluginManagedIdentityAppId = $envVars.ENTRA_API_CLIENT_APP_ID
    $tenantId = $envVars.AZURE_TENANT_ID
}

# If the solutionPath and solutionFileName is not specified, use the default that is in the core\solution\bin folder
if (-not $solutionFilePath) {
    $solutionFilePath = "$PSScriptRoot\..\ContosoRealEstateCore\bin\ContosoRealEstateCore_managed.zip"
}
 # ---------------------------------------------------------------------------------------------
switch -wildcard ($solutionFilePath) {
    "*ContosoRealEstateCore*" {
        # WORKAROUND: Patch the connector resource URL
        # PatchJsonFile -FilePath "$tempFolder\Connector\contoso_contoso-20payments-20api_connectionparameters.json" -JsonPath 'token.oAuthSettings.customParameters.ResourceUri.value' -NewValue  "api://$API_APP_ID"  
        # PatchJsonFile -FilePath "$tempFolder\Connector\contoso_contoso-20stripe-20api_connectionparameters.json" -JsonPath 'token.oAuthSettings.customParameters.ResourceUri.value' -NewValue  "api://$API_APP_ID"

        # WORKAROUND: Until Plugin Managed Identities support deploymentSettings, inject the application and tenant ID
        # Update each of the  ImportExportXml/managedidentities/managedidentity elements, with the child applicationid values to be $envVars.
        # Load customizations.xml as XML
        $tempFolder = UnzipSolution -solutionFilePath $solutionFilePath
        $customizationsXmlPath = "$tempFolder\customizations.xml"
        $customizationsXml = [xml](Get-Content -Path $customizationsXmlPath)
        $managedIdentityElements = $customizationsXml.ImportExportXml.managedidentities.managedidentity
        foreach ($managedIdentityElement in $managedIdentityElements) {
            Write-Host "Updating managed identity '$($managedIdentityElement.name)' element with applicationid = $pluginManagedIdentityAppId and tenantid = $tenantId" -ForegroundColor Green
            $managedIdentityElement.applicationid = $pluginManagedIdentityAppId
            $managedIdentityElement.tenantid = $tenantId
        }

        # The following works around MANAGED_IDENTITY_IMPORT_BUG
        # If overridePluginManagedIdentityId is not empty, find the element ImportExportXml.SolutionPluginAssemblies.PluginAssembly.ManagedIdentityId and update it
        if ($overridePluginManagedIdentityId) {
            $pluginAssemblyElements = $customizationsXml.ImportExportXml.SolutionPluginAssemblies.PluginAssembly
            foreach ($pluginAssemblyElement in $pluginAssemblyElements) {
                # Only update if there is a manged identity id $pluginAssemblyElement.ManagedIdentityId already
                if (-not $pluginAssemblyElement.ManagedIdentityId) {
                    continue
                }
                Write-Host "Updating plugin assembly '$($pluginAssemblyElement.FullName)' element with managedidentityid = $overridePluginManagedIdentityId" -ForegroundColor Green
                $pluginAssemblyElement.ManagedIdentityId = $overridePluginManagedIdentityId
            }
        }

        # Save the updated customizations.xml
        $customizationsXml.Save($customizationsXmlPath)
        # Rezip the solution
        RezipSolution -tempFolder $tempFolder -solutionFilePath $solutionFilePath
    }

# ---------------------------------------------------------------------------------------------
    "*ContosoRealEstatePortal*" {
        $tempFolder = UnzipSolution -solutionFilePath $solutionFilePath
        # WORKAROUND: Patch the portal settings that cannot be updated during the import using deploymentSettings
        PatchXmlFile `
            -FilePath "$tempFolder\powerpagecomponents\87e7f453-6b29-469f-b019-e088b368bbe9\powerpagecomponent.xml" `
            -XPath '//powerpagecomponent/content' `
            -NewValue  @'
        {
          "value": "false"
        }
'@
         # Rezip the solution
         RezipSolution -tempFolder $tempFolder -solutionFilePath $solutionFilePath
    }
    default {
        Write-Host "The solution $solutionFilePath does not need any updates" -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------------------------
Write-Host "Complete" -ForegroundColor Green