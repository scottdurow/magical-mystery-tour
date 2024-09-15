using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentVirtualTableProvider.Services.PaymentApi;

public interface IPaymentsApiService
{
Task<List<Payment>> GetPayments(string query);

Task<Payment> GetPayment(int id);
Task<int> Create(Payment payment);
}
