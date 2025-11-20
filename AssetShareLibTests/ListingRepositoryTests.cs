using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class ListingRepositoryTests
    {
        // Helper: laver en "update"-listing
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

        // ---------- Constructor / seeding ----------

        [TestMethod]
        public void Constructor_SeedsFourListings()
        {
            var repo = new ListingRepository();

            var all = repo.GetAll();

            Assert.AreEqual(4, all.Count, "Repository should seed exactly 4 listings.");
            CollectionAssert.AreEquivalent(
                new List<int> { 1, 2, 3, 4 },
                all.Select(l => l.Id).ToList()
            );
        }

        [TestMethod]
        public void SeededListings_AreValidAccordingToListingValidation()
        {
            var repo = new ListingRepository();

            foreach (var listing in repo.GetAll())
            {
                listing.ValidateAll(); // hvis der kastes exception, fejler testen
            }
        }

        // ---------- GetAll ----------

        [TestMethod]
        public void GetAll_ReturnsAllListings()
        {
            var repo = new ListingRepository();

            var all = repo.GetAll();

            Assert.AreEqual(4, all.Count);
        }

        // ---------- GetById ----------

        [TestMethod]
        public void GetById_ExistingId_ReturnsListing()
        {
            var repo = new ListingRepository();

            var listing = repo.GetById(1);

            Assert.IsNotNull(listing);
            Assert.AreEqual(1, listing!.Id);
        }

        [TestMethod]
        public void GetById_NonExistingId_ReturnsNull()
        {
            var repo = new ListingRepository();

            var listing = repo.GetById(999);

            Assert.IsNull(listing);
        }

        // ---------- Create ----------

        [TestMethod]
        public void Create_ValidListing_AssignsIdAndStores()
        {
            var repo = new ListingRepository();
            int beforeCount = repo.GetAll().Count;

            var created = repo.Create(
                title: "New Listing",
                description: "A valid new listing description.",
                price: 123.45m,
                machineId: 10,
                userId: 20
            );

            var all = repo.GetAll();

            Assert.AreEqual(beforeCount + 1, all.Count);
            Assert.AreEqual(5, created.Id); // 4 seedede → næste er 5
            Assert.AreEqual("New Listing", created.Title);
            Assert.AreEqual("A valid new listing description.", created.Description);
            Assert.AreEqual(123.45m, created.Price);
            Assert.AreEqual(10, created.MachineId);
            Assert.AreEqual(20, created.UserId);
        }

        // (Bemærk: Repositoryet kalder ikke ValidateAll, så vi tester kun simpel opførsel her)

        // ---------- Update ----------

        [TestMethod]
        public void Update_ExistingListingWithValidData_UpdatesFields()
        {
            var repo = new ListingRepository();
            var existingBefore = repo.GetById(2)!;

            var updated = CreateListingForUpdate();

            var result = repo.Update(2, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Id); // Id bevares
            Assert.AreEqual("Updated Title", result.Title);
            Assert.AreEqual("Updated description text.", result.Description);
            Assert.AreEqual(555.55m, result.Price);
            Assert.AreEqual(99, result.MachineId);
            Assert.AreEqual(99, result.UserId);

            var fromRepo = repo.GetById(2)!;
            Assert.AreSame(result, fromRepo); // samme objekt skal opdateres
        }

        [TestMethod]
        public void Update_NonExistingId_ReturnsNull()
        {
            var repo = new ListingRepository();

            var updated = CreateListingForUpdate();

            var result = repo.Update(999, updated);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Update_PartialUpdate_NullOrZeroKeepsExistingValues()
        {
            var repo = new ListingRepository();
            var original = repo.GetById(3)!;

            var originalTitle = original.Title;
            var originalDescription = original.Description;
            var originalPrice = original.Price;
            var originalMachineId = original.MachineId;
            var originalUserId = original.UserId;

            var partial = new Listing
            {
                Title = null,              // skal beholde gammel title
                Description = null,        // skal beholde gammel description
                Price = 0m,                // skal beholde gammel price
                MachineId = 0,             // skal beholde gammel machineId
                UserId = 0                 // skal beholde gammel userId
            };

            var result = repo.Update(3, partial)!;

            Assert.AreEqual(originalTitle, result.Title);
            Assert.AreEqual(originalDescription, result.Description);
            Assert.AreEqual(originalPrice, result.Price);
            Assert.AreEqual(originalMachineId, result.MachineId);
            Assert.AreEqual(originalUserId, result.UserId);
        }

        // ---------- Delete ----------

        [TestMethod]
        public void Delete_ExistingListing_RemovesIt()
        {
            var repo = new ListingRepository();
            int beforeCount = repo.GetAll().Count;

            repo.Delete(1);

            var all = repo.GetAll();
            Assert.AreEqual(beforeCount - 1, all.Count);
            Assert.IsNull(repo.GetById(1));
        }

        [TestMethod]
        public void Delete_NonExistingListing_DoesNothing()
        {
            var repo = new ListingRepository();
            int beforeCount = repo.GetAll().Count;

            repo.Delete(999);

            Assert.AreEqual(beforeCount, repo.GetAll().Count);
        }
    }
}
