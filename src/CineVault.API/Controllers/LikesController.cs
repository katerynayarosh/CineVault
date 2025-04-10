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
public sealed class LikesController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<LikesController> logger;
    private readonly IMapper mapper;

    public LikesController(CineVaultDbContext dbContext, ILogger<LikesController> logger,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> CreateLike(
        ApiRequest<LikeRequest> request)
    {
        this.logger.LogInformation("CreateLike");

        var existingLike =
            await this.dbContext.Likes
                .Where(l => l.ReviewId == request.Data.ReviewId && l.UserId == request.Data.UserId)
                .FirstOrDefaultAsync();
        if (existingLike is not null)
        {
            return this.BadRequest(new ApiResponse { StatusCode = 400, Message = "Like exists" });
        }

        var like = this.mapper.Map<Like>(request.Data);

        this.dbContext.Likes.Add(like);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Like created" });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> DeleteLike(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteLike");

        var like = await this.dbContext.Likes.FindAsync(id);

        if (like is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "Like not found" });
        }

        like.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Like deleted" });
    }
}