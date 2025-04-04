namespace CineVault.API.Controllers.Responses;

public sealed class DeleteMoviesResponse
{
    public ICollection<int> DeletedIds { get; set; } = new List<int>();
    public ICollection<int> NotFoundIds { get; set; } = new List<int>();
    public ICollection<int> HasReviewsIds { get; set; } = new List<int>();
}