using FluentValidation;
using KindRaise.Application.Campaigns;
using KindRaise.Application.Donations;
using KindRaise.Application.Donations.CreateDonation;
using KindRaise.Application.Messaging;
using KindRaise.Application.Payments;
using KindRaise.Application.Services;
using KindRaise.Application.Services.Donations.CreateDonation;
using KindRaise.DonationWorker.OutboxMessages;
using KindRaise.DonationWorker.PaymentProcessor;
using KindRaise.Infrastructure.Database;
using KindRaise.Infrastructure.Database.Repositories;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using KindRaise.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestServiceProvider
    {
        public static ServiceProvider Create(
            IntegrationTestFixture fixture,
            Action<FakePaymentProviderOptions>? configurePaymentProvider = null,
            Action<IServiceCollection>? configureServices = null)
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            services.AddDbContext<KindRaiseDbContext>(options =>
                options.UseNpgsql(fixture.PostgreSqlConnectionString));

            services.AddScoped<ICampaignRepository, EfCampaignRepository>();
            services.AddScoped<IDonationRepository, EfDonationRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            services.Configure<FakePaymentProviderOptions>(options =>
                {
                    options.DefaultResult = PaymentResultStatus.Success;
                    configurePaymentProvider?.Invoke(options);
                });

            services.AddScoped<IPaymentProvider, FakePaymentProvider>();
            services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
            
            services.AddSingleton<FakePaymentProviderState>();

            services.AddSingleton(new RabbitMQOptions
            {
                HostName = fixture.RabbitMqHost,
                Port = fixture.RabbitMqPort,
                UserName = fixture.RabbitMqUsername,
                Password = fixture.RabbitMqPassword,
                VirtualHost = "/"
            });

            services.AddSingleton<RabbitMQConnection>();
            services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();

            services.AddScoped<IProcessedMessageRepository, EfProcessedMessageRepository>();
            services.AddScoped<IOutboxRepository, EfOutboxRepository>();

            services.AddScoped<ICreateDonationService, CreateDonationService>();
            services.AddScoped<IValidator<CreateDonationRequest>, TestCreateDonationRequestValidator>();

            services.AddSingleton<FailOnceOutboxMessagePublisher>();
            services.AddSingleton<IOutboxMessagePublisher>(sp => sp.GetRequiredService<FailOnceOutboxMessagePublisher>());

            services.AddScoped<OutboxMessageProcessor>();
            services.AddSingleton<PaymentConfirmedProcessor>();

            configureServices?.Invoke(services);

            return services.BuildServiceProvider();
        }
    }
}
