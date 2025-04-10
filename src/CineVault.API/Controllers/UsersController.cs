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

    public UsersController(CineVaultDbContext dbContext, ILogger<UsersController> logger,
        IMapper mapper)
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
            Id = user.Id, Username = user.Username, Email = user.Email
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
            Username = request.Username, Email = request.Email, Password = request.Password
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
            StatusCode = 200, Message = "Users retrieved", Data = userResponses
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

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = $"User with ID {id} not found in database" });
        }

        var response = this.mapper.Map<UserResponse>(user);

        return this.Ok(new ApiResponse<UserResponse>
        {
            StatusCode = 200, Message = "User retrieved", Data = response
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
            StatusCode = 200, Message = $"User '{request.Data.Username}' successfully created with ID {user.Id}",  Data = user.Id
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

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = $"User with ID {id} not found in database" });
        }

        this.mapper.Map(request.Data, user);

        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = $"User profile (ID: {id}) was fully updated" 
        });
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

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = $"User with ID {id} not found in database" });
        }

        user.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = $"User account (ID: {id}) marked as deleted" });
    }

    // TODO 13 Пагінація
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<UserResponse>>>> SearchUsers(
        ApiRequest<SearchUsersRequest> request)
    {
        this.logger.LogInformation("SearchUsers");

        var query = this.dbContext.Users.AsQueryable();

        if (!string.IsNullOrEmpty(request.Data.Username))
        {
            query = query.Where(u => u.Username.Contains(request.Data.Username));
        }

        if (!string.IsNullOrEmpty(request.Data.Email))
        {
            query = query.Where(u => u.Email.Contains(request.Data.Email));
        }

        if (request.Data.FromDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= request.Data.FromDate);
        }

        if (request.Data.ToDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= request.Data.ToDate);
        }

        string sortProperty = request.Data.SortBy?.Trim() ?? "Username";
        string sortDirection = request.Data.SortDirection?.Trim().ToLower() ?? "asc";
        query = sortDirection switch
        {
            "asc" => sortProperty switch
            {
                "Email" => query.OrderBy(u => u.Email),
                _ => query.OrderBy(u => u.Username)
            },
            "desc" => sortProperty switch
            {
                "Email" => query.OrderByDescending(u => u.Email),
                _ => query.OrderByDescending(u => u.Username)
            },
            _ => query.OrderBy(u => u.Username)
        };

        var users = await query
            .Skip(((request.Data.PageNumber ?? 1) - 1) * (request.Data.PageSize ?? 10))
            .Take(request.Data.PageSize ?? 10)
            .ToListAsync();

        var userResponses = this.mapper.Map<ICollection<UserResponse>>(users);

        return this.Ok(new ApiResponse<ICollection<UserResponse>>
        {
            StatusCode = 200, Message = "Users retrieved", Data = userResponses
        });
    }

    // TODO 9 GetUserStats
    [HttpPost("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<GetUserStatsResponse>>> GetUserStats(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetUserStats id:{id}", id);

        var user = await this.dbContext.Users
            .Include(u => u.Reviews)
            .ThenInclude(r => r.Movie)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            this.logger.LogWarning("User not found id:{id}", id);
            return this.NotFound(new ApiResponse { StatusCode = 404, Message = $"User with ID {id} not found in database" });
        }

        var stats = new GetUserStatsResponse
        {
            TotalReviews = user.Reviews.Count,
            AverageRating =
                user.Reviews.Any() ? (decimal)user.Reviews.Average(r => r.Rating) : 0,
            LastActivity = user.Reviews.Max(r => (DateTime?)r.CreatedAt),
            GenreStatistics = user.Reviews
                .Where(r => r.Movie != null && !string.IsNullOrEmpty(r.Movie.Genre))
                .GroupBy(r => r.Movie!.Genre)
                .Select(g => new GenreStatistic
                {
                    Genre = g.Key!,
                    Count = g.Count(),
                    AverageRating = (decimal)g.Average(r => r.Rating)
                })
                .OrderByDescending(g => g.Count)
                .ToList()
        };

        return this.Ok(new ApiResponse<GetUserStatsResponse>
        {
            StatusCode = 200, Message = "User stats retrieved", Data = stats
        });
    }
    
    // TODO 10 Покажіть приклад, коли потрібно ігнорувати даний глобальний фільтр
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<int>>> GetTotalUsersCount(ApiRequest request)
    {
        this.logger.LogInformation("GetTotalUsersCount");

        int totalCount = await this.dbContext.Users
            .IgnoreQueryFilters()
            .CountAsync();

        return this.Ok(new ApiResponse<int> 
        { 
            StatusCode = 200,
            Message = "Total registered users count (including deleted)",
            Data = totalCount
        });
    }
}