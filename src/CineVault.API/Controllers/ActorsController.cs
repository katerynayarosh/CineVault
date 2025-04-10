using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/v{v:apiVersion}/[controller]/[action]")]
[ApiVersion(1)]
[ApiVersion(2)]
public class ActorsController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<ActorsController> logger;
    private readonly IMapper mapper;

    public ActorsController(CineVaultDbContext dbContext, ILogger<ActorsController> logger,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<ActorResponse>>>> GetActors(
        ApiRequest request)
    {
        this.logger.LogInformation("GetActors");

        var actors = await this.dbContext.Actors.ToListAsync();

        var actorResponses = this.mapper.Map<ICollection<ActorResponse>>(actors);

        return this.Ok(new ApiResponse<ICollection<ActorResponse>>
        {
            StatusCode = 200, Message = "Actors retrieved", Data = actorResponses
        });
    }

    [HttpPost("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> GetActorById(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetActorById id:{id}", id);

        var actor = await this.dbContext.Actors.FindAsync(id);

        if (actor is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "Actor not found" });
        }

        var response = this.mapper.Map<ActorResponse>(actor);

        return this.Ok(new ApiResponse<ActorResponse>
        {
            StatusCode = 200, Message = "Actor retrieved", Data = response
        });
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<int>>> CreateActor(ApiRequest<ActorRequest> request)
    {
        this.logger.LogInformation("CreateActor");

        var actor = this.mapper.Map<Actor>(request.Data);

        this.dbContext.Actors.Add(actor);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200, Message = "Actor created", Data = actor.Id
        });
    }
    
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<int>>>> CreateActors(ApiRequest<ICollection<ActorRequest>> request)
    {
        this.logger.LogInformation("CreateActors");

        var actors = this.mapper.Map<ICollection<Actor>>(request.Data);

        this.dbContext.Actors.AddRange(actors);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse<ICollection<int>>
        {
            StatusCode = 200, Message = "Actors created", Data = actors.Select(a => a.Id).ToList()
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> UpdateActor(int id, ApiRequest<ActorRequest> request)
    {
        this.logger.LogInformation("UpdateActor");

        var actor = await this.dbContext.Actors.FindAsync(id);

        if (actor is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "Actor not found" });
        }

        this.mapper.Map(request.Data, actor);

        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Actor updated" });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> DeleteActor(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteActor id:{id}", id);

        var actor = await this.dbContext.Actors.FindAsync(id);

        if (actor is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { 
                StatusCode = 404,
                Message = "Actor not found"
            });
        }

        actor.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Actor deleted" });
    }
}