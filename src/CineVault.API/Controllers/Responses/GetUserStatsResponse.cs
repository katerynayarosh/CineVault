namespace CineVault.API.Controllers.Responses;

public class GetUserStatsResponse
{
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public List<GenreStatistic> GenreStatistics { get; set; }
    public DateTime? LastActivity { get; set; }
}

public class GenreStatistic
{
    public string Genre { get; set; }
    public int Count { get; set; }
    public decimal AverageRating { get; set; }
}