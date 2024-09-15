# How to deploy without the Azure API deployment

If you would like to only install the Power Platform aspect of the solution, you will need to adjust the Key Vault Secret Environment Variable to be a string rather than a Secret.
This is because when importing the Core Solution you will receive the following error if the Azure Key Vault Payment API Secret has no value:

`Connector import: FAILURE: Error occured while reading secret: Value cannot be null.`

You can work around this by:

1. Opening the file `src\core\solution\ContosoRealEstateCore\src\environmentvariabledefinitions\contoso_PaymentsApiSecret\environmentvariabledefinition.xml`
2. Changing `<type>100000005</type>` to be `<type>100000000</type>`
    This changes the type to be a string. 
3. Building the core solution as outlined in [02-core-solution-setup.md](/docs/02-core-solution-setup.md)
4. Import the build solution manually, and provide dummy values for the environment variables when prompted.