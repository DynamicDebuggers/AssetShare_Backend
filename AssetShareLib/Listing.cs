using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace AssetShareLib
{
    public class Listing
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MachineId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public Listing(Listing listing)
        {
            Id = listing.Id;
            UserId = listing.UserId;
            MachineId = listing.MachineId;
            Title = listing.Title;
            Description = listing.Description;
            Price = listing.Price;
        }

        public Listing() 
        { 
        }

        public void ValidateIdPositive()
        {
            if (Id <= 0)
                throw new ArgumentException("Id must be a positive number.");
        }

        public void ValidateUserIdPositive()
        {
            if (UserId <= 0)
                throw new ArgumentException("Id must be a positive number.");
        }

        public void ValidateMachineIdPositive()
        {
            if (MachineId <= 0)
                throw new ArgumentException("Id must be a positive number.");
        }

        public void ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
                throw new ArgumentNullException("Title must not be empty");

            if (Title.Length < 3)
                throw new ArgumentException("Title must be at least 3 characters long");
        }

        public void ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
                throw new ArgumentNullException("Description must not be empty");

            if (Description.Length < 10)
                throw new ArgumentException("Description must be at least 10 characters long");
        }

        public void ValidatePrice()
        {
            if (Price <= 0)
                throw new ArgumentException("Price must be greater than 0");
        }

        public void ValidateAll()
        {
            ValidateIdPositive();
            ValidateMachineIdPositive();
            ValidateUserIdPositive();
            ValidateTitle();
            ValidateDescription();
            ValidatePrice();
        }

        public override string ToString()
        {
            return $"Listing(Id={Id}, UserId={UserId}, MachineId={MachineId}, Title={Title}, Description={Description}, Price={Price})";
        }


    }
}
