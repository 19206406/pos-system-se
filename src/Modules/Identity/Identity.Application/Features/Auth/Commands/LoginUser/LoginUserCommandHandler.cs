using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.DTOs.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginUserResponseDto> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email);

            if (user is null)
                throw new NotFoundException("user", command.Email); 

            if (user.HashPassword is null)
                throw new BadRequestException("You cannot perform this action.");

            bool comparePasswords = _passwordHasher.Verify(command.Password, user.HashPassword);

            if (!comparePasswords)
                throw new UnauthorizedException("The credentials do not match. Please try again.");

            return new LoginUserResponseDto(true);
        }
    }
}
