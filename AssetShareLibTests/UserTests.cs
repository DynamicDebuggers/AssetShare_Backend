using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;
using System.Collections.Generic;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class UserTests
    {
        // Helper: laver en gyldig bruger, som passer alle dine regler
        private User CreateValidUser()
        {
            return new User
            {
                Id = 1,
                FirstName = "Mads Aagaard",
                LastName = "Larsen",
                Roles = new List<string> { "normal" },
                Email = "mads@mail.dk",
                PasswordHash = "MadS#123" // ✅ updated
            };
        }

        // ---------- FirstName ----------

        [TestMethod]
        public void ValidateFirstName_Null_ThrowsArgumentNullException()
        {
            var user = CreateValidUser();
            user.FirstName = null;

            Assert.ThrowsException<ArgumentNullException>(() => user.ValidateFirstName());
        }

        [TestMethod]
        public void ValidateFirstName_Whitespace_ThrowsArgumentNullException()
        {
            var user = CreateValidUser();
            user.FirstName = "   ";

            Assert.ThrowsException<ArgumentNullException>(() => user.ValidateFirstName());
        }

        [TestMethod]
        public void ValidateFirstName_TooShort_ThrowsArgumentOutOfRangeException()
        {
            var user = CreateValidUser();
            user.FirstName = "A";

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => user.ValidateFirstName());
        }

        [TestMethod]
        public void ValidateFirstName_TooLong_ThrowsArgumentOutOfRangeException()
        {
            var user = CreateValidUser();
            user.FirstName = new string('a', 101);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => user.ValidateFirstName());
        }

        [TestMethod]
        public void ValidateFirstName_InvalidCharacters_ThrowsArgumentException()
        {
            var user = CreateValidUser();
            user.FirstName = "Mads1";

            Assert.ThrowsException<ArgumentException>(() => user.ValidateFirstName());
        }

        [TestMethod]
        public void ValidateFirstName_Valid_DoesNotThrow()
        {
            var user = CreateValidUser();
            user.FirstName = "Mads Aagaard";

            user.ValidateFirstName();
        }

        // ---------- LastName ----------

        [TestMethod]
        public void ValidateLastName_Null_ThrowsArgumentNullException()
        {
            var user = CreateValidUser();
            user.LastName = null;

            Assert.ThrowsException<ArgumentNullException>(() => user.ValidateLastName());
        }

        [TestMethod]
        public void ValidateLastName_TooShort_ThrowsArgumentOutOfRangeException()
        {
            var user = CreateValidUser();
            user.LastName = "L";

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => user.ValidateLastName());
        }

        [TestMethod]
        public void ValidateLastName_TooLong_ThrowsArgumentOutOfRangeException()
        {
            var user = CreateValidUser();
            user.LastName = new string('a', 101);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => user.ValidateLastName());
        }

        [TestMethod]
        public void ValidateLastName_InvalidCharacters_ThrowsArgumentException()
        {
            var user = CreateValidUser();
            user.LastName = "Larsen1";

            Assert.ThrowsException<ArgumentException>(() => user.ValidateLastName());
        }

        [TestMethod]
        public void ValidateLastName_Valid_DoesNotThrow()
        {
            var user = CreateValidUser();
            user.LastName = "Larsen";

            user.ValidateLastName();
        }

        // ---------- Roles ----------

        [TestMethod]
        public void ValidateRoles_Null_ThrowsArgumentNullException()
        {
            var user = CreateValidUser();
            user.Roles = null;

            Assert.ThrowsException<ArgumentNullException>(() => user.ValidateRoles());
        }

        [TestMethod]
        public void ValidateRoles_EmptyList_ThrowsArgumentOutOfRangeException()
        {
            var user = CreateValidUser();
            user.Roles = new List<string>();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => user.ValidateRoles());
        }

        [TestMethod]
        public void ValidateRoles_ContainsEmptyRole_ThrowsArgumentException()
        {
            var user = CreateValidUser();
            user.Roles = new List<string> { "normal", "   " };

            Assert.ThrowsException<ArgumentException>(() => user.ValidateRoles());
        }

        [TestMethod]
        public void ValidateRoles_Valid_DoesNotThrow()
        {
            var user = CreateValidUser();
            user.Roles = new List<string> { "normal", "machineOwner" };

            user.ValidateRoles();
        }

        // ---------- Email ----------

        [TestMethod]
        public void ValidateEmail_Null_ThrowsArgumentNullException()
        {
            var user = CreateValidUser();
            user.Email = null;

            Assert.ThrowsException<ArgumentNullException>(() => user.ValidateEmail());
        }

        [TestMethod]
        public void ValidateEmail_TooShort_ThrowsArgumentOutOfRangeException()
        {
            var user = CreateValidUser();
            user.Email = "a";

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => user.ValidateEmail());
        }

        [TestMethod]
        public void ValidateEmail_TooLong_ThrowsArgumentOutOfRangeException()
        {
            var user = CreateValidUser();
            user.Email = new string('a', 101) + "@mail.dk";

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => user.ValidateEmail());
        }

        [TestMethod]
        public void ValidateEmail_ContainsSpace_ThrowsArgumentException()
        {
            var user = CreateValidUser();
            user.Email = "mads mail.dk";

            Assert.ThrowsException<ArgumentException>(() => user.ValidateEmail());
        }

        [TestMethod]
        public void ValidateEmail_InvalidFormat_ThrowsArgumentException()
        {
            var user = CreateValidUser();
            user.Email = "mads.mail.dk";

            Assert.ThrowsException<ArgumentException>(() => user.ValidateEmail());
        }

        [TestMethod]
        public void ValidateEmail_Valid_DoesNotThrow()
        {
            var user = CreateValidUser();
            user.Email = "mads@mail.dk";

            user.ValidateEmail();
        }

        // ---------- PasswordHash (was Password) ----------

        [TestMethod]
        public void ValidatePasswordHash_Null_ThrowsArgumentNullException()
        {
            var user = CreateValidUser();
            user.PasswordHash = null!;
            Assert.ThrowsException<ArgumentNullException>(() => user.ValidatePassword());
        }

        [TestMethod]
        public void ValidatePasswordHash_Empty_ThrowsArgumentNullException()
        {
            var user = CreateValidUser();
            user.PasswordHash = "";
            Assert.ThrowsException<ArgumentNullException>(() => user.ValidatePassword());
        }

        [TestMethod]
        public void ValidatePasswordHash_NonEmpty_DoesNotThrow()
        {
            var user = CreateValidUser();
            user.PasswordHash = "some-hash-value";
            user.ValidatePassword();
        }


        // ---------- ValidateAll ----------
         
        [TestMethod]
        public void ValidateAll_ValidUser_DoesNotThrow()
        {
            var user = CreateValidUser();

            user.ValidateAll();
        }

        [TestMethod]
        public void ValidateAll_InvalidUser_ThrowsException()
        {
            var user = CreateValidUser();
            user.Email = "invalid";

            Assert.ThrowsException<ArgumentException>(() => user.ValidateAll());
        }
    }
}
