using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Linq;
using System.Security.Claims;

namespace Contoso.API.Payments;

public class StripeFunctions
{
    private readonly ILogger<StripeFunctions> _log;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;

    public StripeFunctions(IConfiguration configuration, ILogger<StripeFunctions> log, ILoggerFactory loggerFactory)
    {
        _log = log;
        _loggerFactory = loggerFactory;
        _configuration = configuration;
    }

    // Add a ping function to test the function app that accepts a string parameter and returns the same string with the current date and time
    // Add a ping function to test the function app that accepts a string parameter and returns the same string with the current date and time
    [FunctionName("Ping")]
    [OpenApiOperation("Ping", "Ping function")]
    [OpenApiParameter("message", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The message to be echoed")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(PingResponseModel), Description = "The response containing the echoed message and timestamp")]
    public IActionResult Ping(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping/{message}")] HttpRequest _,
        string message, ClaimsPrincipal principal)
    {
        _log.LogInformation("Processing ping request.");
      
        var roles = principal.Claims.Where(e => e.Type == "roles").Select(e => e.Value);
            
        var outMessage = $"Input (v1):{message} Roles:{(principal == null ? "No user" : principal.Identity.Name)} {(principal.Claims == null ? "No Claims" : "")}  {string.Join(", ", roles)}";
        //var user = req.HttpContext?.User;
        //var outMessage = $"Input (v1):{message} Roles:{(user == null ? "No user" : user?.Identity?.Name)} {(user?.Claims == null ? "No Claims" : "")}  {string.Join(", ", roles)}";
        return new OkObjectResult(new PingResponseModel { Message = outMessage, Timestamp = DateTime.Now });
    }


    [FunctionName("CreateCheckoutSession")]
    [OpenApiOperation("Create Checkout Session", "Creates a Stripe checkout session")]
    [OpenApiRequestBody("application/json", typeof(CheckoutRequest), Description = "The request body for creating a checkout session")]
    public async Task<IActionResult> CreateCheckoutSession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stripe/checkout")] HttpRequest req)
    {
        _log.LogInformation("Processing request to create a Stripe checkout session.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        CheckoutRequest checkoutRequest;
        try
        {
            checkoutRequest = JsonConvert.DeserializeObject<CheckoutRequest>(requestBody);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult(new { error = "Invalid JSON format in request body" });
        }

        if (checkoutRequest == null)
        {
            return new BadRequestObjectResult(new { error = "Invalid request body" });
        }

        try
        {
            var stripeApiKey = _configuration["StripeApiKey"];
            _log.LogInformation("API KEY ={0}****", stripeApiKey[..4]);
            StripeConfiguration.ApiKey = stripeApiKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new() {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            Currency = checkoutRequest.Currency,
                            // Convert cents to dollars,
                            UnitAmountDecimal = checkoutRequest.Amount*100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = checkoutRequest.ProductName,
                                Description = checkoutRequest.Description,
                            }
                        }
                    },
                },
                Mode = "payment",
                SuccessUrl = checkoutRequest.SuccessUrl,
                CancelUrl = checkoutRequest.CancelUrl,
                ExpiresAt = DateTime.Now.AddMinutes(30), // Minimum of 30 minutes
                ClientReferenceId = checkoutRequest.ClientReferenceNumber,
                CustomerEmail = checkoutRequest.CustomerEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", checkoutRequest.CustomerNumber }
                }


            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return new OkObjectResult(new CheckoutSession
            {
                SessionUrl = session.Url,
                SessionExpiry = session.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _log.LogError($"Error creating Stripe checkout session: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [FunctionName("StripeWebhook")]
    // Open API definition for the function
    [OpenApiOperation("Stripe Webhook", "Processes Stripe webhook events")]
    [OpenApiRequestBody("application/json", typeof(Event), Description = "The Stripe webhook event")]
    public async Task<IActionResult> StripeWebhook(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "stripe/webhook")] HttpRequest req)
    {
        _log.LogInformation("Processing Stripe webhook event.");

        try
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var signature = req.Headers["Stripe-Signature"];
            var webhookSecret = _configuration["StripeWebhookSecret"];
            _log.LogInformation("Webhook secret={0}****, Signature={1}", webhookSecret?[..8], signature);
            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

            if (stripeEvent == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid webhook event" });
            }

            // Handle the event
            switch (stripeEvent.Type)
            {
                case Events.CheckoutSessionCompleted:
                case Events.CheckoutSessionAsyncPaymentSucceeded:
                case Events.CheckoutSessionAsyncPaymentFailed:
                case Events.CheckoutSessionExpired:
                    var session = stripeEvent.Data.Object as Session;
                    _log.LogInformation($"Checkout session completed: clientReferenceId:{session.ClientReferenceId} Id:{session.Id}");
                    // Handle successful checkout session completion
                    // Create the payment in the back end by creating a payment function object and calling create payment
                    ILogger<PaymentFunction> paymentLogger = _loggerFactory.CreateLogger<PaymentFunction>();
                    var payments = new PaymentFunction(_configuration, paymentLogger);
                    var paymentStatus = stripeEvent.Type switch
                    {
                        Events.CheckoutSessionCompleted or Events.CheckoutSessionAsyncPaymentSucceeded => PaymentStatus.Complete,
                        Events.CheckoutSessionAsyncPaymentFailed or Events.CheckoutSessionExpired => PaymentStatus.Cancelled,
                        _ => PaymentStatus.Cancelled,
                    };
                    await payments.AddPaymentInternal(new Payment
                    {
                        UserId = session.Metadata["userId"],
                        ReservationId = session.ClientReferenceId,
                        Provider = 1,
                        Status = (int)paymentStatus,
                        // Convert cents to dollars
                        Amount = session.AmountTotal.HasValue ? (decimal)session.AmountTotal.Value / 100 : 0,
                        Currency = session.Currency,
                        CreatedAt = DateTime.Now
                    });

                    break;
                // Handle other event types as needed
                default:
                    _log.LogWarning($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }

            return new OkObjectResult(new { message = "Webhook event processed successfully" });
        }
        catch (Exception ex)
        {
            _log.LogError($"Error processing Stripe webhook event: {ex.Message}");
            throw;
        }
    }
}

public class CheckoutRequest
{

    public string ClientReferenceNumber { get; set; }
    public string CustomerNumber { get; set; }
    public string CustomerEmail { get; set; }
    public string ProductName { get; set; }

    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ExpiryRedirectUrl { get; set; }

    public string SuccessUrl { get; set; }

    public string CancelUrl { get; set; }
}

public class CheckoutSession
{
    public string SessionUrl { get; set; }

    public DateTime SessionExpiry { get; set; }
}

public class PingResponseModel
{
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}
