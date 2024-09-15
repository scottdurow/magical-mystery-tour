using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using System;

namespace PaymentProvider;

public class UpdatePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        // Payments are immutable
        throw new GenericDataAccessException("Payments are read only");

    }
}
