param (
    [bool]$skipLoginChecks
)
. "$PSScriptRoot\function-get-environment-variables.ps1"
# Check the user is logged into AZ CLI and PAC
if (-not $skipLoginChecks) {
    CheckPACCLI
    if (-not (ConfirmPrompt -message "Do you want to update environment settings on '${environmentName}'?")) {
        Write-Host "Exiting" -ForegroundColor Yellow
        exit
    }
}

$environment = pac env who --json | ConvertFrom-Json
$environmentName = $environment.FriendlyName


# Read the $PSScriptRoot/../environment-settings.json file
$environmentSettingsPath = Join-Path -Path $PSScriptRoot -ChildPath "/../environment-settings.json"
$environmentSettings = Get-Content -Path $environmentSettingsPath | ConvertFrom-Json

# Loop over each property, and call update-settings with the value
foreach ($setting in $environmentSettings.PSObject.Properties) {
    Write-Host "Updating setting '$($setting.Name)' to '$($setting.Value)'" -ForegroundColor Green
    pac env update-settings --name $setting.Name --value $setting.Value
}


