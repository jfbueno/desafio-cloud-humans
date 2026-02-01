using System.ComponentModel.DataAnnotations;

namespace ClaudiaWebApi.Controllers.Conversations;

public record CompletionRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "HelpdeskId must be greater than 0")]
    public int HelpdeskId { get; init; }


    [Required(ErrorMessage = "ProjectName is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "ProjectName must be between 1 and 200 characters")]
    public string ProjectName { get; init; }


    [Required(ErrorMessage = "Messages are required")]
    [MinLength(1, ErrorMessage = "At least one message is required")]
    [MaxLength(15, ErrorMessage = "Maximum of 15 messages allowed")]
    public Message[] Messages { get; init; }
}

public record Message
{
    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("^(USER|AGENT)$", ErrorMessage = "Role must be either 'USER' or 'AGENT'")]
    public string Role { get; init; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 10000 characters")]
    public string Content { get; init; }
}

public record MessageRole
{
    public const string User = "USER";
    public const string Agent = "AGENT";
}

public record CompletionResponse(Message[] Messages, bool HandoverToHumanNeeded, Section[] SectionsRetrieved);

public record Section(float Score, string Content);
