using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using Shared.Exceptions.Common;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Shared.Exceptions.Handler
{
    public sealed class GlobalExceptionHandler : IExceptionHandler 
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title, errors) = exception switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    "One or more validation errors occurred.",
                    validationEx.Errors.Select(x => x.ErrorMessage).ToList()),

                BaseException baseEx => (
                    MapCodeStatus(baseEx.Code),
                    baseEx.Code,
                    new List<string> { baseEx.Message }),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.",
                    new List<string> { "Ha ocurrido un error interno en el servidor" })
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
            }
            else
            {
                _logger.LogWarning(exception, "Handled exception ({Status}) on {Path}", status, httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = status;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Type = GetTypeForStatusCode(status),
                    Instance = httpContext.Request.Path,
                    Extensions = { ["Erros"] = errors }
                }
            });
        }

        private static int MapCodeStatus(string code) => code switch
        {
            "NotFound" => StatusCodes.Status404NotFound,
            "BadRequest" => StatusCodes.Status400BadRequest,
            "Forbidden" => StatusCodes.Status403Forbidden,
            "Conflict" => StatusCodes.Status409Conflict,
            "Unauthorized" => StatusCodes.Status401Unauthorized,
            "Business" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetTypeForStatusCode(int statusCode) => statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}

