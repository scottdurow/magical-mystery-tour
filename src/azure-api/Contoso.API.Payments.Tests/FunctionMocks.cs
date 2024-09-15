using Contoso.API.Payments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Contoso.API.Payments.Tests;

internal static class FunctionMocks
{
    internal static Mock<ILogger<T>> CreateLoggerMock<T>(ITestOutputHelper output)
    {
        var loggerMock = new Mock<ILogger<T>>();
        // Setup the Log method to output to the console
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback((LogLevel logLevel, EventId eventId, object state, Exception exception, Delegate formatter) =>
            {
                var formattedMessage = formatter.DynamicInvoke(state, exception) as string;

                output.WriteLine(formattedMessage);

            });
        return loggerMock;
    }

    internal static IConfiguration MockConfiguration()
    {
        var configurationMock = new ConfigurationBuilder();
        configurationMock.AddUserSecrets<PaymentFunction>();
        return configurationMock.Build();
    }
}
