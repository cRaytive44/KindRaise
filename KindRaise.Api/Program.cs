using FastEndpoints;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

// Add services to the container.
builder.Services.AddDbContext<KindRaiseDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("KindRaiseDb")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseFastEndpoints();

app.Run();
