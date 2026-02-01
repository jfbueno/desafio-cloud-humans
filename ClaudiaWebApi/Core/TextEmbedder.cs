using ClaudiaWebApi.Infra.OpenAI;

namespace ClaudiaWebApi.Core;

public interface ITextEmbedder
{
    Task<float[]> FromTextAsync(string text);
}

/// <summary>
/// Provides functionality to generate vector embeddings for text using an OpenAI client.
/// </summary>
/// <param name="openAIClient">
/// The OpenAIClient instance used to communicate with the OpenAI API for generating text embeddings. Cannot be null.
/// </param>
public sealed class TextEmbedder(IOpenAIClient openAIClient) : ITextEmbedder
{
    /// <summary>
    /// Generates an embedding vector for the specified text using a large language model.
    /// </summary>
    /// <param name="text">The input text to generate an embedding for. Cannot be null or empty.</param>
    /// <returns>A float array representing the embedding vector for the input text. Returns an empty array if the embedding
    /// could not be generated.</returns>
    public async Task<float[]> FromTextAsync(string text)
    {
        var embeddings = await openAIClient.GetEmbeddings(new GetEmbeddingsRequest
        (
            Model: EmbeddingModels.TextEmbedding3Large,
            Input: text
        ));

        return embeddings?.Data.FirstOrDefault()?.Embedding ?? [];
    }
}
