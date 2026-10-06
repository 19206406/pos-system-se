using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Exceptions.Common;
using System.Text.Json;

namespace PosSystem.Api.ExceptionHandling
{
    public sealed class GlobalExceptionHandler : IExceptionHandler 
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        private const int StatusCodeClientClosedRequest = 499; 

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Request cancelled by the client on {Path}", httpContext.Request.Path);
                httpContext.Response.StatusCode = StatusCodeClientClosedRequest;
                return true; 
            }

            var problem = exception switch
            {
                ValidationException validation => BuildValidationProblem(validation),
                BaseException domain => BuildDomainProblem(domain), 
                _ => BuildUnexpectedProblem()
            };

            var status = problem.Status!.Value;
            problem.Instance = httpContext.Request.Path.Value;

            if (status >= StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
            else
                _logger.LogWarning(
                    "Handled {ExceptionType} ({Status}) on {Path}: {Message}",
                    exception.GetType().Name, status, httpContext.Request.Path, exception.Message);

            httpContext.Response.StatusCode = status;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problem
            }); 
        }

        private static ProblemDetails BuildValidationProblem(ValidationException exception)
        {
            var errors = exception.Errors.
                GroupBy(failure => JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName))
                .ToDictionary(
                    group => group.Key, group => group
                        .Select(failure => failure.ErrorMessage)
                        .Distinct().ToArray());

            return new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Extensions = { ["Errors"] = errors }
            }; 
        }

        private static ProblemDetails BuildDomainProblem(BaseException exception)
        {
            var (status, title) = exception.ErrorType switch
            {
                ErrorType.BadRequest => (StatusCodes.Status400BadRequest, "Bad request."),
                ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized."),
                ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden."),
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found."),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict."),
                ErrorType.Business => (StatusCodes.Status422UnprocessableEntity, "Business rule violation."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
            };

            return new ProblemDetails { Status = status, Title = title, Detail = exception.Message }; 
        }

        private static ProblemDetails BuildUnexpectedProblem() => new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "An internal server error occurred. Please try again later."
        }; 
    }
}

