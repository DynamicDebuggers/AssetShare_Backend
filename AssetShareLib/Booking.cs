using System;

namespace AssetShareLib
{
  
    public class Booking
    {
        public int Id { get; set; }
        public int RentedByUserId { get; set; }
        public int BookedMachineId { get; set; }
        public DateTime Period { get; set; }
        public bool Status { get; set; } = false;

        public Booking()
        {
        }

        public Booking(Booking booking)
        {
            Id = booking.Id;
            RentedByUserId = booking.RentedByUserId;
            BookedMachineId = booking.BookedMachineId;
            Period = booking.Period;
            Status = booking.Status;
        }

        public void ValidateIdPositive()
        {
            if (Id <= 0) throw new ArgumentException("Id must be a positive number.");
        }

        public void ValidateRentedByUserIdPositive()
        {
            if (RentedByUserId <= 0) throw new ArgumentException("RentedByUserId must be a positive number.");
        }

        public void ValidateBookedMachineIdPositive()
        {
            if (BookedMachineId <= 0) throw new ArgumentException("BookedMachineId must be a positive number.");
        }

        public void ValidatePeriod()
        {
            if (Period == DateTime.MinValue) throw new ArgumentException("Period must be set.");
        }

        public void ValidateAll()
        {
            ValidateIdPositive();
            ValidateRentedByUserIdPositive();
            ValidateBookedMachineIdPositive();
            ValidatePeriod();
        }

        public override string ToString()
        {
            return $"Booking(Id={Id}, RentedByUserId={RentedByUserId}, BookedMachineId={BookedMachineId}, Period={Period:o}, Status={Status})";
        }
    }
}
