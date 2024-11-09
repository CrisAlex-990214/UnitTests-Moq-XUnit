using CleanArchitecture.Application;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CleanArchitecture.UnitTests
{
    public class RegisterUserTests
    {
        private Mock<IUserRepo> userRepo;
        private Mock<IEmailService> emailService;
        private readonly UserService userService;

        public RegisterUserTests()
        {
            userRepo = new Mock<IUserRepo>();
            emailService = Mock.Get(Mock.Of<IEmailService>(x => x.TemplateId == 1));

            userService = new UserService(userRepo.Object, emailService.Object);
        }

        [Fact]
        public async Task ShouldNot_RegisterUser_ByValidationError()
        {
            //Arrange
            var user = new User { Username = string.Empty };

            //Act
            var response = await userService.RegisterUser(user);

            //Assert
            response.Messages.Should().Contain(ErrorConstants.UsernameNotProvided);

            userRepo.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Never);
            emailService.Verify(x => x.SendConfirmationEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ShouldNot_RegisterUser_ByDBError()
        {
            //Arrange
            var user = new User { Username = "Cris&Code" };
            userRepo.Setup(x => x.CreateUser(It.IsAny<User>())).ThrowsAsync(new Exception(ErrorConstants.DBTimeoutError));

            //Act
            var action = async () => { await userService.RegisterUser(user); };

            //Assert
            await action.Should().ThrowAsync<Exception>().WithMessage(ErrorConstants.DBTimeoutError);

            userRepo.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
            emailService.Verify(x => x.SendConfirmationEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ShouldNot_RegisterUser_ByConfirmationEmailError()
        {
            //Arrange
            var user = new User { Username = "Cris&Code", Email = "fid@fj" };
            userRepo.Setup(x => x.CreateUser(It.IsAny<User>())).ReturnsAsync(Guid.NewGuid());
            emailService.Setup(x => x.SendConfirmationEmail(It.IsRegex("\\S+@\\S+\\.\\S+"))).ReturnsAsync(true);

            //Act
            var response = await userService.RegisterUser(user);

            //Assert
            userRepo.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);

            emailService.Verify(x => x.SendConfirmationEmail(It.IsRegex("\\S+@\\S+\\.\\S+")), Times.Never);
            emailService.Verify(x => x.SendConfirmationEmail(user.Email), Times.Once);
            emailService.VerifyNoOtherCalls();
            emailService.Reset();
            emailService.VerifyAll();

            response.IsSuccess.Should().BeFalse();
            response.Messages.Should().Contain(ErrorConstants.ConfirmationEmailNotSent);
        }

        [Fact]
        public async Task Should_RegisterUser()
        {
            //Arrange
            var user = new User { Username = "Cris&Code", Email = "fid@fj.com" };
            userRepo.Setup(x => x.CreateUser(It.IsAny<User>())).ReturnsAsync(Guid.NewGuid());
            emailService.Setup(x => x.SendConfirmationEmail(It.IsRegex("\\S+@\\S+\\.\\S+"))).ReturnsAsync(true);

            //Act
            var response = await userService.RegisterUser(user);

            //Assert
            userRepo.Verify(x => x.CreateUser(It.IsAny<User>()), Times.AtLeastOnce);

            emailService.Verify(x => x.SendConfirmationEmail(It.IsRegex("\\S+@\\S+\\.\\S+")), Times.Once);
            emailService.VerifyNoOtherCalls();
            emailService.Reset();
            emailService.VerifyAll();

            response.IsSuccess.Should().BeTrue();
            response.Value.Should().NotBeEmpty();
        }
    }
}