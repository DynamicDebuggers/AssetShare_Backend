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
            // Brug din egen validering
            user.ValidateAll();

            // Find det højeste Id i databasen og læg 1 til
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
            updatedUser.ValidateAll();
            updatedUser.Id = id; // sørg for at Id matcher

            var result = await _users.ReplaceOneAsync(
                u => u.Id == id,
                updatedUser
            );

            if (result.MatchedCount == 0)
            {
                return null;
            }

            return updatedUser;
        }

        public async Task<User?> DeleteAsync(int id)
        {
            return await _users.FindOneAndDeleteAsync(u => u.Id == id);
        }
    }
}
