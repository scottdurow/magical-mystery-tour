# Use Powershell to create a self signed certificate for testing purposes.
Import-Module PKI

# Create a secure string for the password
$password = ConvertTo-SecureString -String "ContosoRealEsate" -Force -AsPlainText

# Create a self-signed certificate
$certificate = New-SelfSignedCertificate -Subject "CN=contoso.com, O=Contoso, C=US" -DnsName "contoso.com" -Type CodeSigning -KeyUsage DigitalSignature -CertStoreLocation Cert:\CurrentUser\My -FriendlyName "Contoso Real Estate Self Signed Cert"

# Export the certificate to PFX
Export-PfxCertificate -Cert $certificate -FilePath "$PSScriptRoot/dev-certificate.pfx" -Password $password
