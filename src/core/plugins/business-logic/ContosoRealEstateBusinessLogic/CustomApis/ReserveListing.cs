using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using ContosoRealEstate.BusinessLogic.Models;
using System;
using System.Linq;
using ContosoRealEstate.BusinessLogic.Resources;
using ContosoRealEstate.BusinessLogic.Plugins;
using System.Globalization;

namespace ContosoRealEstate.BusinessLogic.Plugins;

/// <summary>
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
[CrmPluginRegistration("contoso_ReserveListingApi")]
public class ReserveListing : BusinessLogicPluginBase, IPlugin
{
    public ReserveListing(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(ReserveListing), unsecureConfiguration, secureConfiguration)
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

        ValidateCustomApiExectionContext(context, "contoso_ReserveListingApi");
        try
        {

            var request = MapInputParametersToRequest(context.InputParameters);

            // Lock the listing
            var sessionId = Guid.NewGuid().ToString();
            contoso_listing listing = (contoso_listing)service.Retrieve(
                contoso_listing.EntityLogicalName,
                new Guid(request.ListingID),
                new ColumnSet(contoso_listing.Fields.contoso_listingId, contoso_listing.Fields.contoso_Lock, contoso_listing.Fields.contoso_pricepermonth));

            listing.contoso_Lock = sessionId;
            service.Update(listing);

            localPluginContext.Trace($"Listing locked with session id: {sessionId}");

            // Call the IsListingAvailable custom api
            var isListingAvailableResponse = (contoso_IsListingAvailableAPIResponse)service.Execute(new contoso_IsListingAvailableAPIRequest
            {
                From = request.From,
                ListingID = request.ListingID,
                To = request.To,
                ExcludeReservation = "None"
            });

            var isListingAvailable = isListingAvailableResponse.Available;

            if (!isListingAvailable)
            {
                throw new InvalidPluginExecutionException(ExceptionMessages.LISTING_NOT_AVAILABLE);
            }

            // Calculate the total by summing the fees
            var fromDate = DateTime.Parse(request.From, CultureInfo.InvariantCulture);
            var toDate = DateTime.Parse(request.To, CultureInfo.InvariantCulture);
            TimeSpan dateDiff = toDate - fromDate;
            decimal nights = (decimal)dateDiff.TotalDays;
            decimal monthsFraction = nights / 30;

            /*
            // Calculate the total listing fees per day
            var totalFeesPerDay = (from fee in serviceContext.contoso_ListingFeeSet
                                   where fee.contoso_Listing.Id == listingIdGuid &&
                                    fee.contoso_PerGuest == false
                                   select fee.contoso_FeeAmount.Value).ToList().Sum();
            // Calculate the total listing fees per day per guest                 
            var totalFeesPerDayPerGuest = (from fee in serviceContext.contoso_ListingFeeSet
                                   where fee.contoso_Listing.Id == listingIdGuid &&
                                    fee.contoso_PerGuest == true
                                   select fee.contoso_FeeAmount.Value).ToList().Sum();
            */

            // Calculate the total listing fees per day
            var listingId = new Guid(request.ListingID);
            var totalFeesPerDayQuery = new QueryExpression(contoso_ListingFee.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(contoso_ListingFee.Fields.contoso_FeeAmount),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(contoso_ListingFee.Fields.contoso_Listing, ConditionOperator.Equal, listingId),
                        new ConditionExpression(contoso_ListingFee.Fields.contoso_PerGuest, ConditionOperator.Equal, false)
                    }
                }
            };

            var totalFeesPerDayEntities = service.RetrieveMultiple(totalFeesPerDayQuery).Entities;
            var totalFeesPerDay = totalFeesPerDayEntities.Sum(e => ((Money)e[contoso_ListingFee.Fields.contoso_FeeAmount]).Value);

            // Calculate the total listing fees per day per guest
            var totalFeesPerDayPerGuestQuery = new QueryExpression(contoso_ListingFee.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(contoso_ListingFee.Fields.contoso_FeeAmount),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(contoso_ListingFee.Fields.contoso_Listing, ConditionOperator.Equal, listingId),
                        new ConditionExpression(contoso_ListingFee.Fields.contoso_PerGuest, ConditionOperator.Equal, true)
                    }
                }
            };

            var totalFeesPerDayPerGuestEntities = service.RetrieveMultiple(totalFeesPerDayPerGuestQuery).Entities;
            var totalFeesPerDayPerGuest = totalFeesPerDayPerGuestEntities.Sum(e => ((Money)e[contoso_ListingFee.Fields.contoso_FeeAmount]).Value);

            decimal totalFees =
                listing.contoso_pricepermonth.Value * monthsFraction +
                totalFeesPerDay * nights +
                totalFeesPerDayPerGuest * nights * request.Guests;

            localPluginContext.Trace($"totalFeesPerDayPerGuest: {totalFeesPerDayPerGuest}");
            localPluginContext.Trace($"totalFeesPerDay: {totalFeesPerDay}");
            localPluginContext.Trace($"totalFees: {totalFees}");

            // Create a reservation
            var reservation = new contoso_Reservation
            {
                contoso_Name = "",
                contoso_SessionID = sessionId,
                contoso_Customer = new EntityReference(Contact.EntityLogicalName, new Guid(request.DataverseUserId)),
                contoso_ReservationDate = DateTime.Now,
                contoso_Listing = new EntityReference(contoso_listing.EntityLogicalName, listingId),
                contoso_From = fromDate,
                contoso_To = toDate,
                contoso_Guests = request.Guests,
                contoso_Amount = new Money(totalFees),
                contoso_ReservationStatus = contoso_reservationstatus.Checkout

            };

            var reservationId = service.Create(reservation);
            localPluginContext.Trace($"ReservationId: {reservationId}");

            contoso_Reservation createdReservation = service.Retrieve(
                contoso_Reservation.EntityLogicalName,
                reservationId,
                new ColumnSet(contoso_Reservation.Fields.contoso_ReservationNumber))
                .ToEntity<contoso_Reservation>();

            // set the output parameter            
            localPluginContext.Trace(
                "Output Parameters\n" +
                "------------------\n" +
                $"ReservationNumber: {createdReservation.contoso_ReservationNumber}");

            context.OutputParameters["ReservationNumber"] = createdReservation.contoso_ReservationNumber;

        }
        catch (Exception ex)
        {
            localPluginContext.Trace($"Error: {ex.Message}");
            throw new InvalidPluginExecutionException(ex.Message);
        }
    }

    private static contoso_ReserveListingApiRequest MapInputParametersToRequest(ParameterCollection inputs)
    {
        // Map the keys from the inputs to create a new contoso_ReserveListingApiRequest
        var request = new contoso_ReserveListingApiRequest();

        // Use TryGetValue to safely retrieve values from the inputs 
        if (inputs.TryGetValue<string>("From", out var fromValue)) request.From = fromValue;
        if (inputs.TryGetValue<string>("To", out var toValue)) request.To = toValue;
        if (inputs.TryGetValue<string>("ListingID", out var listingIDValue)) request.ListingID = listingIDValue;
        if (inputs.TryGetValue<string>("DataverseUserId", out var dataverseUserIdValue)) request.DataverseUserId = dataverseUserIdValue;
        if (inputs.TryGetValue<int>("Guests", out var guestsValue)) request.Guests = guestsValue;

        // Check that ListingID, From, To, DataverseUserId are populated
        if (string.IsNullOrEmpty(request.ListingID) || string.IsNullOrEmpty(request.From) || string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.DataverseUserId))
        {
            throw new InvalidPluginExecutionException(ExceptionMessages.MISSING_INPUT_PARAMETERS);
        }

        // Check that ListingID, From, To are set
        if (string.IsNullOrEmpty(request.ListingID) || string.IsNullOrEmpty(request.From) || string.IsNullOrEmpty(request.To))
        {
            throw new InvalidPluginExecutionException(ExceptionMessages.MISSING_INPUT_PARAMETERS);
        }

        ValidateGuid("DataverseUserId", request.DataverseUserId);

        return request;
    }
}
