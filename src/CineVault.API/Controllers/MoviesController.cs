using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/v{v:apiVersion}/[controller]/[action]")]
[ApiVersion(1)]
[ApiVersion(2)]
public sealed class MoviesController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<MoviesController> logger;

    public MoviesController(CineVaultDbContext dbContext, ILogger<MoviesController> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<MovieResponse>>> GetMovies()
    {
        this.logger.LogInformation("GetMovies");
        var movies = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Select(m => new MovieResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genre = m.Genre,
                Director = m.Director,
                AverageRating = m.Reviews.Count != 0
                    ? m.Reviews.Average(r => r.Rating)
                    : 0,
                ReviewCount = m.Reviews.Count
            })
            .ToListAsync();
        return this.Ok(movies);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<MovieResponse>> GetMovieById(int id)
    {
        this.logger.LogInformation("GetMovieById id:{id}", id);
        var movie = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = movie.Reviews.Count != 0
                ? movie.Reviews.Average(r => r.Rating)
                : 0,
            ReviewCount = movie.Reviews.Count
        };
        return this.Ok(response);
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateMovie(MovieRequest request)
    {
        this.logger.LogInformation("CreateMovie title:{title}", request.Title);
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            ReleaseDate = request.ReleaseDate,
            Genre = request.Genre,
            Director = request.Director
        };
        await this.dbContext.Movies.AddAsync(movie);
        await this.dbContext.SaveChangesAsync();
        return this.Created();
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateMovie(int id, MovieRequest request)
    {
        this.logger.LogInformation("UpdateMovie id:{id} title:{title}", id, request.Title);
        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genre = request.Genre;
        movie.Director = request.Director;
        await this.dbContext.SaveChangesAsync();
        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        this.logger.LogInformation("DeleteMovie id:{id}", id);
        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        this.dbContext.Movies.Remove(movie);
        await this.dbContext.SaveChangesAsync();
        return this.Ok();
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<MovieResponse>>>> GetMovies(
        ApiRequest request)
    {
        this.logger.LogInformation("GetMovies");

        var movies = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Select(m => new MovieResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genre = m.Genre,
                Director = m.Director,
                AverageRating = m.Reviews.Count != 0
                    ? m.Reviews.Average(r => r.Rating)
                    : 0,
                ReviewCount = m.Reviews.Count
            })
            .ToListAsync();

        return this.Ok(new ApiResponse<ICollection<MovieResponse>>
        {
            StatusCode = 200,
            Message = "Movies retrieved",
            Data = movies
        });
    }

    [HttpPost("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieById(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetMovieById id:{id}", id);

        var movie = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "Movie not found" });
        }

        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = movie.Reviews.Count != 0
                ? movie.Reviews.Average(r => r.Rating)
                : 0,
            ReviewCount = movie.Reviews.Count
        };

        return this.Ok(new ApiResponse<MovieResponse>
        {
            StatusCode = 200,
            Message = "Movies retrieved",
            Data = response
        });
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<int>>> CreateMovie(
        ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("CreateMovie title:{title}", request.Data.Title);

        var movie = new Movie
        {
            Title = request.Data.Title,
            Description = request.Data.Description,
            ReleaseDate = request.Data.ReleaseDate,
            Genre = request.Data.Genre,
            Director = request.Data.Director
        };

        await this.dbContext.Movies.AddAsync(movie);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200,
            Message = "Movie created",
            Data = movie.Id
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> UpdateMovie(int id,
        ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("UpdateMovie id:{id} title:{title}", id, request.Data.Title);

        var movie = await this.dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "Movie not found" });
        }

        movie.Title = request.Data.Title;
        movie.Description = request.Data.Description;
        movie.ReleaseDate = request.Data.ReleaseDate;
        movie.Genre = request.Data.Genre;
        movie.Director = request.Data.Director;

        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Movie updated" });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult> DeleteMovie(ApiRequest request, int id)
    {
        this.logger.LogInformation("DeleteMovie id:{id}", id);

        var movie = await this.dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "Movie not found" });
        }

        this.dbContext.Movies.Remove(movie);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Movie deleted" });
    }
}