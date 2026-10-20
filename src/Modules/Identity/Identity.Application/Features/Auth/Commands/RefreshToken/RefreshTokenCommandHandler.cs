using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Identity.Application.Dtos.Authentication;
using Identity.Application.DTOs.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.RefreshToken
{
    internal class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthSessionService _sessionService;

        public RefreshTokenCommandHandler(IUserRepository userRepository, IAuthSessionService sessionService)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.id, cancellationToken)
                ?? throw new NotFoundException("user", command.id.ToString()); 

            var client = new ClientContext("not delivered", "not delivered"); 

            var auth = await _sessionService.RefreshSessionAsync(command.RefreshToken, client, cancellationToken); 

            return new AuthResponseDto(
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
