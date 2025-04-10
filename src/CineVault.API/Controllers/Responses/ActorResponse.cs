namespace CineVault.API.Controllers.Responses;

public sealed class ActorResponse
{
    public required int Id { get; set; }
    public required string FullName { get; set; }
    public required DateOnly BirthDate { get; set; }
    public required string? Biography { get; set; }
}