using ClaudiaWebApi.Infra.OpenAI;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace Claudia.Tests;

public class UnitTest1
{
    [Fact]
    public async Task ShouldDeserializeCamelCaseFieldsToPascalCase()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    """
                    {
                      "data": [
                        {
                          "object": "embedding",
                          "index": 0,
                          "embedding": [0.1, 0.2, 0.3]
                        }
                      ]
                    }
                    """,
                 Encoding.UTF8,
                "application/json"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://mock.api.com/")
        };

        var openAIClient = new OpenAIClient(httpClient, Mock.Of<ILogger<OpenAIClient>>());
        var response = await openAIClient.GetEmbeddings(new GetEmbeddingsRequest("test-model", "test input"));

        Assert.NotNull(response);
        Assert.Single(response.Data);

        Assert.Equal("embedding", response.Data[0].Object);
        Assert.Equal(0, response.Data[0].Index);
        Assert.Equal([0.1f, 0.2f, 0.3f], response.Data[0].Embedding);
    }
}
