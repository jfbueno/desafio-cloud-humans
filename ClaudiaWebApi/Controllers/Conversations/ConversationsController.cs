using ClaudiaWebApi.Core;
using Microsoft.AspNetCore.Mvc;

namespace ClaudiaWebApi.Controllers.Conversations;

[Route("api/[controller]")]
[ApiController]
public class ConversationsController(RAGEngine chatbotEngine) : ControllerBase
{
    /// <summary>
    /// Generates a completion response based on the provided user messages and returns the agent's reply along with
    /// related metadata.
    /// </summary>
    /// <param name="request">
    /// The completion request containing the sequence of user messages to process. Cannot be null.
    /// </param>
    /// <returns>
    /// Instance of <see cref="CompletionResponse"/> with the agent's reply, original user message, and any 
    /// relevant context sections retrieved from the knowledge database.
    /// </returns>
    [HttpPost("completions")]
    public async Task<CompletionResponse> GenerateCompletions(CompletionRequest request)
    {
        var answer = await chatbotEngine.GenerateResponseForUserInput(
            [.. request.Messages.Select(m => m.Content)]
        );

        return new CompletionResponse
        (
            Messages:
            [
                new Message(Role: MessageRole.User, Content: request.Messages.Last().Content),
                new Message(Role: MessageRole.Agent, Content: answer.Message)
            ],
            HandoverToHumanNeeded: answer.HandoverToHumanNeeded,
            SectionsRetrieved: [.. answer.RetrievedSections.Select(
                s => new Section(Score: s.Score, Content: s.Content)
            )]
        );
    }
}
