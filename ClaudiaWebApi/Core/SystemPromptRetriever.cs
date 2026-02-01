namespace ClaudiaWebApi.Core;

/// <summary>
/// Provides functionality to retrieve predefined system prompts for supported projects by name.
/// 
/// We should increment this by fetching prompts from a database and using a distributed in-memory cache mechanism.
/// For the sake of simplicity, it is currently maintained as an in-memory hashmap.
/// </summary>
/// <param name="logger">The logger used to record diagnostic and error information for the retriever.</param>
public sealed class SystemPromptRetriever(ILogger<SystemPromptRetriever> logger)
{
    private static readonly Dictionary<string, string> _systemPrompts = new()
    {
        ["tesla_motors"] = "You are a helpful AI assistant that answers questions about products offered by Tesla Motors.\n" +
           "Use the information provided in the context to answer the user's question.\n" +
           "Assume the provided context is accurate and authoritative, even if it sounds informal.\n" +
           "Do not make assumptions beyond the context.\n" +
           "Only answer questions that are directly related to Tesla Motors products.\n" +
           "If the user's question is not related to Tesla Motors, respond that you cannot help with that request.\n" +
           "If the answer cannot be found in the context, politely say that you do not have enough information.\n" +
           "Do not mention or suggest competitor companies.\n" +
           "Only compare Tesla models with competitors if the user explicitly asks for such a comparison.\n"
    };

    public string GetSystemPromptByProjectName(string projectName)
    {
        if(!_systemPrompts.TryGetValue(projectName, out var prompt))
        {
            logger.LogError("System prompt not found for project '{ProjectName}'", projectName);
            throw new KeyNotFoundException($"System prompt not found for project '{projectName}'");
        }

        return prompt;
    }
}
