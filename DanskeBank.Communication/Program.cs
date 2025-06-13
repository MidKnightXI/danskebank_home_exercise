using Microsoft.EntityFrameworkCore;
using DanskeBank.Communication.Databases;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// database
builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlite("Data Source=communication.db")
);

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
