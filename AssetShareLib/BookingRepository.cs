using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AssetShareLib
{
    public class BookingRepository
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingRepository(MongoDbContext context)
        {
            _bookings = context.Bookings;
        }

        // GetAll() : List<Booking>
        public async Task<List<Booking>> GetAll()
        {
            return await _bookings
                .Find(FilterDefinition<Booking>.Empty)
                .ToListAsync();
        }

        // GetById(id) : Booking? (null hvis ikke fundet)
        public async Task<Booking?> GetById(int id)
        {
            return await _bookings
                .Find(b => b.Id == id)
                .FirstOrDefaultAsync();
        }

        // Create(...) : Booking
        // Jeg ændrer signaturen til at tage et Booking-objekt (bedre til Web API)
        public async Task<Booking> Create(Booking booking)
        {
            // brug dine egne valideringsmetoder
            booking.ValidateRentedByUserIdPositive();
            booking.ValidateBookedMachineIdPositive();
            booking.ValidatePeriod();

            // Find højeste Id i databasen og læg 1 til (samme idé som _nextId)
            var lastBooking = await _bookings
                .Find(FilterDefinition<Booking>.Empty)
                .SortByDescending(b => b.Id)
                .Limit(1)
                .FirstOrDefaultAsync();

            booking.Id = (lastBooking?.Id ?? 0) + 1;

            await _bookings.InsertOneAsync(booking);
            return booking;
        }

        // Update(id, updatedBooking) : Booking (eller KeyNotFoundException hvis ikke fundet)
        public async Task<Booking> Update(int id, Booking updatedBooking)
        {
            var existing = await _bookings
                .Find(b => b.Id == id)
                .FirstOrDefaultAsync();

            if (existing is null)
                throw new KeyNotFoundException($"Booking with Id {id} not found.");

            // samme “behold gammel værdi hvis 0/default”-logik som din in-memory version
            if (updatedBooking.RentedByUserId != 0)
                existing.RentedByUserId = updatedBooking.RentedByUserId;

            if (updatedBooking.BookedMachineId != 0)
                existing.BookedMachineId = updatedBooking.BookedMachineId;

            if (updatedBooking.Period != DateTime.MinValue)
                existing.Period = updatedBooking.Period;

            existing.Status = updatedBooking.Status;

            // validate updated state
            existing.ValidateRentedByUserIdPositive();
            existing.ValidateBookedMachineIdPositive();
            existing.ValidatePeriod();

            await _bookings.ReplaceOneAsync(b => b.Id == id, existing);
            return existing;
        }

        // Delete(id) : void (som før, gør ingenting hvis ikke fundet)
        public async Task Delete(int id)
        {
            await _bookings.DeleteOneAsync(b => b.Id == id);
        }
    }
}
