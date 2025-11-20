using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class MachineTests
    {
        // Helper: gyldig maskine
        private Machine CreateValidMachine()
        {
            return new Machine
            {
                Id = 1,
                UserId = 2,
                Title = "Gravemaskine",
                Description = "Stor gravemaskine til udlejning.",
                Price = 1000m,
                Location = "Aarhus"
            };
        }

        // ---------- ValidateTitle ----------

        [TestMethod]
        public void ValidateTitle_Null_ThrowsArgumentNullException()
        {
            var machine = CreateValidMachine();
            machine.Title = null;

            Assert.ThrowsException<ArgumentNullException>(() => machine.ValidateTitle());
        }

        [TestMethod]
        public void ValidateTitle_Whitespace_ThrowsArgumentNullException()
        {
            var machine = CreateValidMachine();
            machine.Title = "   ";

            Assert.ThrowsException<ArgumentNullException>(() => machine.ValidateTitle());
        }

        [TestMethod]
        public void ValidateTitle_TooShort_ThrowsArgumentException()
        {
            var machine = CreateValidMachine();
            machine.Title = "Hi"; // 2 chars < 3

            Assert.ThrowsException<ArgumentException>(() => machine.ValidateTitle());
        }

        [TestMethod]
        public void ValidateTitle_Valid_DoesNotThrow()
        {
            var machine = CreateValidMachine();
            machine.Title = "Graver";

            machine.ValidateTitle();
        }

        // ---------- ValidateDescription ----------

        [TestMethod]
        public void ValidateDescription_Null_ThrowsArgumentNullException()
        {
            var machine = CreateValidMachine();
            machine.Description = null;

            Assert.ThrowsException<ArgumentNullException>(() => machine.ValidateDescription());
        }

        [TestMethod]
        public void ValidateDescription_Whitespace_ThrowsArgumentNullException()
        {
            var machine = CreateValidMachine();
            machine.Description = "   ";

            Assert.ThrowsException<ArgumentNullException>(() => machine.ValidateDescription());
        }

        [TestMethod]
        public void ValidateDescription_TooShort_ThrowsArgumentException()
        {
            var machine = CreateValidMachine();
            machine.Description = "Too short"; // 9 chars < 10

            Assert.ThrowsException<ArgumentException>(() => machine.ValidateDescription());
        }

        [TestMethod]
        public void ValidateDescription_Valid_DoesNotThrow()
        {
            var machine = CreateValidMachine();
            machine.Description = "God maskine til udlejning.";

            machine.ValidateDescription();
        }

        // ---------- ValidatePrice ----------

        [TestMethod]
        public void ValidatePrice_Positive_DoesNotThrow()
        {
            var machine = CreateValidMachine();
            machine.Price = 100m;

            machine.ValidatePrice();
        }

        [TestMethod]
        public void ValidatePrice_Zero_ThrowsArgumentException()
        {
            var machine = CreateValidMachine();
            machine.Price = 0m;

            Assert.ThrowsException<ArgumentException>(() => machine.ValidatePrice());
        }

        [TestMethod]
        public void ValidatePrice_Negative_ThrowsArgumentException()
        {
            var machine = CreateValidMachine();
            machine.Price = -10m;

            Assert.ThrowsException<ArgumentException>(() => machine.ValidatePrice());
        }

        // ---------- ValidateLocation ----------

        [TestMethod]
        public void ValidateLocation_Null_ThrowsArgumentNullException()
        {
            var machine = CreateValidMachine();
            machine.Location = null;

            Assert.ThrowsException<ArgumentNullException>(() => machine.ValidateLocation());
        }

        [TestMethod]
        public void ValidateLocation_Whitespace_ThrowsArgumentNullException()
        {
            var machine = CreateValidMachine();
            machine.Location = "   ";

            Assert.ThrowsException<ArgumentNullException>(() => machine.ValidateLocation());
        }

        [TestMethod]
        public void ValidateLocation_Valid_DoesNotThrow()
        {
            var machine = CreateValidMachine();
            machine.Location = "København";

            machine.ValidateLocation();
        }

        // ---------- ValidateAll ----------

        [TestMethod]
        public void ValidateAll_ValidMachine_DoesNotThrow()
        {
            var machine = CreateValidMachine();

            machine.ValidateAll();
        }

        [TestMethod]
        public void ValidateAll_InvalidTitle_ThrowsException()
        {
            var machine = CreateValidMachine();
            machine.Title = ""; // invalid → ValidateTitle

            Assert.ThrowsException<ArgumentNullException>(() => machine.ValidateAll());
        }

        [TestMethod]
        public void ValidateAll_InvalidPrice_ThrowsException()
        {
            var machine = CreateValidMachine();
            machine.Price = 0m; // invalid → ValidatePrice

            Assert.ThrowsException<ArgumentException>(() => machine.ValidateAll());
        }

        // ---------- Copy constructor ----------

        [TestMethod]
        public void CopyConstructor_CopiesAllFields()
        {
            var original = CreateValidMachine();
            original.Price = 1234.56m;

            var copy = new Machine(original);

            Assert.AreEqual(original.Id, copy.Id);
            Assert.AreEqual(original.UserId, copy.UserId);
            Assert.AreEqual(original.Title, copy.Title);
            Assert.AreEqual(original.Description, copy.Description);
            Assert.AreEqual(original.Price, copy.Price);
            Assert.AreEqual(original.Location, copy.Location);
        }
    }
}
