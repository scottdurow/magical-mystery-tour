using ContosoRealEstate.BusinessLogic.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;

namespace Plugins;

/// <summary>
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
[CrmPluginRegistration(MessageNameEnum.Create,"account",StageEnum.PreValidation,ExecutionModeEnum.Synchronous, "name","OnCreate - Validate",1000,IsolationModeEnum.Sandbox)]
public class Plugin1 : PluginBase
{
    public Plugin1(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(Plugin1), unsecureConfiguration, secureConfiguration)
    {
        // TODO: Implement your custom configuration handling
        // https://docs.microsoft.com/powerapps/developer/common-data-service/register-plug-in#set-configuration-data
    }

    // Entry point for custom business logic execution
    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        if (localPluginContext == null)
        {
            throw new ArgumentNullException(nameof(localPluginContext));
        }

        var context = localPluginContext.PluginExecutionContext;

        var account  = context.InputParameterOrDefault<Entity>("Target");
        if (account.GetAttributeValue<string>("name").Contains("foo"))
        {
            throw new InvalidPluginExecutionException("Name cannot contain 'foo' !");
        }
        
    }
}
