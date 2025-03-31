using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Mapster;

namespace CineVault.API.Profiles;

public class UserProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserRequest, User>();

        config.NewConfig<User, UserResponse>();
    }
}