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
public class UsersController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<UsersController> logger;
    private readonly IMapper mapper;

    public UsersController(CineVaultDbContext dbContext, ILogger<UsersController> logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        this.logger.LogInformation("GetUsers");
        var users = await this.dbContext.Users
            .Select(u => new UserResponse { Id = u.Id, Username = u.Username, Email = u.Email })
            .ToListAsync();

        return this.Ok(users);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        this.logger.LogInformation("GetUserById id:{id}", id);
        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        return this.Ok(response);
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        this.logger.LogInformation("CreateUser username:{username}", request.Username);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };

        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        this.logger.LogInformation("UpdateUser id:{id} username:{username}", id, request.Username);
        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.Password = request.Password;

        await this.dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        this.logger.LogInformation("DeleteUser id:{id}", id);
        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        this.dbContext.Users.Remove(user);
        await this.dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<UserResponse>>>> GetUsers(
        ApiRequest request)
    {
        this.logger.LogInformation("GetUsers");

        var users = await this.dbContext.Users
            .ToListAsync();

        var userResponses = this.mapper.Map<ICollection<UserResponse>>(users);

        return this.Ok(new ApiResponse<ICollection<UserResponse>>
        {
            StatusCode = 200,
            Message = "Users retrieved",
            Data = userResponses
        });
    }

    [HttpPost("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetUserById id:{id}", id);

        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "User not found" });
        }

        var response = this.mapper.Map<UserResponse>(user);

        return this.Ok(new ApiResponse<UserResponse>
        {
            StatusCode = 200,
            Message = "User retrieved",
            Data = response
        });
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<int>>> CreateUser(ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation("CreateUser username:{username}", request.Data.Username);

        var user = this.mapper.Map<User>(request.Data);

        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200,
            Message = "User created",
            Data = user.Id
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> UpdateUser(int id, ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation("UpdateUser id:{id} username:{username}", id,
            request.Data.Username);

        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "User not found" });
        }

        this.mapper.Map(request.Data, user);

        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "User updated" });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteUser id:{id}", id);

        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "User not found" });
        }

        this.dbContext.Users.Remove(user);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "User deleted" });
    }
}