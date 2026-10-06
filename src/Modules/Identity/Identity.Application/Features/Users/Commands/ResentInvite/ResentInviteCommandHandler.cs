using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.ResentInvite
{
    public class ResentInviteCommandHandler : ICommandHandler<ResentInviteCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecureTokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;

        public ResentInviteCommandHandler(IUserRepository userRepository, ISecureTokenGenerator tokenGenerator, ITokenHasher tokenHasher)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
        }

        public async Task<Unit> Handle(ResentInviteCommand command, CancellationToken cancellationToken)
        {

            var user = await _userRepository.GetUserById(command.Id);

            if (user is null)
                throw new NotFoundException("user", command.Id.ToString());

            var token = _tokenGenerator.GenerateToken();
            var tokenHash = _tokenHasher.Hash(token);

            var newToken = new PasswordToken
            {
                TokenHash = tokenHash,
                TokenType = "invite",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(72),
                CreatedAt = DateTimeOffset.UtcNow
            };

            user.PasswordTokens.Add(newToken);

            await _userRepository.UpdateUser();

            // send email 

            return Unit.Value; 
        }
    }
}
