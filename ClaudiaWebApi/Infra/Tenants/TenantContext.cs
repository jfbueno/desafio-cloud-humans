namespace ClaudiaWebApi.Infra.Tenants;

/// <summary>
/// Represents contextual information for a tenant, including project and helpdesk identifiers.
/// </summary>
public sealed record TenantContext()
{
    public string ProjectName { get; set; } = "";
    public int HelpdeskId { get; set; }
}
