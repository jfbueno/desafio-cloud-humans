namespace ClaudiaWebApi.Infra.VectorDB;

public interface IVectorDBApiClient
{
    Task<GetVectorsResponse> SearchDocsFromVector(GetVectorsRequest request);
}
