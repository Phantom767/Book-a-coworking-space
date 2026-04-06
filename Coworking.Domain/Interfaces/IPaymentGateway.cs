namespace CoworkingDomain.Interfaces;

public interface IPaymentGateway
{
    Task<bool> ProcessPaymentAsync(string userId, decimal amount, string paymentMethod);
}