using ClaudiaWebApi.Core;
using ClaudiaWebApi.Infra.OpenAI;
using ClaudiaWebApi.Infra.Tenants;
using ClaudiaWebApi.Infra.VectorDB;
using ClaudiaWebApi.Infra.Web.ErrorHandling;
using ClaudiaWebApi.Infra.Web.Metrics;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

var openAIConfig = builder.Configuration.GetSection("OpenAIApi");
builder.Services.AddHttpClient<IOpenAIClient, OpenAIClient>(client =>
{
    client.BaseAddress = new Uri(openAIConfig["BaseUrl"]);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAIConfig["ApiKey"]}");
});

var vectorDBApiConfig = builder.Configuration.GetSection("VectorDBApi");
builder.Services.AddHttpClient<IVectorDBApiClient, VectorDBApiClient>(client =>
{
    client.BaseAddress = new Uri(vectorDBApiConfig["BaseUrl"]);
    client.DefaultRequestHeaders.Add("api-key", vectorDBApiConfig["ApiKey"]);
});

builder.Services.AddScoped<IRagEngine, RagEngine>()
    .AddScoped<ITextEmbedder, TextEmbedder>()
    .AddScoped<IContextRetriever, ContextRetriever>()
    .AddScoped<IAnswerGenerator, AnswerGenerator>()
    .AddScoped<ISystemPromptRetriever, SystemPromptRetriever>()
    .AddScoped<TenantContext>();

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<TenantContextActionFilter>();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenTelemetry().WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter()
        .AddMeter(AppMetrics.MeterName);
});

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
