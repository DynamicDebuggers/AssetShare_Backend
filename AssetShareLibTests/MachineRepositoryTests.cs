using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using AssetShareLib;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class MachineRepositoryTests
    {
        private MachineRepository _repo = null!;
        private MongoDbContext _context = null!;

        // >>>> Sæt din (gerne test-) connection string her <<<<
        private const string TestConnectionString =
            "mongodb+srv://tester:test123@cluster0.cvjiyiw.mongodb.net/?retryWrites=true&w=majority";

        private const string TestDatabaseName = "AssetShareDb";

        [TestInitialize]
        public void Setup()
        {
            var settings = new MongoDbSettings
            {
                ConnectionString = TestConnectionString,
                DatabaseName = TestDatabaseName
            };

            var options = Options.Create(settings);
            _context = new MongoDbContext(options);

            // Ryd Machines collection før hver test
            _context.Machines.DeleteMany(FilterDefinition<Machine>.Empty);

            _repo = new MachineRepository(_context);
        }

        private Machine CreateMachine(
            int userId = 1,
            string title = "Excavator",
            string description = "Large construction excavator",
            decimal price = 1500m,
            string location = "Hvidovre")
        {
            return new Machine
            {
                UserId = userId,
                Title = title,
                Description = description,
                Price = price,
                Location = location
            };
        }

        [TestMethod]
        public void MachineRepository_CanBeCreated()
        {
            Assert.IsNotNull(_repo);
        }

        [TestMethod]
        public async Task Get_ReturnsAllInsertedMachines()
        {
            // arrange: indsæt 5 maskiner
            await _repo.Add(CreateMachine(title: "Excavator", userId: 1));
            await _repo.Add(CreateMachine(title: "Mini Loader", userId: 1));
            await _repo.Add(CreateMachine(title: "Chainsaw", userId: 2));
            await _repo.Add(CreateMachine(title: "Tractor", userId: 3));
            await _repo.Add(CreateMachine(title: "Cement Mixer", userId: 2));

            // act
            var machines = await _repo.Get();

            // assert
            Assert.IsNotNull(machines);
            Assert.AreEqual(5, machines.Count);
        }

        [TestMethod]
        public async Task GetById_ExistingMachine_ReturnsIt()
        {
            // arrange
            var added = await _repo.Add(CreateMachine(
                title: "Excavator",
                description: "Large construction excavator",
                location: "Hvidovre"));

            // act
            var machine = await _repo.GetById(added.Id);

            // assert
            Assert.IsNotNull(machine);
            Assert.AreEqual(added.Id, machine!.Id);
            Assert.AreEqual("Excavator", machine.Title);
        }

        [TestMethod]
        public async Task Add_AssignsIdAndStoresMachine()
        {
            var newMachine = CreateMachine(
                userId: 10,
                title: "Bulldozer",
                description: "Heavy bulldozer",
                price: 2500m,
                location: "Aalborg");

            var added = await _repo.Add(newMachine);

            Assert.IsNotNull(added);
            Assert.IsTrue(added.Id > 0);
            Assert.AreEqual("Bulldozer", added.Title);

            var fromRepo = await _repo.GetById(added.Id);
            Assert.IsNotNull(fromRepo);
        }

        [TestMethod]
        public async Task Remove_ExistingMachine_RemovesItFromDatabase()
        {
            var added = await _repo.Add(CreateMachine(title: "Chainsaw", userId: 2));

            var removed = await _repo.Remove(added.Id);

            Assert.IsNotNull(removed);
            Assert.AreEqual(added.Id, removed!.Id);

            var again = await _repo.GetById(added.Id);
            Assert.IsNull(again);
        }

        [TestMethod]
        public async Task Update_ExistingMachine_UpdatesFields()
        {
            var added = await _repo.Add(CreateMachine(
                title: "Tractor",
                description: "Farm tractor, good condition",
                price: 1200m,
                location: "Vejle"));

            var updateValues = new Machine
            {
                Title = "Updated Tractor",
                Description = "Updated description",
                Price = 999m,
                Location = "Updated City",
                UserId = added.UserId
            };

            var updated = await _repo.Update(added.Id, updateValues);

            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Tractor", updated!.Title);
            Assert.AreEqual("Updated description", updated.Description);
            Assert.AreEqual(999m, updated.Price);
            Assert.AreEqual("Updated City", updated.Location);

            var fromRepo = await _repo.GetById(added.Id);
            Assert.IsNotNull(fromRepo);
            Assert.AreEqual("Updated Tractor", fromRepo!.Title);
        }
    }
}
