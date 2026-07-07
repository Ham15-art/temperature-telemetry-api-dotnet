using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TemperatureApi.Options;
using TemperatureApi.Repositories;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Bind and validate configuration options
var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
jwtOptions.Validate();

var mongoDbOptions = new MongoDbOptions();
builder.Configuration.GetSection(MongoDbOptions.SectionName).Bind(mongoDbOptions);
mongoDbOptions.Validate();

// Register options in DI container
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton(mongoDbOptions);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

// Define a retry policy for MongoDB operations
var mongoRetryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        (exception, timeSpan) =>
        {
            // Log the error and the retry attempt
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("MongoRetryPolicy");
            logger.LogWarning(exception, "An error occurred connecting to MongoDB. Waiting {TimeSpan} before next retry. ", timeSpan);
        });

// Register repository with the retry policy
builder.Services.AddSingleton<IAsyncPolicy>(mongoRetryPolicy);
builder.Services.AddSingleton<ITemperatureRepository, MongoTemperatureRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173", "https://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromSeconds(5)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure middleware pipeline (explicit ordering)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline order: routing → CORS → authentication → authorization → endpoints
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();  // Who are you?
app.UseAuthorization();   // Are you allowed in?

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Temperature API starting up");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("JWT Issuer: {Issuer}", jwtOptions.Issuer);
logger.LogInformation("MongoDB Database: {DatabaseName}", mongoDbOptions.DatabaseName);

app.Run();
