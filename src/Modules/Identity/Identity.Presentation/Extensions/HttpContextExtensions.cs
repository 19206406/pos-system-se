using Identity.Application.Dtos.Authentication;
using Microsoft.AspNetCore.Http;

namespace Identity.Presentation.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// The only place that turns HttpContext into a ClientContext, so Application never sees HttpContext.
        /// Behind a proxy RemoteIpAddress is the proxy's address unless UseForwardedHeaders is configured.
        /// </summary>
        public static ClientContext GetClientContext(this HttpContext context)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();

            return new ClientContext(
                string.IsNullOrWhiteSpace(userAgent) ? null : userAgent,
                context.Connection.RemoteIpAddress?.ToString());
        }
    }
}
