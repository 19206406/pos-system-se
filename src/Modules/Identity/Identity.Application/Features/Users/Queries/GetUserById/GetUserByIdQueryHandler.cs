using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Queries.GetUserById
{
    public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(query.Id, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", query.Id.ToString());

            return new GetUserByIdResponse(
                user.Id, user.FullName, user.PhoneNumber,
                user.JobTitle, user.Email, user.IsActive, user.CreatedAt); 
        }
    }
}
