using System;
using System.Collections.Generic;
using System.Linq;
using AssetShareLib;

public class MachineRepository
{
    private List<Machine> machines;

    public MachineRepository()
    {
        machines = new List<Machine>
        {
            new Machine { Id = 1, UserId = 1, Title = "Excavator", Description = "Large construction excavator", Price = 1500, Location = "Hvidovre" },
            new Machine { Id = 2, UserId = 1, Title = "Mini Loader", Description = "Compact loader for small tasks", Price = 900, Location = "Hedehusene" },
            new Machine { Id = 3, UserId = 2, Title = "Chainsaw", Description = "Professional chainsaw", Price = 200, Location = "Roskilde" },
            new Machine { Id = 4, UserId = 3, Title = "Tractor", Description = "Farm tractor, good condition", Price = 1200, Location = "Vejle" },
            new Machine { Id = 5, UserId = 2, Title = "Cement Mixer", Description = "Heavy-duty cement mixer", Price = 500, Location = "Horsens" }
        };
    }

    public List<Machine> Get()
    {
        return machines.Select(m => new Machine(m)).ToList();
    }

    public Machine? GetById(int id)
    {
        Machine? found = machines.FirstOrDefault(m => m.Id == id);
        return found == null ? null : new Machine(found);
    }

    public Machine Add(Machine machine)
    {
        int newId = machines.Any() ? machines.Max(m => m.Id) + 1 : 1;
        machine.Id = newId;
        machines.Add(machine);
        return machine;
    }

    public Machine? Remove(int id)
    {
        Machine? machineToRemove = machines.FirstOrDefault(m => m.Id == id);
        if (machineToRemove != null)
        {
            machines.Remove(machineToRemove);
        }
        return machineToRemove;
    }

    public Machine? Update(int id, Machine values)
    {
        Machine? machineToUpdate = machines.FirstOrDefault(m => m.Id == id);
        if (machineToUpdate != null)
        {
            machineToUpdate.Title = values.Title;
            machineToUpdate.Description = values.Description;
            machineToUpdate.Price = values.Price;
            machineToUpdate.Location = values.Location;
        }
        return machineToUpdate;
    }
}
