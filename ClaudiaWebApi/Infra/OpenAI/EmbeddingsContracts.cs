namespace ClaudiaWebApi.Infra.OpenAI;

public record GetEmbeddingsRequest(string Model, string Input);

public record GetEmbeddingsResponse
{
    public Data[] Data { get; init; } = [];
};

public record Data
{
    public string Object { get; init; } = "";
    public int Index { get; init; }
    public float[] Embedding { get; init; } = [];
}
