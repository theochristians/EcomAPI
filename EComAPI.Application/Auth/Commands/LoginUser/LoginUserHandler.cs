using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Results;

namespace EComAPI.Application.Auth.Commands.LoginUser
{
    public class LoginUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<string>> Handle(LoginUserCommand command)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email);

            if (user is null)
                return Result<string>.Failure("Invalid credentials");

            if (!_passwordHasher.Verify(command.Password, user.Password.Value))
                return Result<string>.Failure("Invalid credentials");

            var token = _jwtTokenGenerator.GenerateToken(user);

            return Result<string>.Success(token);
        }
    }
}