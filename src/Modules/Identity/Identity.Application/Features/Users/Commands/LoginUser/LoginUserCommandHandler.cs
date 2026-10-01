using Identity.Application.Contracts.Persistence;
using Identity.Application.DTOs.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Commands.LoginUser
{
    public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResponseDto>
    {
        private readonly IUserRepository _userRepository;

        public LoginUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginUserResponseDto> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmail(command.Email);

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
