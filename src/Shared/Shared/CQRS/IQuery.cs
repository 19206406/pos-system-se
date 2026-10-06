using Shared.Mediator;

namespace Shared.CQRS
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
        where TResponse : notnull
    {
    }
}
