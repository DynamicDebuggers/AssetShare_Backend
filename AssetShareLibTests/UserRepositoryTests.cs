using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class UserRepositoryTests
    {
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

        // ---------- Constructor / seed ----------

        [TestMethod]
        public void Constructor_SeedsOneUser()
        {
            // Arrange + Act
            var repo = new UserRepository();

            // Assert
            var users = repo.GetAll();
            Assert.AreEqual(1, users.Count, "Repository should contain exactly one seeded user");

            var user = users[0];
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Mads Aagaard", user.FirstName);
            Assert.AreEqual("Larsen", user.LastName);
            CollectionAssert.AreEquivalent(
                new List<string> { "normal", "machineOwner" },
                user.Roles!.ToList()
            );
            Assert.AreEqual("mads@mail.com", user.Email);
        }

        // ---------- GetAll ----------

        [TestMethod]
        public void GetAll_ReturnsReadOnlyList()
        {
            var repo = new UserRepository();

            var users = repo.GetAll();

            // Tjek at Count passer
            Assert.AreEqual(1, users.Count);

            // Tjek at den interne liste er read-only (ReadOnlyCollection underneden)
            Assert.IsInstanceOfType(
                users,
                typeof(System.Collections.ObjectModel.ReadOnlyCollection<User>)
            );
        }

        // ---------- GetById ----------

        [TestMethod]
        public void GetById_ExistingId_ReturnsUser()
        {
            var repo = new UserRepository();

            var user = repo.GetById(1);

            Assert.IsNotNull(user);
            Assert.AreEqual(1, user!.Id);
        }

        [TestMethod]
        public void GetById_NonExistingId_ReturnsNull()
        {
            var repo = new UserRepository();

            var user = repo.GetById(999);

            Assert.IsNull(user);
        }

        // ---------- Add ----------

        [TestMethod]
        public void Add_ValidUser_AssignsIdAndStores()
        {
            var repo = new UserRepository();
            int beforeCount = repo.GetAll().Count;

            var newUser = CreateValidUser();

            var added = repo.Add(newUser);

            Assert.AreEqual(beforeCount + 1, repo.GetAll().Count);
            Assert.AreEqual(2, added.Id); // første seed har Id = 1
            Assert.IsTrue(repo.GetAll().Any(u => u.Id == 2 && u.Email == "test@mail.com"));
        }

        [TestMethod]
        public void Add_InvalidUser_ThrowsAndDoesNotChangeCount()
        {
            var repo = new UserRepository();
            int beforeCount = repo.GetAll().Count;

            var invalidUser = CreateValidUser();
            invalidUser.Password = "short"; // ugyldig (for kort)

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => repo.Add(invalidUser));

            int afterCount = repo.GetAll().Count;
            Assert.AreEqual(beforeCount, afterCount, "Repository should not change when Add fails");
        }

        // ---------- Update ----------

        [TestMethod]
        public void Update_ExistingUserWithValidData_UpdatesFields()
        {
            var repo = new UserRepository();

            var updatedUser = CreateValidUser();
            updatedUser.FirstName = "Updated Name";
            updatedUser.LastName = "UpdatedLast";
            updatedUser.Email = "updated@mail.com";
            updatedUser.Roles = new List<string> { "machineOwner" };
            updatedUser.Password = "NewPass#123";

            var result = repo.Update(1, updatedUser);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Id);

            var fromRepo = repo.GetById(1);
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
        public void Update_NonExistingUser_ReturnsNull()
        {
            var repo = new UserRepository();

            var updatedUser = CreateValidUser();

            var result = repo.Update(999, updatedUser);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Update_InvalidUser_ThrowsAndDoesNotChangeExistingUser()
        {
            var repo = new UserRepository();
            var original = repo.GetById(1)!;
            var originalEmail = original.Email;

            var invalidUpdate = CreateValidUser();
            invalidUpdate.Email = "invalid-email"; // vil fejle email-validator

            Assert.ThrowsException<ArgumentException>(() => repo.Update(1, invalidUpdate));

            // Tjek at den eksisterende bruger IKKE er blevet ændret af et fejlet update
            var after = repo.GetById(1)!;
            Assert.AreEqual(originalEmail, after.Email);
        }

        // ---------- Delete ----------

        [TestMethod]
        public void Delete_ExistingUser_RemovesAndReturnsUser()
        {
            var repo = new UserRepository();
            int beforeCount = repo.GetAll().Count;

            var deleted = repo.Delete(1);

            Assert.IsNotNull(deleted);
            Assert.AreEqual(1, deleted!.Id);
            Assert.AreEqual(beforeCount - 1, repo.GetAll().Count);
            Assert.IsNull(repo.GetById(1));
        }

        [TestMethod]
        public void Delete_NonExistingUser_ReturnsNullAndDoesNotChangeCount()
        {
            var repo = new UserRepository();
            int beforeCount = repo.GetAll().Count;

            var deleted = repo.Delete(999);

            Assert.IsNull(deleted);
            Assert.AreEqual(beforeCount, repo.GetAll().Count);
        }
    }
}
