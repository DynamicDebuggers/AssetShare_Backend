using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class UserRepositoryTests
    {
        private InMemoryUserRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            _repo = new InMemoryUserRepository();
        }

        // Helper: laver en gyldig bruger (repo sætter Id)
        private User CreateValidUser(
            string email = "test@mail.com",
            string firstName = "Test User",
            string lastName = "Tester")
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                Roles = new List<string> { "normal" },
                Email = email,

                // vigtigt: i tests kræver vi bare at PasswordHash findes (ikke DB)
                PasswordHash = "SomeHashValue"
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

            var stored = after.First(u => u.Id == added.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(stored.PasswordHash));
        }

        [TestMethod]
        public async Task Add_InvalidUser_ThrowsAndDoesNotChangeCount()
        {
            var before = await _repo.GetAllAsync();
            int beforeCount = before.Count;

            var invalidUser = CreateValidUser();
            invalidUser.PasswordHash = ""; // invalid: required for storage

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
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

            var updatedUser = CreateValidUser(email: "updated@mail.com");
            updatedUser.FirstName = "Updated Name";
            updatedUser.LastName = "UpdatedLast";
            updatedUser.Roles = new List<string> { "machineOwner" };
            updatedUser.PasswordHash = "NewHashValue";

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

            Assert.IsFalse(string.IsNullOrWhiteSpace(fromRepo.PasswordHash));
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
            invalidUpdate.Email = "invalid-email"; // fails ValidateEmail

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
            await _repo.AddAsync(CreateValidUser());
            var before = await _repo.GetAllAsync();
            int beforeCount = before.Count;

            var deleted = await _repo.DeleteAsync(999);

            Assert.IsNull(deleted);

            var after = await _repo.GetAllAsync();
            Assert.AreEqual(beforeCount, after.Count);
        }

        // ---------------------------
        // In-memory repo ONLY for tests
        // ---------------------------
        private class InMemoryUserRepository
        {
            private readonly List<User> _users = new();
            private int _nextId = 1;

            public Task<IReadOnlyList<User>> GetAllAsync()
                => Task.FromResult((IReadOnlyList<User>)_users.Select(Clone).ToList());

            public Task<User?> GetByIdAsync(int id)
                => Task.FromResult(_users.Where(u => u.Id == id).Select(Clone).FirstOrDefault());

            public Task<User?> GetByEmailAsync(string email)
                => Task.FromResult(_users.Where(u => u.Email == email).Select(Clone).FirstOrDefault());

            public Task<User> AddAsync(User user)
            {
                ValidateUserForStorage(user);

                user.Id = _nextId++;
                _users.Add(Clone(user));
                return Task.FromResult(Clone(user));
            }

            public Task<User?> UpdateAsync(int id, User updatedUser)
            {
                ValidateUserForStorage(updatedUser);

                var idx = _users.FindIndex(u => u.Id == id);
                if (idx < 0) return Task.FromResult<User?>(null);

                updatedUser.Id = id;
                _users[idx] = Clone(updatedUser);

                return Task.FromResult<User?>(Clone(updatedUser));
            }

            public Task<User?> DeleteAsync(int id)
            {
                var existing = _users.FirstOrDefault(u => u.Id == id);
                if (existing == null) return Task.FromResult<User?>(null);

                _users.Remove(existing);
                return Task.FromResult<User?>(Clone(existing));
            }

            private static void ValidateUserForStorage(User user)
            {
                if (user == null) throw new ArgumentNullException(nameof(user));

                user.ValidateFirstName();
                user.ValidateLastName();
                user.ValidateRoles();
                user.ValidateEmail();

                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                    throw new ArgumentNullException(nameof(user.PasswordHash), "PasswordHash is required.");
            }

            private static User Clone(User u) => new User
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Roles = u.Roles == null ? null : new List<string>(u.Roles),
                Email = u.Email,
                PasswordHash = u.PasswordHash
            };
        }
    }
}
