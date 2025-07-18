namespace ApiGateway.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            // Log the incoming request
            await LogRequest(context, correlationId);

            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing request {CorrelationId}", correlationId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                await LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds);

                // Copy the contents of the new memory stream to the original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpContext context, string correlationId)
        {
            var request = context.Request;
            
            var requestLog = new
            {
                CorrelationId = correlationId,
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                ContentType = request.ContentType,
                UserAgent = request.Headers["User-Agent"].ToString(),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Incoming Request: {@RequestLog}", requestLog);

            // Log request body for POST/PUT requests (be careful with sensitive data)
            if (HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method))
            {
                request.EnableBuffering();
                var body = await new StreamReader(request.Body).ReadToEndAsync();
                request.Body.Position = 0;

                if (!string.IsNullOrEmpty(body) && body.Length < 1000) // Limit body logging size
                {
                    _logger.LogInformation("Request Body for {CorrelationId}: {Body}", correlationId, body);
                }
            }
        }

        private async Task LogResponse(HttpContext context, string correlationId, long elapsedMilliseconds)
        {
            var response = context.Response;
            
            var responseLog = new
            {
                CorrelationId = correlationId,
                StatusCode = response.StatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                ContentType = response.ContentType,
                ElapsedMilliseconds = elapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Outgoing Response: {@ResponseLog}", responseLog);

            // Log response body for error responses
            if (response.StatusCode >= 400)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(response.Body).ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);

                if (!string.IsNullOrEmpty(body) && body.Length < 1000)
                {
                    _logger.LogWarning("Error Response Body for {CorrelationId}: {Body}", correlationId, body);
                }
            }
        }
    }
}