# Currently there is a bug where you cannot import an update to a managed identity plugin
# So we set to an unmanaged identity that is created manually
# MANAGED_IDENTITY_IMPORT_BUG
param (
    [string]$solutionFilePath,
    [string]$unmanagedManagedIdentityId
)

# If solutionFilePath is not set, then prompt for it
if (-not $solutionFilePath) {
    $solutionFilePath = Read-Host "Enter the path to the solution file"
}

# If unmanagedManagedIdentityId is not set, then prompt for it
if (-not $unmanagedManagedIdentityId) {
    $unmanagedManagedIdentityId = Read-Host "Enter the unmanaged managed identity id"
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
function Get-FileSystemEncoding {
    param (
        [System.Text.Encoding]$encoding
    )

    switch ($encoding.WebName) {
        'utf-8' { return 'UTF8' }
        'utf-16' { return 'Unicode' }
        'utf-16BE' { return 'BigEndianUnicode' }
        'us-ascii' { return 'ASCII' }
        'iso-8859-1' { return 'Latin1' }
        'utf-32' { return 'UTF32' }
        default { return 'Default' }
    }
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

        # For some reason, the Compress-Archive does not create a zip that works during import, so we are using 7zip to rezip the solution
        # Compress-Archive -Path $tempFolder/* -DestinationPath $solutionFilePath -Force -CompressionLevel Fastest

        $sevenZipPath = "C:\Program Files\7-Zip\7z.exe"
        # Check if 7zip exists
        if (-not (Test-Path $sevenZipPath)) {
            Write-Host "7zip is not installed at $sevenZipPath. Exiting..." -ForegroundColor Red
            exit 1
        }
        if (Test-Path $solutionFilePath) {
            Remove-Item $solutionFilePath -Force
        }
        & "$sevenZipPath" a -tzip "$solutionFilePath" "$tempFolder\*" -mx1
    }
    catch {
        Write-Host "An error occurred while rezipping the solution. Exiting..." -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

# If the solutionPath and solutionFileName is not specified, use the default that is in the core\solution\bin folder
if (-not $solutionFilePath) {
    $solutionFilePath = "$PSScriptRoot\..\ContosoRealEstateCore\bin\ContosoRealEstateCore_managed.zip"
}

$existingManagedIdentityId = "<ManagedIdentityId>fcdb7ca6-8d5e-ef11-bfe2-002248083aae</ManagedIdentityId>"

$tempFolder = UnzipSolution -solutionFilePath $solutionFilePath
$customizationsXmlPath = "$tempFolder\customizations.xml"
$customizationsContent = Get-Content -Path $customizationsXmlPath
# Update the managed identity id in the customizations.xml
$customizationsContent = $customizationsContent -replace $existingManagedIdentityId, "<ManagedIdentityId>$unmanagedManagedIdentityId</ManagedIdentityId>"

# Read the original file's encoding
$originalEncoding = [System.Text.Encoding]::Default
if (Test-Path $customizationsXmlPath) {
    $fileStream = [System.IO.File]::OpenRead($customizationsXmlPath)
    $streamReader = New-Object System.IO.StreamReader($fileStream, $originalEncoding, $true)
    $streamReader.ReadLine() | Out-Null  # Read the first line to detect encoding
    $originalEncoding = $streamReader.CurrentEncoding
    $streamReader.Close()
    $fileStream.Close()
}

$fileSystemEncoding = Get-FileSystemEncoding -encoding $originalEncoding
Write-Host $fileSystemEncoding
# Save the updated customizations.xml
Set-Content -Path $customizationsXmlPath -Value $customizationsContent -Encoding $fileSystemEncoding

# Add '_fixed' onto the end of the #solutionfilePath zip file
$solutionFilePath = $solutionFilePath -replace ".zip", "_fixed.zip"

# Rezip the solution
RezipSolution -tempFolder $tempFolder -solutionFilePath $solutionFilePath
    