using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.RegisterUser
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
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
