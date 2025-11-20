using System.Text.RegularExpressions;

namespace AssetShareLib
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string>? Roles { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }


        public void ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                throw new ArgumentNullException(nameof(FirstName), "First name cannot be null or empty");
            }
            if (FirstName.Length < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(FirstName), "First name must be at least 2 characters long");
            }
            if (FirstName.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(FirstName), "First name cannot be longer than 100 characters");
            }
            if (!FirstName.All(c => char.IsLetter(c) || c == ' '))
            {
                throw new ArgumentException("First name must only contain letters", nameof(FirstName));
            }
        }

        public void ValidateLastName()
        {
            if (string.IsNullOrWhiteSpace(LastName))
            {
                throw new ArgumentNullException(nameof(LastName), "Last name cannot be null or empty");
            }
            if (LastName.Length < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(LastName), "Last name must be at least 2 characters long");
            }
            if (LastName.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(LastName), "Last name cannot be longer than 100 characters");
            }
            if (!LastName.All(char.IsLetter))
            {
                throw new ArgumentException("Last name must only contain letters", nameof(LastName));
            }
        }

        public void ValidateRoles()
        {
            if (Roles == null)
            {
                throw new ArgumentNullException(nameof(Roles), "Roles cannot be null");
            }

            if (!Roles.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(Roles), "User must have at least one role");
            }

            if (Roles.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Roles cannot contain empty values", nameof(Roles));
            }
        }

        public void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                throw new ArgumentNullException(nameof(Email), "Email cannot be null or empty");
            }

            if (Email.Length < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(Email), "Email must be at least 2 characters long");
            }

            if (Email.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(Email), "Email cannot be longer than 100 characters");
            }

            if (Email.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException("Email cannot contain spaces", nameof(Email));
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ArgumentException("Email is not in a valid format", nameof(Email));
            }
        }

        public void ValidatePassword()
        {
            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentNullException(nameof(Password), "Password cannot be null or empty");
            }

            if (Password.Length < 8)
            {
                throw new ArgumentOutOfRangeException(nameof(Password), "Password must be at least 8 characters long");
            }

            if (Password.Length > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(Password), "Password cannot be longer than 30 characters");
            }

            if (!Password.Any(char.IsUpper))
            {
                throw new ArgumentException("Password must contain at least one upper case letter", nameof(Password));
            }

            if (!Password.Any(char.IsDigit))
            {
                throw new ArgumentException("Password must contain at least one number", nameof(Password));
            }

            if (!Password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            {
                throw new ArgumentException("Password must contain at least one symbol", nameof(Password));
            }

            if (Password.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException("Password cannot contain spaces", nameof(Password));
            }
        }

        public void ValidateAll()
        {
            ValidateFirstName();
            ValidateLastName();
            ValidateRoles();
            ValidateEmail();
            ValidatePassword();
        }
    }
}
