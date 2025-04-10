using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Mapster;

namespace CineVault.API.Profiles;

public class ActorProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ActorRequest, Actor>();

        config.NewConfig<Actor, ActorResponse>();
    }
}