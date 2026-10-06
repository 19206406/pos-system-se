using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Shared.Mediator
{
    internal sealed class Sender(IServiceProvider serviceProvider) : ISender
    {
        private static readonly ConcurrentDictionary<Type, object> Wrappers = new(); 

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var wrapper = (RequestHandlerWrapper<TResponse>)Wrappers.GetOrAdd(
                request.GetType(),
                static requestType => Activator.CreateInstance(
                    typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse)))!);

            return wrapper.Handle(request, serviceProvider, cancellationToken); 
        }
    }

    internal abstract class RequestHandlerWrapper<TResponse>
    {
        public abstract Task<TResponse> Handle(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken); 
    }

    internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Handle(
            IRequest<TResponse> request, 
            IServiceProvider serviceProvider, 
            CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)request;

            var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>()
                ?? throw new InvalidOperationException(
                    $"No handler registered for '{typeof(TRequest).FullName}'. " +
                    "Did you forget to call AddMediator with the assembly that contains it?");

            RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(typedRequest, cancellationToken);

            var behaviors = serviceProvider
                .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                .ToArray();

            for (var i = behaviors.Length - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var next = pipeline;
                pipeline = () => behavior.Handle(typedRequest, next, cancellationToken);
            }

            return pipeline();
        }
    }
}
