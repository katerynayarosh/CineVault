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
public sealed class MoviesController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<MoviesController> logger;
    private readonly IMapper mapper;

    public MoviesController(CineVaultDbContext dbContext, ILogger<MoviesController> logger,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
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
            .ToListAsync();

        var moviesResponses = this.mapper.Map<List<MovieResponse>>(movies);

        return this.Ok(new ApiResponse<ICollection<MovieResponse>>
        {
            StatusCode = 200,
            Message = "Movies retrieved",
            Data = moviesResponses
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

        var response = this.mapper.Map<MovieResponse>(movie);

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

        var movie = this.mapper.Map<Movie>(request.Data);

        await this.dbContext.Movies.AddAsync(movie);
        await this.dbContext.SaveChangesAsync();

        // TODO 8 Доробити всі методи сreate, додавши повернення id новоствореного об’єкта сутності або масив ids та назвами фільмів для створених об’єктів
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

        this.mapper.Map(request.Data, movie);

        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Movie updated" });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> DeleteMovie(ApiRequest request, int id)
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

    // TODO 1 Додати реалізацію масового завантаження фільмів
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<int>>>> CreateMovies(
        ApiRequest<ICollection<MovieRequest>> request)
    {
        this.logger.LogInformation("CreateMovies");

        var movies = this.mapper.Map<ICollection<Movie>>(request.Data);

        await this.dbContext.Movies.AddRangeAsync(movies);
        await this.dbContext.SaveChangesAsync();

        // TODO 8 Доробити всі методи сreate, додавши повернення id новоствореного об’єкта сутності або масив ids та назвами фільмів для створених об’єктів
        return this.Ok(new ApiResponse<ICollection<int>>
        {
            StatusCode = 200,
            Message = "Movies created",
            Data = movies.Select(m => m.Id).ToList()
        });
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<MovieResponse>>>> SearchMovies(
        ApiRequest<SearchMoviesRequest> request)
    {
        this.logger.LogInformation("SearchMovies");

        var query = this.dbContext.Movies
            .Include(m => m.Reviews)
            .AsQueryable();

        // TODO 3 Реалізувати пошук фільмів за жанром, назвою або режисером
        if (!string.IsNullOrEmpty(request.Data.Title))
        {
            query = query.Where(m => m.Title.Contains(request.Data.Title));
        }

        if (!string.IsNullOrEmpty(request.Data.Genre))
        {
            query = query.Where(m => m.Genre == request.Data.Genre);
        }

        if (!string.IsNullOrEmpty(request.Data.Director))
        {
            query = query.Where(m =>
                m.Director != null && m.Director.Contains(request.Data.Director));
        }

        // TODO 3 Додати фільтрацію за роком випуску та середнім рейтингом
        if (request.Data.Year.HasValue)
        {
            query = query.Where(m =>
                m.ReleaseDate != null && m.ReleaseDate.Value.Year == request.Data.Year.Value);
        }

        if (request.Data.MinRating.HasValue)
        {
            query = query.Where(m =>
                (decimal?)m.Reviews.Average(r => r.Rating) >= request.Data.MinRating ||
                (!m.Reviews.Any() && request.Data.MinRating <= 0));
        }

        var movies = await query.ToListAsync();
        var response = this.mapper.Map<List<MovieResponse>>(movies);

        return this.Ok(new ApiResponse<ICollection<MovieResponse>>
        {
            StatusCode = 200,
            Message = "Movies retrieved",
            Data = response
        });
    }

    // TODO 7 Додати реалізацію для масового видалення за списком ID
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<DeleteMoviesResponse>>> DeleteMovies(
        ApiRequest<ICollection<int>> request)
    {
        this.logger.LogInformation("DeleteMovies with IDs: {Ids}", string.Join(", ", request.Data));

        if (request.Data.Count == 0)
        {
            this.logger.LogWarning("Empty or null IDs list provided");

            return this.BadRequest(new ApiResponse
            {
                StatusCode = 400,
                Message = "List of movie IDs is required"
            });
        }

        var ids = request.Data.Distinct().ToList();

        var existingMovies = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Where(m => ids.Contains(m.Id))
            .ToListAsync();

        var existingIds = existingMovies.Select(m => m.Id).ToList();
        var notFoundIds = ids.Except(existingIds).ToList();

        // TODO 7 Додати перевірку, чи є фільми у відгуках, перед видаленням. Якщо є, то не видаляти такий, а виводити попередження, а інші фільми з масиву видалити
        var hasReviewsIds = existingMovies
            .Where(m => m.Reviews.Any())
            .Select(m => m.Id)
            .ToList();

        var moviesToDelete = existingMovies
            .Where(m => !m.Reviews.Any())
            .ToList();

        if (moviesToDelete.Any())
        {
            this.dbContext.Movies.RemoveRange(moviesToDelete);
            await this.dbContext.SaveChangesAsync();
        }

        var deletedIds = moviesToDelete.Select(m => m.Id).ToList();

        var response = new DeleteMoviesResponse
        {
            DeletedIds = deletedIds,
            NotFoundIds = notFoundIds,
            HasReviewsIds = hasReviewsIds
        };

        return this.Ok(new ApiResponse<DeleteMoviesResponse>
        {
            StatusCode = 200,
            Message = "Movies deleted",
            Data = response
        });
    }
}