using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

[assembly: FunctionsStartup(typeof(Contoso.API.Payments.Startup))]

namespace Contoso.API.Payments;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly());
        configurationBuilder.AddEnvironmentVariables();

        var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_ENDPOINT");

        // If AZURE_KEY_VAULT_ENDPOINT is set, create a configuration using KeyVault 
        if (keyVaultUri != null)
        {
            try
            {
                var keyVaultEndpoint = new Uri(keyVaultUri);
                configurationBuilder.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error configuring keyvault: {e.Message}");
            }

        }

        var configuration = configurationBuilder.Build();
        builder.Services.AddSingleton<IConfiguration>(configuration as IConfiguration);
    }
}

