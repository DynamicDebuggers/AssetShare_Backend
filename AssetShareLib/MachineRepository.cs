using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AssetShareLib
{
    public class MachineRepository
    {
        private readonly IMongoCollection<Machine> _machines;

        public MachineRepository(MongoDbContext context)
        {
            _machines = context.Machines;
        }

        public async Task<List<Machine>> GetAll()
        {
            return await _machines
                .Find(FilterDefinition<Machine>.Empty)
                .ToListAsync();
        }

        public async Task<Machine?> GetById(int id)
        {
            return await _machines
                .Find(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Machine> Create(Machine machine)
        {
            var lastMachine = await _machines
                .Find(FilterDefinition<Machine>.Empty)
                .SortByDescending(m => m.Id)
                .Limit(1)
                .FirstOrDefaultAsync();

            machine.Id = (lastMachine?.Id ?? 0) + 1;

            await _machines.InsertOneAsync(machine);
            return machine;
        }

        public async Task<Machine?> Update(int id, Machine updatedMachine)
        {
            var existing = await _machines
                .Find(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (existing == null)
                return null;

            existing.Title = updatedMachine.Title ?? existing.Title;
            existing.Description = updatedMachine.Description ?? existing.Description;
            if (updatedMachine.Price != 0)
                existing.Price = updatedMachine.Price;
            if (updatedMachine.UserId != 0)
                existing.UserId = updatedMachine.UserId;
            if (!string.IsNullOrWhiteSpace(updatedMachine.Location))
                existing.Location = updatedMachine.Location;

            await _machines.ReplaceOneAsync(m => m.Id == id, existing);
            return existing;
        }

        public async Task Delete(int id)
        {
            await _machines.DeleteOneAsync(m => m.Id == id);
        }
    }
}