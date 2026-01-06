namespace AssetShareLib
{
    public class Booking
    {
        public int Id { get; set; }
        public int RentedByUserId { get; set; }
        public int BookedMachineId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive => DateTime.UtcNow <= EndDate;

        public Booking() { }

        public Booking(Booking booking)
        {
            Id = booking.Id;
            RentedByUserId = booking.RentedByUserId;
            BookedMachineId = booking.BookedMachineId;
            StartDate = booking.StartDate;
            EndDate = booking.EndDate;
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

        public void ValidateDates()
        {
            if (StartDate == DateTime.MinValue)
                throw new ArgumentException("StartDate must be set.");

            if (EndDate == DateTime.MinValue)
                throw new ArgumentException("EndDate must be set.");

            if (EndDate < StartDate)
                throw new ArgumentException("EndDate cannot be before StartDate.");
        }

        public void ValidateAll()
        {
            ValidateIdPositive();
            ValidateRentedByUserIdPositive();
            ValidateBookedMachineIdPositive();
            ValidateDates();
        }
    }
}
