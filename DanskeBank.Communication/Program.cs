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

app.MapControllers();
app.MapMetrics();

app.Run();
