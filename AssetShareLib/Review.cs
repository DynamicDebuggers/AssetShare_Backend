using System;
using System.Text.RegularExpressions;

namespace AssetShareLib
{
    public class Review
    {
        public int Id { get; set; }
        public int ListingId { get; set; }      
        public int UserId { get; set; }         
        public int Rating { get; set; }         
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public void ValidateRating()
        {
            if (Rating < 1 || Rating > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(Rating), "Rating must be between 1 and 5");
            }
        }

        public void ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                throw new ArgumentNullException(nameof(Title), "Title cannot be null or empty");
            }
            if (Title.Length < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Title), "Title must be at least 3 characters long");
            }
            if (Title.Length > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(Title), "Title cannot be longer than 200 characters");
            }
        }

        public void ValidateContent()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                throw new ArgumentNullException(nameof(Content), "Content cannot be null or empty");
            }
            if (Content.Length < 10)
            {
                throw new ArgumentOutOfRangeException(nameof(Content), "Content must be at least 10 characters long");
            }
            if (Content.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(Content), "Content cannot be longer than 5000 characters");
            }
        }

        public void ValidateIds()
        {
            if (ListingId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ListingId), "ListingId must be greater than 0");
            }
            if (UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(UserId), "UserId must be greater than 0");
            }
        }

        public void ValidateAll()
        {
            ValidateRating();
            ValidateTitle();
            ValidateContent();
            ValidateIds();
        }
    }
}