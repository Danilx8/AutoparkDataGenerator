using Autopark.Data;
using Autopark.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoparkDataGenerator
{
    public class CarsGenerator(ApplicationDbContext db)
    {
        protected ApplicationDbContext _db = db;

        public List<Vehicle> Generate(int enterpriseId, int carsNumber)
        {

            Enterprise? enterprise = _db
                .Enterprises
                .Where(e => e.Id == enterpriseId)
                .FirstOrDefault()
                ?? throw new ArgumentException("Can't find enterprise by given id");

            List<int> brands = [.. _db
                    .Brands
                    .Select(b => b.Id)];

            List<int> drivers = [.. _db
                    .Drivers
                    .Where(d => d.EnterpriseId == enterpriseId)
                    .Select(d => d.Id)];

            HashSet<int> busyDrivers = [.. _db
                .Vehicles
                .Where(v => v.DriverId != null)
                .Select(v => v.DriverId ?? -1)];

            List<int> availableDrivers = drivers.Where(d => !busyDrivers.Contains(d)).ToList();

            var vehicle = new Faker<Vehicle>("ru")
                .RuleFor(v => v.Name, f => f.Vehicle.Manufacturer() + ' ' + f.Vehicle.Model())
                .RuleFor(v => v.Price, f => f.Random.Int(300_000, 500_000_000))
                .RuleFor(v => v.ZeroToHundred, f => f.Random.Float(3.0f, 50.0f))
                .RuleFor(v => v.Mileage, f => f.Random.Int(0, 1_000_000))
                .RuleFor(v => v.Year, f => f.Random.Int(1970, 2023))
                .RuleFor(v => v.HorsePower, f => f.Random.Int(80, 600))
                .RuleFor(v => v.BrandId, f => f.PickRandom(brands))
                .RuleFor(v => v.EnterpriseId, enterpriseId)
                .RuleFor(v => v.DriverId, f => f.PickRandom(availableDrivers).OrNull(f, .9f));

            List<Vehicle> vehicles = [];
            for (int i = 0; i < carsNumber; ++i)
            {
                var current = vehicle.Generate();
                if (busyDrivers.Contains(current.DriverId ?? default)) current.DriverId = default;
                else if (current.DriverId != null) busyDrivers.Add((int)current.DriverId);
                vehicles.Add(current);
            }

            _db.SaveChanges();

            return vehicles;
        }
    }
}
