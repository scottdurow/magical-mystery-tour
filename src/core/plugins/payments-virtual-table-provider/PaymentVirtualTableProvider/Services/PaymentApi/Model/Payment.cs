using System;

namespace PaymentVirtualTableProvider.Services.PaymentApi.Model;

public class Payment
{
public int id { get; set; }
public string userId { get; set; }
public string reservationId { get; set; }
public int provider { get; set; }
public int status { get; set; }
public decimal amount { get; set; }
public string currency { get; set; }
public DateTime createdAt { get; set; }
}
