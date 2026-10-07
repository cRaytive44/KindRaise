using DonationWorker;
using KindRaise.Application.Donations;
using KindRaise.Application.Payments;
using KindRaise.Application.Services;
using KindRaise.Infrastructure.Database;
using KindRaise.Infrastructure.Database.Repositories;
using KindRaise.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<KindRaiseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("KindRaiseDb")));

builder.Services.AddScoped<IDonationRepository, EfDonationRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.Configure<FakePaymentProviderOptions>(
    builder.Configuration.GetSection("FakePaymentProvider"));

builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
