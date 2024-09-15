using ContosoRealEstate.BusinessLogic.Resources;
using ContosoRealEstate.BusinessLogic.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Globalization;
using System.Runtime.Remoting.Contexts;

namespace ContosoRealEstate.BusinessLogic.Plugins;

public abstract class BusinessLogicPluginBase : PluginBase, IPlugin
{

    internal BusinessLogicPluginBase(Type pluginClassName, string unsecureConfiguration, string secureConfiguration) : base(pluginClassName, unsecureConfiguration, secureConfiguration)
    {
    }

    /// <summary>
    /// Validates the custom API execution context to ensure it matches the expected message name and stage.
    /// </summary>
    /// <param name="context">The plugin execution context.</param>
    /// <param name="messageName">The expected message name.</param>
    /// <exception cref="InvalidPluginExecutionException">
    /// Thrown when the plugin execution context does not match the expected message name or stage.
    /// </exception>
    protected static void ValidateCustomApiExectionContext(IPluginExecutionContext context, string messageName)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!(context.MessageName.Equals(messageName) &&
            context.Stage.Equals(30)))
        {
            throw new InvalidPluginExecutionException(ExceptionMessages.INVALID_PLUGIN_MESSAGE_REGISTRATION);
        }
    }

    /// <summary>
    /// Validates the plugin execution context to ensure it matches the expected message name, stage, and logical name.
    /// </summary>
    /// <param name="context">The plugin execution context.</param>
    /// <param name="messageName">The expected message name.</param>
    /// <param name="stage">The expected stage of the plugin execution.</param>
    /// <param name="logicalName">The expected logical name of the target entity.</param>
    /// <exception cref="InvalidPluginExecutionException">
    /// Thrown when the plugin execution context does not match the expected message name, stage, or logical name.
    /// </exception>
    protected static void ValidatePluginExecutionContext(IPluginExecutionContext context, MessageNameEnum messageName, StageEnum stage, string logicalName)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!(context.MessageName == messageName.ToString() &&
              context.Stage == (int)stage &&
              context.InputParameters.Contains("Target") &&
              context.InputParameters["Target"] is Entity targetEntity &&
              targetEntity.LogicalName == logicalName))
        {
            throw new InvalidPluginExecutionException(ExceptionMessages.INVALID_PLUGIN_MESSAGE_REGISTRATION);
        }
    }

    /// <summary>
    /// Retrieves the pre-image from the plugin execution context.
    /// </summary>
    /// <param name="context">The plugin execution context.</param>
    /// <returns>The pre-image entity.</returns>
    /// <exception cref="InvalidPluginExecutionException">Thrown when the pre-image is missing.</exception>
    protected static Entity GetPreImage(IPluginExecutionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }


        if (!context.PreEntityImages.TryGetValue("PreImage", out Entity preImage) || preImage == null)
        {
            throw new InvalidPluginExecutionException(ExceptionMessages.PREIMAGE_MISSING);
        }
        return preImage;
    }

    /// <summary>
    /// Validates that the provided string can be parsed into a valid Guid.
    /// </summary>
    /// <param name="parameterName">The name of the parameter being validated.</param>
    /// <param name="value">The string value to be validated as a Guid.</param>
    /// <returns>The parsed Guid value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provided string cannot be parsed into a valid Guid.
    /// </exception>
    protected static Guid ValidateGuid(string parameterName, string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, ExceptionMessages.INVALID_PARAMETER, parameterName, value));
        }
        return guid;
    }

    /// <summary>
    /// Validates that the provided string can be parsed into a valid DateTime.
    /// </summary>
    /// <param name="parameterName">The name of the parameter being validated.</param>
    /// <param name="value">The string value to be validated as a DateTime.</param>
    /// <returns>The parsed DateTime value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provided string cannot be parsed into a valid DateTime.
    /// </exception>
    protected static DateTime ValidateDateTime(string parameterName, string value)
    {
        if (!DateTime.TryParse(value, out var date))
        {
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, ExceptionMessages.INVALID_PARAMETER, parameterName, value));
        }

        return date;
    }

    protected bool UsePowerFxPlugins(IEnvironmentVariableService environmentVariableService)
    {
        var value = environmentVariableService.RetrieveEnvironmentVariableValue("contoso_FCB_UsePowerFxPlugins");
        return value == "yes";
    }

    public override void RegisterServices(IServiceIOCContainer container)
    {
        // Register the services needed by the plugins
        // This is called by the base class PluginBase during the Execute method
        // The services provided by the plugin pipeline will be retrieved internally from
        // the plugin service provider and do not need to be registered
        container.Register<IEnvironmentVariableService, EnvironmentVariableService>();
    }
}
