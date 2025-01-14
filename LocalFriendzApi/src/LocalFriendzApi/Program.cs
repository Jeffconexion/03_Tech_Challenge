using FluentValidation;
using LocalFriendzApi.Commom.Api;
using LocalFriendzApi.Core.Validations;
using LocalFriendzApi.Endpoints;
using LocalFriendzApi.Middlewares;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<CreateContactRequestValidator>();
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
builder.AddConfiguration();
builder.AddDataContexts();
builder.AddServices();
builder.AddDocumentation();
builder.AddLogging();

var app = builder.Build();
app.MapEndpoints();


if (app.Environment.IsDevelopment())
{
    app.ConfigureDevEnvironment();
}

app.UseLoggingMiddleware();
app.UseHttpsRedirection();

app.Run();

//Exporte da program
public partial class Program { }