using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AssetShareLib;

namespace AssetShareLib.Tests
{
    [TestClass]
    public class MachineRepositoryTests
    {
        private MachineRepository repo;

        [TestInitialize]
        public void Setup()
        {
            repo = new MachineRepository();
        }

        [TestMethod]
        public void MachineRepositoryTest()
        {
            Assert.IsNotNull(repo);
        }

        [TestMethod]
        public void GetTest()
        {
            var machines = repo.Get();

            Assert.IsNotNull(machines);
            Assert.AreEqual(5, machines.Count);
        }

        [TestMethod]
        public void GetByIdTest()
        {
            var machine = repo.GetById(1);

            Assert.IsNotNull(machine);
            Assert.AreEqual(1, machine.Id);
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
            Assert.AreEqual(3, removed.Id);

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
            Assert.AreEqual("Updated Tractor", updated.Title);
            Assert.AreEqual("Updated description", updated.Description);
            Assert.AreEqual(999, updated.Price);
            Assert.AreEqual("Updated City", updated.Location);
        }
    }
}
