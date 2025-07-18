using System.Collections.Concurrent;
using System.Net;

namespace ApiGateway.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
        private readonly int _maxRequests = 100; // Max requests per window
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1); // Time window

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientId(context);
            var now = DateTime.UtcNow;

            // Clean up old requests
            _requests.AddOrUpdate(clientId, new List<DateTime> { now }, (key, existingRequests) =>
            {
                existingRequests.RemoveAll(r => now - r > _window);
                existingRequests.Add(now);
                return existingRequests;
            });

            var requestCount = _requests[clientId].Count;

            if (requestCount > _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId}. Requests: {RequestCount}", clientId, requestCount);
                
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(_window).ToUnixTimeSeconds().ToString();
                
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = (_maxRequests - requestCount).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(_window).ToUnixTimeSeconds().ToString();

            await _next(context);
        }

        private string GetClientId(HttpContext context)
        {
            // Try to get client ID from various sources
            var clientId = context.Request.Headers["X-Client-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientId))
                return clientId;

            // Fallback to IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
                return ipAddress;

            // Fallback to user agent
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
            return !string.IsNullOrEmpty(userAgent) ? userAgent : "anonymous";
        }
    }
}