namespace CineVault.API.Controllers.Responses;

public class GetMovieDetailsResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string Genre { get; set; }
    public string Director { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<InternalReviewResponse> RecentReviews { get; set; }
}

public class InternalReviewResponse
{
    public string Username { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}