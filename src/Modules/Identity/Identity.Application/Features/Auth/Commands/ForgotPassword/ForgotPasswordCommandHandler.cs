using Identity.Application.Contracts.Persistence;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    internal class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public ForgotPasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<bool> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
