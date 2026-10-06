using FluentValidation;
using FluentValidation.Results;

namespace Shared.Mediator.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {

        private readonly IValidator<TRequest>[] _validators = validators.ToArray(); 

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Length == 0)
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var failures = new List<ValidationFailure>(); 

            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(context, cancellationToken);
                failures.AddRange(result.Errors); 
            }

            if (failures.Count > 0)
                throw new ValidationException(failures);

            return await next(); 
        }
    }
}
