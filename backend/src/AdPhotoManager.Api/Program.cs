using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using AdPhotoManager.Api.Middleware;
using AdPhotoManager.Api.HealthChecks;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Infrastructure.ActiveDirectory;
using AdPhotoManager.Infrastructure.Data;
using AdPhotoManager.Infrastructure.Repositories;
using AdPhotoManager.Infrastructure.Services;
using AdPhotoManager.Infrastructure.ImageProcessing;
using AdPhotoManager.Shared.Constants;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/adphotomanager-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting AD Photo Manager API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("AdPhotoManager.Infrastructure")));

    // Add repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // Add services
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserSyncService, UserSyncService>();
    builder.Services.AddScoped<ILdapConnection, LdapConnectionAdapter>();
    builder.Services.AddScoped<IPhotoService, PhotoService>();
    builder.Services.AddScoped<IImageProcessor, ImageProcessor>();

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database")
        .AddCheck<AdConnectionHealthCheck>("ad_connection");

    // Add controllers
    builder.Services.AddControllers(options =>
    {
        // Add global organization authorization filter
        options.Filters.Add<OrganizationAuthorizationFilter>();
        // Add global exception filter
        options.Filters.Add<GlobalExceptionMiddleware>();
    });

// Add JWT Authentication
var jwtSecretKey = builder.Configuration[ConfigurationKeys.JwtSecretKey];
if (!string.IsNullOrEmpty(jwtSecretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration[ConfigurationKeys.JwtIssuer],
                ValidAudience = builder.Configuration[ConfigurationKeys.JwtAudience],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
}

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AD Photo Manager API",
        Version = "v1",
        Description = "API for managing Active Directory user photos"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

// Add Serilog request logging
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
