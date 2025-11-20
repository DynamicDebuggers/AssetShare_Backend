using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class BookingRepositoryTests
    {
        // Helper: laver en gyldig booking (uden Id, det sætter repo)
        private Booking CreateValidBooking()
        {
            return new Booking
            {
                RentedByUserId = 10,
                BookedMachineId = 20,
                Period = DateTime.UtcNow.AddDays(5),
                Status = false
            };
        }

        // ---------- Constructor / seeding ----------

        [TestMethod]
        public void Constructor_SeedsThreeBookings()
        {
            var repo = new BookingRepository();

            var all = repo.GetAll();

            Assert.AreEqual(3, all.Count, "Repository should seed exactly 3 bookings.");
            CollectionAssert.AreEquivalent(
                new List<int> { 1, 2, 3 },
                all.Select(b => b.Id).ToList()
            );
            Assert.IsTrue(all.All(b => b.Period != DateTime.MinValue));
        }

        // ---------- GetAll ----------

        [TestMethod]
        public void GetAll_ReturnsListWithAllBookings()
        {
            var repo = new BookingRepository();

            var all = repo.GetAll();

            Assert.AreEqual(3, all.Count);
        }

        // ---------- GetById ----------

        [TestMethod]
        public void GetById_ExistingId_ReturnsBooking()
        {
            var repo = new BookingRepository();

            var booking = repo.GetById(1);

            Assert.IsNotNull(booking);
            Assert.AreEqual(1, booking!.Id);
        }

        [TestMethod]
        public void GetById_NonExistingId_ReturnsNull()
        {
            var repo = new BookingRepository();

            var booking = repo.GetById(999);

            Assert.IsNull(booking);
        }

        // ---------- Create ----------

        [TestMethod]
        public void Create_ValidBooking_AssignsIdAndStores()
        {
            var repo = new BookingRepository();
            int beforeCount = repo.GetAll().Count;

            var newBooking = CreateValidBooking();

            var created = repo.Create(newBooking.RentedByUserId, newBooking.BookedMachineId, newBooking.Period);

            var all = repo.GetAll();
            Assert.AreEqual(beforeCount + 1, all.Count);
            Assert.IsTrue(created.Id > 0);
            Assert.IsTrue(all.Any(b =>
                b.Id == created.Id &&
                b.RentedByUserId == newBooking.RentedByUserId &&
                b.BookedMachineId == newBooking.BookedMachineId &&
                b.Period == newBooking.Period));
        }

        [TestMethod]
        public void Create_InvalidRentedByUserId_ThrowsArgumentException()
        {
            var repo = new BookingRepository();
            int beforeCount = repo.GetAll().Count;

            Assert.ThrowsException<ArgumentException>(() =>
                repo.Create(0, 1, DateTime.UtcNow.AddDays(1))
            );

            Assert.AreEqual(beforeCount, repo.GetAll().Count, "Repository should not change when Create fails.");
        }

        [TestMethod]
        public void Create_InvalidBookedMachineId_ThrowsArgumentException()
        {
            var repo = new BookingRepository();
            int beforeCount = repo.GetAll().Count;

            Assert.ThrowsException<ArgumentException>(() =>
                repo.Create(1, 0, DateTime.UtcNow.AddDays(1))
            );

            Assert.AreEqual(beforeCount, repo.GetAll().Count);
        }

        [TestMethod]
        public void Create_InvalidPeriod_ThrowsArgumentException()
        {
            var repo = new BookingRepository();
            int beforeCount = repo.GetAll().Count;

            Assert.ThrowsException<ArgumentException>(() =>
                repo.Create(1, 1, DateTime.MinValue)
            );

            Assert.AreEqual(beforeCount, repo.GetAll().Count);
        }

        // ---------- Update ----------

        [TestMethod]
        public void Update_ExistingBookingWithValidData_UpdatesFields()
        {
            var repo = new BookingRepository();
            var existing = repo.GetById(1)!;

            var updated = new Booking
            {
                // Id ignoreres – repo bruger id-parametret til at finde eksisterende
                RentedByUserId = 99,
                BookedMachineId = 88,
                Period = DateTime.UtcNow.AddDays(10),
                Status = true
            };

            var result = repo.Update(1, updated);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(99, result.RentedByUserId);
            Assert.AreEqual(88, result.BookedMachineId);
            Assert.AreEqual(updated.Period, result.Period);
            Assert.IsTrue(result.Status);
        }

        [TestMethod]
        public void Update_NonExistingId_ThrowsKeyNotFoundException()
        {
            var repo = new BookingRepository();

            var updated = CreateValidBooking();

            Assert.ThrowsException<KeyNotFoundException>(() =>
                repo.Update(999, updated)
            );
        }

        [TestMethod]
        public void Update_InvalidRentedByUserId_ThrowsArgumentException()
        {
            var repo = new BookingRepository();

            var updated = CreateValidBooking();
            updated.RentedByUserId = -1;

            Assert.ThrowsException<ArgumentException>(() =>
                repo.Update(1, updated)
            );
        }

        // ---------- Delete ----------

        [TestMethod]
        public void Delete_ExistingBooking_RemovesIt()
        {
            var repo = new BookingRepository();
            int beforeCount = repo.GetAll().Count;

            repo.Delete(1);

            var after = repo.GetAll();
            Assert.AreEqual(beforeCount - 1, after.Count);
            Assert.IsNull(repo.GetById(1));
        }

        [TestMethod]
        public void Delete_NonExistingBooking_DoesNotThrowOrChangeCount()
        {
            var repo = new BookingRepository();
            int beforeCount = repo.GetAll().Count;

            repo.Delete(999);

            Assert.AreEqual(beforeCount, repo.GetAll().Count);
        }
    }
}
