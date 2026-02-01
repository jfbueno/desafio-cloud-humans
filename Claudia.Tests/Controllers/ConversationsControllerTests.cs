using ClaudiaWebApi.Controllers.Conversations;
using ClaudiaWebApi.Core;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Claudia.Tests.Controllers;

public class ConversationsControllerTests
{
    private readonly Mock<IRagEngine> _mockRagEngine = new();
    private readonly ConversationsController _controller;

    public ConversationsControllerTests() 
        => _controller = new ConversationsController(_mockRagEngine.Object);

    [Fact]
    public async Task GenerateCompletions_WithValidRequest_ReturnsCompletionResponse()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "TestProject",
            Messages =
            [
                new Message
                {
                    Role = MessageRole.User, Content = "Hello, I need help"
                }
            ]
        };

        var answerDto = new AnswerDTO(
            Message: "I'm here to help!",
            HandoverToHumanNeeded: false,
            RetrievedSections:
            [
                new SectionRetrievedDTO(Content: "Context section 1", Score: 0.95f),
                new SectionRetrievedDTO(Content: "Context section 2", Score: 0.85f)
            ]
        );

        _mockRagEngine
            .Setup(x => x.GenerateResponseForUserInput(It.IsAny<string[]>()))
            .ReturnsAsync(answerDto);

        var result = await _controller.GenerateCompletions(request);

        Assert.NotNull(result);
        var actionResult = Assert.IsType<ActionResult<CompletionResponse>>(result);
        var completionResponse = actionResult.Value;

        Assert.NotNull(completionResponse);
        Assert.Equal(2, completionResponse.Messages.Length);
        Assert.Equal(MessageRole.User, completionResponse.Messages[0].Role);
        Assert.Equal("Hello, I need help", completionResponse.Messages[0].Content);
        Assert.Equal(MessageRole.Agent, completionResponse.Messages[1].Role);
        Assert.Equal("I'm here to help!", completionResponse.Messages[1].Content);
        Assert.False(completionResponse.HandoverToHumanNeeded);
        Assert.Equal(2, completionResponse.SectionsRetrieved.Length);
        Assert.Equal(0.95f, completionResponse.SectionsRetrieved[0].Score);
        Assert.Equal("Context section 1", completionResponse.SectionsRetrieved[0].Content);
    }

    [Fact]
    public async Task GenerateCompletions_WithMultipleMessages_PassesAllMessagesToRagEngine()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "TestProject",
            Messages =
            [
                new Message { Role = MessageRole.User, Content = "First message" },
                new Message{ Role = MessageRole.User, Content = "Agent response" },
                new Message{ Role = MessageRole.User, Content = "Second message" }
            ]
        };

        var answerDto = new AnswerDTO(
            Message: "Response to second message",
            HandoverToHumanNeeded: false,
            RetrievedSections: []
        );

        _mockRagEngine
            .Setup(x => x.GenerateResponseForUserInput(It.IsAny<string[]>()))
            .ReturnsAsync(answerDto);

        var result = await _controller.GenerateCompletions(request);

        _mockRagEngine.Verify(
            x => x.GenerateResponseForUserInput(
                It.Is<string[]>(msgs =>
                    msgs.Length == 3 &&
                    msgs[0] == "First message" &&
                    msgs[1] == "Agent response" &&
                    msgs[2] == "Second message"
                )
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GenerateCompletions_WithHandoverNeeded_ReturnsHandoverFlag()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "TestProject",
            Messages = [new Message { Role = MessageRole.User, Content = "Complex question" }]
        };

        var answerDto = new AnswerDTO(
            Message: "I need to transfer you to a human",
            HandoverToHumanNeeded: true,
            RetrievedSections: []
        );

        _mockRagEngine
            .Setup(x => x.GenerateResponseForUserInput(It.IsAny<string[]>()))
            .ReturnsAsync(answerDto);

        var result = await _controller.GenerateCompletions(request);

        var completionResponse = result.Value;
        Assert.NotNull(completionResponse);
        Assert.True(completionResponse.HandoverToHumanNeeded);
    }

    [Fact]
    public async Task GenerateCompletions_ReturnsLastUserMessageInResponse()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "TestProject",
            Messages =
            [
                new Message{ Role = MessageRole.User, Content = "First message" },
                new Message{ Role = MessageRole.Agent, Content = "Agent response" },
                new Message{ Role = MessageRole.User, Content = "Last user message" }
            ]
        };

        var answerDto = new AnswerDTO(
            Message: "Agent reply",
            HandoverToHumanNeeded: false,
            RetrievedSections: []
        );

        _mockRagEngine
            .Setup(x => x.GenerateResponseForUserInput(It.IsAny<string[]>()))
            .ReturnsAsync(answerDto);

        var result = await _controller.GenerateCompletions(request);

        var completionResponse = result.Value;
        Assert.NotNull(completionResponse);
        Assert.Equal("Last user message", completionResponse.Messages[0].Content);
        Assert.Equal(MessageRole.User, completionResponse.Messages[0].Role);
    }

    [Fact]
    public async Task GenerateCompletions_WithInvalidModelState_ReturnsBadRequest()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "TestProject",
            Messages = [new Message{ Role = MessageRole.User, Content = "Test" }]
        };

        _controller.ModelState.AddModelError("Messages", "At least one message is required");

        var result = await _controller.GenerateCompletions(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateCompletions_MapsRetrievedSectionsCorrectly()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "TestProject",
            Messages = [new Message{ Role = MessageRole.User, Content = "Test" }]
        };

        var answerDto = new AnswerDTO(
            Message: "Answer",
            HandoverToHumanNeeded: false,
            RetrievedSections:
            [
                new SectionRetrievedDTO(Content: "Section A", Score: 0.9f),
                new SectionRetrievedDTO(Content: "Section B", Score: 0.8f),
                new SectionRetrievedDTO(Content: "Section C", Score: 0.7f)
            ]
        );

        _mockRagEngine
            .Setup(x => x.GenerateResponseForUserInput(It.IsAny<string[]>()))
            .ReturnsAsync(answerDto);

        var result = await _controller.GenerateCompletions(request);

        var completionResponse = result.Value;
        Assert.NotNull(completionResponse);
        Assert.Equal(3, completionResponse.SectionsRetrieved.Length);
        Assert.Equal("Section A", completionResponse.SectionsRetrieved[0].Content);
        Assert.Equal(0.9f, completionResponse.SectionsRetrieved[0].Score);
        Assert.Equal("Section B", completionResponse.SectionsRetrieved[1].Content);
        Assert.Equal(0.8f, completionResponse.SectionsRetrieved[1].Score);
        Assert.Equal("Section C", completionResponse.SectionsRetrieved[2].Content);
        Assert.Equal(0.7f, completionResponse.SectionsRetrieved[2].Score);
    }
}
