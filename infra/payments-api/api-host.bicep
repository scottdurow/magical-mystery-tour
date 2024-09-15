param name string
param location string = resourceGroup().location
param tags object = {}
param managedIdentity bool
param keyVaultName string
param applicationInsightsName string
param hostingPlanId string
param allowedOrigins array = []
param storageAccountName string
param appSettings object = {}
param storageManagedIdentity bool = false

param apiApplicationID string
@secure()
param apiAppicationSecret string

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' existing = {
  name: storageAccountName
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = if (!(empty(keyVaultName))) {
  name: keyVaultName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = if (!empty(applicationInsightsName)) {
  name: applicationInsightsName
}


resource appService 'Microsoft.Web/sites@2021-03-01' = {
  name: name
  location: location
  tags: tags
  kind: 'functionapp'
  identity: { type: managedIdentity ? 'SystemAssigned' : 'None' }
  properties: {
    serverFarmId: hostingPlanId
    siteConfig: {
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: union([ 'https://portal.azure.com', 'https://ms.portal.azure.com' ], allowedOrigins)
      }
    }
  }
  resource basicPublishingCredentialsPoliciesFtp 'basicPublishingCredentialsPolicies' = {
    name: 'ftp'
    properties: {
      allow: true
    }
  }
  
  resource basicPublishingCredentialsPoliciesScm 'basicPublishingCredentialsPolicies' = {
    name: 'scm'
    properties: {
      allow: false
    }
  }
  }

var storageAccountConnection = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

resource configAppSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'appsettings'
  parent: appService // must be performed sequentially
  properties: union(appSettings,
    {
      name: '${name}-appSettings'
      FUNCTIONS_EXTENSION_VERSION: '~4'
      FUNCTIONS_WORKER_RUNTIME: 'dotnet'
      WEBSITE_RUN_FROM_PACKAGE: '1'
      FUNCTIONS_INPROC_NET8_ENABLED: '1'
      WEBSITE_CONTENTSHARE: appService.name
      WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnection
      MICROSOFT_PROVIDER_AUTHENTICATION_SECRET: apiAppicationSecret
      netFrameworkVersion: 'v4.0'
      minimumElasticInstanceCount: 0
      httpsOnly: true
      AzureWebJobsStorage__accountName: storageManagedIdentity ? storageAccount.name : null
      AzureWebJobsStorage: storageManagedIdentity ? null : storageAccountConnection
      WEBSITE_AUTH_AAD_ALLOWED_TENANTS: tenant().tenantId // This sets the 'Allow requests from specific tenants' setting on the authconfig added below
      
    },
    !empty(applicationInsightsName) ? { APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsights.properties.ConnectionString } : {},
    !empty(keyVaultName) ? { AZURE_KEY_VAULT_ENDPOINT: keyVault.properties.vaultUri } : {}
  )
}

// sites/web/config 'logs'
resource configLogs 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'logs'
  parent: appService
  properties: {
    applicationLogs: { fileSystem: { level: 'Verbose' } }
    detailedErrorMessages: { enabled: true }
    failedRequestsTracing: { enabled: true }
    httpLogs: { fileSystem: { enabled: true, retentionInDays: 1, retentionInMb: 35 } }
  }
  dependsOn: [configAppSettings]
  }

// To allow the azure function to enforce Entra ID authentication
// add an Azure Entra ID authentication provider that uses the application registration
resource authSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  parent: appService
  name: 'authsettingsV2'
  properties: {
    login:{
      tokenStore: null
    }
    globalValidation: {
      requireAuthentication:false
      unauthenticatedClientAction: 'AllowAnonymous'
    }
    platform: {
      enabled: true
    }
    identityProviders: {
      azureActiveDirectory: {
        enabled: true
        registration: {
          clientId: apiApplicationID
          clientSecretSettingName: 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'
          openIdIssuer: 'https://sts.windows.net/${tenant().tenantId}'
        }
        validation: {
          allowedAudiences: [
            'api://${apiApplicationID}'
          ]
        }
      }
    }
  }
}

output identityPrincipalId string = appService.identity.principalId
output defaultHostName string = 'https://${appService.properties.defaultHostName}'
