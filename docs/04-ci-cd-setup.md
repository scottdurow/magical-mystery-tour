# Setting up CI/CD

This project is setup for GitHub Continuous integration and deployment workflows.

The workflows uses the Power Platform CLI (pac) and connects using GitHub [federated identity credentials](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-create-trust?pivots=identity-wif-apps-methods-azp).

> [!IMPORTANT]
> You can connect to Power Platform using a Application ID and Secret, however using federated identity credentials avoids the need to store client secrets in GitHub.

For the deployment workflow to deploy to each environment the following setup is required:

- GitHub environments created (**GitHub** -> **Settings** -> **Environments**)
    - **solution-checker** - Used to run the Solution Checker during build workflows. This requires a Power Platform connection to use the PAC CLI
    - **development** - Environment that the releases are deployed to first to stabilize a release. Integration and UI tests are performed in this environment.
    - **testing** - Environment that the releases are deployed to after a release is stabilized, for acceptance testing.
    - **production** - Final production environment

- Environment secrets added to each environment (Add environment secret)
    - `PAC_DEPLOY_AZURE_TENANT_ID` - The Azure Tenant ID of the Power Platform Tenant being deployed to
    - `PAC_DEPLOY_CLIENT_ID` - The Application ID of an Entra ID application registration that has been added to the target Power Platform environment as an application user.
    - `PAC_DEPLOY_ENV_URL` - The url of the target environment e.g. https://org123.crm.dynamics.com

- Environment variables are needed to provide deployment settings:
    - `PAC_DEPLOY_CONFIG` - The deployment settings json that contains the environment variables, connection references, and data files to import for each solution
    

Approvals are used to gate each environment deployment. 
> [!NOTE]
> This feature is only available to **public** repos on Pro or Team based accounts. 

## Adding an environments

Perform the following steps for each GitHub environment ( `solution-checker`, `development`, `testing`, `production`). A minimum of solution-checker and development must be created to use the CI/CD workflows.

1. Go to **Settings | Environments**
1. Select **New environment**
1. Give the environment a name (e.g. `solution-checker`, `development`, `testing`, `production`)
1. For environments other than `solution-checker`, Check **Require reviewers**
1. Add one or more required reviewers by searching for their names
1. Select **Save protection rules**
1. Repeat for other environments

## Configuring Federated Identity Credentials

GitHub workflow pac commands can connect using:
```
pac auth create --githubFederated --tenant ${{ secrets.PAC_DEPLOY_AZURE_TENANT_ID }} --applicationId ${{ secrets.PAC_DEPLOY_CLIENT_ID }} --environment ${{ secrets.PAC_DEPLOY_ENV_URL }}
```
Federated credentials must be added to Entra ID to establish a trust.

👉 To setup these credentials, drag the script `/src/core/solution/deployment-scripts/3-github-environment-add-fed-creds.ps1` into your VSCode termainal and press **ENTER**.

