namespace CleanArchitecture.Application.Interfaces
{
    public interface IEmailService
    {
        int TemplateId { get; set; }
        Task<bool> SendConfirmationEmail(string email);
        Task<bool> SendResetPasswordEmail(string email);
    }
}
