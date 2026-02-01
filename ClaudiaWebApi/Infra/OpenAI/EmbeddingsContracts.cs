namespace ClaudiaWebApi.Infra.OpenAI;

public record GetEmbeddingsRequest(string Model, string Input);

public record GetEmbeddingsResponse
{
    // TODO: Escrever isso no Readme.md
    // Aqui não vamos usar o construtor porque o System.Text precisa que os parâmetros do ctor tenham
    // o mesmo nome dos campos JSON (não usando a convenção de converter de camelCase pra PascalCase).
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/immutability
    public Data[] Data { get; init; } = [];
};

public record Data
{
    public string Object { get; init; } = "";
    public int Index { get; init; }
    public float[] Embedding { get; init; } = [];
}
