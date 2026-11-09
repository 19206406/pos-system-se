using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Commands.UpdateUser
{
    public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<UpdateUserResponse> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken)
                ?? throw new NotFoundException("The user could not be found to perform the update.");

            user.FullName = command.FullName;
            user.JobTitle = command.JobTitle;
            user.PhoneNumber = command.PhoneNumber; 
            user.CreatedAt = _timeProvider.GetUtcNow().UtcDateTime;

            await _unitOfWork.SaveChangesAsync();

            return new UpdateUserResponse(user.Id, user.FullName, user.JobTitle, user.PhoneNumber, user.UpdatedAt); 
        }
    }
}
