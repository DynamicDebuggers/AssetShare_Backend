using MongoDB.Driver;

namespace AssetShareLib
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.Users;
        }

        private static void ValidateUserForStorage(User user)
        {
            user.ValidateFirstName();
            user.ValidateLastName();
            user.ValidateRoles();
            user.ValidateEmail();

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new ArgumentNullException(nameof(user.PasswordHash), "PasswordHash is required.");
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            var list = await _users
                .Find(FilterDefinition<User>.Empty)
                .ToListAsync();

            return list.AsReadOnly();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _users
                .Find(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            ValidateUserForStorage(user);

            var lastUser = await _users
                .Find(FilterDefinition<User>.Empty)
                .SortByDescending(u => u.Id)
                .Limit(1)
                .FirstOrDefaultAsync();

            user.Id = (lastUser?.Id ?? 0) + 1;

            await _users.InsertOneAsync(user);
            return user;
        }


        public async Task<User?> UpdateAsync(int id, User updatedUser)
        {
            updatedUser.Id = id;
            ValidateUserForStorage(updatedUser);

            var result = await _users.ReplaceOneAsync(u => u.Id == id, updatedUser);

            if (result.MatchedCount == 0)
                return null;

            return updatedUser;
        }


        public async Task<User?> DeleteAsync(int id)
        {
            return await _users.FindOneAndDeleteAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }


    }
}
