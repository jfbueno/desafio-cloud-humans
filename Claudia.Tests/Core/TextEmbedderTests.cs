using ClaudiaWebApi.Core;
using ClaudiaWebApi.Infra.OpenAI;
using Moq;

namespace Claudia.Tests.Core;

public class TextEmbedderTests
{
    private readonly Mock<IOpenAIClient> _mockOpenAIClient;
    private readonly TextEmbedder _textEmbedder;

    public TextEmbedderTests()
    {
        _mockOpenAIClient = new Mock<IOpenAIClient>();
        _textEmbedder = new TextEmbedder(_mockOpenAIClient.Object);
    }

    [Fact]
    public async Task FromTextAsync_CallsOpenAIClientWithCorrectParameters()
    {
        var inputText = "Hello, world!";
        var mockResponse = new GetEmbeddingsResponse
        {
            Data = [new Data { Embedding = [0.1f, 0.2f, 0.3f] }]
        };

        _mockOpenAIClient
            .Setup(x => x.GetEmbeddings(It.IsAny<GetEmbeddingsRequest>()))
            .ReturnsAsync(mockResponse);

        await _textEmbedder.FromTextAsync(inputText);

        _mockOpenAIClient.Verify(
            x => x.GetEmbeddings(
                It.Is<GetEmbeddingsRequest>(req =>
                    req.Model == EmbeddingModels.TextEmbedding3Large &&
                    req.Input == inputText
                )
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task FromTextAsync_ReturnsEmbeddingsFromResponse()
    {
        var expectedEmbeddings = new float[] { 0.1f, 0.2f, 0.3f, 0.4f };
        var mockResponse = new GetEmbeddingsResponse
        {
            Data = [new Data { Embedding = expectedEmbeddings }]
        };

        _mockOpenAIClient
            .Setup(x => x.GetEmbeddings(It.IsAny<GetEmbeddingsRequest>()))
            .ReturnsAsync(mockResponse);

        var result = await _textEmbedder.FromTextAsync("test");

        Assert.Equal(expectedEmbeddings, result);
    }

    [Fact]
    public async Task FromTextAsync_WithNullResponse_ReturnsEmptyArray()
    {
        _mockOpenAIClient
            .Setup(x => x.GetEmbeddings(It.IsAny<GetEmbeddingsRequest>()))
            .ReturnsAsync((GetEmbeddingsResponse)null!);

        var result = await _textEmbedder.FromTextAsync("test");

        Assert.Empty(result);
    }

    [Fact]
    public async Task FromTextAsync_WithEmptyDataArray_ReturnsEmptyArray()
    {
        var mockResponse = new GetEmbeddingsResponse
        {
            Data = []
        };

        _mockOpenAIClient
            .Setup(x => x.GetEmbeddings(It.IsAny<GetEmbeddingsRequest>()))
            .ReturnsAsync(mockResponse);

        var result = await _textEmbedder.FromTextAsync("test");

        Assert.Empty(result);
    }
}
