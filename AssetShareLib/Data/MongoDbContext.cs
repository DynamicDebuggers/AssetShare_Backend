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

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Machine> Machines => _database.GetCollection<Machine>("Machines");
    public IMongoCollection<Listing> Listings => _database.GetCollection<Listing>("Listings");
    public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
}
