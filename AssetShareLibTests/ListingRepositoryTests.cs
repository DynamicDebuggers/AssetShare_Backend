using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class ListingRepositoryTests
    {
        private ListingRepository _repo = null!;
        private MongoDbContext _context = null!;

        // >>>> Sæt din egen (gerne test-) connection string her <<<<
        private const string TestConnectionString =
            "mongodb+srv://tester:test123@cluster0.cvjiyiw.mongodb.net/?retryWrites=true&w=majority";

        private const string TestDatabaseName = "AssetShareDb";

        [TestInitialize]
        public void Setup()
        {
            var settings = new MongoDbSettings
            {
                ConnectionString = TestConnectionString,
                DatabaseName = TestDatabaseName
            };

            var options = Options.Create(settings);
            _context = new MongoDbContext(options);

            // Ryd test-data før hver test
            _context.Listings.DeleteMany(FilterDefinition<Listing>.Empty);

            _repo = new ListingRepository(_context);
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
            var l1 = await _repo.Create(CreateValidListing());
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            await _repo.Delete(999);

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }
    }
}
