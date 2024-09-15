using Microsoft.Xrm.Sdk;
using PaymentVirtualTableProvider.QueryExpressionToRest;
using System;
using System.Collections.Generic;

namespace PaymentVirtualTableProvider.Services.PaymentApi.Mappers;

public class PaymentFieldValueMapper : IFieldValueMapper
{
    private static readonly Dictionary<string, string> FieldMappings = new Dictionary<string, string>
        {
            { "contoso_userid", "UserId" },
            { "contoso_reservation", "ReservationId" },
            { "contoso_createdon", "CreatedAt" },
            { "contoso_provider", "Provider" },
            { "contoso_status", "Status" },
            { "contoso_amount", "Amount" },
            { "contoso_currencycode", "Currency" },
            { "contoso_portaluser", "UserId" }

        };

    public KeyValuePair<string, object> MapToApi(string fieldName, object value)
    {

        // Map the field names using the FieldMappings dictionary
        if (FieldMappings.ContainsKey(fieldName.ToLower()))
        {
            fieldName = FieldMappings[fieldName.ToLower()];
        }

        // If the type of value = EntityReference then update to be a string with the prefix
        var id = String.Empty;
        if (value is EntityReference)
        {
            id = (value as EntityReference).Id.ToString("D");
        }
        if (value is Guid?)
        {
            id = ((Guid?)value)?.ToString("D");
        }

        // Map the special lookup values
        switch (fieldName.ToLower())
        {
            case "userid":
                value = $"portal-{id}";
                break;
            case "reservationid":
                value = $"portal-reservation-{id}";
                break;
        }


        return new KeyValuePair<string, object>(fieldName, value);
    }

    public KeyValuePair<string, object> MapToDataverse(string fieldName, object value)
    {
        if (value == null)
        {
            return new KeyValuePair<string, object>(fieldName, null);
        }
        var valueString = value.ToString();
        switch (fieldName.ToLower())
        {
            case "userid":
                if (valueString.StartsWith("portal-"))
                {
                    value = new EntityReference("contact", new Guid(valueString.Replace("portal-", "")));
                }
                else value = null;
                break;
            case "reservationid":
                if (valueString.StartsWith("portal-reservation-"))
                {
                    value = new EntityReference("contoso_reservation", new Guid(valueString.Replace("portal-reservation-", "")));
                }
                else value = null;
                break;
        }

        return new KeyValuePair<string, object>(fieldName, value);
    }
}
