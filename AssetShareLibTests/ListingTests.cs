using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class ListingTests
    {
        // Helper: laver en gyldig listing
        private Listing CreateValidListing()
        {
            return new Listing
            {
                Id = 1,
                UserId = 2,
                MachineId = 3,
                Title = "Gravemaskine",
                Description = "Stor gravemaskine til udlejning.",
                Price = 1000m
            };
        }

        // ---------- ValidateIdPositive ----------

        [TestMethod]
        public void ValidateIdPositive_Positive_DoesNotThrow()
        {
            var listing = CreateValidListing();
            listing.Id = 1;

            listing.ValidateIdPositive();
        }

        [TestMethod]
        public void ValidateIdPositive_Zero_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.Id = 0;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateIdPositive());
        }

        [TestMethod]
        public void ValidateIdPositive_Negative_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.Id = -1;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateIdPositive());
        }

        // ---------- ValidateUserIdPositive ----------

        [TestMethod]
        public void ValidateUserIdPositive_Positive_DoesNotThrow()
        {
            var listing = CreateValidListing();
            listing.UserId = 2;

            listing.ValidateUserIdPositive();
        }

        [TestMethod]
        public void ValidateUserIdPositive_Zero_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.UserId = 0;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateUserIdPositive());
        }

        [TestMethod]
        public void ValidateUserIdPositive_Negative_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.UserId = -5;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateUserIdPositive());
        }

        // ---------- ValidateMachineIdPositive ----------

        [TestMethod]
        public void ValidateMachineIdPositive_Positive_DoesNotThrow()
        {
            var listing = CreateValidListing();
            listing.MachineId = 3;

            listing.ValidateMachineIdPositive();
        }

        [TestMethod]
        public void ValidateMachineIdPositive_Zero_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.MachineId = 0;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateMachineIdPositive());
        }

        [TestMethod]
        public void ValidateMachineIdPositive_Negative_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.MachineId = -3;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateMachineIdPositive());
        }

        // ---------- ValidateTitle ----------

        [TestMethod]
        public void ValidateTitle_Null_ThrowsArgumentNullException()
        {
            var listing = CreateValidListing();
            listing.Title = null;

            Assert.ThrowsException<ArgumentNullException>(() => listing.ValidateTitle());
        }

        [TestMethod]
        public void ValidateTitle_Whitespace_ThrowsArgumentNullException()
        {
            var listing = CreateValidListing();
            listing.Title = "   ";

            Assert.ThrowsException<ArgumentNullException>(() => listing.ValidateTitle());
        }

        [TestMethod]
        public void ValidateTitle_TooShort_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.Title = "Hi"; // 2 chars < 3

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateTitle());
        }

        [TestMethod]
        public void ValidateTitle_Valid_DoesNotThrow()
        {
            var listing = CreateValidListing();
            listing.Title = "Graver";

            listing.ValidateTitle();
        }

        // ---------- ValidateDescription ----------

        [TestMethod]
        public void ValidateDescription_Null_ThrowsArgumentNullException()
        {
            var listing = CreateValidListing();
            listing.Description = null;

            Assert.ThrowsException<ArgumentNullException>(() => listing.ValidateDescription());
        }

        [TestMethod]
        public void ValidateDescription_Whitespace_ThrowsArgumentNullException()
        {
            var listing = CreateValidListing();
            listing.Description = "   ";

            Assert.ThrowsException<ArgumentNullException>(() => listing.ValidateDescription());
        }

        [TestMethod]
        public void ValidateDescription_TooShort_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.Description = "Too short"; // < 10 chars

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateDescription());
        }

        [TestMethod]
        public void ValidateDescription_Valid_DoesNotThrow()
        {
            var listing = CreateValidListing();
            listing.Description = "God maskine til udlejning.";

            listing.ValidateDescription();
        }

        // ---------- ValidatePrice ----------

        [TestMethod]
        public void ValidatePrice_Positive_DoesNotThrow()
        {
            var listing = CreateValidListing();
            listing.Price = 100m;

            listing.ValidatePrice();
        }

        [TestMethod]
        public void ValidatePrice_Zero_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.Price = 0m;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidatePrice());
        }

        [TestMethod]
        public void ValidatePrice_Negative_ThrowsArgumentException()
        {
            var listing = CreateValidListing();
            listing.Price = -10m;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidatePrice());
        }

        // ---------- ValidateAll ----------

        [TestMethod]
        public void ValidateAll_ValidListing_DoesNotThrow()
        {
            var listing = CreateValidListing();

            listing.ValidateAll();
        }

        [TestMethod]
        public void ValidateAll_InvalidTitle_ThrowsException()
        {
            var listing = CreateValidListing();
            listing.Title = ""; // invalid → ValidateTitle

            Assert.ThrowsException<ArgumentNullException>(() => listing.ValidateAll());
        }

        [TestMethod]
        public void ValidateAll_InvalidPrice_ThrowsException()
        {
            var listing = CreateValidListing();
            listing.Price = 0m;

            Assert.ThrowsException<ArgumentException>(() => listing.ValidateAll());
        }

        // ---------- Copy constructor ----------

        [TestMethod]
        public void CopyConstructor_CopiesAllFields()
        {
            var original = CreateValidListing();
            original.Price = 1234.56m;

            var copy = new Listing(original);

            Assert.AreEqual(original.Id, copy.Id);
            Assert.AreEqual(original.UserId, copy.UserId);
            Assert.AreEqual(original.MachineId, copy.MachineId);
            Assert.AreEqual(original.Title, copy.Title);
            Assert.AreEqual(original.Description, copy.Description);
            Assert.AreEqual(original.Price, copy.Price);
        }
    }
}
