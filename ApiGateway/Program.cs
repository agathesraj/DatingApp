using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiGateway.Middleware;
using ApiGateway.Aggregators;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add services to the container
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // Your main API authority
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super secret unguessable key")), // Use the same key as your main API
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.RequireHttpsMetadata = false; // Set to true in production
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowCredentials();
    });
});

// Add custom aggregators
builder.Services.AddSingleton<UserProfileAggregator>();

// Add Ocelot
builder.Services.AddOcelot()
    .AddSingletonDefinedAggregator<UserProfileAggregator>();

// Add controllers for custom endpoints
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Custom middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

// Add a simple status endpoint
app.MapGet("/status", () => new { Status = "API Gateway is running", Timestamp = DateTime.UtcNow });

// Add gateway info endpoint
app.MapGet("/gateway/info", () => new 
{ 
    Gateway = "Dating App API Gateway",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Routes = new[]
    {
        "/gateway/api/account/*",
        "/gateway/api/users/*",
        "/gateway/api/buggy/*",
        "/gateway/weatherforecast"
    }
});

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
