using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Responses;
using Identity.Domain.Entities;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, RegisterUserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISecureTokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;

        public CreateUserCommandHandler(
            IUserRepository userRepository, IUnitOfWork unitOfWork, ISecureTokenGenerator tokenGenerator, ITokenHasher tokenHasher)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
        }

        public async Task<RegisterUserResponseDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {

            var token = _tokenGenerator.GenerateToken();
            var tokenHash = _tokenHasher.Hash(token);

            var tokenInvite = new PasswordToken
            {
                TokenHash = tokenHash,
                TokenType = "invite",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(72),
                CreatedAt = DateTimeOffset.UtcNow
            }; 

            var user = new User
            {
                FullName = command.FullName,
                PhoneNumber = command.FullName,
                JobTitle = command.Position,
                Email = command.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user.PasswordTokens.Add(tokenInvite); 

            _userRepository.AddUser(user);

            await _unitOfWork.SaveChangesAsync(); 

            // send email 
            
            return new RegisterUserResponseDto(
                user.Id, user.FullName, user.JobTitle, 
                user.PhoneNumber, user.Email, user.CreatedAt); 
        }
    }
}
