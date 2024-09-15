using ContosoRealEstate.BusinessLogic.Models;
using ContosoRealEstate.BusinessLogic.Plugins;
using ContosoRealEstate.BusinessLogic.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;

namespace ContosoRealEstate.BusinessLogic.Plugins;

/// <summary>
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
[CrmPluginRegistration(MessageNameEnum.Create, contoso_Reservation.EntityLogicalName, StageEnum.PreValidation, ExecutionModeEnum.Synchronous, "name", "ReservationOnCreatePreValidation", 1000, IsolationModeEnum.Sandbox)]
public class ReservationOnCreatePreValidation : BusinessLogicPluginBase, IPlugin
{
    public ReservationOnCreatePreValidation(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(ReservationOnCreatePreValidation), unsecureConfiguration, secureConfiguration)
    {
    }

    // Entry point for custom business logic execution
    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        if (localPluginContext == null)
        {
            throw new ArgumentNullException(nameof(localPluginContext));
        }

        var context = localPluginContext.PluginExecutionContext;
        var service = localPluginContext.ServiceProvider.Get<IOrganizationService>();
        var environmentVariableService = localPluginContext.ServiceProvider.Get<IEnvironmentVariableService>();

        if (UsePowerFxPlugins(environmentVariableService))
        {
            localPluginContext.Trace("PowerFx plugins are enabled. Skipping plugin execution.");
            return;
        }

        ValidatePluginExecutionContext(context, MessageNameEnum.Create, StageEnum.PreValidation, contoso_Reservation.EntityLogicalName);

        contoso_Reservation reservation = ((Entity)localPluginContext.PluginExecutionContext.InputParameters["Target"]).ToEntity<contoso_Reservation>() ?? throw new InvalidPluginExecutionException(ExceptionMessages.RESERVATION_NULL);
        try
        {
            // Call the IsListingAvailable custom api
            var isListingAvailableResponse = (contoso_IsListingAvailableAPIResponse)service.Execute(new contoso_IsListingAvailableAPIRequest
            {
                From = reservation.contoso_From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ListingID = reservation.contoso_Listing.Id.ToString(),
                To = reservation.contoso_To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ExcludeReservation = "None"
            });

            var isListingAvailable = isListingAvailableResponse.Available;
            if (!isListingAvailable)
            {
                throw new InvalidPluginExecutionException(ExceptionMessages.LISTING_NOT_AVAILABLE);
            }
            localPluginContext.Trace("Listing is available");

            // Validate that the To is after the from date
            if (reservation.contoso_To < reservation.contoso_From)
            {
                throw new InvalidPluginExecutionException(ExceptionMessages.TO_DATE_MUST_BE_AFTER_FROM_DATE);
            }

            localPluginContext.Trace("Reservation dates are valid");

            // Set the reservation date if not already
            if (reservation.contoso_ReservationDate == null)
            {
                localPluginContext.Trace("Setting Reservation Date");
                reservation.contoso_ReservationDate = DateTime.UtcNow; // TODO: Get contact's local time
            }

            // Set the name if not already set
            contoso_listing listing = service.Retrieve(
                contoso_listing.EntityLogicalName,
                reservation.contoso_Listing.Id,
                new ColumnSet(contoso_listing.Fields.contoso_name))
                .ToEntity<contoso_listing>();

            if (string.IsNullOrEmpty(reservation.contoso_Name))
            {
                localPluginContext.Trace("Setting Name");
                reservation.contoso_Name = $"{listing.contoso_name} - {reservation.contoso_From} - {reservation.contoso_To}";
            }
        }

        catch (Exception ex)
        {
            throw new InvalidPluginExecutionException(ex.Message);
        }
    }
}
