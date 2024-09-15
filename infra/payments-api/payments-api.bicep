param name string
param location string = resourceGroup().location
param tags object = {}
param storageAccountName string
param keyVaultName string
param appServicePlanName string
param applicationInsightsName string
param managedIdentity bool = !empty(keyVaultName) || storageManagedIdentity
param storageManagedIdentity bool = false
param apiApplicationID string
@secure()
param appClientKeyVaultSecretName string

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' existing = {
  name: appServicePlanName
 
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = if (!(empty(keyVaultName))) {
  name: keyVaultName
}

module api './api-host.bicep' = {
  name: 'payments-api-host'
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': 'payments-api' })
    applicationInsightsName  : applicationInsightsName
    storageAccountName: storageAccountName
    keyVaultName: keyVaultName
    apiApplicationID: apiApplicationID
    // Requires access to the vault
    // See https://learn.microsoft.com/en-us/azure/azure-resource-manager/managed-applications/key-vault-access
    apiAppicationSecret: keyVault.getSecret(appClientKeyVaultSecretName)
    managedIdentity: managedIdentity
    hostingPlanId: hostingPlan.id
    }
  }

module storageOwnerRole '../core/security/role.bicep' = if (storageManagedIdentity) {
  name: 'search-index-contrib-role-api'
  params: {
    principalId: api.outputs.identityPrincipalId
    // Search Index Data Contributor
    roleDefinitionId: '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
    principalType: 'ServicePrincipal'
  }
}

var builtInRoles = loadJsonContent('../built-in-roles.json')

// Give the API access to KeyVault
resource roleIdMapping_roleName_objectId_Microsoft_KeyVault_vaults_keyVault 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  scope: keyVault
  name: guid(builtInRoles.KeyVaultSecretsUser, keyVault.id)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions',builtInRoles.KeyVaultSecretsUser)
    principalId: api.outputs.identityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output SERVICE_API_IDENTITY_PRINCIPAL_ID string = api.outputs.identityPrincipalId
output SERVICE_API_NAME string = name
output SERVICE_API_URI string = api.outputs.defaultHostName
