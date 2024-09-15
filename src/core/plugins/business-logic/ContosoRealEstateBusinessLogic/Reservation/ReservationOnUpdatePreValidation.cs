using ContosoRealEstate.BusinessLogic.Models;
using ContosoRealEstate.BusinessLogic.Plugins;
using ContosoRealEstate.BusinessLogic.Resources;
using ContosoRealEstate.BusinessLogic.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ContosoRealEstate.BusinessLogic.Plugins;

/// <summary>
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
[CrmPluginRegistration(MessageNameEnum.Update, contoso_Reservation.EntityLogicalName, StageEnum.PreValidation, ExecutionModeEnum.Synchronous, "name", "ReservationOnCreatePreValidation", 1000, IsolationModeEnum.Sandbox)]
public class ReservationOnUpdatePreValidation : BusinessLogicPluginBase, IPlugin
{
    public ReservationOnUpdatePreValidation(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(ReservationOnUpdatePreValidation), unsecureConfiguration, secureConfiguration)
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

        ValidatePluginExecutionContext(context, MessageNameEnum.Update, StageEnum.PreValidation, contoso_Reservation.EntityLogicalName);
        
        // Get the pre-image
        var preImage = GetPreImage(localPluginContext.PluginExecutionContext);

        contoso_Reservation oldReservation = preImage.ToEntity<contoso_Reservation>();
        contoso_Reservation newReservation = ((Entity)localPluginContext.PluginExecutionContext.InputParameters["Target"]).ToEntity<contoso_Reservation>();

        // Prevent changing the listing associated
        if (newReservation.contoso_Listing != null &&
            oldReservation.contoso_Listing.Id != newReservation.contoso_Listing.Id)
        {
            throw new InvalidPluginExecutionException(ExceptionMessages.LISTING_CANNOT_BE_CHANGED);
        }

        var from = newReservation.contoso_From ?? oldReservation.contoso_From;
        var to = newReservation.contoso_To ?? oldReservation.contoso_To;

        try
        {
            // Check if the listing is available
            var isListingAvailableResponse = (contoso_IsListingAvailableAPIResponse)service.Execute(new contoso_IsListingAvailableAPIRequest
            {
                From = from.ToString(),
                ListingID = oldReservation.contoso_Listing.Id.ToString(),
                To = to.ToString(),
                ExcludeReservation = oldReservation.contoso_ReservationId.ToString()
            });

            var isListingAvailable = isListingAvailableResponse.Available;
            if (!isListingAvailable)
            {
                localPluginContext.Trace(ExceptionMessages.LISTING_NOT_AVAILABLE);
                throw new InvalidPluginExecutionException(ExceptionMessages.LISTING_NOT_AVAILABLE);
            }

            // Validate that the To is after the from date
            if (to < from)
            {
                localPluginContext.Trace(ExceptionMessages.TO_DATE_MUST_BE_AFTER_FROM_DATE);
                throw new InvalidPluginExecutionException(ExceptionMessages.TO_DATE_MUST_BE_AFTER_FROM_DATE);
            }


            // Set the name if not already set
            contoso_listing listing = service.Retrieve(
                contoso_listing.EntityLogicalName,
                oldReservation.contoso_Listing.Id,
                new ColumnSet(contoso_listing.Fields.contoso_name))
                .ToEntity<contoso_listing>();

            if (oldReservation.contoso_From != newReservation.contoso_From ||
                oldReservation.contoso_To != newReservation.contoso_To)
            {
                localPluginContext.Trace("Setting Name");
                newReservation.contoso_Name = $"{listing.contoso_name} - {from} - {to}";
            }
        }

        catch (Exception ex)
        {
            throw new InvalidPluginExecutionException(ex.Message);
        }
    }
}
