using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DanskeBank.Communication.Services;
using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Repositories;
using DanskeBank.Communication.Repositories.Interfaces;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Threading.RateLimiting;
using DanskeBank.Communication.Models.Responses;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);


// JWT configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
var refreshExpiryMinutes = int.Parse(jwtSettings["RefreshExpiryMinutes"] ?? "43200");

if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    throw new InvalidOperationException("JWT configuration is missing or incomplete.");
}

builder.Services.AddSingleton(new JwtService(key, issuer, audience, expiryMinutes, refreshExpiryMinutes));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});


// Mailing service configuration
var mailingSettings = builder.Configuration.GetSection("Mailing");
var enabled = bool.Parse(mailingSettings["Enabled"] ?? "false");
var smtpServer = mailingSettings["SmtpServer"];
var smtpPort = int.Parse(mailingSettings["SmtpPort"] ?? "587");
var smtpUser = mailingSettings["SmtpUser"];
var smtpPassword = mailingSettings["SmtpPassword"];

if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
{
    throw new InvalidOperationException("Mailing configuration is missing or incomplete.");
}

builder.Services.AddSingleton(new MailingService(smtpServer, smtpPort, smtpUser, smtpPassword));


builder.Services.AddControllers();


if (builder.Environment.IsDevelopment())
{
    // Add Swagger only in development environment
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "DanskeBank.Communication", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token in the format: Bearer {token}"
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
}


// databases
builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlite("Data Source=communication.db")
);


// repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


// Add built-in rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("RefreshTokenPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(60),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        )
    );
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(60),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        )
    );
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.HttpContext.Request.Path.StartsWithSegments("/api/v1/auth/refresh")
            || context.HttpContext.Request.Path.StartsWithSegments("/api/v1/auth/login"))
        {
            var response = JsonSerializer.Serialize(new LoginResponse
            {
                Success = false,
                Message = "Too many requests. Please try again later."
            });
            return new ValueTask(context.HttpContext.Response.WriteAsync(response, cancellationToken));
        }
        context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(new BaseResponse
        {
            Success = false,
            Message = "Too many requests. Please try again later."
        }), cancellationToken);
        return ValueTask.CompletedTask;
    };
});


var app = builder.Build();


var scope = app.Services.CreateScope();
try
{
    var context = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
    context.Database.EnsureCreated();
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred creating the DB.");
}
scope.Dispose();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DanskeBank.Communication v1"));
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpMetrics();
app.UseRateLimiter();

app.MapControllers();
app.MapMetrics();

app.Run();
