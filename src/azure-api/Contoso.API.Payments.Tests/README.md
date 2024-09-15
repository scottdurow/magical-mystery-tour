# Contoso.API.Payments.Tests

## Integration tests

In order to run the integration tests or the azure functions locally, there must be some environment variables/configuration available.
These are set up using 'User secrets' on the Azure Functions project.

### Method 1 - dotnet command line
Run the following commands from the command line.
**NOTE**: Replace the placeholder for the Contoso.API.Payments project root with the actual path.

```
cd <Azure Functions Project Root>/Contoso.API.Payments
dotnet user-secrets init

dotnet user-secrets set "AZURE-SQL-CONNECTION-STRING-payments-api"  "Server=tcp:sql-contoso-real-estate.database.windows.net,1433;Initial Catalog=contoso-real-estate;"
dotnet user-secrets set "StripeApiKey" "sk_test_******"

```

## Method 2 - Visual Studio UI

Right click on the Contoso.API.Payments Azure Functions project and select 'Manage User Secrets'.
This will open a JSON file where you can add the following configuration:

```
{
  "AZURE-SQL-CONNECTION-STRING-payments-api": "Server=tcp:sql-contoso-real-estate.database.windows.net,1433;Initial Catalog=contoso-real-estate;",
  "StripeApiKey": "sk_test_******"
}
```