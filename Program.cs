using Autopark.Data;
using Autopark.Models;
using AutoparkDataGenerator;
using CommandLine;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel;

var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=helloappdb;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;
using var _db = new ApplicationDbContext(contextOptions);

Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       CarsGenerator generator = new(_db);
                       List<Vehicle> vehicles = generator.Generate(o.EnterpriseId, o.CarsNumber);
                       _db.Vehicles.AddRange(vehicles);
                       _db.SaveChanges();

                       vehicles.ForEach(vehicle =>
                       {
                           foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(vehicle))
                           {
                               string name = descriptor.Name;
                               object? value = descriptor.GetValue(vehicle);
                               if (value?.GetType() == typeof(List<Driver>))
                               {
                                   Console.WriteLine($"{name}: ");
                                   (value as List<Driver>)!.ForEach(v => Console.Write($"{v} "));
                               }
                               else if (value != null) Console.WriteLine("{0}={1}", name, value);
                           }
                       });
                   });

public class Options
{
    [Option('e', "enterpriseId", Required = true, HelpText = "Set vehicles' enterprise")]
    public int EnterpriseId { get; set; }

    [Option('c', "cars", Required = false, Default = 1, HelpText = "Set amount of generated vehicles. ")]
    public int CarsNumber { get; set; }
}