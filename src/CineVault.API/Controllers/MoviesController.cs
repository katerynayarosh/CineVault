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
    
    // TODO 13 Додати AsNoTracking для читання даних
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<MovieResponse>>>> GetMovies(
        ApiRequest request)
    {
        this.logger.LogInformation("GetMovies");

        var movies = await this.dbContext.Movies
            .AsNoTracking() // Вимкнено відстеження змін
            .Include(m => m.Reviews)
            .ToListAsync();

        var moviesResponses = this.mapper.Map<List<MovieResponse>>(movies);

        return this.Ok(new ApiResponse<ICollection<MovieResponse>>
        {
            StatusCode = 200, Message = "Movies retrieved", Data = moviesResponses
        });
    }

    // TODO 13 Оптимізація Select для завантаження лише необхідних полів
    [HttpPost("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieById(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetMovieById id:{id}", id);

        var movie = await this.dbContext.Movies
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new 
            {
                m.Id,
                m.Title,
                m.Description,
                m.ReleaseDate,
                m.Genre,
                m.Director,
                Reviews = m.Reviews.Select(r => new { r.Rating })
            })
            .FirstOrDefaultAsync();

        if (movie == null)
        {
            return this.NotFound(new ApiResponse { StatusCode = 404, Message = $"Movie with ID {id} not found" });
        }

        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = movie.Reviews.Any() ? movie.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = movie.Reviews.Count()
        };

        return this.Ok(new ApiResponse<MovieResponse>
        {
            StatusCode = 200, Message = "Movie retrieved", Data = response
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

        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200, Message = "Movie created", Data = movie.Id
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

            return this.NotFound(new ApiResponse { StatusCode = 404, Message = $"Movie with ID {id} not found in catalog" });
        }

        this.mapper.Map(request.Data, movie);

        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Movie updated" });
    }

    // TODO 13 Скомпільований запит для часто використовуваного методу
    private static readonly Func<CineVaultDbContext, int, Task<Movie?>> GetMovieByIdCompiled =
        EF.CompileAsyncQuery((CineVaultDbContext context, int id) =>
            context.Movies
                .AsNoTracking()
                .FirstOrDefault(m => m.Id == id));

    // TODO 13 Скомпільований запит для часто використовуваного методу
    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> DeleteMovie(ApiRequest request, int id)
    {
        this.logger.LogInformation("DeleteMovie id:{id}", id);

        var movie = await GetMovieByIdCompiled(this.dbContext, id);

        if (movie is null)
        {
            return this.NotFound(new ApiResponse { StatusCode = 404, Message = $"Movie not found" });
        }

        movie.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Movie deleted" });
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<int>>>> CreateMovies(
        ApiRequest<ICollection<MovieRequest>> request)
    {
        this.logger.LogInformation("CreateMovies");

        var movies = this.mapper.Map<ICollection<Movie>>(request.Data);

        await this.dbContext.Movies.AddRangeAsync(movies);
        await this.dbContext.SaveChangesAsync();

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
            StatusCode = 200, Message = "Movies retrieved", Data = response
        });
    }

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
                StatusCode = 400, Message = "Minimum one valid movie ID required for deletion"
                
            });
        }

        var ids = request.Data.Distinct().ToList();

        var existingMovies = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Where(m => ids.Contains(m.Id))
            .ToListAsync();

        var existingIds = existingMovies.Select(m => m.Id).ToList();
        var notFoundIds = ids.Except(existingIds).ToList();

        var hasReviewsIds = existingMovies
            .Where(m => m.Reviews.Any())
            .Select(m => m.Id)
            .ToList();

        var moviesToDelete = existingMovies
            .Where(m => !m.Reviews.Any())
            .ToList();

        if (moviesToDelete.Any())
        {
            foreach (var movie in moviesToDelete)
            {
                movie.IsDeleted = true;
            }
            await this.dbContext.SaveChangesAsync();
        }

        var deletedIds = moviesToDelete.Select(m => m.Id).ToList();

        var response = new DeleteMoviesResponse
        {
            DeletedIds = deletedIds, NotFoundIds = notFoundIds, HasReviewsIds = hasReviewsIds
        };

        return this.Ok(new ApiResponse<DeleteMoviesResponse>
        {
            StatusCode = 200, Message = "Movies deleted", Data = response
        });
    }

    // TODO 9 GetMovieDetails
    [HttpPost("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<GetMovieDetailsResponse>>> GetMovieDetails(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetMovieDetails id:{id}", id);

        var movie = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            this.logger.LogWarning("Movie not found id:{id}", id);
            return this.NotFound(new ApiResponse { 
                StatusCode = 404,
                Message = $"Movie details unavailable: ID {id} not found" 
            });
        }

        var response = new GetMovieDetailsResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description!,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre!,
            Director = movie.Director!,
            AverageRating = movie.Reviews.Any() ? (decimal)movie.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = movie.Reviews.Count,
            RecentReviews = movie.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new InternalReviewResponse
                {
                    Username = r.User?.Username ?? "Deleted User",
                    Rating = r.Rating,
                    Comment = r.Comment!,
                    CreatedAt = r.CreatedAt
                })
                .ToList()
        };

        return this.Ok(new ApiResponse<GetMovieDetailsResponse>
        {
            StatusCode = 200, Message = "Movie details retrieved", Data = response
        });
    }

    // TODO 9 SearchMovies
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<MovieResponse>>>> SearchMovies2(
        ApiRequest<SearchMovies2Request> request)
    {
        this.logger.LogInformation("SearchMovies");

        var query = this.dbContext.Movies
            .Include(m => m.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Data.SearchText))
        {
            string searchText = request.Data.SearchText.ToLower();
            query = query.Where(m =>
                m.Description != null && m.Director != null &&
                (m.Title.ToLower().Contains(searchText) ||
                 m.Description.ToLower().Contains(searchText) ||
                 m.Director.ToLower().Contains(searchText)));
        }

        if (!string.IsNullOrEmpty(request.Data.Genre))
        {
            query = query.Where(m => m.Genre == request.Data.Genre);
        }

        if (request.Data.MinRating.HasValue)
        {
            query = query.Where(m =>
                (decimal?)m.Reviews.Average(r => r.Rating) >= request.Data.MinRating ||
                (!m.Reviews.Any() && request.Data.MinRating <= 0));
        }

        if (request.Data.ReleaseDateFrom.HasValue)
        {
            query = query.Where(m => m.ReleaseDate >= request.Data.ReleaseDateFrom);
        }

        if (request.Data.ReleaseDateTo.HasValue)
        {
            query = query.Where(m => m.ReleaseDate <= request.Data.ReleaseDateTo);
        }

        var movies = await query.ToListAsync();
        var response = this.mapper.Map<List<MovieResponse>>(movies);

        return this.Ok(new ApiResponse<ICollection<MovieResponse>>
        {
            StatusCode = 200, Message = "Movies retrieved", Data = response
        });
    }
}