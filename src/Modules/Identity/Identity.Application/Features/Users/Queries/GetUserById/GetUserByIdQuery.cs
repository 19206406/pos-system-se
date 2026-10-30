using Shared.CQRS;

namespace Identity.Application.Features.Users.Queries.GetUserById
{
    public sealed record GetUserByIdQuery(Guid Id) : IQuery<GetUserByIdResponse>; 
}
