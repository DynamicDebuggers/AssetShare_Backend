namespace AssetShareLib
{
    public class Machine
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Location { get; set; }

        public Machine(Machine machine)
        {
            Id = machine.Id;
            UserId = machine.UserId;
            Title = machine.Title;
            Description = machine.Description;
            Price = machine.Price;
            Location = machine.Location;
        }

        public Machine() { }

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

        public void ValidateLocation()
        {
            if (string.IsNullOrWhiteSpace(Location))
                throw new ArgumentNullException("Location must not be empty");
        }

        public void ValidateAll()
        {
            ValidateTitle();
            ValidateDescription();
            ValidatePrice();
            ValidateLocation();
        }

        public override string ToString()
        {
            return $"Machine Id: {Id}, UserId: {UserId}, Title: {Title}, Price: {Price}, Location: {Location}";
        }
    }
}
