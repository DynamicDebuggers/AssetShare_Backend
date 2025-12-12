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
    public class BookingRepositoryTests
    {
        private BookingRepository _repo = null!;
        private MongoDbContext _context = null!;

        // >>>> Sæt din egen test-connection string her <<<<
        private const string TestConnectionString =
            "mongodb+srv://tester:test123@cluster0.cvjiyiw.mongodb.net/?retryWrites=true&w=majority";

        private const string TestDatabaseName = "AssetShareDb";

        [TestInitialize]
        public void Setup()
        {
            // Byg settings til test
            var settings = new MongoDbSettings
            {
                ConnectionString = TestConnectionString,
                DatabaseName = TestDatabaseName
            };

            var options = Options.Create(settings);
            _context = new MongoDbContext(options);

            // Ryd Bookings collection før hver test, så vi starter clean
            _context.Bookings.DeleteMany(FilterDefinition<Booking>.Empty);

            _repo = new BookingRepository(_context);
        }

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

        // ---------- GetAll ----------

        [TestMethod]
        public async Task GetAll_EmptyAtStart_ReturnsEmptyList()
        {
            var all = await _repo.GetAll();

            Assert.AreEqual(0, all.Count, "New test DB should start with 0 bookings.");
        }

        // ---------- Create ----------

        [TestMethod]
        public async Task Create_ValidBooking_AssignsIdAndStores()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var newBooking = CreateValidBooking();

            var created = await _repo.Create(newBooking);

            var all = await _repo.GetAll();
            Assert.AreEqual(beforeCount + 1, all.Count);
            Assert.IsTrue(created.Id > 0);

            Assert.IsTrue(all.Any(b =>
                b.Id == created.Id &&
                b.RentedByUserId == newBooking.RentedByUserId &&
                b.BookedMachineId == newBooking.BookedMachineId &&
                b.Period == newBooking.Period));
        }

        [TestMethod]
        public async Task Create_InvalidRentedByUserId_ThrowsArgumentException_AndDoesNotStore()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var booking = CreateValidBooking();
            booking.RentedByUserId = 0; // ugyldig

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.Create(booking);
            });

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        [TestMethod]
        public async Task Create_InvalidBookedMachineId_ThrowsArgumentException_AndDoesNotStore()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var booking = CreateValidBooking();
            booking.BookedMachineId = 0; // ugyldig

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.Create(booking);
            });

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        [TestMethod]
        public async Task Create_InvalidPeriod_ThrowsArgumentException_AndDoesNotStore()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var booking = CreateValidBooking();
            booking.Period = DateTime.MinValue; // ugyldig

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.Create(booking);
            });

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        // ---------- GetById ----------

        [TestMethod]
        public async Task GetById_ExistingId_ReturnsBooking()
        {
            var created = await _repo.Create(CreateValidBooking());

            var booking = await _repo.GetById(created.Id);

            Assert.IsNotNull(booking);
            Assert.AreEqual(created.Id, booking!.Id);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNull()
        {
            var booking = await _repo.GetById(999);

            Assert.IsNull(booking);
        }

        // ---------- Update ----------

        [TestMethod]
        public async Task Update_ExistingBookingWithValidData_UpdatesFields()
        {
            var created = await _repo.Create(CreateValidBooking());

            var updatedValues = new Booking
            {
                RentedByUserId = 99,
                BookedMachineId = 88,
                Period = DateTime.UtcNow.AddDays(10),
                Status = true
            };

            var result = await _repo.Update(created.Id, updatedValues);

            Assert.IsNotNull(result);
            Assert.AreEqual(created.Id, result.Id);
            Assert.AreEqual(99, result.RentedByUserId);
            Assert.AreEqual(88, result.BookedMachineId);
            Assert.AreEqual(updatedValues.Period, result.Period);
            Assert.IsTrue(result.Status);
        }

        [TestMethod]
        public async Task Update_NonExistingId_ThrowsKeyNotFoundException()
        {
            var updated = CreateValidBooking();

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            {
                await _repo.Update(999, updated);
            });
        }

        [TestMethod]
        public async Task Update_InvalidRentedByUserId_ThrowsArgumentException()
        {
            var created = await _repo.Create(CreateValidBooking());

            var updated = CreateValidBooking();
            updated.RentedByUserId = -1; // ugyldig

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.Update(created.Id, updated);
            });
        }

        // ---------- Delete ----------

        [TestMethod]
        public async Task Delete_ExistingBooking_RemovesIt()
        {
            var created = await _repo.Create(CreateValidBooking());
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            await _repo.Delete(created.Id);

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount - 1, after.Count);
            var shouldBeNull = await _repo.GetById(created.Id);
            Assert.IsNull(shouldBeNull);
        }

        [TestMethod]
        public async Task Delete_NonExistingBooking_DoesNotThrowOrChangeCount()
        {
            var b1 = await _repo.Create(CreateValidBooking());
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            await _repo.Delete(999);

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }
    }
}
