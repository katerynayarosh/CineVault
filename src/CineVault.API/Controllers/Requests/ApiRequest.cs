namespace CineVault.API.Controllers.Requests;

public class ApiRequest
{
    public Dictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();
    public string ClientVersion { get; set; } = "Undefined";
    public string ClientSource { get; set; } = "Unknown";
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Guid RequestId { get; } = Guid.NewGuid();
}

public class ApiRequest<TRequestData> : ApiRequest
{
    public required TRequestData Data { get; set; }
}