using System.Text.Json.Serialization;

namespace ClaudiaWebApi.Infra.VectorDB;

public record GetVectorsRequest(string Filter, int Top, string Select, bool Count, VectorQuery[] VectorQueries);

public record VectorQuery(float[] Vector, int K, string Fields, string Kind);

public record GetVectorsResponse
{
    public Vector[] Value { get; init; } = [];
}

public record Vector
{
    [JsonPropertyName("@search.score")]
    public float Score { get; init; }
    public string Content { get; init; } = "";
    public string Type { get; init; } = "";
}
