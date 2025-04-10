namespace CineVault.API.Controllers.Requests;

public class SearchMovies2Request
{
    public string SearchText { get; set; }
    public string Genre { get; set; }
    public decimal? MinRating { get; set; }
    public DateOnly? ReleaseDateFrom { get; set; }
    public DateOnly? ReleaseDateTo { get; set; }
}