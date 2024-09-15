# 🌐Contoso Real Estate Portal Solution 

The source for the Portal solution is built using:

- Power Pages
- Power Apps Component Framework
- Copilot Studio

> [!NOTE]
> For a full end-to-end set of instructions on how to install prerequisites, clone, build, deploy, and test the solutions, refer to [full-development-setup-instructions.md](./docs/00-full-development-setup-instructions.md).


## ✅Pre deployment configuration

The following tasks must be carried out before you import that `ContosoRealEstatePortal` solution

### 🤖Turn off Automatic Copilot Creation for Power Pages

When a power pages site is created, by default a new Copilot will be created and the `SiteComponent` with type `BotConsumer` will be updated to point to the new copilot. This interferes with the CI/CD process. As a work around the the `enableChatbotOnWebsiteCreation` Tenant setting should be turned off using the Power Platform admin PowerShell:

```powershell
$requestBody = @{
        powerPlatform = @{
            powerPages = @{
                enableChatbotOnWebsiteCreation = $false
            } 
        } 
    }

Set-TenantSettings -RequestBody $requestBody
```

For more information, see: https://learn.microsoft.com/en-us/power-pages/getting-started/enable-chatbot

The [powerpagesites.xml](\src\portal\solution\ContosoRealEstatePortal\src\Assets\powerpagesites.xml) contains a reference to the `BotConsumer` record via the `defaultbotconsumerid` field:

```xml
<powerpagesites>
  <powerpagesite powerpagesiteid="20f5d326-50b3-492b-aa2f-b29c913c932a">
    <content>
        {
        ...
        "defaultbotconsumerid":"102991e9-714b-ef11-a317-7c1e52150b3d"
        }
     </content>
...
</powerpagesites>
```

This is the id of the Power Pages Components that contains the schema name of the bot to use:

```xml
<powerpagecomponent powerpagecomponentid="102991e9-714b-ef11-a317-7c1e52150b3d">
  <content>
  {
  "botschemaname":
    "contoso_3c237b1a-7213-4759-b062-c6294730ec77",
    "configjson":"{\"skillConfigViewName\":\"Contoso Real Estate Portal bot Answers\"
  }"}
  </content>
 ...
</powerpagecomponent>
```

The schema name `contoso_3c237b1a-7213-4759-b062-c6294730ec77` refers to the bot that is deployed as part of the solution.

## ✅Deploying to a Development Environment to work on ContosoRealEstatePortal

Follow these steps to create a development environment:
- Install the dependency solutions `ContosoRealEstateCustomControls_managed.zip` and `ContosoRealEstateCore_managed.zip`
- Build the `ContosoRealEstatePortal.zip` solution from this repo source
- Import the built solution into the environment that you are currently authenticated with using `pac auth`
- Install reference and test data

### 🌱Create Your Developer Environment

To start contributing, you'll need to set up your developer environment.

1. Create a new developer Power Platform environment to work on the portal. This must be different from the `ContosoRealEstateCore` environment (unless you do not plan on checking in any changes and only want to test the solution). This is because the Portal solution takes a dependency on the managed layer below it.

