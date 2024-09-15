using ContosoRealEstate.BusinessLogic.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text.Json;

namespace ContosoRealEstate.BusinessLogic.Plugins;

/// <summary>
/// Base class for all plug-in classes.
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
public abstract class PluginBase : IPlugin
{
    protected string PluginClassName { get; }
    protected string UnsecureConfiguration { get; }
    protected string SecureConfiguration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginBase"/> class.
    /// </summary>
    /// <param name="pluginClassName">The <see cref=" cred="Type"/> of the plugin class.</param>
    internal PluginBase(Type pluginClassName, string unsecureConfiguration, string secureConfiguration)
    {
        PluginClassName = pluginClassName.ToString();
        UnsecureConfiguration = unsecureConfiguration;
        SecureConfiguration = secureConfiguration;
    }

    /// <summary>
    /// Main entry point for he business logic that the plug-in is to execute.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <remarks>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Execute")]
    public void Execute(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider));
        }

        // Construct the local plug-in context.
        var localPluginContext = new LocalPluginContext(serviceProvider);
        RegisterServices(localPluginContext.ServiceProvider as IServiceIOCContainer);

        localPluginContext.Trace($"Entered {PluginClassName}.Execute() " +
            $"Correlation Id: {localPluginContext.PluginExecutionContext.CorrelationId}, " +
            $"Initiating User: {localPluginContext.PluginExecutionContext.InitiatingUserId}, " +
            $"Config :{UnsecureConfiguration} - {SecureConfiguration}");

        try
        {
            // Trace a serialised version of the InputParameters
            localPluginContext.Trace("InputParameters: " + JsonSerializer.Serialize(localPluginContext.PluginExecutionContext.InputParameters));
            localPluginContext.Trace("SharedVariable: " + JsonSerializer.Serialize(localPluginContext.PluginExecutionContext.SharedVariables));
            localPluginContext.Trace("PreEntityImages: " + JsonSerializer.Serialize(localPluginContext.PluginExecutionContext.PreEntityImages));
            localPluginContext.Trace("ParentContext: " + JsonSerializer.Serialize(localPluginContext.PluginExecutionContext.ParentContext));


            // Invoke the custom implementation
            ExecuteDataversePlugin(localPluginContext);
            localPluginContext.Trace($"Completed");
            // Now exit - if the derived plugin has incorrectly registered overlapping event registrations, guard against multiple executions.
            return;
        }
        catch (FaultException<OrganizationServiceFault> orgServiceFault)
        {
            localPluginContext.Trace($"Exception");
            localPluginContext.Trace($"Exception: {orgServiceFault}");

            throw new InvalidPluginExecutionException($"OrganizationServiceFault: {orgServiceFault.Message}", orgServiceFault);
        }
        catch (Exception ex)
        {
            localPluginContext.TraceException($"{PluginClassName}.Execute() failed", ex);
            throw;
        }
        finally
        {
            localPluginContext.Trace($"Exiting {PluginClassName}.Execute()");
        }
    }

    /// <summary>
    /// Placeholder for a custom plug-in implementation.
    /// </summary>
    /// <param name="localPluginContext">Context for the current plug-in.</param>
    public virtual void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        // Do nothing.
    }

    /// <summary>
    /// Override this method to add custom services to the container.
    /// </summary>
    public virtual void RegisterServices(IServiceIOCContainer container)
    {
        // Do nothing.
    }
}

/// <summary>
/// This interface provides an abstraction on top of IServiceProvider for commonly used PowerPlatform Dataverse Plugin development constructs
/// </summary>
public interface ILocalPluginContext
{
    /// <summary>
    /// IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
    /// </summary>
    IPluginExecutionContext PluginExecutionContext { get; }

    /// <summary>
    /// Provides logging run-time trace information for plug-ins.
    /// </summary>
    ITracingService TracingService { get; }

    /// <summary>
    /// General Service Provide for things not accounted for in the base class.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// ILogger for this plugin.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// Writes a trace message to the trace log.
    /// </summary>
    /// <param name="message">Message name to trace.</param>
    void Trace(string message, [CallerMemberName] string method = null);

    void TraceException(string message, Exception ex, [CallerMemberName] string method = null);
}

/// <summary>
/// Plug-in context object.
/// </summary>
public class LocalPluginContext : ILocalPluginContext
{
    /// <summary>
    /// IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
    /// </summary>
    public IPluginExecutionContext PluginExecutionContext { get; }

    /// <summary>
    /// Provides logging run-time trace information for plug-ins.
    /// </summary>
    public ITracingService TracingService { get; }

