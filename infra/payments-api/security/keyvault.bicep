metadata description = 'Creates an Azure Key Vault.'
param name string
param location string = resourceGroup().location
param tags object = {}

@description('Allow the key vault to be used during resource creation.')
param enabledForDeployment bool = false
@description('Allow the key vault to be used for template deployment.')
param enabledForTemplateDeployment bool = false

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: { family: 'A', name: 'standard' }
    enabledForDeployment: enabledForDeployment
    enabledForTemplateDeployment: enabledForTemplateDeployment
    // RBAC Authorization requires that you have Key Vault Administrator and User Access Administrator roles on the subscription
    // Important for using keyvault with managed identity and environment variables in Power Platform
    enableRbacAuthorization: true 
    
  }
}

output endpoint string = keyVault.properties.vaultUri
output id string = keyVault.id
output name string = keyVault.name
