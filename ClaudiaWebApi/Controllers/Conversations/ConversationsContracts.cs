namespace ClaudiaWebApi.Controllers.Conversations;

public record CompletionRequest(int HelpdeskId, string ProjectName, Message[] Messages);

public record Message(string Role, string Content);

public record MessageRole
{
    public const string User = "USER";
    public const string Agent = "AGENT";
}

public record CompletionResponse(Message[] Messages, bool HandoverToHumanNeeded, Section[] SectionsRetrieved);

public record Section(float Score, string Content);
