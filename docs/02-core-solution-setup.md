# 🏠Contoso Real Estate Core Solution 

Welcome to the Contoso Real Estate Core solution! This solution includes:

- Dataverse Tables 📊
- Model Driven Admin App 📱
- Canvas Admin App 🖌️
- Dataverse Plugins 🔌
- Custom Connector for Payments 💳
- Virtual Table Provided for Payments 💼
- Power Fx Plugins 🔋
- Reference Data (e.g., Fee Types) 📚
- Sample Data (Listings with images) 🏡

## ✅Deploying to a Development Environment to work on ContosoRealEstateCore

> [!NOTE]
> For a full end-to-end set of instructions on how to install prerequisites, clone, build, deploy, and test the solutions, refer to [full-development-setup-instructions.md](./docs/00-full-development-setup-instructions.md).

Follow these steps to create a development environment:

### 🌱Create Your Developer Environment

To start contributing, you'll need to set up your developer environment. Here's a step-by-step guide:

1. Deploy the Azure resources using the instructions in [01-azure-api-setup.md](01-azure-api-setup.md).

> [!IMPORTANT]
> You must deploy the Azure resources before you can import the core solution due to the dependency on Azure Key Vault and the managed identity.

2. Create a new developer Power Platform environment.

1. Ensure you have all the updates installed via the **Dynamics 365 apps page** in the [admin portal](https://admin.powerplatform.microsoft.com/)
   1. Inside the **admin center**, select your **Environment**.

   1. Select **Resources** -> **Dynamics 365 apps**.

   1. Where **Update available** is shown, select the link.

   1. Check **I agree to the terms of service**.

   1. Select **Update**.

   1. Repeat for all updates

   1. Wait until the updates are installed.

1. Install the latest version of the Dataverse Accelerator
   1. In the **Dynamics 365 apps** area, select **Open App Source**.

   1. Search for Dataverse Accelerator

   1. Select **Get it now** on the Dataverse Accelerator Card, and **Get it now** again 

   1. Select your development environment, and check '**I agree**' to the two checkboxes.

   1. Select **Install**.

1. Follow the instructions when running the script [1-deploy-to-development-environment.ps1](/src/core/solution/deployment-scripts/1-deploy-to-development-environment.ps1)

### Manually building and deploying the solution

1. To build the solution use the following from a terminal inside VS Code:

   ```powershell
   cd <repo_root>/src/core/ContosoRealEstateCore
   dotnet restore
   dotnet build -c Release
   ```

**IMPORTANT:** Unless you use the Release mode to build, you will see an error during import:

```text
Import Solution Failed: CustomControl with name Contoso.PortalReactUI failed to import with error: Webresource content size is too big.
CustomControl with name Contoso.PortalReactUI failed to import with error: Webresource content size is too big.
```

1. Import the newly built `ContosoRealEstateCore.zip` found at `<repo_root>\src\core\ContosoRealEstateCore\bin`
   **IMPORTANT:** You must install the **unmanaged** version of the solution.

1. Install the reference and sample data using:

   ```powershell
   cd <repo_root>/src/core
   pac data import -d ./data/reference-data.zip
   pac data import -d ./data/sample-data.zip
   ```

### 📜Post Solution Install Instructions

There are some manual tasks that cannot be performed during solution deployment - if you have run the `azure-api` installation procedure.

- Reply URLs added to the Payments API Entra ID application registration to match the custom connectors
- Update the Plugin Managed Identity to match your azure deployment

Run the following PowerShell and follow the instructions carefully:

```powershell
./src/core/solution/deployment-scripts/2-post-deployment-setup.ps1
```

## ✅Make changes and sync

1. Once you have made changes to the solution using make.powerapps.com, you can create a changeset using the following:

    ```powershell
    ./src/core/solution/sync.ps1
    ```
    
    **NOTE:** The solution was initially setup using:
    
    ```powershell
    cd <repo_root>/src/core/ContosoRealEstateCore
    pac solution clone -n ContosoRealEstateCore -a -pca -p Both
    ```
    
1. Examine the changes that are synced, and remove any '*noisy*' diffs that are not part of your changes.

1. **Commit** your changes

1. Create a **Pull Request** against the main branch in this repo.

**IMPORTANT:** Do not install the portal on this same environment if you want to work on the solution. The Portal solution must be installed on an environment with the `ContosoRealEstateCore` solution deployed as a managed solution.

Happy coding! 🚀
