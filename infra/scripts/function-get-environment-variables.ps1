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
        Write-Host "Reading from '.env' at '${envFile}'. Remove this to use the .env file in the .azure folder"
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
                Write-Host "Found $targetFolderName in $currentDirectory" -ForegroundColor Green
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
            Write-Host "The .azure folder does not exist. Run azd up first"
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
            Write-Host "Invalid selection"
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
