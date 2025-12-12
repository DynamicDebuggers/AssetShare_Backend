using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AssetShareLib
{
    public class ListingRepository
    {
        private readonly IMongoCollection<Listing> _listings;

        public ListingRepository(MongoDbContext context)
        {
            _listings = context.Listings;
        }

        // GetAll() : List<Listing>
        public async Task<List<Listing>> GetAll()
        {
            return await _listings
                .Find(FilterDefinition<Listing>.Empty)
                .ToListAsync();
        }

        // GetById(id) : Listing? (returns null if not found)
        public async Task<Listing?> GetById(int id)
        {
            return await _listings
                .Find(l => l.Id == id)
                .FirstOrDefaultAsync();
        }

        // Create(...) : Listing
        // Jeg ændrer signaturen en lille smule så du giver et Listing-objekt ind
        public async Task<Listing> Create(Listing listing)
        {
            // Find højeste Id i databasen og læg 1 til
            var lastListing = await _listings
                .Find(FilterDefinition<Listing>.Empty)
                .SortByDescending(l => l.Id)
                .Limit(1)
                .FirstOrDefaultAsync();

            listing.Id = (lastListing?.Id ?? 0) + 1;

            await _listings.InsertOneAsync(listing);
            return listing;
        }

        // Update(id, updatedListing) : Listing? (beholder din "kun opdatér hvis værdi != 0/null"-logik)
        public async Task<Listing?> Update(int id, Listing updatedListing)
        {
            var existing = await _listings
                .Find(l => l.Id == id)
                .FirstOrDefaultAsync();

            if (existing == null)
                return null;

            // samme logik som din in-memory version
            existing.Title = updatedListing.Title ?? existing.Title;
            existing.Description = updatedListing.Description ?? existing.Description;
            if (updatedListing.Price != 0)
                existing.Price = updatedListing.Price;
            if (updatedListing.MachineId != 0)
                existing.MachineId = updatedListing.MachineId;
            if (updatedListing.UserId != 0)
                existing.UserId = updatedListing.UserId;

            await _listings.ReplaceOneAsync(l => l.Id == id, existing);
            return existing;
        }

        // Delete(id) : void
        public async Task Delete(int id)
        {
            await _listings.DeleteOneAsync(l => l.Id == id);
        }
    }
}
