using MongoDB.Driver;

namespace AssetShareLib
{
    public class ReviewRepository
    {
        private readonly IMongoCollection<Review> _reviews;

        public ReviewRepository(MongoDbContext context)
        {
            _reviews = context.Reviews;
        }

        private static void ValidateReviewForStorage(Review review)
        {
            review.ValidateRating();
            review.ValidateTitle();
            review.ValidateContent();
            review.ValidateIds();
        }

        public async Task<IReadOnlyList<Review>> GetAllAsync()
        {
            var list = await _reviews
                .Find(FilterDefinition<Review>.Empty)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();

            return list.AsReadOnly();
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _reviews
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Review>> GetByListingIdAsync(int listingId)
        {
            var list = await _reviews
                .Find(r => r.ListingId == listingId)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();

            return list.AsReadOnly();
        }

        public async Task<IReadOnlyList<Review>> GetByUserIdAsync(int userId)
        {
            var list = await _reviews
                .Find(r => r.UserId == userId)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();

            return list.AsReadOnly();
        }

        public async Task<Review> AddAsync(Review review)
        {
            ValidateReviewForStorage(review);

            var lastReview = await _reviews
                .Find(FilterDefinition<Review>.Empty)
                .SortByDescending(r => r.Id)
                .Limit(1)
                .FirstOrDefaultAsync();

            review.Id = (lastReview?.Id ?? 0) + 1;
            review.CreatedAt = DateTime.UtcNow;

            await _reviews.InsertOneAsync(review);
            return review;
        }

        public async Task<Review?> UpdateAsync(int id, Review updatedReview)
        {
            updatedReview.Id = id;
            updatedReview.UpdatedAt = DateTime.UtcNow;
            ValidateReviewForStorage(updatedReview);

            var result = await _reviews.ReplaceOneAsync(r => r.Id == id, updatedReview);

            if (result.MatchedCount == 0)
                return null;

            return updatedReview;
        }

        public async Task<Review?> DeleteAsync(int id)
        {
            return await _reviews.FindOneAndDeleteAsync(r => r.Id == id);
        }

        public async Task<double> GetAverageRatingByListingIdAsync(int listingId)
        {
            var reviews = await GetByListingIdAsync(listingId);
            if (!reviews.Any())
                return 0.0;

            return reviews.Average(r => r.Rating);
        }
    }
}