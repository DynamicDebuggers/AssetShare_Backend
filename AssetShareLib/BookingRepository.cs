using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetShareLib
{
    public class BookingRepository
    {
        private readonly List<Booking> _bookings = new();
        private int _nextId = 1;

        public BookingRepository()
        {
            // Seed: bookinger på 1 dag (fremtidige bookinger tæller som "aktive")
            var now = DateTime.UtcNow;
            Create(1, 1, now.AddDays(1), now.AddDays(2));
            Create(2, 2, now.AddDays(2), now.AddDays(3));
            Create(3, 3, now.AddDays(3), now.AddDays(4));
        }

        public Booking? GetById(int id)
        {
            return _bookings.FirstOrDefault(b => b.Id == id);
        }

        public List<Booking> GetAll()
        {
            return _bookings;
        }

        // Overlap-regel: [start, end) (end er eksklusiv)
        // => hvis A slutter præcis når B starter, er det OK (ingen konflikt).
        private static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
            => startA < endB && startB < endA;

        public Booking Create(int rentedByUserId, int bookedMachineId, DateTime startDate, DateTime endDate)
        {
            // tjek overlap på samme maskine
            bool conflict = _bookings.Any(b =>
                b.BookedMachineId == bookedMachineId &&
                Overlaps(startDate, endDate, b.StartDate, b.EndDate));

            if (conflict)
                throw new InvalidOperationException("The machine is already booked for the specified period.");

            var booking = new Booking
            {
                Id = _nextId++,
                RentedByUserId = rentedByUserId,
                BookedMachineId = bookedMachineId,
                StartDate = startDate,
                EndDate = endDate
            };

            booking.ValidateRentedByUserIdPositive();
            booking.ValidateBookedMachineIdPositive();
            booking.ValidateDates();

            _bookings.Add(booking);
            return booking;
        }

        public Booking Update(int id, Booking updatedBooking)
        {
            var existing = _bookings.FirstOrDefault(b => b.Id == id);
            if (existing is null)
                throw new KeyNotFoundException($"Booking with Id {id} not found.");

            // beregn nye værdier (så vi kan konflikttjekke før vi gemmer)
            int newRentedByUserId = updatedBooking.RentedByUserId != 0
                ? updatedBooking.RentedByUserId
                : existing.RentedByUserId;

            int newBookedMachineId = updatedBooking.BookedMachineId != 0
                ? updatedBooking.BookedMachineId
                : existing.BookedMachineId;

            DateTime newStartDate = updatedBooking.StartDate != DateTime.MinValue
                ? updatedBooking.StartDate
                : existing.StartDate;

            DateTime newEndDate = updatedBooking.EndDate != DateTime.MinValue
                ? updatedBooking.EndDate
                : existing.EndDate;

            // tjek overlap (ekskluder booking'en selv)
            bool conflict = _bookings.Any(b =>
                b.Id != id &&
                b.BookedMachineId == newBookedMachineId &&
                Overlaps(newStartDate, newEndDate, b.StartDate, b.EndDate));

            if (conflict)
                throw new InvalidOperationException("The machine is already booked for the specified period.");

            // apply
            existing.RentedByUserId = newRentedByUserId;
            existing.BookedMachineId = newBookedMachineId;
            existing.StartDate = newStartDate;
            existing.EndDate = newEndDate;

            // validate
            existing.ValidateRentedByUserIdPositive();
            existing.ValidateBookedMachineIdPositive();
            existing.ValidateDates();

            return existing;
        }

        public void Delete(int id)
        {
            var existing = _bookings.FirstOrDefault(b => b.Id == id);
            if (existing != null)
                _bookings.Remove(existing);
        }
    }
}
