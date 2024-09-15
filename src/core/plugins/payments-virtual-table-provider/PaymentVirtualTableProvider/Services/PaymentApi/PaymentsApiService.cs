using Microsoft.Xrm.Sdk;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaymentVirtualTableProvider.Services.PaymentApi;

public class PaymentsApiService : IPaymentsApiService
{
    private readonly IEnvironmentVariableService _environmentVariableService;
    private readonly IManagedIdentityService _managedIdentityService;
    private readonly ITracingService _tracingService;
    private readonly string host;
    private readonly string baseUrl;
    private readonly string resourceUrl;
    // Constructor accepts the IServiceProvider interface and uses it to get an instance of the IKeyVaultService interface.
    // The IKeyVaultService interface is used to get the function key from the Azure Key Vault.
    // The function key is used to authenticate the request to the Azure function.
    // The GetPayments method makes an HTTP GET request to the Azure function to get the payments.
    // The function key is added to the request headers.
    // The response is deserialized into a list of Payment objects and returned.
    public PaymentsApiService(IEnvironmentVariableService environmentVariableService, IManagedIdentityService managedIdentityService, ITracingService tracingService)
    {

        _environmentVariableService = environmentVariableService;
        _managedIdentityService = managedIdentityService;
        _tracingService = tracingService;

        if (_managedIdentityService == null || _environmentVariableService == null || _tracingService == null)
        {
            throw new Exception("Failed to get required services.");
        }


        host = _environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiHost");
        baseUrl = _environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiBaseUrl");
        resourceUrl = _environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiResourceUrl");

    }

    public async Task<int> Create(Payment payment)
    {
        HttpClient client = GetHttpClient();

        // Add defaults
        payment.reservationId ??= "1";

        // Serialize the payment object to JSON and add to the HttpContent
        var json = JsonSerializer.Serialize(payment);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(GetApiUri(), content);

        // Get the integer value of the payment id from the response
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var entity = JsonSerializer.Deserialize<Payment>(jsonString);
            return entity.id;
        }
        else
        {
            throw new Exception($"Failed to call Azure function payments: {response.StatusCode}");
        }
    }

    public async Task<Payment> GetPayment(int id)
    {
        HttpClient client = GetHttpClient();
        var response = await client.GetAsync($"{GetApiUri()}/{id}");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var entity = JsonSerializer.Deserialize<Payment>(jsonString);
            return entity;
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        else
        {
            throw new Exception($"Failed to call Azure function payments/{id}: {response.StatusCode}");
        }
    }

    public async Task<List<Payment>> GetPayments(string query)
    {
        HttpClient client = GetHttpClient();
        var response = await client.GetAsync($"{GetApiUri()}?{query}");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var entities = JsonSerializer.Deserialize<List<Payment>>(jsonString);
            return entities;
        }
        else
        {
            throw new Exception($"Failed to call Azure function payments: {response.StatusCode}");
        }
    }

    private Uri GetApiUri()
    {
        return new Uri($"https://{host.TrimEnd('/')}/{baseUrl.TrimStart('/').TrimEnd('/')}/payments");
    }

    private HttpClient GetHttpClient()
    {
        string authToken;

        try
        {
            var scopes = new List<string> { $"{resourceUrl}/.default" };
            _tracingService.Trace($"Acquiring token for the managed identity using '{string.Join(",", scopes)}'");
            authToken = _managedIdentityService.AcquireToken(scopes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to acquire token for the managed identity. {ex.Message}", ex);
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("accept", "application/json");

        // Add the bearer token to the request headers
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        //client.DefaultRequestHeaders.Add("x-functions-key", this._keyVaultService.GetSecret(FunctionKeySecretName));
        return client;
    }
}
