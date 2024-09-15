using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Organization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentVirtualTableProvider.Services;

/// <summary>
/// Implementation of IEnvironmentVariableService that retrieves environment variables from the environment variable definition entity.
/// Values are cached in a dictionary to improve performance.
/// This cache will be only for in-memory instances of the plugin.
/// A lock is used so that only one thread can update the dictionary at a time.
/// Once IEnvironmentVariableService is implemented and available from the serviceProvider, this will no longer be necessary.
/// To invalidate the cache, the plugin package must be reimported to force existing instances to be cleared.
/// </summary>
public class EnvironmentVariableService : IEnvironmentVariableService
{
    static Dictionary<string, string> _envVariables;
    static readonly object _lock = new object();
    private IOrganizationService _organizationService;
    private ITracingService _tracingService;

    // Constructor that accepts the IOrganizationService interface and uses it to get the environment variables.
    // The environment variables are stored in a dictionary.
    // The RetrieveEnvironmentVariableValue method is used to get the value of an environment variable.
    public EnvironmentVariableService(IOrganizationService organizationService, ITracingService tracingService)
    {
        _organizationService = organizationService;
        _tracingService = tracingService;
    }


    public string RetrieveEnvironmentVariableValue(string environmentVariableSchemaName)
    {
        _tracingService.Trace($"Entered RetrieveEnvironmentVariableValue");

        // Check if the environment variable is already in the dictionary
        if (_envVariables != null && _envVariables.ContainsKey(environmentVariableSchemaName))
        {
            _tracingService.Trace($"Environment variable {environmentVariableSchemaName} found in cache");
            return _envVariables[environmentVariableSchemaName];
        }

        _tracingService.Trace($"Environment variable {environmentVariableSchemaName} not found in cache");
        string variableValue = null;
        switch (environmentVariableSchemaName)
        {
            case "EnvironmentWebApiEndpoint":
                variableValue = RetrieveOrganizationEndpoint();
                break;

            default:

                OrganizationRequest request = new OrganizationRequest("RetrieveEnvironmentVariableValue");
                request.Parameters["DefinitionSchemaName"] = environmentVariableSchemaName;

                var response = _organizationService.Execute(request);
                variableValue = response.Results["Value"].ToString();

                break;
        }

        if (variableValue == null)
        {
            throw new Exception($"Environment variable {environmentVariableSchemaName} not found.");
        }
        // Lock the dictionary to prevent multiple threads from updating it at the same time
        // and add the variableValue to the dictionary using the Environment variabe name so it is cached.
        lock (_lock)
        {
            if (_envVariables == null)
            {
                _envVariables = new Dictionary<string, string>();
            }
            _envVariables[environmentVariableSchemaName] = variableValue;
        }

        return variableValue;
    }

    private string RetrieveOrganizationEndpoint()
    {
        _tracingService.Trace($"Entered RetrieveOrganizationEndpoint");
        var request = new RetrieveCurrentOrganizationRequest();
        var response = (RetrieveCurrentOrganizationResponse)_organizationService.Execute(request);
        if (response != null && response.Detail.Endpoints.ContainsKey(EndpointType.OrganizationDataService))
        {
            _tracingService.Trace($"Endpoints: {String.Join(", ", response.Detail.Endpoints.Select(e => String.Concat(e.Key, ": ", e.Value)))}");
            return response.Detail.Endpoints[EndpointType.OrganizationDataService].ToString();
        }
        else
        {
            throw new Exception("Organization endpoint not found.");
        }
    }
}
