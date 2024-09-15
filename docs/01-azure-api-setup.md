# 📦Contoso Real Estate Azure Resources Setup

The Contoso Real Estate (Power Platform Edition) project utilizes several Azure components to enable its functionality. These components include:

1. **Payments API Azure Functions** - This component is used to host the payments api that is used by:
   - **Power Platform Custom Connector** to create a Stripe Checkout session that is called by the portal using a Power Automate Flow. This is used for listing reservations.
   - **Power Platform Custom Connector** to list Payments
   - **Dataverse Virtual Table Provider** C# Plugin to expose the Payments as a Dataverse Virtual Table
   - **Webhook** that is called by Stripe when payments events are raised
2. **Azure SQL Database** - Stores the payments made via Stripe
3. **Azure Key Vault** - Used to store all secrets that are necessary and is configured as the backing for Power Platform Environment Variable secrets.
    - `AZURE-SQL-CONNECTION-STRING-payments-api` - SQL connection string for the Payments database. This does not contain any login username or password because the Azure Function uses a Managed Identity to connect to the database. E.g. `Server=tcp:sql-***.database.windows.net,1433;Initial Catalog=contoso-real-estate;`. [Learn about Managed Identities in Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/security-concepts)
    - `cre-..-development-payments-api-client-secret`- Custom Connector Client Secret that is referenced by the Power Platform Environment Variable Secret and used in the custom connectors.
    - `StripeApiKey` - The Stripe API key to use for payment processing
    - `StripeWebhookSecret` - The Stripe webhook secret to use for verifying webhook events
4. **Entra ID Application registrations** used to authenticate Power Platform against the Payments API

> [!NOTE]
> For a full end-to-end set of instructions on how to install prerequisites, clone, build, deploy, and test the solutions, refer to [full-development-setup-instructions.md](./docs/00-full-development-setup-instructions.md).

## 🦾Deploying Azure Resources

To deploy the Azure resources required for the Contoso Real Estate (Power Platform Edition) project, you can use the `azd up` command. This command is part of the Azure DevOps CLI extension and allows you to define and manage your infrastructure as code.

Here are the steps to deploy the Azure resources using `azd up`:

1. Install the Azure CLI by following the instructions provided in the [official Azure CLI documentation for your operating system](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli).

1. Once the Azure CLI is installed, open a PowerShell terminal window inside VSCode at the root of the cloned repo.

1. Log in to your Azure account by running the following command and following the prompts:

    ```powershell
    az login
    ```

    This will open a browser window where you can authenticate with your Azure account.

1. After successfully logging in, select the subscription you would like to deploy into:

    ```powershell
    az account list --output table
    az account set --subscription <Subscription Id you wish to use>
    ```

1. You must be a member of the **User Access Administrator** and **Key Vault Administrator** on your subscription to manage the access control on a key vault.

    ```powershell
    # Set the subscription and user principal name
    $subscriptionId = az account show --query id --output tsv
    $currentUPN = az ad signed-in-user show --query userPrincipalName
    
    # Find the roles of the current user for the currently selected subscription
    az role assignment list --query "[?principalName=='$currentUPN' && scope=='/subscriptions/$subscriptionId'].roleDefinitionName" --output table
    ```

    If you do not see the two roles `Key Vault Administrator` and `User Access Administrator`, then use the following:

    ```powershell
    # Assign the "Key Vault Administrator" and "User Access Administrator" role to the user
    az role assignment create --role "Key Vault Administrator" --assignee $currentUPN --scope /subscriptions/$subscriptionId
    az role assignment create --role "User Access Administrator" --assignee $currentUPN --scope /subscriptions/$subscriptionId
    az role assignment create --role "Key Vault Secrets User" --assignee $currentUPN --scope /subscriptions/$subscriptionId
    ```

1. The Power Platform Resource Provider must be registered in your Azure subscription so that Dataverse can access Azure Key Vaults in that subscription. At the VSCode terminal, use:

    ```powershell
    az provider show -n Microsoft.PowerPlatform
    ```

    Registration will take a few minutes.

1. Install the Azure DevOps CLI extension by running the following command:

    ```powershell
    infra\azd\install-azd.ps1
    ```

> [!NOTE]
> Check you have the latest version of Bicep (>= 0.29.47) installed using `az bicep version`. Bicep is located in your azd bin folder `%USERPROFILE%\.azd\bin`

