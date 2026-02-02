using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Results;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;

namespace EComAPI.Application.Auth.Commands.RegisterUser
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<RegisteredUserDto>> Handle(RegisterUserCommand command)
        {
            if (await _userRepository.ExistsAsync(command.Email))
                return Result<RegisteredUserDto>.Failure("Email already registered");

            var role = await _roleRepository.GetByNameAsync("Customer");
            if (role is null)
                return Result<RegisteredUserDto>.Failure("Default role not found");

            var email = EmailAddress.Create(command.Email);

            var hashedPassword = _passwordHasher.Hash(command.Password);
            var password = PasswordHash.FromHash(hashedPassword);

            var user = new User(
                command.FullName,
                email,
                password,
                role.Id
            );

            await _userRepository.AddAsync(user);

            return Result<RegisteredUserDto>.Success(
                new RegisteredUserDto(
                    user.Id,
                    user.FullName,
                    user.Email.Value,
                    user.IsEmailVerified
                    ) );
        }
    }
}