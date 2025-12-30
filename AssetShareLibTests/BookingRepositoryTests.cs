using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class BookingRepositoryTests
    {
        private InMemoryBookingRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            _repo = new InMemoryBookingRepository();
        }

        // Helper: laver en gyldig booking
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
            Assert.AreEqual(0, all.Count);
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
            booking.RentedByUserId = 0;

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
            booking.BookedMachineId = 0;

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
            booking.Period = DateTime.MinValue;

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
            updated.RentedByUserId = -1;

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
            await _repo.Create(CreateValidBooking());

            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            await _repo.Delete(999);

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        // ---------------------------
        // In-memory repo ONLY for tests
        // ---------------------------
        private class InMemoryBookingRepository
        {
            private readonly List<Booking> _bookings = new();
            private int _nextId = 1;

            public Task<List<Booking>> GetAll()
                => Task.FromResult(_bookings.ToList());

            public Task<Booking?> GetById(int id)
                => Task.FromResult(_bookings.FirstOrDefault(b => b.Id == id));

            public Task<Booking> Create(Booking booking)
            {
                Validate(booking);

                booking.Id = _nextId++;
                _bookings.Add(Clone(booking));

                return Task.FromResult(booking);
            }

            public Task<Booking> Update(int id, Booking updated)
            {
                Validate(updated);

                var idx = _bookings.FindIndex(b => b.Id == id);
                if (idx < 0)
                    throw new KeyNotFoundException($"Booking with id {id} not found.");

                updated.Id = id;
                _bookings[idx] = Clone(updated);

                return Task.FromResult(updated);
            }

            public Task Delete(int id)
            {
                var existing = _bookings.FirstOrDefault(b => b.Id == id);
                if (existing != null) _bookings.Remove(existing);
                return Task.CompletedTask;
            }

            private static void Validate(Booking b)
            {
                if (b == null) throw new ArgumentNullException(nameof(b));
                if (b.RentedByUserId <= 0) throw new ArgumentException("RentedByUserId must be > 0");
                if (b.BookedMachineId <= 0) throw new ArgumentException("BookedMachineId must be > 0");
                if (b.Period == DateTime.MinValue) throw new ArgumentException("Period is invalid");
            }

            private static Booking Clone(Booking b) => new Booking
            {
                Id = b.Id,
                RentedByUserId = b.RentedByUserId,
                BookedMachineId = b.BookedMachineId,
                Period = b.Period,
                Status = b.Status
            };
        }
    }
}
