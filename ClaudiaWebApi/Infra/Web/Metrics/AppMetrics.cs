using System.Diagnostics.Metrics;

namespace ClaudiaWebApi.Infra.Web.Metrics;

public static class AppMetrics
{
    public const string MeterName = "ClaudiaApi";
    public const string MeterVersion = "1.0.0";

    public static readonly Meter Meter = new(MeterName, MeterVersion);
}

public static class AppCustomMetrics
{
    public static readonly Counter<long> TokensUsed =
        AppMetrics.Meter.CreateCounter<long>(
            "openai_tokens_used", description: "Total number of tokens used in OpenAI API calls"
        );

    public static readonly Counter<long> UserPrompts =
        AppMetrics.Meter.CreateCounter<long>(
            "claudia_user_prompts", description: "Total number of prompts received"
        );

    public static readonly Counter<long> RejectedUserPrompts =
        AppMetrics.Meter.CreateCounter<long>(
            "claudia_rejected_user_prompts", description: "Total number of prompts rejected"
        );
}
