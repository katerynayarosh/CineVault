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
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<ReviewsController> logger;
    private readonly IMapper mapper;

    public ReviewsController(CineVaultDbContext dbContext, ILogger<ReviewsController> logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        this.logger.LogInformation("GetReviews");
        var reviews = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie!.Title,
                UserId = r.UserId,
                Username = r.User!.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return this.Ok(reviews);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        this.logger.LogInformation("GetReviewById id:{id}", id);
        var review = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        var response = new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return this.Ok(response);
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        this.logger.LogInformation("CreateReview movieId:{movieId} userId:{userId}",
            request.MovieId, request.UserId);
        var review = new Review
        {
            MovieId = request.MovieId,
            UserId = request.UserId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        this.dbContext.Reviews.Add(review);
        await this.dbContext.SaveChangesAsync();

        return this.Created();
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        this.logger.LogInformation("UpdateReview id:{id} movieId:{movieId} userId:{userId}", id,
            request.MovieId, request.UserId);
        var review = await this.dbContext.Reviews.FindAsync(id);

        if (review is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        review.MovieId = request.MovieId;
        review.UserId = request.UserId;
        review.Rating = request.Rating;
        review.Comment = request.Comment;

        await this.dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteReview(int id)
    {
        this.logger.LogInformation("DeleteReview id:{id}", id);
        var review = await this.dbContext.Reviews.FindAsync(id);

        if (review is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);
            return this.NotFound();
        }

        this.dbContext.Reviews.Remove(review);
        await this.dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ICollection<ReviewResponse>>>> GetReviews(
        ApiRequest request)
    {
        this.logger.LogInformation("GetReviews");

        var reviews = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();

        var reviewsResponses = mapper.Map<ICollection<ReviewResponse>>(reviews);

        return this.Ok(new ApiResponse<ICollection<ReviewResponse>>
        {
            StatusCode = 200,
            Message = "Reviews retrieved",
            Data = reviewsResponses
        });
    }

    [HttpPost("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewById(ApiRequest request,
        int id)
    {
        this.logger.LogInformation("GetReviewById id:{id}", id);

        var review = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Review not found"
            });
        }

        var response = this.mapper.Map<ReviewResponse>(review);

        return this.Ok(new ApiResponse<ReviewResponse>
        {
            StatusCode = 200,
            Message = "Review retrieved",
            Data = response
        });
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<int>>> CreateReview(
        ApiRequest<ReviewRequest> request)
    {
        this.logger.LogInformation("CreateReview movieId:{movieId} userId:{userId}",
            request.Data.MovieId, request.Data.UserId);

        var review = this.mapper.Map<Review>(request.Data);

        this.dbContext.Reviews.Add(review);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200,
            Message = "Review created",
            Data = review.Id
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> UpdateReview(int id,
        ApiRequest<ReviewRequest> request)
    {
        this.logger.LogInformation("UpdateReview id:{id} movieId:{movieId} userId:{userId}", id,
            request.Data.MovieId, request.Data.UserId);

        var review = await this.dbContext.Reviews.FindAsync(id);

        if (review is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Review not found"
            });
        }

        this.mapper.Map(request.Data, review);

        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Review updated" });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse>> DeleteReview(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteReview id:{id}", id);

        var review = await this.dbContext.Reviews.FindAsync(id);

        if (review is null)
        {
            this.logger.LogWarning("NotFound id:{id}", id);

            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Review not found"
            });
        }

        this.dbContext.Reviews.Remove(review);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Review deleted" });
    }
}