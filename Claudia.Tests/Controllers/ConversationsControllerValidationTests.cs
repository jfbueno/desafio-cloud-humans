using ClaudiaWebApi.Controllers.Conversations;
using System.ComponentModel.DataAnnotations;

namespace Claudia.Tests.Controllers;

public class ConversationsControllerValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void CompletionRequest_WithValidData_PassesValidation()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "ValidProject",
            Messages = [new Message { Role = MessageRole.User, Content = "Valid message" }]
        };

        var validationResults = ValidateModel(request);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CompletionRequest_WithInvalidHelpdeskId_FailsValidation(int helpdeskId)
    {
        var request = new CompletionRequest
        {
            HelpdeskId = helpdeskId,
            ProjectName = "ValidProject",
            Messages = [new Message { Role = MessageRole.User, Content = "Valid message" }]
        };

        var validationResults = ValidateModel(request);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "HelpdeskId must be greater than 0");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CompletionRequest_WithNullOrEmptyProjectName_FailsValidation(string projectName)
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = projectName,
            Messages = [new Message { Role = MessageRole.User, Content = "Valid message" }]
        };

        var validationResults = ValidateModel(request);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => 
            v.ErrorMessage == "ProjectName is required" || 
            v.ErrorMessage == "ProjectName must be between 1 and 200 characters");
    }

    [Fact]
    public void CompletionRequest_WithProjectNameTooLong_FailsValidation()
    {
        var longProjectName = new string('A', 201);
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = longProjectName,
            Messages = [new Message{ Role = MessageRole.User, Content = "Valid message" }]
        };

        var validationResults = ValidateModel(request);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "ProjectName must be between 1 and 200 characters");
    }

    [Fact]
    public void CompletionRequest_WithNullMessages_FailsValidation()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "ValidProject",
            Messages = null!
        };

        var validationResults = ValidateModel(request);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "Messages are required");
    }

    [Fact]
    public void CompletionRequest_WithEmptyMessagesArray_FailsValidation()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "ValidProject",
            Messages = []
        };

        var validationResults = ValidateModel(request);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "At least one message is required");
    }

    [Fact]
    public void CompletionRequest_WithTooManyMessages_FailsValidation()
    {
        var messages = Enumerable.Range(0, 16)
            .Select(i => new Message { Role = MessageRole.User, Content = $"Message {i}" })
            .ToArray();

        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "ValidProject",
            Messages = messages
        };

        var validationResults = ValidateModel(request);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "Maximum of 15 messages allowed");
    }

    [Fact]
    public void CompletionRequest_With15Messages_PassesValidation()
    {
        var messages = Enumerable.Range(0, 15)
            .Select(i => new Message{ Role = MessageRole.User, Content = $"Message {i}" })
            .ToArray();

        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "ValidProject",
            Messages = messages
        };

        var validationResults = ValidateModel(request);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Message_WithNullOrEmptyRole_FailsValidation(string role)
    {
        var message = new Message
        {
            Role = role,
            Content = "Valid content"
        };

        var validationResults = ValidateModel(message);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "Role is required");
    }

    [Theory]
    [InlineData("user")]
    [InlineData("agent")]
    [InlineData("SYSTEM")]
    [InlineData("ADMIN")]
    [InlineData("Unknown")]
    public void Message_WithInvalidRole_FailsValidation(string role)
    {
        var message = new Message
        {
            Role = role,
            Content = "Valid content"
        };

        var validationResults = ValidateModel(message);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "Role must be either 'USER' or 'AGENT'");
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("AGENT")]
    public void Message_WithValidRole_PassesValidation(string role)
    {
        var message = new Message
        {
            Role = role,
            Content = "Valid content"
        };

        var validationResults = ValidateModel(message);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Message_WithNullOrEmptyContent_FailsValidation(string content)
    {
        var message = new Message
        {
            Role = MessageRole.User,
            Content = content
        };

        var validationResults = ValidateModel(message);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => 
            v.ErrorMessage == "Content is required" || 
            v.ErrorMessage == "Content must be between 1 and 10000 characters");
    }

    [Fact]
    public void Message_WithContentTooLong_FailsValidation()
    {
        var longContent = new string('A', 10001);
        var message = new Message
        {
            Role = MessageRole.User,
            Content = longContent
        };

        var validationResults = ValidateModel(message);

        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage == "Content must be between 1 and 10000 characters");
    }

    [Fact]
    public void Message_WithMaxLengthContent_PassesValidation()
    {
        var maxLengthContent = new string('A', 10000);
        var message = new Message
        {
            Role = MessageRole.User,
            Content = maxLengthContent
        };

        var validationResults = ValidateModel(message);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void CompletionRequest_WithAllFieldsAtBoundaries_PassesValidation()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 1,
            ProjectName = "A",
            Messages =
            [
                new Message{ Role = MessageRole.User, Content = "A" }
            ]
        };

        var validationResults = ValidateModel(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void CompletionRequest_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        var request = new CompletionRequest
        {
            HelpdeskId = 0,
            ProjectName = "",
            Messages = []
        };

        var validationResults = ValidateModel(request);

        Assert.NotEmpty(validationResults);
        Assert.True(validationResults.Count >= 3, "Should have at least 3 validation errors");
    }
}
