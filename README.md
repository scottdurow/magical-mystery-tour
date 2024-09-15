# Contoso Real Estate - Power Platform Edition
![Build Status](../../actions/workflows/build.yml/badge.svg)  ![Deploy Status](../../actions/workflows/deploy-release.yml/badge.svg)

## 👀Overview

The Contoso Real Estate (Power Platform Edition) project is an adaptation of the Azure Sample 'Contoso Real Estate'. This project demonstrates how Power Platform can be integrated with Azure resources to create a comprehensive real estate management solution. By leveraging the capabilities of Power Platform, such as Power Apps, Power Automate, and Power BI, alongside Azure services, this project showcases a seamless and powerful approach to building and managing real estate applications.

### 🌟Key Features

- **Power Apps**: Custom applications for managing real estate listings, client interactions, and property details.
- **Power Automate**: Automated workflows for handling tasks such as notifications, approvals, and Power Pages operations.
- **Power Pages**: The Power Pages portal is one of the key features of the Contoso Real Estate (Power Platform Edition) project allowing searching and reservations of listings.
- **Azure Integration**: Utilizes Azure services like Azure SQL Database, Azure Functions, to enhance the functionality and scalability of the solution.

This project serves as a practical example of how Power Platform can be used in conjunction with Azure to build robust, scalable, and efficient business solutions.

> [!IMPORTANT]
> The application code is meant to serve as a reference. Please incorporate your security governance, audits and conventions before applying it to your own deployments.

- **ALM and Deployment**: The project emphasizes ALM practices, including source control, semantic versioning, environment variables, and automated deployments, reflecting enterprise-level application development standards.
- **Security and Best Practices**: Security is a foundational aspect of the project, with a focus on best practices such as not storing secrets outside of Key Vault and using managed identities for plugin authentication, showcasing the latest Power Platform capabilities. The project also serves as an example of managing permissions at a granular level through Entra ID rather than relying on secrets or keys passed in headers, promoting a secure and scalable architecture.
- **Integration of Features:** The project incorporates features like virtual entities, custom connectors, Custom APIs, and calling cloud flows from Power Pages. It also includes a payment system based on Azure functions, demonstrating end-to-end enterprise deployment considerations.

For more information see [Contoso Real Estate Architecture Overview](./docs/06-design.md)

### 📦Solution Segmentation

Contoso Real Estate is segmented into 3 solutions:

1. **ContosoRealEstateControl**s - PCF Code Controls to extend the Power Apps UI. These must be deployed as a managed solution built using the CI/CD pipeline and then used an a separate solution to ensure that the correct version dependency is created. This is because when adding to a canvas apps, a copy of the code component is created and so it must already be built with the correct version.
1. **ContosoRealEstateCore** - The main solution that contains tables, plugins & custom connectors. 
1. **ContosoRealEstatePortal** - The solution that contains the Power Pages Site for the Portal, and Cloud Flows that reference the custom connectors. This is because you cannot import a custom connector and cloud flows that use that custom connector in the same solution.

## ✅Getting Started

> [!NOTE]
> For a full end-to-end set of instructions on how to install prerequisites, clone, build, deploy, and test the solutions, refer to [full-development-setup-instructions.md](./docs/00-full-development-setup-instructions.md).

## ✅Deploying Azure Resources

Follow the instructions [docs/01-azure-api-setup.md](./docs/01-azure-api-setup.md). This will deploy all the required Azure resources to a new resource group in your selected subscription.

You will be asked to give your environment a name - use the name of the Power Platform environment you are deploying to (e.g. development). This environment name will be added as a folder under the `.azure\` folder where a `.env` file will store the settings required below. 

**Note**: All files under the `.azure` folder are not intended to be checked in to source control.

## ✅Deploying Power Platform Resources

Follow these instructions once the Azure resources are deployed.

### 📦Building the Solutions

Everything is stored as code for the Contoso Real Estate Solutions. No solution zip files should be present in this repo.

- To install the **ContosoRealEstateCore** solution, follow the instructions at [./docs/02-core-solution-setup.md](./docs/02-core-solution-setup.md)

- To install the **ContosoRealEstatePortal** solution, follow the instructions at [./docs/03-portal-solution-setup.md](./docs/03-portal-solution-setup.md)

Because of the segmentation, you will need to develop each of the solutions in a separate environment so that you only take dependencies on managed solution components that are part of the dependencies.

### 📜Post Solution Install Instructions

There are some manual tasks that cannot be performed during solution deployment.

- Reply URLs added to the Payments API Entra ID application registration to match the custom connectors
- Adding Cloud Flows to the Portal to refresh the trigger to point correctly to the new environment.

Follow the setup instructions carefully for the core or portal solution setup.

## Contributing

This project welcomes contributions and suggestions.

### Pull request review

Pull requests to this repo will be reviewed, at a minimum, by the Open Source Programs Office at
Microsoft, as well as a set of Microsoft's "Open Source Champs", for guidance.

Please understand that these templates often need to be kept rather simple, since
they are a starting point, and if there is too much guidance, teams may not be familiar
with how to react and manage projects with too much initial content.

### Contribution requirements

Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Contributor License Agreements](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

# Microsoft Open Source Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).

Resources:

- [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/)
- [Microsoft Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
- Contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with questions or concerns
- Employees can reach out at [aka.ms/opensource/moderation-support](https://aka.ms/opensource/moderation-support)

# Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow Microsoft’s Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party’s policies.
