using KindRaise.Application.Campaigns;
using KindRaise.Application.Donations;
using KindRaise.Application.Messaging;
using KindRaise.Application.Payments;
using KindRaise.Application.Services;
using KindRaise.DonationWorker;
using KindRaise.DonationWorker.OutboxMessages;
using KindRaise.DonationWorker.Publishers;
using KindRaise.Infrastructure.Database;
using KindRaise.Infrastructure.Database.Repositories;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using KindRaise.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMQOptions = new RabbitMQOptions
{
    HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost",
    Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    UserName = builder.Configuration["RabbitMQ:UserName"]
        ?? throw new InvalidOperationException(
            "RabbitMQ username is not configured."),
    Password = builder.Configuration["RabbitMQ:Password"]
        ?? throw new InvalidOperationException(
            "RabbitMQ password is not configured."),
    VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/"
};

builder.Services.AddDbContext<KindRaiseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("KindRaiseDb")));

builder.Services.AddScoped<ICampaignRepository, EfCampaignRepository>();

builder.Services.AddScoped<IDonationRepository, EfDonationRepository>();
builder.Services.AddScoped<IProcessedMessageRepository, EfProcessedMessageRepository>();
builder.Services.AddScoped<IOutboxRepository, EfOutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.Configure<FakePaymentProviderOptions>(
    builder.Configuration.GetSection("FakePaymentProvider"));

builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
builder.Services.AddScoped<FakePaymentProviderState>();
builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();

builder.Services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();

builder.Services.AddSingleton(rabbitMQOptions);
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<IOutboxMessagePublisher, OutboxRabbitMQPublisher>();

builder.Services.AddScoped<OutboxMessageProcessor>();

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

var host = builder.Build();
host.Run();
