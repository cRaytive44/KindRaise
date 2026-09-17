using FastEndpoints;
using FluentValidation;
using KindRaise.Application.Campaigns.CreateCampaign;
using KindRaise.Application.Services;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateCampaignValidator>();

// Add services to the container.
builder.Services.AddDbContext<KindRaiseDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("KindRaiseDb")));

// Services
builder.Services.AddScoped<ICreateCampaignService, CreateCampaignService>();

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
