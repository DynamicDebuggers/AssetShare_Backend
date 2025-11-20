using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetShareLib
{
    // Very small, in-memory repository for Listing.
    // Intentionally simple: no locking, no exceptions on missing items.
    public class ListingRepository
    {
        private readonly List<Listing> _listings = new();
        private int _nextId = 1;

        public ListingRepository()
        {
            // Seed simple data using Create so IDs are assigned consistently.
            Create("Sample Listing 1", "This is a sample listing description.", 99.99M, machineId: 1, userId: 1);
            Create("Sample Listing 2", "This is another sample listing description.", 149.99M, machineId: 2, userId: 2);
            Create("Sample Listing 3", "This is yet another sample listing description.", 199.99M, machineId: 3, userId: 3);
            Create("Sample Listing 4", "This is a different sample listing description.", 249.99M, machineId: 4, userId: 4);
        }

        // GetById(id) : Listing? (returns null if not found)
        public Listing? GetById(int id)
        {
            return _listings.FirstOrDefault(l => l.Id == id);
        }

        // GetAll() : List<Listing>
        public List<Listing> GetAll()
        {
            return _listings;
        }

        // Create(title, description, price, machineId, userId) : Listing
        public Listing Create(string title, string description, decimal price, int machineId, int userId)
        {
            var listing = new Listing
            {
                Id = _nextId++,
                Title = title,
                Description = description,
                Price = price,
                MachineId = machineId,
                UserId = userId
            };

            _listings.Add(listing);
            return listing;
        }

        // Update(id, updatedListing) : Listing? (returns updated listing or null if not found)
        public Listing? Update(int id, Listing updatedListing)
        {
            var existing = _listings.FirstOrDefault(l => l.Id == id);
            if (existing == null) return null;

            // Apply updates (keep Id)
            existing.Title = updatedListing.Title ?? existing.Title;
            existing.Description = updatedListing.Description ?? existing.Description;
            existing.Price = updatedListing.Price != 0 ? updatedListing.Price : existing.Price;
            existing.MachineId = updatedListing.MachineId != 0 ? updatedListing.MachineId : existing.MachineId;
            existing.UserId = updatedListing.UserId != 0 ? updatedListing.UserId : existing.UserId;

            return existing;
        }

        // Delete(id) : void (silently does nothing if not found)
        public void Delete(int id)
        {
            var existing = _listings.FirstOrDefault(l => l.Id == id);
            if (existing != null) 
                _listings.Remove(existing);
        }
    }
}
