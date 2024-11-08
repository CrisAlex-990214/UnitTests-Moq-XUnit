using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application
{
    public class UserService(IUserRepo userRepo, IEmailService emailService)
    {
        private readonly IUserRepo userRepo = userRepo;
        private readonly IEmailService emailService = emailService;

        public async Task<Result<Guid>> RegisterUser(User user)
        {
            //Validation
            if (string.IsNullOrEmpty(user.Username))
                return new() { Messages = [ErrorConstants.UsernameNotProvided] };

            //Database Call
            var userId = await userRepo.CreateUser(user);

            //Email Confirmation
            var emailSent = await emailService.SendConfirmationEmail(user.Email);
            if (!emailSent) return new() { Messages = [ErrorConstants.ConfirmationEmailNotSent] };

            return new() { IsSuccess = true, Value = userId };
        }
    }
}
