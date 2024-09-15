# Define the path to the PFX file and the password
$certPath = Join-Path -Path $PSScriptRoot -ChildPath "certificate.pfx"
$password = ConvertTo-SecureString -String "ContosoRealEsate" -Force -AsPlainText

# Import the certificate from the PFX file
$certificate = Import-PfxCertificate -FilePath $certPath -CertStoreLocation Cert:\CurrentUser\My -Password $password 

# Check if the certificate was imported successfully
if ($certificate) {
    # Output the thumbprint of the certificate
    $thumbprint = $certificate.Thumbprint
    Write-Host "Certificate Thumbprint: $thumbprint"
    # Output the thumbprint of the certificate to thumbprint.txt
    $thumbprint | Out-File -FilePath "$PSScriptRoot/thumbprint.txt"
} else {
    Write-Host "Failed to import the certificate."
}