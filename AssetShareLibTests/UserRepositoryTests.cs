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
    public class UserRepositoryTests
    {
        private UserRepository _repo = null!;
        private MongoDbContext _context = null!;

        // >>>> Sæt din (gerne test-) connection string her <<<<
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

            // Ryd Users collection før hver test
            _context.Users.DeleteMany(FilterDefinition<User>.Empty);

            _repo = new UserRepository(_context);
        }

        // Helper: laver en gyldig bruger (uden Id, det sætter repo)
        private User CreateValidUser()
        {
            return new User
            {
                FirstName = "Test User",
                LastName = "Tester",
                Roles = new List<string> { "normal" },
                Email = "test@mail.com",
                Password = "Test#123"
            };
        }

        // ---------- GetAll ----------

        [TestMethod]
        public async Task GetAll_EmptyAtStart_ReturnsEmptyList()
        {
            var users = await _repo.GetAllAsync();

            Assert.IsNotNull(users);
            Assert.AreEqual(0, users.Count);
        }

        // ---------- Add ----------

        [TestMethod]
        public async Task Add_ValidUser_AssignsIdAndStores()
        {
            var before = await _repo.GetAllAsync();
            int beforeCount = before.Count;

            var newUser = CreateValidUser();

            var added = await _repo.AddAsync(newUser);

            var after = await _repo.GetAllAsync();

            Assert.AreEqual(beforeCount + 1, after.Count);
            Assert.IsTrue(added.Id > 0);

            Assert.IsTrue(after.Any(u =>
                u.Id == added.Id &&
                u.Email == newUser.Email &&
                u.FirstName == newUser.FirstName &&
                u.LastName == newUser.LastName));
        }

        [TestMethod]
        public async Task Add_InvalidUser_ThrowsAndDoesNotChangeCount()
        {
            var before = await _repo.GetAllAsync();
            int beforeCount = before.Count;

            var invalidUser = CreateValidUser();
            invalidUser.Password = "short"; // ugyldig (for kort)

            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _repo.AddAsync(invalidUser);
            });

            var after = await _repo.GetAllAsync();
            Assert.AreEqual(beforeCount, after.Count);
        }

        // ---------- GetById ----------

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsUser()
        {
            var added = await _repo.AddAsync(CreateValidUser());

            var user = await _repo.GetByIdAsync(added.Id);

            Assert.IsNotNull(user);
            Assert.AreEqual(added.Id, user!.Id);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNull()
        {
            var user = await _repo.GetByIdAsync(999);

            Assert.IsNull(user);
        }

        // ---------- Update ----------

        [TestMethod]
        public async Task Update_ExistingUserWithValidData_UpdatesFields()
        {
            var added = await _repo.AddAsync(CreateValidUser());

            var updatedUser = CreateValidUser();
            updatedUser.FirstName = "Updated Name";
            updatedUser.LastName = "UpdatedLast";
            updatedUser.Email = "updated@mail.com";
            updatedUser.Roles = new List<string> { "machineOwner" };
            updatedUser.Password = "NewPass#123";

            var result = await _repo.UpdateAsync(added.Id, updatedUser);

            Assert.IsNotNull(result);
            Assert.AreEqual(added.Id, result!.Id);

            var fromRepo = await _repo.GetByIdAsync(added.Id);
            Assert.IsNotNull(fromRepo);
            Assert.AreEqual("Updated Name", fromRepo!.FirstName);
            Assert.AreEqual("UpdatedLast", fromRepo.LastName);
            Assert.AreEqual("updated@mail.com", fromRepo.Email);
            CollectionAssert.AreEquivalent(
                new List<string> { "machineOwner" },
                fromRepo.Roles!.ToList()
            );
        }

        [TestMethod]
        public async Task Update_NonExistingUser_ReturnsNull()
        {
            var updatedUser = CreateValidUser();

            var result = await _repo.UpdateAsync(999, updatedUser);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Update_InvalidUser_ThrowsAndDoesNotChangeExistingUser()
        {
            var added = await _repo.AddAsync(CreateValidUser());
            var original = await _repo.GetByIdAsync(added.Id);
            var originalEmail = original!.Email;

            var invalidUpdate = CreateValidUser();
            invalidUpdate.Email = "invalid-email"; // vil fejle email-validator

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.UpdateAsync(added.Id, invalidUpdate);
            });

            var after = await _repo.GetByIdAsync(added.Id);
            Assert.IsNotNull(after);
            Assert.AreEqual(originalEmail, after!.Email);
        }

        // ---------- Delete ----------

        [TestMethod]
        public async Task Delete_ExistingUser_RemovesAndReturnsUser()
        {
            var added = await _repo.AddAsync(CreateValidUser());
            var before = await _repo.GetAllAsync();
            int beforeCount = before.Count;

            var deleted = await _repo.DeleteAsync(added.Id);

            Assert.IsNotNull(deleted);
            Assert.AreEqual(added.Id, deleted!.Id);

            var after = await _repo.GetAllAsync();
            Assert.AreEqual(beforeCount - 1, after.Count);

            var fromRepo = await _repo.GetByIdAsync(added.Id);
            Assert.IsNull(fromRepo);
        }

        [TestMethod]
        public async Task Delete_NonExistingUser_ReturnsNullAndDoesNotChangeCount()
        {
            var added = await _repo.AddAsync(CreateValidUser());
            var before = await _repo.GetAllAsync();
            int beforeCount = before.Count;

            var deleted = await _repo.DeleteAsync(999);

            Assert.IsNull(deleted);

            var after = await _repo.GetAllAsync();
            Assert.AreEqual(beforeCount, after.Count);
        }
    }
}
