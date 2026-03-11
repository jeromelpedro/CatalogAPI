using Serilog.Context;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        public const string HeaderKey = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogTrace("CorrelationIdMiddleware iniciado para {Method} {Path}", context.Request.Method, context.Request.Path);
            var correlationId = context.Request.Headers[HeaderKey].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            context.Items[HeaderKey] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderKey] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogDebug("CorrelationId assigned: {correlationId}", correlationId);
                await _next(context);
                _logger.LogTrace("CorrelationIdMiddleware finalizado para CorrelationId {CorrelationId}", correlationId);
            }
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