To perform these steps manually you can use the following steps:
1. Authenticate the [Power Platform CLI](https://marketplace.visualstudio.com/items?itemName=microsoft-IsvExpTools.powerplatform-vscode) and select the target Power Platform environment:
    ```powershell
    pac auth create
    
    pac env list
    
    # Alternatively use the VSCode extension to authenticate and select the environment
    pac env select --environment <Environment Name>
    ```

1. Install the Azure CLI by following the instructions provided in the [official Azure CLI documentation for your operating system](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli).

1. Drag the the script [/src/core/solution/deployment-scripts/3-github-environment-add-fed-creds.ps1]([/src/core/solution/deployment-scripts/3-github-environment-add-fed-creds.ps1) into your VSCode terminal, and press **ENTER** to setup the GitHub CI/CD authentication. 

## Manual Steps
The script performs the following steps for you:

1. Log in to your Azure account by running the following command and following the prompts:

    ```powershell
    az login
    ```

    This will open a browser window where you can authenticate with your Azure account.

1. Create an Entra ID Application and application ID for that environment.

    ```powershell
    $environmentName = "development"
    
    $tenantId = az account show --query tenantId -o tsv
    $environmentDetails = pac env who --json | ConvertFrom-Json
    $environmentUrl = $environmentDetails.OrgUrl.TrimEnd('/')
    $spnName = "cre-github-workflows-$environmentName"
    $remoteUrl = git remote get-url origin
    if ($remoteUrl -match "github\.com[:/](.+?)/(.+?)(\.git)?$") {$repoName = $matches[1] + "/" + $matches[2] }
    $spn = pac admin create-service-principal --name $spnName --json | ConvertFrom-Json
    ```
    
1. For an existing SPN, add the SPN as an application user to the target Power Platform environment. For the solution-packager environment, use the same as the development environment:

    ```powershell
    $applicationId = az ad sp list --display-name $spnName --query "[0].appId" -o tsv
    # Currently the pac admin create-service-principal or list-service-principal verbs do not support the --json command, so use az to get the application id
    pac admin assign-user --environment $environmentUrl --application-user --user $applicationId --role "System Administrator"
    ```

1. Register the application id as federated credentials for GitHub:

    ```powershell
    $applicationId = az ad sp list --display-name $spnName --query "[0].appId" -o tsv
    az ad app federated-credential create --id $applicationId --parameters "{'name': '$spnName','issuer': 'https://token.actions.githubusercontent.com','subject': 'repo:$($repoName):environment:$environmentName','description': 'GitHub access for the environment $environmentName and repo $repoName ','audiences': ['api://AzureADTokenExchange']}" >> $null
    ```

1. Inside GitHub, navigate to **Settings** -> **Environments** -> **Select Environment**
1. Select **Add environment secret**
1. Use the following script to create the secrets:

    ```powershell
    Write-Host @"
    PAC_DEPLOY_AZURE_TENANT_ID = $tenantId
    PAC_DEPLOY_CLIENT_ID = $applicationId
    PAC_DEPLOY_ENV_URL = $environmentUrl
    "@
    ```

## Deployment Settings

👉 To setup the Deployment Config, drag the script `src\core\solution\deployment-scripts\4-github-environment-create-deployment-settings.ps1` into your VSCode terminal and press **ENTER**.

This script will prompt you to create an environment variable called `PAC_DEPLOY_CONFIG` for each envrionment.

The variable must be in the form:

```json
{
"ContosoRealEstateCore": {
    "data": [
    "reference-data.zip",
    "sample-data.zip"
    ],
    "deploymentSettings": {
    ...
    }
},
"ContosoRealEstatePortal": {
    "data": [],
    "deploymentSettings": {
    ...
    }
}
}
```

This script will also prompt you to create an environment secret called `PLUGIN_MANAGED_IDENTITY_APP_ID` containing the Application ID of the Payment API Client that the C# Plugin Virtual Table Provider will use to connect to the Payment API. This is injected into the solution before it is deployed because at this time the Managed Identity Application Id and Tenant Id are not configurable using `deploymentSettings.json`.

## Set repository variables
A responsitory variable is needed to control which solutions are built.

1. Navigate to **GitHub** -> **Settings** -> **Secrets and Variables** -> **Actions** -> **Variable Tab**
1. Select **New repository variable**
1. Enter the following variables:
 - `SOLUTIONS_CONFIG`

    ```json
    [
    {
        "solutionName": "ContosoRealEstateCustomControls",
        "changeScope": "src/controls",
        "solutionSubFolder": "solution/ContosoRealEstateCustomControls",
        "runSolutionChecker": true,
        "solutionCheckerRuleLevelOverride":""
    },
    {
        "solutionName": "ContosoRealEstateCore",
        "changeScope": "src/core",
        "solutionSubFolder": "solution/ContosoRealEstateCore",
        "runSolutionChecker": true,
        "solutionCheckerRuleLevelOverride":""
    },
    {
        "solutionName": "ContosoRealEstatePortal",
        "changeScope": "src/portal",
        "solutionSubFolder": "solution/ContosoRealEstatePortal",
        "runSolutionChecker": true,
        "solutionCheckerRuleLevelOverride":""
    }
    ]
    ```

- (Optional) `ACTIONS_STEP_DEBUG` = true/false

