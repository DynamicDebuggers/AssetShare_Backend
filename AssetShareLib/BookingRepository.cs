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
            // seed a few simple bookings
            Create(1, 1, DateTime.UtcNow.AddDays(1));
            Create(2, 2, DateTime.UtcNow.AddDays(2));
            Create(3, 3, DateTime.UtcNow.AddDays(3));
        }

        public Booking? GetById(int id)
        {
            return _bookings.FirstOrDefault(l => l.Id == id);
        }

        public List<Booking> GetAll()
        {
            return _bookings;
        }

        public Booking Create(int rentedByUserId, int bookedMachineId, DateTime period)
        {
            var booking = new Booking
            {
                Id = _nextId++,
                RentedByUserId = rentedByUserId,
                BookedMachineId = bookedMachineId,
                Period = period,
                Status = false
            };

            // use model validation helpers (keeps rules in one place)
            booking.ValidateRentedByUserIdPositive();
            booking.ValidateBookedMachineIdPositive();
            booking.ValidatePeriod();

            _bookings.Add(booking);
            return booking;
        }

        public Booking Update(int id, Booking updatedBooking)
        {
            var existing = _bookings.FirstOrDefault(b => b.Id == id);
            if (existing is null) throw new KeyNotFoundException($"Booking with Id {id} not found.");

            // apply updates (preserve Id)
            existing.RentedByUserId = updatedBooking.RentedByUserId != 0 ? updatedBooking.RentedByUserId : existing.RentedByUserId;
            existing.BookedMachineId = updatedBooking.BookedMachineId != 0 ? updatedBooking.BookedMachineId : existing.BookedMachineId;
            existing.Period = updatedBooking.Period != DateTime.MinValue ? updatedBooking.Period : existing.Period;
            existing.Status = updatedBooking.Status;

            // validate updated state
            existing.ValidateRentedByUserIdPositive();
            existing.ValidateBookedMachineIdPositive();
            existing.ValidatePeriod();

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
