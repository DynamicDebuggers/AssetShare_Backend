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

        // Hent alle maskiner
        public async Task<List<Machine>> Get()
        {
            return await _machines
                .Find(FilterDefinition<Machine>.Empty)
                .ToListAsync();
        }

        // Hent én maskine pr. Id
        public async Task<Machine?> GetById(int id)
        {
            return await _machines
                .Find(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        // Opret en ny maskine
        public async Task<Machine> Add(Machine machine)
        {
            // Find højeste Id og læg 1 til (samme idé som din gamle Add)
            var lastMachine = await _machines
                .Find(FilterDefinition<Machine>.Empty)
                .SortByDescending(m => m.Id)
                .Limit(1)
                .FirstOrDefaultAsync();

            machine.Id = (lastMachine?.Id ?? 0) + 1;

            await _machines.InsertOneAsync(machine);
            return machine;
        }

        // Slet maskine
        public async Task<Machine?> Remove(int id)
        {
            return await _machines.FindOneAndDeleteAsync(m => m.Id == id);
        }

        // Opdater maskine
        public async Task<Machine?> Update(int id, Machine values)
        {
            values.Id = id; // sørg for at Id matcher

            var result = await _machines.ReplaceOneAsync(
                m => m.Id == id,
                values
            );

            if (result.MatchedCount == 0)
            {
                return null;
            }

            return values;
        }
    }
}
