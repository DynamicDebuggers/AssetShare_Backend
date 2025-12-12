using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace AssetShareLib;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;

        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<User> Users
        => _database.GetCollection<User>("Users");

    // Senere kan du tilføje Machines, Listings, Bookings osv.
}
