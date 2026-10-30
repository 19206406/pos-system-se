using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Contracts.Services;
using Identity.Application.Dtos.Authentication;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    public sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IAuthSessionService _sessionService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginUserCommandHandler(
            IUserRepository userRepository, 
            ISessionRepository sessionRepository, 
            IAuthSessionService sessionService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _sessionService = sessionService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginUserResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", command.Email); 

            if (user.HashPassword is null)
                throw new BadRequestException("You cannot perform this action");

            bool comparePasswords = _passwordHasher.Verify(command.Password, user.HashPassword);
            if (!comparePasswords)
                throw new UnauthorizedException("The credentials do not match. Please try again");

            var activeSessions = await _sessionRepository.CountAllActiveByUserAsync(user.Id, cancellationToken);
            if (activeSessions > 3)
                throw new ForbiddenException("You cannot have more than three open sessions"); 

            var client = new ClientContext("not delivered", "not delivered");
            var auth =  await _sessionService.CreateSessionAsync(user, client, cancellationToken); 

            return new LoginUserResponse(
                user.FullName,
                user.Email,
                auth.AccessToken,
                auth.AccessTokenExpiresAt, 
                auth.RefreshToken, 
                auth.RefreshTokenExpiresAt,
                true);
        }
    }
}
