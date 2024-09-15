using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit.Abstractions;

namespace Contoso.API.Payments.Tests.IntegrationTests;

public class PaymentFunctionTests
{
    private readonly Mock<ILogger<PaymentFunction>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly PaymentFunction _paymentFunction;

    public PaymentFunctionTests(ITestOutputHelper output)
    {
        _loggerMock = FunctionMocks.CreateLoggerMock<PaymentFunction>(output);
        _configuration = FunctionMocks.MockConfiguration();
        _paymentFunction = new PaymentFunction(_configuration, _loggerMock.Object);
    }

    [Fact]
    public async Task AddPayment_Returns_Id()
    {

        // Act
        var paymentid = await _paymentFunction.AddPaymentInternal(new Payment
        {
            UserId = Guid.NewGuid().ToString(),
            ReservationId = Guid.NewGuid().ToString(),
            Provider = 1,
            Status = (int)PaymentStatus.Cancelled,
            // Convert cents to dollars
            Amount = 4242,
            Currency = "USD",
            CreatedAt = DateTime.Now
        });

        // Assert
        Assert.True(paymentid > 0);

    }

    // Test payment function ListPayments using a filter and order by
    [Fact]
    public async Task ListPayments_Returns_Payments()
    {

        // Arrange
        var modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EntitySet<Payment>("Payments");
        IEdmModel model = modelBuilder.GetEdmModel();
        Uri serviceRoot = new("https://payments/");

        Uri requestUri = new("Payments?&$filter=Status eq 1&$orderby=Id desc&$top=10&$skip=0",
                          UriKind.Relative);
        ODataUriParser parser = new(model, serviceRoot, requestUri);
        FilterClause filter = parser.ParseFilter();                // parse $filter
        OrderByClause orderby = parser.ParseOrderBy();             // parse $orderby
        long? top = parser.ParseTop();                             // parse $top
        long? skip = parser.ParseSkip();                           // parse $skip



        // Act
        var payments = await _paymentFunction.ListPaymentsInternal(filter, orderby, top, skip);

        // Assert
        Assert.NotNull(payments);
        Assert.NotEmpty(payments);
        Assert.True(payments.All(p => p.Status == 1));
        Assert.True(payments.SequenceEqual(payments.OrderByDescending(p => p.Status)));





    }


}