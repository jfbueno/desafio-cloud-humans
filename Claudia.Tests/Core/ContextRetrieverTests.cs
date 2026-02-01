using ClaudiaWebApi.Core;
using ClaudiaWebApi.Infra.Tenants;
using ClaudiaWebApi.Infra.VectorDB;
using Moq;

namespace Claudia.Tests.Core;

public class ContextRetrieverTests
{
    private readonly Mock<IVectorDBApiClient> _mockVectorDbClient;
    private readonly TenantContext _tenantContext;
    private readonly ContextRetriever _contextRetriever;

    public ContextRetrieverTests()
    {
        _mockVectorDbClient = new Mock<IVectorDBApiClient>();
        _tenantContext = new TenantContext
        {
            ProjectName = "test_project",
            HelpdeskId = 1
        };
        _contextRetriever = new ContextRetriever(_mockVectorDbClient.Object, _tenantContext);
    }

    [Fact]
    public async Task GetContextFromEmbeddingsAsync_CallsVectorDbWithCorrectParameters()
    {
        var embeddings = new float[] { 0.1f, 0.2f, 0.3f };
        var mockResponse = new GetVectorsResponse
        {
            Value = []
        };

        _mockVectorDbClient
            .Setup(x => x.SearchDocsFromVector(It.IsAny<GetVectorsRequest>()))
            .ReturnsAsync(mockResponse);

        await _contextRetriever.GetContextFromEmbeddingsAsync(embeddings);

        _mockVectorDbClient.Verify(
            x => x.SearchDocsFromVector(
                It.Is<GetVectorsRequest>(req =>
                    req.Filter == "projectName eq 'test_project'" &&
                    req.Top == 10 &&
                    req.Select == "content,type" &&
                    req.Count == true &&
                    req.VectorQueries.Length == 1 &&
                    req.VectorQueries[0].Vector == embeddings &&
                    req.VectorQueries[0].K == 3 &&
                    req.VectorQueries[0].Fields == "embeddings" &&
                    req.VectorQueries[0].Kind == "vector"
                )
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetContextFromEmbeddingsAsync_ReturnsContextSections()
    {
        var embeddings = new float[] { 0.1f, 0.2f };
        var mockResponse = new GetVectorsResponse
        {
            Value =
            [
                new Vector { Content = "Content 1", Score = 0.95f, Type = "N1" },
                new Vector { Content = "Content 2", Score = 0.85f, Type = "N2" },
                new Vector { Content = "Content 3", Score = 0.75f, Type = "N1" }
            ]
        };

        _mockVectorDbClient
            .Setup(x => x.SearchDocsFromVector(It.IsAny<GetVectorsRequest>()))
            .ReturnsAsync(mockResponse);

        var result = await _contextRetriever.GetContextFromEmbeddingsAsync(embeddings);

        Assert.Equal(3, result.Length);
        Assert.Equal("Content 1", result[0].Content);
        Assert.Equal(0.95f, result[0].Score);
        Assert.Equal("N1", result[0].Type);
        Assert.Equal("Content 2", result[1].Content);
        Assert.Equal(0.85f, result[1].Score);
        Assert.Equal("N2", result[1].Type);
        Assert.Equal("Content 3", result[2].Content);
        Assert.Equal(0.75f, result[2].Score);
        Assert.Equal("N1", result[2].Type);
    }

    [Fact]
    public async Task GetContextFromEmbeddingsAsync_WithEmptyResponse_ReturnsEmptyArray()
    {
        var embeddings = new float[] { 0.1f };
        var mockResponse = new GetVectorsResponse
        {
            Value = []
        };

        _mockVectorDbClient
            .Setup(x => x.SearchDocsFromVector(It.IsAny<GetVectorsRequest>()))
            .ReturnsAsync(mockResponse);

        var result = await _contextRetriever.GetContextFromEmbeddingsAsync(embeddings);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetContextFromEmbeddingsAsync_UsesProjectNameFromTenantContext()
    {
        var embeddings = new float[] { 0.5f };
        var customTenantContext = new TenantContext
        {
            ProjectName = "different_project",
            HelpdeskId = 2
        };
        var customRetriever = new ContextRetriever(_mockVectorDbClient.Object, customTenantContext);
        var mockResponse = new GetVectorsResponse { Value = [] };

        _mockVectorDbClient
            .Setup(x => x.SearchDocsFromVector(It.IsAny<GetVectorsRequest>()))
            .ReturnsAsync(mockResponse);

        await customRetriever.GetContextFromEmbeddingsAsync(embeddings);

        _mockVectorDbClient.Verify(
            x => x.SearchDocsFromVector(
                It.Is<GetVectorsRequest>(req => req.Filter == "projectName eq 'different_project'")
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetContextFromEmbeddingsAsync_PreservesScoresAndTypes()
    {
        var embeddings = new float[] { 0.1f };
        var mockResponse = new GetVectorsResponse
        {
            Value =
            [
                new Vector { Content = "High score", Score = 0.99f, Type = "N1" },
                new Vector { Content = "Low score", Score = 0.10f, Type = "N2" }
            ]
        };

        _mockVectorDbClient
            .Setup(x => x.SearchDocsFromVector(It.IsAny<GetVectorsRequest>()))
            .ReturnsAsync(mockResponse);

        var result = await _contextRetriever.GetContextFromEmbeddingsAsync(embeddings);

        Assert.Equal(0.99f, result[0].Score);
        Assert.Equal("N1", result[0].Type);
        Assert.Equal(0.10f, result[1].Score);
        Assert.Equal("N2", result[1].Type);
    }
}
