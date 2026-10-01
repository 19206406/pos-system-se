using Identity.Application.Contracts.Persistence;
using Identity.Application.DTOs.Responses;
using Identity.Domain.Entities;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserResponseDto>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<RegisterUserResponseDto> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var user = new User
            {
                FullName = command.FullName,
                PhoneNumber = command.FullName,
                JobTitle = command.Position,
                Email = command.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _userRepository.CreateUser(user); 
            
            return new RegisterUserResponseDto(user.Id); 
        }
    }
}
