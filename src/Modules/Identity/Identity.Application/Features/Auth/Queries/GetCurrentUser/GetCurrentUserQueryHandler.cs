using Identity.Application.Contracts.Authentication;
using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        private readonly IUserReadRepository _userReadRepository;
        private readonly IUserAccessReader _userAccessReader;

        public GetCurrentUserQueryHandler(IUserReadRepository userReadRepository, IUserAccessReader userAccessReader)
        {
            _userReadRepository = userReadRepository;
            _userAccessReader = userAccessReader;
        }

        /// <summary>
        /// UserId comes from the "sub" claim. Roles and permissions are read from the database,
        /// not from the token, so the response reflects role changes made after the token was issued.
        /// </summary>
        public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
        {
            var profile = await _userReadRepository.GetProfileByIdAsync(query.UserId, cancellationToken)
                ?? throw new NotFoundException("user", query.UserId.ToString());

            var access = await _userAccessReader.GetByUserIdAsync(query.UserId, cancellationToken);

            return new GetCurrentUserResponse(
                profile.Id, profile.FullName, profile.Email, profile.PhoneNumber, profile.JobTitle,
                access.Roles, access.Permissions);
        }
    }
}
