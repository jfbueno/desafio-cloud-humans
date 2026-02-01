using ClaudiaWebApi.Infra.Web.ErrorHandling;
using System.Text.Json;

namespace ClaudiaWebApi.Infra.OpenAI;

public interface IOpenAIClient
{
    Task<GetEmbeddingsResponse> GetEmbeddings(GetEmbeddingsRequest request);
    Task<ChatCompletionResponse> GenerateCompletion(GenerateCompletionRequest request);
}

/// <summary>
/// HTTP Client for interacting with the OpenAI API.
/// Provides methods for interacting with the OpenAI API, including generating chat completions and retrieving
/// embeddings.
/// </summary>
/// <param name="httpClient">The HTTP client instance used to send requests to the OpenAI API. 
/// Must be configured with the appropriate base address and authentication headers.</param>
/// <param name="logger">The logger used to record diagnostic and error information during API operations.</param>
public sealed class OpenAIClient(HttpClient httpClient, ILogger<OpenAIClient> logger) : IOpenAIClient
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<GetEmbeddingsResponse> GetEmbeddings(GetEmbeddingsRequest request)
    {
        logger.LogDebug("Requesting embeddings from OpenAI API for model: {Model}", request.Model);

        var response = await _httpClient.PostAsJsonAsync("embeddings", request);
        response.EnsureSuccessStatusCode();

        UpstreamException.ThrowIfResponseIsEmpty(response);

        return await response.Content.ReadFromJsonAsync<GetEmbeddingsResponse>(_jsonSerializerOptions) 
            ?? throw new UpstreamException(response.RequestMessage?.RequestUri?.ToString() ?? "", "Received null response for embeddings request.");
    }

    public async Task<ChatCompletionResponse> GenerateCompletion(GenerateCompletionRequest request)
    {
        logger.LogDebug("Requesting chat completion from OpenAI API for model: {Model}", request.Model);

        var response = await _httpClient.PostAsJsonAsync("chat/completions", request);
        response.EnsureSuccessStatusCode();

        UpstreamException.ThrowIfResponseIsEmpty(response);

        return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(_jsonSerializerOptions)
            ?? throw new UpstreamException(response.RequestMessage?.RequestUri?.ToString() ?? "", "Received null response for chat completion request.");
    }
}

