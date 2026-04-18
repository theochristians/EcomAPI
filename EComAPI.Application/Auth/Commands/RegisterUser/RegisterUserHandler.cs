using EComAPI.Application.Auth.Common;
using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Auth.Commands.RegisterUser
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICartRepository _cartRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            ICartRepository cartRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _cartRepository = cartRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RegisteredUserDto>> Handle(
            RegisterUserCommand registerUserCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(registerUserCommand.Email))
                    return Result<RegisteredUserDto>.Failure("Email is required");

                if (string.IsNullOrWhiteSpace(registerUserCommand.Password))
                    return Result<RegisteredUserDto>.Failure("Password is required");

                if (registerUserCommand.Password.Length < 8)
                    return Result<RegisteredUserDto>.Failure("Password must be at least 8 characters");

                if (string.IsNullOrWhiteSpace(registerUserCommand.FullName))
                    return Result<RegisteredUserDto>.Failure("Full name is required");

                var role = await _roleRepository.GetRoleByNameAsync("Customer", cancellationToken);
                if (role is null)
                    return Result<RegisteredUserDto>.Failure("Default role not found");

                var emailAddress = EmailAddress.Create(registerUserCommand.Email);

                var emailExists = await _userRepository.ExistsUserAsync(emailAddress.Value, cancellationToken);
                if (emailExists)
                    return Result<RegisteredUserDto>.Failure("Email already registered");

                var hashedPassword = _passwordHasher.Hash(registerUserCommand.Password);
                var password = PasswordHash.FromHash(hashedPassword);

                var user = new User(
                    registerUserCommand.FullName,
                    emailAddress,
                    password,
                    role.Id,
                    SystemUsers.SystemUserId,
                    registerUserCommand.Phone
                );

                await _userRepository.AddUserAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var cart = new Cart(user.Id, SystemUsers.SystemUserId);
                await _cartRepository.AddCartAsync(cart, cancellationToken);

                var verificationCode = OtpCodeGenerator.GenerateSixDigits();
                var expiresAt = SecurityTime.UtcNow.AddMinutes(10);
                var emailVerification = new EmailVerification(
                    user.Id,
                    verificationCode,
                    expiresAt,
                    SystemUsers.SystemUserId);
                await _emailVerificationRepository.AddEmailVerificationAsync(emailVerification, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                try
                {
                    await _emailSender.SendEmailVerificationCodeAsync(
                        user.Email.Value,
                        user.FullName,
                        verificationCode,
                        expiresAt,
                        cancellationToken);
                }
                catch
                {
                    // User can request resend OTP later from public endpoint.
                }

                return Result<RegisteredUserDto>.Success(
                    new RegisteredUserDto(user.Id, user.FullName, user.Email.Value, user.IsEmailVerified));
            }
            catch (DomainException domainException)
            {
                return Result<RegisteredUserDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<RegisteredUserDto>.Failure($"Registration failed: {exception.Message}");
            }
        }
    }
}