    /// <summary>
    /// General Service Provider for things not accounted for in the base class.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Used to register services for the plugin.
    /// Services that are specific to a single plugin are added in the overridden RegisterServices method.
    /// </summary>
    private IServiceIOCContainer ServiceContainer { get; }

    /// <summary>
    /// ILogger for this plugin.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Helper object that stores the services available in this plug-in.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public LocalPluginContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new InvalidPluginExecutionException(nameof(serviceProvider));

        Logger = serviceProvider.Get<ILogger>();

        PluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();

        TracingService = new LocalTracingService(serviceProvider);

        ServiceContainer = new ServiceIOCContainer(serviceProvider);
        ServiceContainer.Register<ITracingService, ITracingService>(TracingService);
        ServiceContainer.Register<IExecutionContext, IExecutionContext>(PluginExecutionContext);

        // Add a factory functions to the ServiceContainer to create the IOrganizationService
        ServiceContainer.Register(serviceProvider => {
            // Get the Organization Service for the System User
            return serviceProvider.Get<IOrganizationServiceFactory>().CreateOrganizationService(null);
        });

        ServiceContainer.Register(serviceProvider => {
            // Get the Organization Service for the Initiating User
            return serviceProvider.GetOrganizationService(PluginExecutionContext.InitiatingUserId);
        }, "InitiatingUserId");

        ServiceContainer.Register(serviceProvider => {
            // Get the Organization Service for the Plugin User
            return serviceProvider.GetOrganizationService(PluginExecutionContext.UserId);
        }, "UserId");

        ServiceContainer.Register(serviceProvider =>
        {
            // Get the Service Endpoint Notification Service
            return serviceProvider.Get<IServiceEndpointNotificationService>();
        });

        ServiceProvider = ServiceContainer as IServiceProvider;

    }

    /// <summary>
    /// Writes a trace message to the trace log.
    /// </summary>
    /// <param name="message">Message name to trace.</param>
    public void Trace(string message, [CallerMemberName] string method = null)
    {
        if (string.IsNullOrWhiteSpace(message) || TracingService == null)
        {
            return;
        }

        if (method != null)
            TracingService.Trace($"[{method}] - {message}");
        else
            TracingService.Trace($"{message}");
    }

    public void TraceException(string message, Exception ex, [CallerMemberName] string method = null)
    {
        if (ex == null)
        {
            throw new ArgumentNullException(nameof(ex));
        }

        var formattedException = FormatException(ex);
        Trace($"{message} Exception: {formattedException}", method);
    }

    private static string FormatException(Exception ex, int recursionCount = 0)
    {
        var message = ex.Message;
        if (message != null && message.Length > 500)
        {
            message = message.Substring(0, 500) + "...";
        }
        var formattedException = $"{ex.GetType().FullName}: {message}";

        if (recursionCount < 3)
        {
            if (ex.InnerException != null)
            {
                formattedException += Environment.NewLine + FormatException(ex.InnerException, recursionCount + 1);
            }

            if (ex.StackTrace != null)
            {
                var stackTrace = ex.StackTrace;
                // Truncate the StackTrace to 500 chars if there is any content
                if (stackTrace != null && stackTrace.Length > 500)
                {
                    stackTrace = stackTrace.Substring(0, 500) + "...";
                }

                formattedException += Environment.NewLine + "Stack Trace:" + Environment.NewLine + stackTrace;
            }
        }

        return formattedException;
    }
}

/// <summary>
/// Specialized ITracingService implementation that prefixes all traced messages with a time delta for Plugin performance diagnostics
/// </summary>
public class LocalTracingService : ITracingService
{
    private readonly ITracingService _tracingService;

    private DateTime _previousTraceTime;

    public LocalTracingService(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        DateTime utcNow = DateTime.UtcNow;

        var context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));

        DateTime initialTimestamp = context.OperationCreatedOn;

        if (initialTimestamp > utcNow)
        {
            initialTimestamp = utcNow;
        }

        _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        _previousTraceTime = initialTimestamp;
    }

    public void Trace(string message, params object[] args)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var utcNow = DateTime.UtcNow;

        // The duration since the last trace.
        var deltaMilliseconds = utcNow.Subtract(_previousTraceTime).TotalMilliseconds;
        _tracingService.Trace($"[+{deltaMilliseconds:N0}ms] - {string.Format(CultureInfo.InvariantCulture, message.Replace("{", "{{").Replace("}", "}}"), args)}");

        _previousTraceTime = utcNow;
    }
}
