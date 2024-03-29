namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task SendResetPasswordEmail(string resetEmailCode, string toEmail);
    }
}
