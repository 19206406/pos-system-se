using Identity.Application.Common;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    public sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResponse>
    {
        // Same message for every cause: unknown email, no password yet, inactive account or wrong password
        private const string InvalidCredentialsMessage = "The email or password is incorrect.";

        private readonly IUserRepository _userRepository;
        private readonly IAuthSessionService _sessionService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IAuthSessionService sessionService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginUserResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(EmailNormalizer.Normalize(command.Email), cancellationToken);

            // Verify runs even without a user or a stored hash (against a dummy hash),
            // so the response time does not reveal whether the email exists
            var passwordMatches = _passwordHasher.Verify(command.Password, user?.HashPassword);

            if (user is null || !user.IsActive || !passwordMatches)
                throw new UnauthorizedException(InvalidCredentialsMessage);

            var tokens = await _sessionService.CreateSessionAsync(user, command.Client, cancellationToken);

            return new LoginUserResponse(
                user.FullName,
                user.Email,
                tokens.AccessToken,
                tokens.AccessTokenExpiresAt,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiresAt);
        }
    }
}
