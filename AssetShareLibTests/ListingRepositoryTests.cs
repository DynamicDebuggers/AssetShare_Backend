using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class ListingRepositoryTests
    {
        private InMemoryListingRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            _repo = new InMemoryListingRepository();
        }

        // Helper til updates
        private Listing CreateListingForUpdate()
        {
            return new Listing
            {
                Title = "Updated Title",
                Description = "Updated description text.",
                Price = 555.55m,
                MachineId = 99,
                UserId = 99
            };
        }

        private Listing CreateValidListing(
            string title = "Sample Listing",
            string description = "Sample description",
            decimal price = 123.45m,
            int machineId = 1,
            int userId = 1)
        {
            return new Listing
            {
                Title = title,
                Description = description,
                Price = price,
                MachineId = machineId,
                UserId = userId
            };
        }

        // ---------- GetAll ----------

        [TestMethod]
        public async Task GetAll_EmptyAtStart_ReturnsEmptyList()
        {
            var all = await _repo.GetAll();
            Assert.AreEqual(0, all.Count);
        }

        // ---------- Create ----------

        [TestMethod]
        public async Task Create_ValidListing_AssignsIdAndStores()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var listing = CreateValidListing(
                title: "New Listing",
                description: "A valid new listing description.",
                price: 123.45m,
                machineId: 10,
                userId: 20);

            var created = await _repo.Create(listing);
            var all = await _repo.GetAll();

            Assert.AreEqual(beforeCount + 1, all.Count);
            Assert.IsTrue(created.Id > 0);
            Assert.AreEqual("New Listing", created.Title);
            Assert.AreEqual("A valid new listing description.", created.Description);
            Assert.AreEqual(123.45m, created.Price);
            Assert.AreEqual(10, created.MachineId);
            Assert.AreEqual(20, created.UserId);
        }

        // ---------- GetById ----------

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsListing()
        {
            var created = await _repo.Create(CreateValidListing());
            var listing = await _repo.GetById(created.Id);

            Assert.IsNotNull(listing);
            Assert.AreEqual(created.Id, listing!.Id);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNull()
        {
            var listing = await _repo.GetById(999);
            Assert.IsNull(listing);
        }

        // ---------- Update ----------

        [TestMethod]
        public async Task Update_ExistingListingWithValidData_UpdatesFields()
        {
            var created = await _repo.Create(CreateValidListing());
            var updated = CreateListingForUpdate();

            var result = await _repo.Update(created.Id, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual(created.Id, result!.Id);
            Assert.AreEqual("Updated Title", result.Title);
            Assert.AreEqual("Updated description text.", result.Description);
            Assert.AreEqual(555.55m, result.Price);
            Assert.AreEqual(99, result.MachineId);
            Assert.AreEqual(99, result.UserId);

            var fromRepo = await _repo.GetById(created.Id);
            Assert.IsNotNull(fromRepo);
            Assert.AreEqual("Updated Title", fromRepo!.Title);
        }

        [TestMethod]
        public async Task Update_NonExistingId_ReturnsNull()
        {
            var updated = CreateListingForUpdate();
            var result = await _repo.Update(999, updated);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Update_PartialUpdate_NullOrZeroKeepsExistingValues()
        {
            var created = await _repo.Create(CreateValidListing(
                title: "Original Title",
                description: "Original description",
                price: 200m,
                machineId: 5,
                userId: 6));

            var partial = new Listing
            {
                Title = null,          // behold gammel title
                Description = null,    // behold gammel description
                Price = 0m,            // behold gammel price
                MachineId = 0,         // behold gammel machineId
                UserId = 0             // behold gammel userId
            };

            var result = await _repo.Update(created.Id, partial);

            Assert.IsNotNull(result);
            Assert.AreEqual("Original Title", result!.Title);
            Assert.AreEqual("Original description", result.Description);
            Assert.AreEqual(200m, result.Price);
            Assert.AreEqual(5, result.MachineId);
            Assert.AreEqual(6, result.UserId);
        }

        // ---------- Delete ----------

        [TestMethod]
        public async Task Delete_ExistingListing_RemovesIt()
        {
            var created = await _repo.Create(CreateValidListing());
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            await _repo.Delete(created.Id);

            var all = await _repo.GetAll();
            Assert.AreEqual(beforeCount - 1, all.Count);

            var fromRepo = await _repo.GetById(created.Id);
            Assert.IsNull(fromRepo);
        }

        [TestMethod]
        public async Task Delete_NonExistingListing_DoesNothing()
        {
            await _repo.Create(CreateValidListing());
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            await _repo.Delete(999);

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        // ---------------------------
        // In-memory repo ONLY for tests
        // ---------------------------
        private class InMemoryListingRepository
        {
            private readonly List<Listing> _listings = new();
            private int _nextId = 1;

            public Task<List<Listing>> GetAll()
                => Task.FromResult(_listings.Select(Clone).ToList());

            public Task<Listing?> GetById(int id)
                => Task.FromResult(_listings.Where(l => l.Id == id).Select(Clone).FirstOrDefault());

            public Task<Listing> Create(Listing listing)
            {
                // minimal validation (samme “type” som de fleste repos forventer)
                if (listing == null) throw new System.ArgumentNullException(nameof(listing));
                if (string.IsNullOrWhiteSpace(listing.Title)) throw new System.ArgumentException("Title is required.");
                if (string.IsNullOrWhiteSpace(listing.Description)) throw new System.ArgumentException("Description is required.");
                if (listing.Price <= 0) throw new System.ArgumentException("Price must be > 0.");
                if (listing.MachineId <= 0) throw new System.ArgumentException("MachineId must be > 0.");
                if (listing.UserId <= 0) throw new System.ArgumentException("UserId must be > 0.");

                listing.Id = _nextId++;
                _listings.Add(Clone(listing));
                return Task.FromResult(Clone(listing));
            }

            public Task<Listing?> Update(int id, Listing updated)
            {
                if (updated == null) throw new System.ArgumentNullException(nameof(updated));

                var existingIndex = _listings.FindIndex(l => l.Id == id);
                if (existingIndex < 0) return Task.FromResult<Listing?>(null);

                var existing = _listings[existingIndex];

                // Partial update rules (null/0 means “keep existing”)
                var merged = new Listing
                {
                    Id = id,
                    Title = updated.Title ?? existing.Title,
                    Description = updated.Description ?? existing.Description,
                    Price = updated.Price == 0m ? existing.Price : updated.Price,
                    MachineId = updated.MachineId == 0 ? existing.MachineId : updated.MachineId,
                    UserId = updated.UserId == 0 ? existing.UserId : updated.UserId
                };

                _listings[existingIndex] = Clone(merged);
                return Task.FromResult<Listing?>(Clone(merged));
            }

            public Task Delete(int id)
            {
                var existing = _listings.FirstOrDefault(l => l.Id == id);
                if (existing != null) _listings.Remove(existing);
                return Task.CompletedTask;
            }

            private static Listing Clone(Listing l) => new Listing
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                Price = l.Price,
                MachineId = l.MachineId,
                UserId = l.UserId
            };
        }
    }
}