8. Install bicep using:

    ```powershell
    az bicep install
    ```

1. Login to azd using:

    ```powershell
    azd auth login
    ```

    Select the same account that you authenticated with `az login` using.

1. Run the `azd up` command to start the deployment process:

    ```powershell
    az extension add --name graphservices
    azd up --environment development
    ```

    The `environment` parameter would normally be the Power Platform environment that is being deployed to e.g. `elaiza-dev1` `UAT` `PROD`

1. When asked if you want to create the Environment, press 'Y'.

1. Select the Subscription to use when prompted.

1. Select the location to use - e.g. Central US

> [!IMPORTANT]
> When selecting a Azure location to deploy into, avoid using WEST US 2 due to there often being resource capacity limitations.

14. When prompted for the 'principalLoginName' infrastructure parameter, enter your Power Platform username e.g. `some.user@myenvironment.omicrosoft.com`. This will be stored in a file called `.azure/<environmentname>/config.json`. It is important to ensure that this user name is the one you have authenticated against Azure with. It will be used to set the administrator of the SQL Server.

> [!NOTE]
> If you want to deploy updates to the code later on after the infrastructure has been published you can use `azd deploy`

15. Monitor the deployment progress and check for any errors or warnings. The `azd up` command will provide detailed logs and feedback during the deployment process.

> [!NOTE]
> You may need to run `azd up` a second time if there are errors the first time (due to azure replication)

## 📜Post deployment steps

Some settings cannot be performed by Bicep/ARM scripts (or are complex and beyond the scope of this sample). To complete the deployment the following tasks must be carried out:

- Grant the Azure Function Payment API managed identity access to SQL Server. The Azure Functions run under a System Assigned Managed Identity (SAMI). This identity must be added to the SQL Server as an external user. This cannot be performed by a SQL Login (inside the bicep script), because it needs access to Entra ID.
- Add admin consent to the Payment API client Entra ID Application registration, so that it can be used as an SPN connection in Power Platform
- Grant your user access to the Payment API so that it can be used as an OAuth user connection in Power Platform for testing.

1. You will need a stripe account for the payment API to function. 
1. Navigate to [Sign Up and Create a Stripe Account | Stripe](https://dashboard.stripe.com/register)
1. Enter you Email, Name, Country, and Password. 
1. Select **Create account**.
1. Validate your email address by following the instructions.
1. Navigate to https://dashboard.stripe.com/test/dashboard

7. Run the following and follow the instructions carefully:

```powershell
./infra/scripts/post-deployment-setup.ps1
```

## 🗑️Deleting deployment

It is important to note that deploying these resources to your Azure subscription will have a consumption cost. 
Once you have finished with the deployment you can remove the deployment by using:

```powershell
./infra/scripts/delete-entraid-applications-before-azd-down.ps1
azd down

```

**NOTE:** When prompted 'Would you like to permanently delete these resources instead, allowing their names to be reused?' select **YES**

# Appendix

## Manual setup of Azure Function Managed Identity Access
The [Azure function managed identity](https://learn.microsoft.com/en-us/azure/azure-functions/security-concepts) is used to give access to the Key Vault and to the database.

To create a managed identity for the Azure Function:

1. Open the Azure Function in the Azure Portal
1. Select the `Identity` menu item
1. Set the `Status` to `On`
1. Select `Save`
1. Select `Yes` to enable the system assigned identity
1. Copy the `Object ID` of the managed identity

### Give the managed identity access to the key vault
1. Inside the Azure Function Identity page:
1. Select 'Add role assignment'
1. Select 'Add role assignment'
1. Select the Scope `Key Vault'
1. Select the Key Vault that contains the secrets
1. Select the role `Key Vault Secrets User`
1. Select `Save`

### Give the managed identity access to the database
1. Open the Azure SQL Database in Management Studio
1. Run the following SQL command to create a user for the managed identity:

```sql
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'AzureFunctionAppName')
BEGIN
    CREATE USER [AzureFunctionAppName] FROM EXTERNAL PROVIDER;
END
ALTER ROLE db_datareader ADD MEMBER [AzureFunctionAppName];
ALTER ROLE db_datawriter ADD MEMBER [AzureFunctionAppName];
```

> [!NOTE]
> Replace **AzureFunctionAppName** with the name of the Managed Identity created for the Azure Function App.