using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    internal class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetCurrentUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// This action is used to retrieve the user ID of the user who is using the application. 
        /// </summary>
        /// <param name="query">returns the ID obtained from the JWT </param> 
        public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
        {
            var profile = await _userRepository.GetByIdAsync(query.UserId, cancellationToken)
                ?? throw new NotFoundException("user", query.UserId.ToString());

            return new GetCurrentUserResponse(profile.Id, profile.FullName, profile.Email, profile.PhoneNumber, profile.JobTitle); 
        }
    }
}
