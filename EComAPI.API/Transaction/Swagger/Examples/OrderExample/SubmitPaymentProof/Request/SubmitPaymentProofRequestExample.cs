using EComAPI.API.Transaction.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Transaction.Swagger.Examples.OrderExample.SubmitPaymentProof.Request
{
    public class SubmitPaymentProofRequestExample : IExamplesProvider<SubmitPaymentProofRequest>
    {
        public SubmitPaymentProofRequest GetExamples()
            => new SubmitPaymentProofRequest(
                ProofImageUrl: "https://storage.example.com/payments/proof-123.jpg",
                PaymentMethod: "transfer_bca");
    }
}
