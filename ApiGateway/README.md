# Dating App API Gateway

A comprehensive API Gateway built with .NET 9 and Ocelot for the Dating App API. This gateway provides centralized routing, authentication, rate limiting, logging, and aggregation capabilities.

## Features

- **Routing**: Intelligent request routing to backend services
- **Authentication**: JWT Bearer token authentication
- **Rate Limiting**: Configurable rate limiting per client
- **CORS**: Cross-Origin Resource Sharing support
- **Logging**: Comprehensive request/response logging
- **Health Checks**: Built-in health monitoring
- **Aggregation**: Custom user profile aggregation
- **Monitoring**: Performance monitoring and metrics

## Architecture

```
┌─────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client    │───▶│   API Gateway   │───▶│   Dating API    │
│ (Angular)   │    │   (Port 5000)   │    │   (Port 5001)   │
└─────────────┘    └─────────────────┘    └─────────────────┘
```

## API Routes

### Gateway Routes
- `GET /status` - Gateway status
- `GET /gateway/info` - Gateway information and available routes
- `GET /health` - Health check endpoint

### Proxied Routes (to Dating API)
- `POST /gateway/api/account/register` - User registration
- `POST /gateway/api/account/login` - User authentication
- `GET /gateway/api/users` - Get users (requires authentication)
- `GET /gateway/api/users/{username}` - Get specific user (requires authentication)
- `PUT /gateway/api/users` - Update user (requires authentication)
- `GET /gateway/api/buggy/*` - Buggy endpoints for testing
- `GET /gateway/weatherforecast` - Weather forecast

### Aggregated Routes
- `GET /gateway/api/user-profile/{username}` - Aggregated user profile data

## Configuration

### ocelot.json
The main configuration file that defines routing rules, authentication, and other gateway behaviors.

### appsettings.json
Contains application-specific settings including:
- JWT configuration
- Rate limiting settings
- API endpoints
- Logging configuration

## Getting Started

### Prerequisites
- .NET 9 SDK
- Dating App API running on port 5001

### Running the Gateway

1. **Clone and navigate to the project:**
   ```bash
   cd ApiGateway
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Access the gateway:**
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

### Docker Deployment

1. **Build the Docker image:**
   ```bash
   docker build -t dating-api-gateway .
   ```

2. **Run the container:**
   ```bash
   docker run -p 5000:5000 -p 5001:5001 dating-api-gateway
   ```

## Authentication

The gateway uses JWT Bearer token authentication. To access protected endpoints:

1. Register or login through `/gateway/api/account/register` or `/gateway/api/account/login`
2. Include the JWT token in the Authorization header:
   ```
   Authorization: Bearer <your-jwt-token>
   ```

## Rate Limiting

- **Default**: 100 requests per minute per client
- **Development**: 1000 requests per minute per client
- Headers returned:
  - `X-RateLimit-Limit`: Maximum requests allowed
  - `X-RateLimit-Remaining`: Remaining requests in current window
  - `X-RateLimit-Reset`: When the rate limit resets

## Monitoring and Logging

### Request/Response Logging
Every request and response is logged with:
- Correlation ID for tracing
- Request details (method, path, headers)
- Response details (status, headers, timing)
- Error logging for failures

### Health Checks
Monitor gateway health at `/health` endpoint.

## Custom Middleware

### RateLimitingMiddleware
- Implements sliding window rate limiting
- Configurable limits per client
- Client identification via IP, User-Agent, or custom headers

### RequestResponseLoggingMiddleware
- Logs all incoming requests and outgoing responses
- Includes correlation IDs for request tracing
- Performance timing

## Aggregators

### UserProfileAggregator
Combines data from multiple endpoints to provide a unified user profile view.

## CORS Configuration

Configured to allow requests from:
- `http://localhost:4200` (Angular development server)
- `https://localhost:4200` (Angular HTTPS)

## Development

### Project Structure
```
ApiGateway/
├── Aggregators/
│   └── UserProfileAggregator.cs
├── Middleware/
│   ├── RateLimitingMiddleware.cs
│   └── RequestResponseLoggingMiddleware.cs
├── ocelot.json
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Dockerfile
```

### Adding New Routes

1. Update `ocelot.json` with new route configuration
2. Restart the gateway
3. Test the new route

### Custom Middleware

To add custom middleware:
1. Create middleware class in `Middleware/` folder
2. Register in `Program.cs` using `app.UseMiddleware<YourMiddleware>()`

## Security Considerations

- JWT tokens are validated using the same secret key as the main API
- HTTPS is enforced in production
- Rate limiting prevents abuse
- Request/response logging (be careful with sensitive data)
- CORS is configured for specific origins

## Troubleshooting

### Common Issues

1. **Gateway not starting:**
   - Check if port 5000/5001 is available
   - Verify `ocelot.json` syntax

2. **Routes not working:**
   - Verify backend API is running
   - Check route configuration in `ocelot.json`
   - Review logs for errors

3. **Authentication issues:**
   - Verify JWT secret key matches main API
   - Check token format and expiration

## Future Enhancements

- [ ] Service discovery integration (Consul)
- [ ] Circuit breaker pattern
- [ ] Caching layer
- [ ] API versioning support
- [ ] Swagger/OpenAPI documentation
- [ ] Metrics and monitoring dashboard
- [ ] Load balancing improvements