using Contoso;
using Microsoft.Xrm.Sdk;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;

namespace PaymentVirtualTableProvider.Services.PaymentApi.Mappers;

public static class PaymentEntityMapper
{
    public static contoso_payment ToEntity(this Payment apiPayment)
    {

        var fieldMapper = new PaymentFieldValueMapper();

        var reservationId = fieldMapper.MapToDataverse("reservationid", apiPayment.reservationId);
        var userId = fieldMapper.MapToDataverse("userid", apiPayment.userId);
        contoso_payment paymentEntity = new contoso_payment
        {
            contoso_name = apiPayment.id.ToString(),
            contoso_paymentId = apiPayment.id.ToGuid(),
            contoso_Reservation = reservationId.Value as EntityReference,
            contoso_UserId = apiPayment.userId,
            contoso_PortalUser = userId.Value as EntityReference,
            contoso_Provider = (contoso_payment_contoso_provider)apiPayment.provider,
            contoso_Status = (contoso_payment_contoso_status)apiPayment.status,
            contoso_Amount = apiPayment.amount,
            contoso_CurrencyCode = apiPayment.currency,
            contoso_CreatedOn = apiPayment.createdAt
        };

        return paymentEntity;
    }

    public static Payment ToApi(this contoso_payment payment)
    {
        var fieldMapper = new PaymentFieldValueMapper();

        var apiPayment = new Payment
        {
            userId = fieldMapper.MapToApi("contoso_userid",payment.contoso_UserId).Value as string,
            reservationId = fieldMapper.MapToApi("contoso_reservation", payment.contoso_Reservation).Value as string,
            provider = (int)payment.contoso_Provider,
            status = (int)payment.contoso_Status,
            amount = payment.contoso_Amount.Value,
            currency = payment.contoso_CurrencyCode,
            createdAt = payment.contoso_CreatedOn.Value
        };

        return apiPayment;
    }
}
