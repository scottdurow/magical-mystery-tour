# Locate the SignTool.exe without using Get-Command
$signToolPath = (Get-ChildItem -Path "C:\Program Files (x86)\Windows Kits" -Recurse -Filter "signtool.exe" -ErrorAction SilentlyContinue -Force | Where-Object { $_.FullName -like "*x86\signtool.exe" })

if ($signToolPath) {
    $signToolPath = $signToolPath[0].FullName
}
else {
    Write-Error "SignTool.exe not found"
    exit 1
}

$certPath = Join-Path -Path $PSScriptRoot -ChildPath "certificate.pfx"

$pluginPath = Join-Path -Path $PSScriptRoot -ChildPath  "../PaymentVirtualTableProvider/bin/PaymentVirtualTableProvider.dll" 

Write-Host "Signing the plugin at $pluginPath with the certificate at $certPath" -ForegroundColor Green
& $signToolPath sign /fd SHA256 /f $certPath /p "ContosoRealEsate" "$pluginPath"
