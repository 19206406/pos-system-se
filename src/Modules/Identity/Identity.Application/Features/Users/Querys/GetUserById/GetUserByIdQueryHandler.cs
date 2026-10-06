using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Querys.GetUserById
{
    internal class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserResponseDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<GetUserResponseDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserById(query.Id);

            if (user is null)
                throw new NotFoundException("user", query.Id.ToString());

            return new GetUserResponseDto(
                user.Id, user.FullName, user.PhoneNumber,
                user.JobTitle, user.Email, user.IsActive, user.CreatedAt); 
        }
    }
}
