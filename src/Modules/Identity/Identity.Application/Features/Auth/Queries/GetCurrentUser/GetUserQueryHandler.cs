using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    internal class GetUserQueryHandler : IQueryHandler<GetUserQuery, GetUserResponseDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// This action is used to retrieve the user ID of the user who is using the application. 
        /// </summary>
        /// <param name="query">returns the ID obtained from the JWT </param> 
        public async Task<GetUserResponseDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(query.Id, cancellationToken)
                ?? throw new NotFoundException("user", query.Id.ToString());

            return new GetUserResponseDto(
                user.Id, user.FullName, user.PhoneNumber, user.JobTitle, user.Email, user.IsActive, user.CreatedAt); 
        }
    }
}
