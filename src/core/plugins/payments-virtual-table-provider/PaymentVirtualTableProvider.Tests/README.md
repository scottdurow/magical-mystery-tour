# PaymentVirtualTableProvider.Tests

## Running the integration tests

In order to run the integration tests, you will need to add secrets to use for the integration tests.
The secrets should never be added to this repo and are stored in your user appdata folder.

The `MicrosoftConfigurationBuilders.UserSecrets.UserSecretsConfigurationBuilder` NuGet package is used to read the secrets from the user secrets store.

You can create the secrets file using the following PowerShell:

```powershell
./src/core/plugins/payments-virtual-table-provider/PaymentVirtualTableProvider.Tests/set-integration-test-secrets.ps1
```

### NOTE

This test project is using .net 4.7.2 because  the `MicrosoftConfigurationBuilders.UserSecrets.UserSecretsConfigurationBuilder` NuGet package is not compatible with .net 4.6.2
This means that there are some assembly binding redirects in the app.config file to ensure that the correct version of the package is used that are used by the plugin project.

