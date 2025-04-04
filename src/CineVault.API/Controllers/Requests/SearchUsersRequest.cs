namespace CineVault.API.Controllers.Requests;

public class SearchUsersRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}