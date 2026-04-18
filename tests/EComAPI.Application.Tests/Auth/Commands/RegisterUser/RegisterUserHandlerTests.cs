using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Auth.Commands.RegisterUser;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Tests.Auth.Commands.RegisterUser
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IEmailVerificationRepository> _mockEmailVerificationRepository;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RegisterUserHandler _registerUserHandler; 

        public RegisterUserHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockCartRepository = new Mock<ICartRepository>();
            _mockEmailVerificationRepository = new Mock<IEmailVerificationRepository>();
            _mockEmailSender = new Mock<IEmailSender>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            
            _registerUserHandler = new RegisterUserHandler(
                _mockUserRepository.Object,
                _mockRoleRepository.Object,
                _mockPasswordHasher.Object,
                _mockCartRepository.Object,
                _mockEmailVerificationRepository.Object,
                _mockEmailSender.Object,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public async Task Handle_EmailKosong_ShouldReturnFailure()
        {
            var registerUserCommand = new RegisterUserCommand(
                FullName: "John Doe",
                Email: "", 
                Password: "password123"
            );

            var registerUserResult = await _registerUserHandler.Handle(registerUserCommand);

            registerUserResult.IsSuccess.Should().BeFalse(); 
            registerUserResult.Error.Should().Be("Email is required"); 
        }

        [Fact]
        public async Task Handle_PasswordKosong_ShouldReturnFailure()
        {
            var registerUserCommand = new RegisterUserCommand(
                FullName: "John Doe",
                Email: "john@example.com",
                Password: "" 
            );

            var registerUserResult = await _registerUserHandler.Handle(registerUserCommand);

            registerUserResult.IsSuccess.Should().BeFalse();
            registerUserResult.Error.Should().Be("Password is required");
        }

        [Fact]
        public async Task Handle_PasswordTerlaluPendek_ShouldReturnFailure()
        {
            var registerUserCommand = new RegisterUserCommand(
                FullName: "John Doe",
                Email: "john@example.com", 
                Password: "123" 
            );

            var registerUserResult = await _registerUserHandler.Handle(registerUserCommand);

            registerUserResult.IsSuccess.Should().BeFalse();
            registerUserResult.Error.Should().Be("Password must be at least 8 characters");
        }

        [Fact] 
        public async Task Handle_FullNameKosong_ShouldReturnFailure()
        {
            var registerUserCommand = new RegisterUserCommand(
                FullName: "", 
                Email: "john@example.com",
                Password: "password123"
            );

            var registerUserResult = await _registerUserHandler.Handle(registerUserCommand);

            registerUserResult.IsSuccess.Should().BeFalse();
            registerUserResult.Error.Should().Be("Full name is required");
        }

        [Fact]
        public async Task Handle_RoleNotFound_ShouldReturnFailure()
        {
            var registerUserCommand = new RegisterUserCommand(
                FullName: "John Doe",
                Email: "john@example.com", 
                Password: "password123"
            );

            _mockRoleRepository
                .Setup(x => x.GetRoleByNameAsync("Customer", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null); 

            var registerUserResult = await _registerUserHandler.Handle(registerUserCommand);

            registerUserResult.IsSuccess.Should().BeFalse();
            registerUserResult.Error.Should().Be("Default role not found");
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldReturnSuccess()
        {
            var registerUserCommand = new RegisterUserCommand(
                FullName: "John Doe",
                Email: "john@example.com",
                Password: "password123"
            );

            var customerRole = new Role("Customer", Guid.NewGuid());
            _mockRoleRepository
                .Setup(x => x.GetRoleByNameAsync("Customer", It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerRole); 

            _mockUserRepository
                .Setup(x => x.ExistsUserAsync(registerUserCommand.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); 

            _mockPasswordHasher
                .Setup(x => x.Hash(registerUserCommand.Password))
                .Returns("hashedPassword123"); 

            var registerUserResult = await _registerUserHandler.Handle(registerUserCommand);

            registerUserResult.IsSuccess.Should().BeTrue();
            registerUserResult.Value.Should().NotBeNull();
            registerUserResult.Value!.Email.Should().Be(registerUserCommand.Email.ToLowerInvariant());
            registerUserResult.Value.FullName.Should().Be(registerUserCommand.FullName);

            _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEmailVerificationRepository.Verify(
                x => x.AddEmailVerificationAsync(It.IsAny<EmailVerification>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockEmailSender.Verify(
                x => x.SendEmailVerificationCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_EmailSudahTerdaftar_ShouldReturnFailure()
        {
            var registerUserCommand = new RegisterUserCommand(
                FullName: "John Doe",
                Email: "existing@example.com",
                Password: "password123"
            );

            var customerRole = new Role("Customer", Guid.NewGuid());
            _mockRoleRepository
                .Setup(x => x.GetRoleByNameAsync("Customer", It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerRole);

            _mockUserRepository
                .Setup(x => x.ExistsUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // ❌ Email sudah ada

            var registerUserResult = await _registerUserHandler.Handle(registerUserCommand);

            registerUserResult.IsSuccess.Should().BeFalse();
            registerUserResult.Error.Should().Be("Email already registered"); // Dari kode actual!
        }
    }
}
