using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/[controller]/[action]")]
public class UsersController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<UsersController> logger;

    public UsersController(CineVaultDbContext dbContext, ILogger<UsersController> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        logger.LogInformation("GetUsers");
        var users = await this.dbContext.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        logger.LogInformation("GetUserById id:{id}", id);
        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            logger.LogWarning("NotFound id:{id}", id);
            return NotFound();
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        logger.LogInformation("CreateUser username:{username}", request.Username);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };

        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        logger.LogInformation("UpdateUser id:{id} username:{username}", id, request.Username);
        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            logger.LogWarning("NotFound id:{id}", id);
            return NotFound();
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.Password = request.Password;

        await this.dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        logger.LogInformation("DeleteUser id:{id}", id);
        var user = await this.dbContext.Users.FindAsync(id);

        if (user is null)
        {
            logger.LogWarning("NotFound id:{id}", id);
            return NotFound();
        }

        this.dbContext.Users.Remove(user);
        await this.dbContext.SaveChangesAsync();

        return Ok();
    }
}