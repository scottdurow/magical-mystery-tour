function GetEnvironmentVariables {
    param (
        [string]$azureEnv
    )
    $outputVariables = $false
    $scriptDirectory = $PSScriptRoot
    # Check if there is an .env file next to this script
    $envFile = Join-Path -Path $scriptDirectory -ChildPath '.env'
    if (Test-Path -Path $envFile) {
        # .env file is next to script so use that
        if ($azureEnv -eq "") { Write-Host "Reading from '.env' at '${envFile}'. Remove this to use the .env file in the .azure folder" }
        $envFolder = $envFile
    }
    else
    {
        # Walk back up the directory until we find the .azure folder or run out of directories
        # Define the target folder name
        $targetFolderName = ".azure"

        # Start from the current directory
        $currentDirectory = $scriptDirectory

        # Loop until we find the target folder or run out of directories
        while ($currentDirectory -ne [System.IO.Directory]::GetDirectoryRoot($currentDirectory)) {
            # Check if the target folder exists in the current directory
            if (Test-Path -Path (Join-Path -Path $currentDirectory -ChildPath $targetFolderName)) {
                break
            }

            # Move up one directory
            $currentDirectory = Get-Item -Path (Join-Path -Path $currentDirectory -ChildPath "..")
        }

        # If we reached the root directory without finding the target folder
        if ($currentDirectory -eq [System.IO.Directory]::GetDirectoryRoot($currentDirectory)) {
            Write-Host "$targetFolderName not found in any parent directories." -ForegroundColor Red
            exit
        }

        $azureFolderPath = Join-Path -Path $currentDirectory -ChildPath ".azure" 

        # Check the .azure folder exists
        if (-not (Test-Path -Path $azureFolderPath)) {
            Write-Host "The .azure folder does not exist. Run azd up first" -ForegroundColor Red
            exit
        }
    
        $folders = Get-ChildItem -Path $azureFolderPath -Directory
        if ($azureEnv -eq "") {
            $folders | ForEach-Object {
                Write-Host "[$($_.Name)]"
            }
            $selectedFolder = Read-Host "Enter the azure environment configuration"
            # Strip square brakets if they are entered
            $selectedFolder = $selectedFolder -replace "\[|\]",""
        }
        else{
            $selectedFolder = $azureEnv
        }
        $selectedFolder = $folders | Where-Object { $_.Name -eq $selectedFolder }
        if ($null -eq $selectedFolder) {
            Write-Host "Invalid .azure environment '$selectedFolder'" -ForegroundColor Red
            exit
        }
    
        $ENVIRONMENT = $selectedFolder.Name
    
        Write-Host "Reading from '$ENVIRONMENT/.env'"
        $envFolder = Join-Path -Path $azureFolderPath -ChildPath "$ENVIRONMENT/.env"
    
        # Check the file exists
        if (-not (Test-Path -Path $envFolder)) {
            Write-Host "The file '$ENVIRONMENT/.env' does not exist"
            exit
        }
    }

    # Read the contents of the .env file
    $envFile = Get-Content -Path $envFolder

    # Read all the name value pairs from the file where they are in the format NAME="Value"
    $envVars = New-Object PSObject
    $envFile | ForEach-Object {
        if ($_ -match '^(.*)="(.*)"$') {
            $matches[1] = $matches[1].ToUpper()
            $envVars | Add-Member -MemberType NoteProperty -Name $matches[1] -Value $matches[2]
            # if the outputVariables flag is set then output the name value pairs
            if ($outputVariables) {
                Write-Host " $($matches[1]) = '$($matches[2])'" -ForegroundColor Gray
            }
        }
    }

    return $envVars
}

function ConfirmPrompt {
    param (
        [string]$message
    )

    Write-Host @"
$message (Y/N)
"@ -ForegroundColor Yellow

    $confirm = Read-Host 

    if ($confirm.ToUpper() -ne 'Y') {
        return $false
    }

    return $true
}

function CheckAZCLI {
    # Check if the user is logged into AZ CLI
    Write-Progress -Activity "Checking access via Azure CLI..."
    try {
        $accountInfo = az account show 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "You are not logged into Azure CLI. Please run 'az login' to log in." -ForegroundColor Red
            exit 1
        }
        $azureAccount = $accountInfo | ConvertFrom-Json
        # report the current user and subscription
        Write-Host "You are logged in to Azure as '$($azureAccount.user.name)' for the subscription '$($azureAccount.user.name)'" -ForegroundColor Cyan
    } catch {
        Write-Host "An error occurred while checking Azure CLI login status." -ForegroundColor Red
        exit 1
    }
    Write-Progress -Activity "Checking access via Azure CLI..." -Completed
}

function CheckPACCLI {
    Write-Progress -Activity "Checking access via Power Platform CLI..."
    try {
    # Get the environment name that the user is currently authenticated for the Power Apps CLI and check that they are happy with this
    $environment = pac env who --json | ConvertFrom-Json
    $environmentName = $environment.FriendlyName
    $pacUserName = $environment.UserEmail

    Write-Host "You are currently authenticated to the Power Platform CLI as '$pacUserName' for the environment '$environmentName'" -ForegroundColor Cyan
    } catch {
        Write-Host "An error occurred while checking Power Platform CLI login status." -ForegroundColor Red
        exit 1
    }
    Write-Progress -Activity "Checking access via Power Platform CLI..." -Completed

}