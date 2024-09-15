using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit.Abstractions;

namespace Contoso.API.Payments.Tests.IntegrationTests;

public class StripeFunctionsTests
{
    private readonly Mock<ILogger<StripeFunctions>> _stripeLoggerMock;
    private readonly Mock<ILogger<PaymentFunction>> _paymentLoggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly IConfiguration _configurationMock;

    private const string ValidJson = @"
    {
        ""ProductName"": ""Big house in the city"",
        ""Amount"": 100,
        ""Currency"": ""usd"",
        ""CreatedAt"": ""2023-01-01T00:00:00Z"",
        ""SuccessUrl"": ""https://localhost/success"",
        ""CancelUrl"": ""https://localhost/cancel""
    }";
    private const string InvalidJson = "invalid json";

    public StripeFunctionsTests(ITestOutputHelper output)
    {
        _stripeLoggerMock = FunctionMocks.CreateLoggerMock<StripeFunctions>(output);
        _paymentLoggerMock = FunctionMocks.CreateLoggerMock<PaymentFunction>(output);

        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) =>
            {
                if (categoryName == typeof(PaymentFunction).FullName)
                {
                    return _paymentLoggerMock.Object;
                }
                else if (categoryName == typeof(StripeFunctions).FullName)
                {
                    return _stripeLoggerMock.Object;
                }
                return Mock.Of<ILogger>();
            });

        _configurationMock = FunctionMocks.MockConfiguration();
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsOkResult_WithSessionUrl()
    {
        // Arrange
        var request = CreateMockHttpRequest(ValidJson);

        // Act
        var result = await CreateStripeFunctions().CreateCheckoutSession(request.Object) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var response = result.Value as CheckoutSession;
        Assert.NotNull(response);
        Assert.NotNull(response?.SessionUrl);
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsBadRequest_WhenRequestBodyIsInvalid()
    {
        // Arrange
        var request = CreateMockHttpRequest(InvalidJson);

        // Act
        var result = await CreateStripeFunctions().CreateCheckoutSession(request.Object) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsBadRequest_WhenRequestBodyIsEmpty()
    {
        // Arrange
        var request = CreateMockHttpRequest(string.Empty);

        // Act
        var result = await CreateStripeFunctions().CreateCheckoutSession(request.Object) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    private Mock<HttpRequest> CreateMockHttpRequest(string json)
    {
        var request = new Mock<HttpRequest>();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.Setup(r => r.Body).Returns(stream);
        return request;
    }

    private StripeFunctions CreateStripeFunctions()
    {
        return new StripeFunctions(_configurationMock, _stripeLoggerMock.Object, _loggerFactoryMock.Object);
    }
}