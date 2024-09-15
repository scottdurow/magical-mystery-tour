# First time setup you need to run the following command
# pac solution clone -n <SolutionName> -a -pca -p Both

# Subsequent syncs can be done using the following command
# pac solution sync -f <folder> -a -pca -p Both
$solutionName = "ContosoRealEstatePortal"
$sourceFolder = Join-Path -Path $PSScriptRoot -ChildPath $solutionName
Write-Host "Checking access via Power Platform CLI..." -ForegroundColor Green

# Get the environment name that the user is currentl authenticated for the Power Apps CLI and check that they are happy with this
$environment = pac env who --json | ConvertFrom-Json
$environmentName = $environment.FriendlyName
Write-Host @"
You are currently authenticated to the Power Apps CLI for the environment '${environmentName}'

Do you want to sync '$solutionName' source code in '$sourceFolder' ? (Y/N)
"@ -ForegroundColor Yellow

$confirm = Read-Host 

if ($confirm.ToUpper() -ne 'Y') {
    Write-Host "Exiting"
    return
}

pac solution sync -f "$sourceFolder" -a -p Both

# Build the solution 
dotnet build -c Release "$sourceFolder"

## Update the deployment settings
pac solution create-settings -z "$sourceFolder/bin/$solutionName.zip" -s "$PSScriptRoot/deploymentSettings.json" 

Write-Host "Complete" -ForegroundColor Green