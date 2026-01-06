using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class BookingTests
    {
        // Helper: laver en gyldig booking
        private Booking CreateValidBooking()
        {
            var now = DateTime.UtcNow;

            return new Booking
            {
                Id = 1,
                RentedByUserId = 2,
                BookedMachineId = 3,
                StartDate = now.AddDays(1),
                EndDate = now.AddDays(2)
            };
        }

        // -------- ValidateIdPositive --------

        [TestMethod]
        public void ValidateIdPositive_PositiveId_DoesNotThrow()
        {
            var booking = CreateValidBooking();
            booking.Id = 1;

            booking.ValidateIdPositive(); // passer, hvis ingen exception
        }

        [TestMethod]
        public void ValidateIdPositive_Zero_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.Id = 0;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateIdPositive());
        }

        [TestMethod]
        public void ValidateIdPositive_Negative_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.Id = -1;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateIdPositive());
        }

        // -------- ValidateRentedByUserIdPositive --------

        [TestMethod]
        public void ValidateRentedByUserIdPositive_Positive_DoesNotThrow()
        {
            var booking = CreateValidBooking();
            booking.RentedByUserId = 2;

            booking.ValidateRentedByUserIdPositive();
        }

        [TestMethod]
        public void ValidateRentedByUserIdPositive_Zero_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.RentedByUserId = 0;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateRentedByUserIdPositive());
        }

        [TestMethod]
        public void ValidateRentedByUserIdPositive_Negative_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.RentedByUserId = -5;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateRentedByUserIdPositive());
        }

        // -------- ValidateBookedMachineIdPositive --------

        [TestMethod]
        public void ValidateBookedMachineIdPositive_Positive_DoesNotThrow()
        {
            var booking = CreateValidBooking();
            booking.BookedMachineId = 3;

            booking.ValidateBookedMachineIdPositive();
        }

        [TestMethod]
        public void ValidateBookedMachineIdPositive_Zero_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.BookedMachineId = 0;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateBookedMachineIdPositive());
        }

        [TestMethod]
        public void ValidateBookedMachineIdPositive_Negative_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.BookedMachineId = -10;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateBookedMachineIdPositive());
        }

        // -------- ValidateDates --------

        [TestMethod]
        public void ValidateDates_ValidDates_DoesNotThrow()
        {
            var booking = CreateValidBooking();

            booking.ValidateDates();
        }

        [TestMethod]
        public void ValidateDates_StartDateMinValue_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.StartDate = DateTime.MinValue;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateDates());
        }

        [TestMethod]
        public void ValidateDates_EndDateMinValue_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.EndDate = DateTime.MinValue;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateDates());
        }

        [TestMethod]
        public void ValidateDates_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.StartDate = new DateTime(2025, 1, 2);
            booking.EndDate = new DateTime(2025, 1, 1);

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateDates());
        }

        // -------- ValidateAll --------

        [TestMethod]
        public void ValidateAll_ValidBooking_DoesNotThrow()
        {
            var booking = CreateValidBooking();

            booking.ValidateAll(); // ingen exception = test passer
        }

        [TestMethod]
        public void ValidateAll_InvalidId_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.Id = 0; // ugyldig

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateAll());
        }

        [TestMethod]
        public void ValidateAll_InvalidDates_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.StartDate = DateTime.MinValue;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateAll());
        }

        // -------- IsActive --------

        [TestMethod]
        public void IsActive_EndDateInFuture_ReturnsTrue()
        {
            var booking = CreateValidBooking();
            booking.StartDate = DateTime.UtcNow.AddDays(1);
            booking.EndDate = DateTime.UtcNow.AddDays(2);

            Assert.IsTrue(booking.IsActive);
        }

        [TestMethod]
        public void IsActive_EndDateInPast_ReturnsFalse()
        {
            var booking = CreateValidBooking();
            booking.StartDate = DateTime.UtcNow.AddDays(-3);
            booking.EndDate = DateTime.UtcNow.AddDays(-1);

            Assert.IsFalse(booking.IsActive);
        }

        // -------- Copy constructor --------

        [TestMethod]
        public void CopyConstructor_CopiesAllFields()
        {
            var original = CreateValidBooking();

            var copy = new Booking(original);

            Assert.AreEqual(original.Id, copy.Id);
            Assert.AreEqual(original.RentedByUserId, copy.RentedByUserId);
            Assert.AreEqual(original.BookedMachineId, copy.BookedMachineId);
            Assert.AreEqual(original.StartDate, copy.StartDate);
            Assert.AreEqual(original.EndDate, copy.EndDate);
        }
    }
}
