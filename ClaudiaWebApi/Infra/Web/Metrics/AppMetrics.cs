using System.Diagnostics.Metrics;

namespace ClaudiaWebApi.Infra.Web.Metrics;

public static class AppMetrics
{
    public const string MeterName = "ClaudiaApi";
    public const string MeterVersion = "1.0.0";

    public static readonly Meter Meter = new(MeterName, MeterVersion);
}

public static class OpenAPIClientMetrics
{
    public static readonly Counter<long> TokensUsed =
        AppMetrics.Meter.CreateCounter<long>(
            "openai_tokens_used", description: "Total number of tokens used in OpenAI API calls"
        );
}
