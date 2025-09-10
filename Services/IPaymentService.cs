using System.Threading.Tasks;
using dotnet_store.Models;

namespace dotnet_store.Services;

public interface IPaymentService
{
    Task<(bool Success, string? ErrorMessage, PaymentInfo? Info)> ChargeOrderAsync(
        Order order,
        AppUser user,
        string cardHolderName,
        string cardNumber,
        string expireMonth,
        string expireYear,
        string cvc,
        string? remoteIpAddress
    );
}

public record PaymentInfo(
    string? PaymentId,
    string? ConversationId,
    string? Status,
    string? AuthCode,
    string? HostReference
);


