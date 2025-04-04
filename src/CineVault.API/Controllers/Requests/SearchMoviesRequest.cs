namespace CineVault.API.Controllers.Requests;

public class SearchMoviesRequest
{
    public string? Title { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public int? Year { get; set; }
    public decimal? MinRating { get; set; }
}