using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Queries.GetUserById
{
    public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdResponse>
    {
        private readonly IUserReadRepository _userReadRepository;

        public GetUserByIdQueryHandler(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository;
        }

        public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await _userReadRepository.GetProfileByIdAsync(query.Id, cancellationToken)
                ?? throw new NotFoundException("user", query.Id.ToString());

            return new GetUserByIdResponse(
                user.Id, user.FullName, user.PhoneNumber,
                user.JobTitle, user.Email, user.IsActive, user.CreatedAt);
        }
    }
}
