using ClaudiaWebApi.Infra.Tenants;
using ClaudiaWebApi.Infra.VectorDB;

namespace ClaudiaWebApi.Core;

/// <summary>
/// Provides methods to retrieve relevant context sections from a vector database based on embedding similarity 
/// within the current tenant's project.
/// </summary>
/// <param name="vectorDbClient">The client used to interact with the vector database for document retrieval.</param>
/// <param name="tenantContext">
/// The context containing tenant-specific information, such as the current project name, used to scope database queries.
/// </param>
public sealed class ContextRetriever(VectorDBApiClient vectorDbClient, TenantContext tenantContext)
{
    public async Task<ContextSection[]> GetContextFromEmbeddingsAsync(float[] embeddings)
    {
        var docsFound = await vectorDbClient.SearchDocsFromVector(new GetVectorsRequest
        (
            Filter: $"projectName eq '{tenantContext.ProjectName}'",
            Top: 10,
            Select: "content,type",
            Count: true,
            VectorQueries:
            [
                new VectorQuery
                (
                    Vector: embeddings,
                    K: 3,
                    Fields: "embeddings",
                    Kind: "vector"
                )
            ]
        ));

        return [.. docsFound.Value.Select(doc => new ContextSection(doc.Content, doc.Score, doc.Type))];
    }
}

/// <summary>
/// Represents a section of contextual information with associated content, relevance score, and type.
/// </summary>
/// <param name="Content">The textual content of the context section.</param>
/// <param name="Score">The relevance score of the section. Higher values indicate greater relevance.</param>
/// <param name="Type">
/// The type or category of the context section.
/// N1: Defines that the response is sufficient to meet the user's demand.
/// N2: Defines that the response may not be sufficient and human intervention may be necessary
/// </param>
public record ContextSection(string Content, float Score, string Type);
