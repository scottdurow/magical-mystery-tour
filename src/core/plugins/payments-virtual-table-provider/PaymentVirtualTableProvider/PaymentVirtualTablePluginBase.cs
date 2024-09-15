using Microsoft.Xrm.Sdk;
using PaymentVirtualTableProvider.Services;
using PaymentVirtualTableProvider.Services.PaymentApi;
using System;

namespace PaymentVirtualTableProvider;

public abstract class PaymentVirtualTablePluginBase : PluginBase, IPlugin
{
    internal PaymentVirtualTablePluginBase(Type pluginClassName, string unsecureConfiguration, string secureConfiguration) : base(pluginClassName, unsecureConfiguration, secureConfiguration)
    {
    }


    public override void RegisterServices(IServiceIOCContainer container)
    {
        // Register the services needed by the plugins
        // This is called by the base class PluginBase during the Execute method
        // The services provided by the plugin pipeline will be retrieved internally from
        // the plugin service provider and do not need to be registered
        container.Register<IPaymentsApiService, PaymentsApiService>();
        container.Register<IEnvironmentVariableService, EnvironmentVariableService>();
    }

}
