using FastEndpoints;
using FluentValidation;
using KindRaise.Application.Campaigns.CreateCampaign;
using KindRaise.Application.Services.CreateCampaign;
using KindRaise.Application.Services.CreateDonation;
using KindRaise.Application.Services.DeleteCampaign;
using KindRaise.Application.Services.GetCampaign;
using KindRaise.Application.Services.GetCampaigns;
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
builder.Services.AddScoped<IGetCampaignService, GetCampaignService>();
builder.Services.AddScoped<IDeleteCampaignService, DeleteCampaignService>();
builder.Services.AddScoped<IGetCampaignsService, GetCampaignsService>();

builder.Services.AddScoped<ICreateDonationService, CreateDonationService>();

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
