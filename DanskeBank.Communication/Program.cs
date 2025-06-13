using Microsoft.EntityFrameworkCore;
using DanskeBank.Communication.Databases;
using DanskeBank.Communication.Repositories.Interfaces;
using DanskeBank.Communication.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// databases
builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlite("Data Source=communication.db")
);

// repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
