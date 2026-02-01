using ClaudiaWebApi.Infra.Web.ErrorHandling;
using System.Text.Json;

namespace ClaudiaWebApi.Infra.VectorDB;

/// <summary>
/// HTTP Client for interacting with a vector database API.
/// Provides methods for performing document search operations based on similarity.
/// </summary>
/// <param name="httpClient">The HTTP client instance used to send requests to the vector database API. 
/// Must be configured with the appropriate base address and authentication.
/// </param>
/// <param name="logger">The logger used to record diagnostic and operational information for the client.</param>
public sealed class VectorDBApiClient(HttpClient httpClient, ILogger<VectorDBApiClient> logger) : IVectorDBApiClient
{
    private const string ApiVersion = "2023-11-01";
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<GetVectorsResponse> SearchDocsFromVector(GetVectorsRequest request)
    {
        logger.LogDebug("Sending VectorDB SearchDocsFromVector request using API Version {ApiVersion}", ApiVersion);
        var response = await httpClient.PostAsJsonAsync($"indexes/claudia-ids-index-large/docs/search?api-version={ApiVersion}", request);

        response.EnsureSuccessStatusCode();
        UpstreamException.ThrowIfResponseIsEmpty(response);

        return await response.Content.ReadFromJsonAsync<GetVectorsResponse>(_jsonSerializerOptions) 
            ?? throw new UpstreamException(response.RequestMessage?.RequestUri?.ToString() ?? "", "Received null response for VectorDB Search request.");
    }
}
