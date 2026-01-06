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
            var start = DateTime.UtcNow.AddDays(5);
            var end = start.AddDays(1);

            return new Booking
            {
                RentedByUserId = 10,
                BookedMachineId = 20,
                StartDate = start,
                EndDate = end
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
                b.StartDate == newBooking.StartDate &&
                b.EndDate == newBooking.EndDate));
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
        public async Task Create_InvalidStartDate_ThrowsArgumentException_AndDoesNotStore()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var booking = CreateValidBooking();
            booking.StartDate = DateTime.MinValue;

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.Create(booking);
            });

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        [TestMethod]
        public async Task Create_InvalidEndDate_ThrowsArgumentException_AndDoesNotStore()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var booking = CreateValidBooking();
            booking.EndDate = DateTime.MinValue;

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.Create(booking);
            });

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        [TestMethod]
        public async Task Create_EndDateBeforeStartDate_ThrowsArgumentException_AndDoesNotStore()
        {
            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            var booking = CreateValidBooking();
            booking.StartDate = new DateTime(2025, 1, 2);
            booking.EndDate = new DateTime(2025, 1, 1);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _repo.Create(booking);
            });

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        // ---------- Create (Overlap / Conflict) ----------

        [TestMethod]
        public async Task Create_SameMachine_OverlappingDates_ThrowsInvalidOperationException_AndDoesNotStore()
        {
            // Arrange
            var start1 = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var end1 = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc);

            var start2 = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc); // overlapper
            var end2 = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc);

            await _repo.Create(new Booking
            {
                RentedByUserId = 1,
                BookedMachineId = 99,
                StartDate = start1,
                EndDate = end1
            });

            var before = await _repo.GetAll();
            int beforeCount = before.Count;

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await _repo.Create(new Booking
                {
                    RentedByUserId = 2,
                    BookedMachineId = 99, // samme maskine
                    StartDate = start2,
                    EndDate = end2
                });
            });

            var after = await _repo.GetAll();
            Assert.AreEqual(beforeCount, after.Count);
        }

        [TestMethod]
        public async Task Create_SameMachine_BackToBackDates_DoesNotThrow_StoresBoth()
        {
            // Arrange: [10,12) og [12,14) => ingen overlap
            var start1 = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var end1 = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc);

            var start2 = end1; // back-to-back
            var end2 = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc);

            await _repo.Create(new Booking
            {
                RentedByUserId = 1,
                BookedMachineId = 77,
                StartDate = start1,
                EndDate = end1
            });

            // Act
            await _repo.Create(new Booking
            {
                RentedByUserId = 2,
                BookedMachineId = 77,
                StartDate = start2,
                EndDate = end2
            });

            // Assert
            var all = await _repo.GetAll();
            Assert.AreEqual(2, all.Count);
        }

        [TestMethod]
        public async Task Create_DifferentMachine_SameDates_DoesNotThrow_StoresBoth()
        {
            var start = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc);

            await _repo.Create(new Booking
            {
                RentedByUserId = 1,
                BookedMachineId = 1,
                StartDate = start,
                EndDate = end
            });

            await _repo.Create(new Booking
            {
                RentedByUserId = 2,
                BookedMachineId = 2, // anden maskine
                StartDate = start,
                EndDate = end
            });

            var all = await _repo.GetAll();
            Assert.AreEqual(2, all.Count);
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
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(12)
            };

            var result = await _repo.Update(created.Id, updatedValues);

            Assert.IsNotNull(result);
            Assert.AreEqual(created.Id, result.Id);
            Assert.AreEqual(99, result.RentedByUserId);
            Assert.AreEqual(88, result.BookedMachineId);
            Assert.AreEqual(updatedValues.StartDate, result.StartDate);
            Assert.AreEqual(updatedValues.EndDate, result.EndDate);

            // Fremtidig booking => aktiv
            Assert.IsTrue(result.IsActive);
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

        [TestMethod]
        public async Task Update_ToOverlappingPeriodOnSameMachine_ThrowsInvalidOperationException()
        {
            // Arrange
            var b1 = await _repo.Create(new Booking
            {
                RentedByUserId = 1,
                BookedMachineId = 50,
                StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc)
            });

            var b2 = await _repo.Create(new Booking
            {
                RentedByUserId = 2,
                BookedMachineId = 50,
                StartDate = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 2, 7, 0, 0, 0, DateTimeKind.Utc)
            });

            // Act + Assert: prøv at flytte b2 så den overlapper b1
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await _repo.Update(b2.Id, new Booking
                {
                    RentedByUserId = b2.RentedByUserId,
                    BookedMachineId = b2.BookedMachineId,
                    StartDate = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc), // overlap
                    EndDate = new DateTime(2026, 2, 6, 0, 0, 0, DateTimeKind.Utc)
                });
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

            // Overlap-regel: [start, end)
            private static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
                => startA < endB && startB < endA;

            public Task<Booking> Create(Booking booking)
            {
                Validate(booking);

                bool conflict = _bookings.Any(b =>
                    b.BookedMachineId == booking.BookedMachineId &&
                    Overlaps(booking.StartDate, booking.EndDate, b.StartDate, b.EndDate));

                if (conflict)
                    throw new InvalidOperationException("The machine is already booked for the specified period.");

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

                bool conflict = _bookings.Any(b =>
                    b.Id != id &&
                    b.BookedMachineId == updated.BookedMachineId &&
                    Overlaps(updated.StartDate, updated.EndDate, b.StartDate, b.EndDate));

                if (conflict)
                    throw new InvalidOperationException("The machine is already booked for the specified period.");

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
                if (b.StartDate == DateTime.MinValue) throw new ArgumentException("StartDate is invalid");
                if (b.EndDate == DateTime.MinValue) throw new ArgumentException("EndDate is invalid");
                if (b.EndDate < b.StartDate) throw new ArgumentException("EndDate cannot be before StartDate");
            }

            private static Booking Clone(Booking b) => new Booking
            {
                Id = b.Id,
                RentedByUserId = b.RentedByUserId,
                BookedMachineId = b.BookedMachineId,
                StartDate = b.StartDate,
                EndDate = b.EndDate
            };
        }
    }
}
