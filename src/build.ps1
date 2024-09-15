# Builds the Power Platform solutions
$childPaths = @(
    '/controls/solution/ContosoRealEstateCustomControls/ContosoRealEstateCustomControls.cdsproj',
    '/core/solution/ContosoRealEstateCore/ContosoRealEstateCore.cdsproj',
    '/portal/solution/ContosoRealEstatePortal/ContosoRealEstatePortal.cdsproj'
)

foreach ($childPath in $childPaths) {
    $sourceFolder = Join-Path -Path $PSScriptRoot -ChildPath $childPath

     # Build the project in Release configuration
    dotnet build $sourceFolder -c Release
}