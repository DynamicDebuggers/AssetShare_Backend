using System;
using System.Collections.Generic;
using System.Linq;
using AssetShareLib;

public class MachineRepository
{
    private List<Machine> _machines = new List<Machine>();
    private int _nextId = 1;

    public MachineRepository()
    {
        _machines.Add(new Machine { UserId = 1, Title = "Excavator", Description = "Large construction excavator", Price = 1500, Location = "Hvidovre" });
        _machines.Add(new Machine { UserId = 1, Title = "Mini Loader", Description = "Compact loader for small tasks", Price = 900, Location = "Hedehusene" });
        _machines.Add(new Machine { UserId = 2, Title = "Chainsaw", Description = "Professional chainsaw", Price = 200, Location = "Roskilde" });
        _machines.Add(new Machine { UserId = 3, Title = "Tractor", Description = "Farm tractor, good condition", Price = 1200, Location = "Vejle" });
        _machines.Add(new Machine { UserId = 2, Title = "Cement Mixer", Description = "Heavy-duty cement mixer", Price = 500, Location = "Horsens" });
    }

    public IReadOnlyList<Machine> GetAll()
    {
        return _machines.AsReadOnly();
    }

    public Machine? GetById(int id)
    {
        Machine? machine = _machines.FirstOrDefault(m => m.Id == id);
        if (machine == null)
        {
            return null;
        }
        return machine;
    }

    public Machine Add(Machine machine)
    {
        if (machine == null)
            throw new ArgumentNullException(nameof(machine));

        machine.ValidateAll();
        machine.Id = _nextId++;
        _machines.Add(machine);
        return machine;
    }


public Machine? Remove(int id)
    {
        Machine? machineToRemove = _machines.FirstOrDefault(m => m.Id == id);
        if (machineToRemove != null)
        {
            _machines.Remove(machineToRemove);
        }
        return machineToRemove;
    }

    public Machine? Update(int id, Machine values)
    {
        Machine? machineToUpdate = _machines.FirstOrDefault(m => m.Id == id);
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
