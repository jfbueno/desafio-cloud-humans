using ClaudiaWebApi.Infra.Tenants;
using ClaudiaWebApi.Infra.Web.Metrics;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ClaudiaWebApi.Core;

public interface IRagEngine
{
    Task<AnswerDTO> GenerateResponseForUserInput(string[] messages);
}

/// <summary>
/// Provides a retrieval-augmented generation (RAG) engine that generates answers to user queries by combining context
/// retrieval with generative AI models.
/// </summary>
/// <remarks>The RAGEngine coordinates the process of embedding user input, retrieving relevant context from a
/// vector database, and generating an answer using a language model.
/// </remarks>
/// <param name="logger">The logger used to record diagnostic and operational information for the RAG engine.</param>
/// <param name="textEmbedder">The text embedder used to convert user input into vector embeddings for context retrieval.</param>
/// <param name="contextRetriever">The context retriever responsible for fetching relevant context sections based on input embeddings.</param>
/// <param name="answerGenerator">The answer generator that produces responses using the user input and retrieved context.</param>
public sealed class RagEngine(
    ILogger<RagEngine> logger,
    ITextEmbedder textEmbedder,
    IContextRetriever contextRetriever,
    IAnswerGenerator answerGenerator,
    TenantContext tenantContext
) : IRagEngine
{
    private static readonly string[] DangerousPatterns =
    {
        "ignore previous instructions",
        "you are chatgpt",
        "system prompt",
        "act as",
        "jailbreak"
    };

    /// <summary>
    /// Generates an answer to the user's latest input message by retrieving relevant context and composing a response.
    /// </summary>
    /// <remarks>The method analyzes the user's latest message, retrieves context from a vector database, and
    /// generates a response using the available context. If the context is insufficient, the returned AnswerDTO may
    /// indicate that handover to a human is required.</remarks>
    /// <param name="messages">An array of messages representing the raw conversation history (without roles), 
    /// with the user's latest input as the last element.
    /// Must contain at least one message.
    /// </param>
    /// <returns>
    /// An AnswerDTO with the generated answer, a flag indicating if handover to a human is needed, 
    /// and the context sections used.
    /// </returns>
    public async Task<AnswerDTO> GenerateResponseForUserInput(string[] messages)
    {
        Debug.Assert(messages.Length > 0, "At least one message is required.");
        AppCustomMetrics.UserPrompts.Add(1, new KeyValuePair<string, object?>("project", tenantContext.ProjectName));

        var userQuery = SanitizeUserInput(messages.Last());

        ArgumentOutOfRangeException.ThrowIfNullOrEmpty(userQuery, "UserInput");

        if (userQuery.Length > 1000)
        {
            logger.LogWarning("User input exceeded maximum length. Rejecting user input.");
            AppCustomMetrics.RejectedUserPrompts.Add(
                1, new KeyValuePair<string, object?>("project", tenantContext.ProjectName)
            );
            throw new ArgumentException("Query too long");
        }

        logger.LogInformation("Received user input: {}", userQuery);

        var embeddings = await textEmbedder.FromTextAsync(userQuery);

        logger.LogDebug("Retrieved {} embeddings for user input.", embeddings.Length);

        logger.LogInformation("Retrieving context sections from vector database.");

        var contextSections = await contextRetriever.GetContextFromEmbeddingsAsync(embeddings);

        logger.LogDebug("Retrieved {} context sections.", contextSections.Length);

        var sections = contextSections.OrderByDescending(s => s.Score).ToArray();

        var handoverToHumanNeeded = RequiresHandoverToHuman(sections);

        logger.LogInformation("Generating answer based on user input and context sections.");

        var modelAnswer = await answerGenerator.GenerateAnswer(
            userQuery,
            messages,
            [.. sections.Select(s => s.Content)]
        );

        logger.LogDebug("Generated answer: {}", modelAnswer);

        return new AnswerDTO(
            modelAnswer,
            handoverToHumanNeeded,
            [.. sections.Select(s => new SectionRetrievedDTO(s.Content, s.Score))]
        );
    }

    /// <summary>
    /// Basic implementation to sanitize user input.
    /// Removes potentially dangerous patterns and normalizes line endings in the specified user input string.
    /// </summary>
    /// <remarks>This method replaces any substrings matching predefined dangerous patterns with a placeholder
    /// and converts all line endings to line feeds ("\n"). It is intended to help prevent injection attacks or unsafe
    /// content from being processed further.</remarks>
    /// <param name="userInput">The user-provided input string to sanitize. Cannot be null.</param>
    /// <returns>A sanitized string with dangerous patterns replaced and line endings normalized to line feeds. Leading and
    /// trailing whitespace is removed.</returns>
    private static string SanitizeUserInput(string userInput)
    {
        foreach (var pattern in DangerousPatterns)
        {
            userInput = Regex.Replace(
                userInput,
                pattern,
                "[removed]",
                RegexOptions.IgnoreCase
            );
        }

        return userInput.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    /// <summary>
    /// Determines whether the current context requires escalation to a human agent based on the types and scores of the
    /// provided context sections.
    /// </summary>
    /// <remarks>This method assumes that the input array contains at least two elements and that the sections
    /// are pre-sorted in descending order by score. If these conditions are not met, the result may be
    /// invalid.</remarks>
    /// <param name="sections">An array of context sections, ordered by descending score, used to evaluate whether a handover to a human is
    /// necessary. Must contain at least two elements.</param>
    /// <returns>true if the highest scoring section is of type "N2", the second highest is not, and the score difference exceeds
    /// 0.05; otherwise, false.</returns>
    private static bool RequiresHandoverToHuman(ContextSection[] sections)
    {
        var (highestScoreSection, secondHighestScoreSection) = (sections[0], sections[1]);

        // If the section with the highest score is N2, we will still give a chance for the question
        // to be answered by the second section IF the score difference between them is small
        return highestScoreSection.Type == "N2" && secondHighestScoreSection.Type != "N2" &&
            highestScoreSection.Score - secondHighestScoreSection.Score > 0.05f;
    }
}

public record AnswerDTO(
    string Message,
    bool HandoverToHumanNeeded,
    SectionRetrievedDTO[] RetrievedSections
);

public record SectionRetrievedDTO(
    string Content,
    float Score
);