using MebelMaster.Models;

namespace MebelMaster.Services
{
    public interface IEmailService
    {
        Task SendOrderNotificationAsync(Order order);
        Task SendContactFormAsync(string name, string email, string message);
    }
}