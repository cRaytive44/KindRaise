using FastEndpoints;
using FluentValidation;
using KindRaise.Application.Campaigns;
using KindRaise.Application.Campaigns.CreateCampaign;
using KindRaise.Application.Donations;
using KindRaise.Application.Messaging;
using KindRaise.Application.Services;
using KindRaise.Application.Services.Campaigns.CreateCampaign;
using KindRaise.Application.Services.Campaigns.DeleteCampaign;
using KindRaise.Application.Services.Campaigns.GetCampaign;
using KindRaise.Application.Services.Campaigns.GetCampaigns;
using KindRaise.Application.Services.Donations.CreateDonation;
using KindRaise.Application.Services.Donations.GetDonation;
using KindRaise.Application.Services.Donations.GetDonations;
using KindRaise.Infrastructure.Database;
using KindRaise.Infrastructure.Database.Repositories;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
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
builder.Services.AddScoped<ICampaignRepository, EfCampaignRepository>();
builder.Services.AddScoped<IDonationRepository, EfDonationRepository>();
builder.Services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();

builder.Services.AddSingleton<RabbitMQOptions>();
builder.Services.AddSingleton<RabbitMQConnection>();

builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddScoped<ICreateCampaignService, CreateCampaignService>();
builder.Services.AddScoped<IGetCampaignService, GetCampaignService>();
builder.Services.AddScoped<IDeleteCampaignService, DeleteCampaignService>();
builder.Services.AddScoped<IGetCampaignsService, GetCampaignsService>();

builder.Services.AddScoped<ICreateDonationService, CreateDonationService>();
builder.Services.AddScoped<IGetDonationService, GetDonationService>();
builder.Services.AddScoped<IGetDonationsService, GetDonationsService>();

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
