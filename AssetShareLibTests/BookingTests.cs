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
            return new Booking
            {
                Id = 1,
                RentedByUserId = 2,
                BookedMachineId = 3,
                Period = new DateTime(2025, 1, 1),
                Status = false
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

        // -------- ValidatePeriod --------

        [TestMethod]
        public void ValidatePeriod_ValidDate_DoesNotThrow()
        {
            var booking = CreateValidBooking();
            booking.Period = new DateTime(2025, 1, 1);

            booking.ValidatePeriod();
        }

        [TestMethod]
        public void ValidatePeriod_MinValue_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.Period = DateTime.MinValue;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidatePeriod());
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
        public void ValidateAll_InvalidPeriod_ThrowsArgumentException()
        {
            var booking = CreateValidBooking();
            booking.Period = DateTime.MinValue;

            Assert.ThrowsException<ArgumentException>(() => booking.ValidateAll());
        }

        // -------- Copy constructor --------

        [TestMethod]
        public void CopyConstructor_CopiesAllFields()
        {
            var original = CreateValidBooking();
            original.Status = true;

            var copy = new Booking(original);

            Assert.AreEqual(original.Id, copy.Id);
            Assert.AreEqual(original.RentedByUserId, copy.RentedByUserId);
            Assert.AreEqual(original.BookedMachineId, copy.BookedMachineId);
            Assert.AreEqual(original.Period, copy.Period);
            Assert.AreEqual(original.Status, copy.Status);
        }
    }
}