using LocalFriendzApi.Application.IServices;
using LocalFriendzApi.Application.Services;
using LocalFriendzApi.Core.Configuration;
using LocalFriendzApi.Core.IRepositories;
using LocalFriendzApi.Core.Logging;
using LocalFriendzApi.Infrastructure.Data;
using LocalFriendzApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;

namespace LocalFriendzApi.Commom.Api
{
    public static class BuildExtension
    {
        public static void AddConfiguration(this WebApplicationBuilder builder)
        {
            // Alterando a string de conexão para PostgreSQL
            ApiConfiguration.ConnectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection") ?? string.Empty;
        }

        public static void AddDataContexts(this WebApplicationBuilder builder)
        {
            builder
                .Services
                .AddDbContext<AppDbContext>(
                    x =>
                    {
                        // Alterando para usar PostgreSQL
                        x.UseNpgsql(ApiConfiguration.ConnectionString);
                    });

        }

        public static void AddServices(this WebApplicationBuilder builder)
        {
            builder
                .Services
                .AddTransient<IContactServices, ContactServices>();

            builder.Services.AddOpenTelemetry()
                .WithMetrics(builder =>
                {
                    builder.AddPrometheusExporter();
                    builder.AddMeter("Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel");
                    builder.AddView("http.server.request.duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                                0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                        });
                });

            builder
                .Services
                .AddTransient<IContactRepository, ContactRepository>();
        }

        public static void AddDocumentation(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(x =>
            {
                x.CustomSchemaIds(n => n.FullName);
            });
        }

        public static void AddCrossOrigin(this WebApplicationBuilder builder)
        {
            // inserir implementação do cross
        }

        public static void AddLogging(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
            {
                LogLevel = LogLevel.Information
            }));
        }
    }
}