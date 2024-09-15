param (
    [string]$azureEnv
)

Write-Host "This script adds the stripe keys to the keyvault" -ForegroundColor White
# Import the environment variables
. "$PSScriptRoot\function-get-environment-variables.ps1"
$envVars = GetEnvironmentVariables -azureEnv $azureEnv

# Check AZ CLI is authenticated
Write-Host "Checking Access to Azure CLI..." -ForegroundColor Green

# Check if the user has logged in
$user = az account show --query user.name -o tsv
if ($? -eq $false) {
    Write-Error "You must be logged in to Azure CLI to run this script. Run 'az login' to log in."
    return
}

# Give instructions on how to get the stripe api key
Write-Host @"

To get the Stripe API keys, follow these steps:
1. Go to https://dashboard.stripe.com/test/apikeys
2. Reveal and copy the 'Secret key'
"@
# Get the stripe secret key
$stripeSecret = Read-Host "3. Enter the Stripe Secret Key"


# Get the stripe webhook function key using the Azure CLI
$stripeWebhookFunctionKey = az functionapp function keys list --name $envVars.SERVICE_API_NAME --function-name "StripeWebhook" --resource-group $envVars.AZURE_RESOURCE_GROUP --query default -o tsv

# Give instructions on how to register the webhook and get the webhook secret
Write-Host @"

To register the webhook and get the webhook secret, follow these steps:
1. Go to https://dashboard.stripe.com/test/webhooks
2. Click 'Create new endpoint'
3. Enter the following details:
    - Endpoint URL: https://$($envVars.SERVICE_API_URI)/api/stripe/webhook?code=$stripeWebhookFunctionKey 
    - Events to send: Select Checkout -> Select all Checkout events
4. Click 'Add endpoint'
5. Reveal and copy the 'Signing secret'
"@
# Get the stripe webhook secret
$stripeWebhookSecret = Read-Host "6. Enter the Stripe Webhook Secret"

# Add the stripe keys to the keyvault (StripeApiKey & StripWebhookSecret)
Write-Host "Adding the Stripe API keys to the keyvault..." -ForegroundColor Green
az keyvault secret set --vault-name $envVars.AZURE_KEY_VAULT_NAME --name "StripeApiKey" --value $stripeSecret >> $null
az keyvault secret set --vault-name $envVars.AZURE_KEY_VAULT_NAME --name "StripeWebhookSecret" --value $stripeWebhookSecret >> $null
Write-Host "Complete" -ForegroundColor Green