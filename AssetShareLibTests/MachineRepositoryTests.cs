using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetShareLib;
using System.Collections.Generic;
using System.Linq;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class MachineRepositoryTests
    {
        private InMemoryMachineRepository repo = null!;

        [TestInitialize]
        public void Setup()
        {
            repo = new InMemoryMachineRepository();

            // Seed 5 machines (så dine eksisterende forventninger giver mening)
            repo.Add(new Machine { UserId = 1, Title = "Excavator", Description = "Excavator machine", Price = 1000, Location = "Aalborg" }); // id 1
            repo.Add(new Machine { UserId = 2, Title = "Tractor", Description = "Tractor machine", Price = 800, Location = "Odense" }); // id 2
            repo.Add(new Machine { UserId = 3, Title = "Crane", Description = "Crane machine", Price = 1500, Location = "Aarhus" }); // id 3
            repo.Add(new Machine { UserId = 4, Title = "Loader", Description = "Loader machine", Price = 900, Location = "Cph" }); // id 4
            repo.Add(new Machine { UserId = 5, Title = "Forklift", Description = "Forklift machine", Price = 600, Location = "Aalborg" }); // id 5
        }

        [TestMethod]
        public void MachineRepositoryTest()
        {
            Assert.IsNotNull(repo);
        }

        [TestMethod]
        public void GetTest()
        {
            var machines = repo.GetAll();

            Assert.IsNotNull(machines);
            Assert.AreEqual(5, machines.Count);
        }

        [TestMethod]
        public void GetByIdTest()
        {
            var machine = repo.GetById(1);

            Assert.IsNotNull(machine);
            Assert.AreEqual(1, machine!.Id);
            Assert.AreEqual("Excavator", machine.Title);
        }

        [TestMethod]
        public void AddTest()
        {
            var newMachine = new Machine
            {
                UserId = 10,
                Title = "Bulldozer",
                Description = "Heavy bulldozer",
                Price = 2500,
                Location = "Aalborg"
            };

            var added = repo.Add(newMachine);

            Assert.IsNotNull(added);
            Assert.AreEqual(6, added.Id);
            Assert.AreEqual("Bulldozer", added.Title);
        }

        [TestMethod]
        public void RemoveTest()
        {
            var removed = repo.Remove(3);

            Assert.IsNotNull(removed);
            Assert.AreEqual(3, removed!.Id);

            var again = repo.GetById(3);
            Assert.IsNull(again);
        }

        [TestMethod]
        public void UpdateTest()
        {
            var updateValues = new Machine
            {
                Title = "Updated Tractor",
                Description = "Updated description",
                Price = 999,
                Location = "Updated City"
            };

            var updated = repo.Update(4, updateValues);

            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Tractor", updated!.Title);
            Assert.AreEqual("Updated description", updated.Description);
            Assert.AreEqual(999, updated.Price);
            Assert.AreEqual("Updated City", updated.Location);
        }

        // ---------------------------
        // In-memory repo ONLY for tests
        // ---------------------------
        private class InMemoryMachineRepository
        {
            private readonly List<Machine> _machines = new();
            private int _nextId = 1;

            public List<Machine> GetAll()
                => _machines.Select(Clone).ToList();

            public Machine? GetById(int id)
                => _machines.Where(m => m.Id == id).Select(Clone).FirstOrDefault();

            public Machine Add(Machine machine)
            {
                if (machine == null) throw new System.ArgumentNullException(nameof(machine));

                // minimal "sane" validation (kan udvides)
                if (machine.UserId <= 0) throw new System.ArgumentException("UserId must be > 0.");
                if (string.IsNullOrWhiteSpace(machine.Title)) throw new System.ArgumentException("Title is required.");
                if (string.IsNullOrWhiteSpace(machine.Description)) throw new System.ArgumentException("Description is required.");
                if (machine.Price <= 0) throw new System.ArgumentException("Price must be > 0.");
                if (string.IsNullOrWhiteSpace(machine.Location)) throw new System.ArgumentException("Location is required.");

                machine.Id = _nextId++;
                _machines.Add(Clone(machine));
                return Clone(machine);
            }

            public Machine? Remove(int id)
            {
                var existing = _machines.FirstOrDefault(m => m.Id == id);
                if (existing == null) return null;

                _machines.Remove(existing);
                return Clone(existing);
            }

            public Machine? Update(int id, Machine update)
            {
                if (update == null) throw new System.ArgumentNullException(nameof(update));

                var idx = _machines.FindIndex(m => m.Id == id);
                if (idx < 0) return null;

                var existing = _machines[idx];

                // partial update: null/0 => behold eksisterende
                var merged = new Machine
                {
                    Id = id,
                    UserId = existing.UserId, // behold ejer
                    Title = update.Title ?? existing.Title,
                    Description = update.Description ?? existing.Description,
                    Price = update.Price == 0 ? existing.Price : update.Price,
                    Location = update.Location ?? existing.Location
                };

                _machines[idx] = Clone(merged);
                return Clone(merged);
            }

            private static Machine Clone(Machine m) => new Machine
            {
                Id = m.Id,
                UserId = m.UserId,
                Title = m.Title,
                Description = m.Description,
                Price = m.Price,
                Location = m.Location
            };
        }
    }
}
