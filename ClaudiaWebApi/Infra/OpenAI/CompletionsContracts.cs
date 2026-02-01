namespace ClaudiaWebApi.Infra.OpenAI;
public record GenerateCompletionRequest(string Model, List<Message> Messages);

public record Message(string Role, string Content);

public record MessageRole
{
    public const string User = "user";
    public const string System = "system";
}

public record ChatCompletionResponse
{
    public string Id { get; init; } = "";
    public string Object { get; init; } = "";
    public long Created { get; init; }
    public string Model { get; init; } = "";
    public Choice[] Choices { get; init; } = [];
    public Usage Usage { get; init; } = new();
    public string ServiceTier { get; init; } = "";
    public string SystemFingerprint { get; init; } = "";
}

public record Choice
{
    public int Index { get; init; }
    public CompletionMessage Message { get; init; } = new();
    public object? Logprobs { get; init; }
    public string FinishReason { get; init; } = "";
}

public record CompletionMessage
{
    public string Role { get; init; } = "";
    public string Content { get; init; } = "";
    public string? Refusal { get; init; }
    public object[] Annotations { get; init; } = [];
}

public record Usage
{
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
    public PromptTokensDetails PromptTokensDetails { get; init; } = new();
    public CompletionTokensDetails CompletionTokensDetails { get; init; } = new();
}

public record PromptTokensDetails
{
    public int CachedTokens { get; init; }
    public int AudioTokens { get; init; }
}

public record CompletionTokensDetails
{
    public int ReasoningTokens { get; init; }
    public int AudioTokens { get; init; }
    public int AcceptedPredictionTokens { get; init; }
    public int RejectedPredictionTokens { get; init; }
}