1. Ensure you have all the updates installed via the Dynamics 365 apps page in the [admin portal](https://admin.powerplatform.microsoft.com/)

1. Run the script at `src\portal\solution\deployment-scripts\deploy-to-development-environment.ps1` and follow the instructions carefully.

### Building the solution without the deployment script

1. To build the solution use the following from a terminal inside VS Code:

   ```powershell
   cd <repo_root>/src/portal/ContosoRealEstateCore
   dotnet restore
   dotnet build -c Release
   ```

1. Import the newly built `ContosoRealEstateCore.zip` found at `<repo_root>/src/portal/ContosoRealEstatePortal/bin`
   **IMPORTANT:** You must install the **unmanaged** version of the solution.

1. Install the reference and sample data using:

   ```powershell
   cd <repo_root>/src/core
   pac data import -d ./data/reference-data.zip
   pac data import -d ./data/sample-data.zip
   ```

## ✅Post deployment steps
Some settings are not possible to make during the solution import:

- Setup Connections for Connection References (this can be done using pac connection create)
- Creating a power pages web site to host the Portal that is deployed via the solution
- Configuring the Power Pages flow trigger to point to the newly imported cloud flows
- Configuring Authentication of the Copilot Studio copilot that is deployed to the Power Pages site.

### 🔌Core Post Deployment Set up
Run the Core solution post deployment setup script:

```powershell
src\core\solution\deployment-scripts\2-post-deployment-setup.ps1
```

This will:
- Reply URLs added to the Payments API Entra ID application registration to match the custom connectors
- Update the Plugin Managed Identity to match your azure deployment

### 🔌Create connections

The portal solution uses a couple of connections. When importing the solution manually you will be prompted to wire up the different connection references to an actual connection, but when using the deployment script you will need to create them after deployment. The CI/CD pipeline automatically associates the connection references to connections using the deploymentSettings.json

Note: You will need to have run the post deployment steps for the core solution to setup the reply urls for the connectors.

This can be done automatically in the CI/CD deployment pipeline using the `deploymentSettings.json` but is easiest done manually when working on your development environment.

- `Dataverse` - Connection used by Cloud Flows and Copilot Studio
- `Contoso Stripe API` - Connection for Portal Cloud Flows

1. Open make.powerapps.com and open the `Contoso Real Estate Portal` solution
1. Open **Connection References**
1. Select each connection reference and select **+ New connection** under the **Connection** dropdown.
1. Search for the Connector type (Dataverse or Contoso Stripe API) and select the **+** add button, and then **Create**.
1. For production, SPNs will be used, however for development you can use your own account.
1. Return to the Connection References panel, select **Refresh**, and select the connection you have created (it will show as your login name)
1. Repeat for all connection references.
1. Navigate to Cloud Flows and select **Turn on** for each flow. (This isn't needed in CI/CD since the connection references are configured using the deploymentSettings.json and the flows are automatically turned on)

### 🌐Activate Power Pages Site

1. Open [Power Pages](https://make.powerpages.microsoft.com/)
1. Select your environment using the Environment picker on the top right.
1. Navigate to **Inactive Sites**.
1. Locate the **Contoso Real Estate Portal**, and select **Reactivate**.
1. Append the environment name to the website name (for ease of identification)
1. Enter a website address that references your environment - e.g `cre-my-developer-environment`
1. Select **Done**
1. Open the solution in [make.powerapps.com](https://make.powerapps.com/)
1. Select **Environment Variables** -> **Contoso Real Estate Portal Url** and enter the Url of your new site (e.g. https://cre-my-developer-environment.powerappsportals.com/)
NOTE: This is used by the Copilot Studio Copilot to search the site.
1. Wait for the portal to finish being created.

### ⚡Setup cloud flow triggers

When flows that are added to power pages are deployed, the trigger is not updated to match the target environment. For this reason, they must be manually re-configured. This creates unfortunately creates an unmanaged layer:

1. Open your site in [Power Pages](https://make.powerpages.microsoft.com/)
1. Select  **Set Up** - **Integrations** - **Cloud Flows**
1. For each flow in the site, select the **ellipsis ...**
1. **Edit** - **Save** (without changing anything). 
1. Power Pages will re-configure the trigger to point at the cloud flow in the current environment.

### 🤖Publish Chatbot

In order that you can test the portal chatbot in Copilot Studio you will need to configure authentication, if you don't want to test the Copilot you can simply skip to the Publish.

1. Open [Entra ID Application Registrations](https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps)
1. Select **All applications**
1. Search for the name you gave to your site above (e.g. Contoso Real Estate Portal cre-my-developer-environment) and open the application registration
1. Make a note of the **Application (client) ID**
1. Select **Certificates & Secrets** -> **Client Secrets**
1. Select **Add new client select** -> **Add**
1. Copy the Secret Value (Not the Secret ID)
1. Select **Authentication**
1. Under Web Redirect URIs, select **Add URI**
1. Enter `https://token.botframework.com/.auth/web/redirect` 
1. Select **Save**
1. Open [Copilot Studio](https://copilotstudio.microsoft.com/) -> **Select your environment** using the picker on the top right -> Open the **Contoso Real Estate Bot** under **Copilots**
1. Select **Settings** on the top right
1. Select **Security**
1. Select **Authentication**
1. Select **Authenticate Manually**
1. Enter the following:

- **Service Provide**r: `Generic OAuth 2`
- **Client ID**: *The Application ID of the Application copied above*
- **Client Secret**: *The secret value copied above*
- **Scope list delimited**: `,`
- **Authorization URL template**: `https://login.microsoftonline.com/common/oauth2/v2.0/authorize`
- Authorization URL query string template: `?client_id={ClientId}&response_type=code&redirect_uri={RedirectUrl}&scope={Scopes}&state={State}`
- **Token URL template**: `https://login.microsoftonline.com/common/oauth2/v2.0/token`
- Token URL query string template: `?`
- **Token body template**: `code={Code}&grant_type=authorization_code&redirect_uri={RedirectUrl}&client_id={ClientId}&client_secret={ClientSecret}`
- Refresh URL template: `https://login.microsoftonline.com/common/oauth2/v2.0/token`
- **Refresh URL query string template**: `?`
- Refresh body template: `refresh_token={RefreshToken}&redirect_uri={RedirectUrl}&grant_type=refresh_token&client_id={ClientId}&client_secret={ClientSecret}`
- **Scopes**: `profile email openid`

NOTE: If the client application is not configured for multi-tenant then you will need to replace common with your tenant ID.

1. Select **Save** -> **Save**
1. Select **Publish** on the top right and wait for the publish to complete.

## ✅Make your changes and then sync to create a change set

1. Once you have made changes to the solution using `make.powerapps.com`, you can create a changeset using the following:

   ```powershell
   ./src/portal/solution/sync.ps1
   ```

   **NOTE:** The solution was initially setup using:

   ```powershell
   cd <repo_root>/src/portal/ContosoRealEstatePortal
   pac solution clone -n ContosoRealEstateCore -a -p Both
   ```

1. Examine the changes that are synced, and remove any '*noisy*' diffs that are not part of your changes.

1. **Commit** your changes

1. Create a **Pull Request** against the main branch in this repo.

Happy coding! 🚀
