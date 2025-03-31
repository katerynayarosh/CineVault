namespace CineVault.API.Controllers.Responses;

public class ApiResponse
{
    public required int StatusCode { get; set; }
    public required string Message { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    public Dictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Guid ResponseId { get; } = Guid.NewGuid();
}

public class ApiResponse<TResponseData> : ApiResponse
{
    public required TResponseData Data { get; set; }
}