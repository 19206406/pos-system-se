using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.SetUserPassword
{
    public class SetUserPasswordCommandHandler : ICommandHandler<SetUserPasswordCommand, SetUserPasswordResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public SetUserPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<SetUserPasswordResponseDto> Handle(SetUserPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmail(command.Email); 

            if (user is null)
                throw new UnauthorizedException("You cannot perform the following action.");

            if (!user.IsActive)
                throw new UnauthorizedException("You cannot perform the following action."); 

            if (user.HashPassword is null)
                throw new UnauthorizedException("You cannot perform the following action.");


            var password = _passwordHasher.Hash(command.Password);
            user.HashPassword = password;

            await _userRepository.UpdateUser();

            return new SetUserPasswordResponseDto(true);
        }
    }
}
