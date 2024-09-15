@description('The unique name of the application.')
param applicationUniqueName string

@description('The name of the SQL logical server.')
param serverName string

@description('The name of the SQL Database.')
param sqlDBName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The administrator username of the SQL logical server.')
param administratorLogin string = 'sqlAdmin'

@description('The managed identity name of the api.')
param appManagedIdentityName string

@description('The administrator password of the SQL logical server.')
@secure()
param administratorLoginPassword string
param connectionStringKey string = 'AZURE-SQL-CONNECTION-STRING-${applicationUniqueName}'
param principalLoginName string
param principalId string
param keyVaultName string

// Note: Ideally, SQL Server would be created passwordless using Azure Active Directory authentication only,
// with a managed identity as the administrator. However, this is not supported in Bicep 
// becuase the managed identity needs to be created before the SQL Server is created and given
// Directory Readers role, requiring Global Admin privileges.
// See https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-service-principal-tutorial?view=azuresql
// When running the sql script, the sql admin account is used, which can't be used to create the managed identity user.
// Therefore, there is a post deployment step to do this
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    administrators: {
        administratorType: 'ActiveDirectory'
        azureADOnlyAuthentication: false
        login: principalLoginName
        sid: principalId
    }
  }

  resource firewall 'firewallRules' = {
    name: 'Azure Services'
    properties: {
      // Allow all clients
      // Note: range [0.0.0.0-0.0.0.0] means "allow all Azure-hosted clients only".
      // This is not sufficient, because we also want to allow direct access from developer machine, for debugging purposes.
      startIpAddress: '0.0.0.1'
      endIpAddress: '255.255.255.254'
    }
  }
}


resource database 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: sqlDBName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

resource sqlAdminLoginName 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: 'sqlAdminLogin'
  properties: {
    value: administratorLogin
  }
}

resource sqlAdminPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: 'sqlAdminPassword'
  properties: {
    value: administratorLoginPassword
  }
}

resource sqlAzureConnectionStringSercret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: connectionStringKey
  properties: {
    value: '${connectionString};'
  }
}


resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

var connectionString = 'Server=${sqlServer.properties.fullyQualifiedDomainName}; Database=${database.name};'


resource sqlDeploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${sqlDBName}-deployment-script'
  location: location
  kind: 'AzureCLI'
  dependsOn:[
    database
  ]
  properties: {
    azCliVersion: '2.37.0'
    retentionInterval: 'PT1H' // Retain the script resource for 1 hour after it ends running
    timeout: 'PT5M' // Five minutes
    cleanupPreference: 'OnSuccess'
    environmentVariables: [
      {
        name: 'APPMANAGEDIDENTITY'
        value: appManagedIdentityName
      }
      {
        name: 'DBNAME'
        value: sqlDBName
      }
      {
        name: 'DBSERVER'
        value: sqlServer.properties.fullyQualifiedDomainName
      }
      {
        name: 'SQLCMDPASSWORD'
        secureValue: administratorLoginPassword
      }
      {
        name: 'SQLADMIN'
        value: administratorLogin
      }
    ]

    scriptContent: '''
# wget https://github.com/microsoft/go-sqlcmd/releases/download/v0.8.1/sqlcmd-v0.8.1-linux-x64.tar.bz2

# Retry logic for wget
retry_count=0
max_retries=5
url="https://github.com/microsoft/go-sqlcmd/releases/download/v0.8.1/sqlcmd-v0.8.1-linux-x64.tar.bz2"

while [ $retry_count -lt $max_retries ]; do
    wget $url -O sqlcmd-v0.8.1-linux-x64.tar.bz2 && break
    retry_count=$((retry_count+1))
    echo "Retry $retry_count/$max_retries..."
    sleep 10
done

if [ $retry_count -eq $max_retries ]; then
    echo "Failed to download file after $max_retries attempts."
    exit 1
fi
tar x -f sqlcmd-v0.8.1-linux-x64.tar.bz2 -C .

cat <<SCRIPT_END > ./initDb.sql
DROP USER IF EXISTS [${APPMANAGEDIDENTITY}]
GO
-- Create a user in the database for the managed identity
CREATE USER [${APPMANAGEDIDENTITY}] FROM EXTERNAL PROVIDER;
GO

-- The object ID of the managed identity
EXEC sp_addrolemember 'db_datareader', '${APPMANAGEDIDENTITY}';
GO
EXEC sp_addrolemember 'db_datawriter', '${APPMANAGEDIDENTITY}';
GO

-- The payments table (Drop if it exists)
-- This ideally would be done using EF migrations in a real-world scenario
DROP TABLE IF EXISTS [dbo].[payment];
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[payment](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [nvarchar](255) NOT NULL,
	[reservationId] [nvarchar](255) NOT NULL,
	[provider] [int] NOT NULL,
	[status] [int] NOT NULL,
	[amount] [decimal](18, 0) NOT NULL,
	[currency] [nvarchar](255) NOT NULL,
	[createdAt] [datetime] NOT NULL,
 CONSTRAINT [PK_payment] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SCRIPT_END

./sqlcmd -S ${DBSERVER} -d ${DBNAME} -U ${SQLADMIN} -i ./initDb.sql
    '''
  }
}
