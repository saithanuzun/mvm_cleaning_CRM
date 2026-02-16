using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using mvmclean.backend.Application.Services;
using mvmclean.backend.Domain.Aggregates.Booking;
using mvmclean.backend.Domain.Aggregates.Contact;
using mvmclean.backend.Domain.Aggregates.Contractor;
using mvmclean.backend.Domain.Aggregates.Invoice;
using mvmclean.backend.Domain.Aggregates.Promotion;
using mvmclean.backend.Domain.Aggregates.SeoPage;
using mvmclean.backend.Domain.Aggregates.Service;
using mvmclean.backend.Infrastructure.Persistence;
using mvmclean.backend.Infrastructure.Persistence.Repositories;
using mvmclean.backend.Infrastructure.Repositories;
using mvmclean.backend.Infrastructure.Services;
using mvmclean.backend.Infrastructure.Seeding;
using Resend;

namespace mvmclean.backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureRegistration(this IServiceCollection serviceCollection)
    {
        var configuration = serviceCollection.BuildServiceProvider().GetRequiredService<IConfiguration>();

        var resend = configuration["RESEND:RESEND_APITOKEN"];
        var connStr = configuration.GetConnectionString("MyDbConnection");


        serviceCollection.AddDbContext<MVMdbContext>(conf =>
        {
            conf.UseNpgsql(connStr, opt => { opt.EnableRetryOnFailure(); });
        });


        serviceCollection.AddScoped<IBookingRepository, BookingRepository>();
        serviceCollection.AddScoped<IContractorRepository, ContractorRepository>();
        serviceCollection.AddScoped<IInvoiceRepository, InvoiceRepository>();
        serviceCollection.AddScoped<IPromotionRepository, PromotionRepository>();
        serviceCollection.AddScoped<ISeoPageRepository, SeoPageRepository>();
        serviceCollection.AddScoped<IServiceRepository, ServiceRepository>();
        serviceCollection.AddScoped<IContactRepository, ContactRepository>();


        // Register infrastructure services
        serviceCollection.AddScoped<IStripeService, StripeService>();
        serviceCollection.AddScoped<IInvoiceService, Services.InvoiceService>();

        // Register Resend email provider
        serviceCollection.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = resend;

            if (string.IsNullOrEmpty(options.ApiToken))
            {
                throw new InvalidOperationException(
                    "RESEND:RESEND_APITOKEN is not configured. Please set it in appsettings or environment variables.");
            }
        });

        // Register ResendClient as factory with HttpClient
        serviceCollection.AddScoped<ResendClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptionsSnapshot<ResendClientOptions>>();
            var httpClient = new HttpClient();
            return new ResendClient(options, httpClient);
        });

        // Register IResend
        serviceCollection.AddScoped<IResend>(provider =>
            provider.GetRequiredService<ResendClient>());

        // Register mailing service (depends on IResend)
        serviceCollection.AddScoped<IMailingService, Services.MailingService>();

        // Register SMS service
        serviceCollection.AddScoped<ISmsService, SmsService>();

        // Register database seeder
        serviceCollection.AddScoped<DatabaseSeeder>();




        return serviceCollection;
    }
}
