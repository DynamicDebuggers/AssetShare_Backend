using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetShareLib
{
    public class BookingRepository
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingRepository(MongoDbContext context)
        {
            _bookings = context.Bookings; // <-- "Bookings" collection i MongoDB
        }

        // Overlap-regel: [start, end)
        private static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
            => startA < endB && startB < endA;

        public List<Booking> GetAll()
        {
            return _bookings.Find(FilterDefinition<Booking>.Empty).ToList();
        }

        public Booking? GetById(int id)
        {
            return _bookings.Find(b => b.Id == id).FirstOrDefault();
        }

        public Booking Create(int rentedByUserId, int bookedMachineId, DateTime startDate, DateTime endDate)
        {
            // Model-validering
            var tmp = new Booking
            {
                Id = 1, // dummy til ValidateAll (Id bruges ikke til insert her)
                RentedByUserId = rentedByUserId,
                BookedMachineId = bookedMachineId,
                StartDate = startDate,
                EndDate = endDate
            };
            tmp.ValidateRentedByUserIdPositive();
            tmp.ValidateBookedMachineIdPositive();
            tmp.ValidateDates();

            // Konflikt-tjek direkte i MongoDB: samme maskine og overlap
            var conflictFilter =
                Builders<Booking>.Filter.Eq(b => b.BookedMachineId, bookedMachineId) &
                Builders<Booking>.Filter.Lt(b => b.StartDate, endDate) &
                Builders<Booking>.Filter.Gt(b => b.EndDate, startDate);

            bool conflict = _bookings.Find(conflictFilter).Any();
            if (conflict)
                throw new InvalidOperationException("The machine is already booked for the specified period.");

            // Simpel int-id (OK til projekt; ikke perfekt under concurrency)
            int nextId = (_bookings.Find(FilterDefinition<Booking>.Empty)
                                  .SortByDescending(b => b.Id)
                                  .Limit(1)
                                  .FirstOrDefault()?.Id ?? 0) + 1;

            var booking = new Booking
            {
                Id = nextId,
                RentedByUserId = rentedByUserId,
                BookedMachineId = bookedMachineId,
                StartDate = startDate,
                EndDate = endDate
            };

            _bookings.InsertOne(booking);
            return booking;
        }

        public Booking Update(int id, Booking updatedBooking)
        {
            var existing = GetById(id);
            if (existing is null)
                throw new KeyNotFoundException($"Booking with Id {id} not found.");

            // Partial update
            int newRentedByUserId = updatedBooking.RentedByUserId != 0 ? updatedBooking.RentedByUserId : existing.RentedByUserId;
            int newBookedMachineId = updatedBooking.BookedMachineId != 0 ? updatedBooking.BookedMachineId : existing.BookedMachineId;
            DateTime newStartDate = updatedBooking.StartDate != DateTime.MinValue ? updatedBooking.StartDate : existing.StartDate;
            DateTime newEndDate = updatedBooking.EndDate != DateTime.MinValue ? updatedBooking.EndDate : existing.EndDate;

            // Valider samlet resultat
            var tmp = new Booking
            {
                Id = id,
                RentedByUserId = newRentedByUserId,
                BookedMachineId = newBookedMachineId,
                StartDate = newStartDate,
                EndDate = newEndDate
            };
            tmp.ValidateAll();

            // Konflikt-tjek mod andre bookinger på samme maskine
            var conflictFilter =
                Builders<Booking>.Filter.Ne(b => b.Id, id) &
                Builders<Booking>.Filter.Eq(b => b.BookedMachineId, newBookedMachineId) &
                Builders<Booking>.Filter.Lt(b => b.StartDate, newEndDate) &
                Builders<Booking>.Filter.Gt(b => b.EndDate, newStartDate);

            bool conflict = _bookings.Find(conflictFilter).Any();
            if (conflict)
                throw new InvalidOperationException("The machine is already booked for the specified period.");

            // Apply
            existing.RentedByUserId = newRentedByUserId;
            existing.BookedMachineId = newBookedMachineId;
            existing.StartDate = newStartDate;
            existing.EndDate = newEndDate;

            _bookings.ReplaceOne(b => b.Id == id, existing);
            return existing;
        }

        public void Delete(int id)
        {
            _bookings.DeleteOne(b => b.Id == id);
        }
    }
}
