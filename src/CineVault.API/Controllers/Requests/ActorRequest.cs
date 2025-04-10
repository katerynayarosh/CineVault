namespace CineVault.API.Controllers.Requests;

public sealed class ActorRequest
{
    public required string FullName { get; init; }
    public required DateOnly BirthDate { get; init; }
    public required string? Biography { get; init; }
}