using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Mapster;

namespace CineVault.API.Profiles;

public class ReviewProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ReviewRequest, Review>();

        config.NewConfig<Review, ReviewResponse>()
            .Map(r => r.MovieTitle,
                r => r.Movie!.Title)
            .Map(r => r.Username,
                r => r.User!.Username);
    }
}