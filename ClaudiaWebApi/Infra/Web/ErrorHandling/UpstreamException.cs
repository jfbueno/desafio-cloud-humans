namespace ClaudiaWebApi.Infra.Web.ErrorHandling;

public sealed class UpstreamException(string upstream, string message) : Exception(
    $"Upstream service '{upstream}' returned an invalid response. {message}"
)
{
    public string Upstream { get; } = upstream;

    public static void ThrowIfResponseIsEmpty(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentLength == 0)
            throw new UpstreamException(response.RequestMessage?.RequestUri?.ToString() ?? "", "Empty response from server.");
    }
}
