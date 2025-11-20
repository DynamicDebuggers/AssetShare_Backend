namespace AssetShareLib
{
    public class UserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;


        public UserRepository() 
        {
            User user1 = new User()
            {
                FirstName = "Mads Aagaard",
                LastName = "Larsen",
                Roles = ["normal", "machineOwner"],
                Email = "mads@mail.com",
                Password = "madS#123"
            };

            Add(user1);
        }

        public IReadOnlyList<User> GetAll()
        {
            return _users.AsReadOnly();
        }

        public User? GetById(int id)
        {
            User? user = _users.FirstOrDefault(w => w.Id == id);
            if (user == null)
            {
                return null;
            }

            return user;
        }

        public User Add(User user)
        {
            user.ValidateAll();
            user.Id = _nextId++;
            _users.Add(user);
            return user;
        }

        public User? Update(int id, User updatedUser)
        {
            User? user = _users.FirstOrDefault(w => w.Id == id);
            if (user == null)
            {
                return null;
            }
            updatedUser.ValidateAll();

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Roles = updatedUser.Roles;
            user.Email = updatedUser.Email;
            user.Password = updatedUser.Password;

            return user;
        }

        public User? Delete(int id)
        {
            User? user = GetById(id);
            if (user == null)
            {
                return null;
            }

            _users.Remove(user);
            return user;
        }
    }
}
