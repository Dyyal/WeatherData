using System;
using System.IO;
using System.Linq;
using WeatherData.Models;

namespace WeatherData
{
    class Program
    {
        static void Main(string[] args)
        {
            StartUpMenu();
        }

        private static void StartUpMenu()
        {
            Console.Clear();
            Console.WriteLine("Choose an alternative: \n\n" +
                "[1] Insert data to database\n" +
                "[2] Print average temperature\n" +
                "[3] Sort by warmest to coldest day\n" +
                "[4] Sort by driest to most humid day\n" +
                "[5] Sort by mold risk\n" +
                "[6] Meteorological autumn\n" +
                "[7] Meteorological winter\n" +
                "[8] Exit");
            ConsoleKeyInfo consoleKey = Console.ReadKey(true);
            switch (consoleKey.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    AddDataToDB();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    AverageTemp(new DateTime());
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    HighestToLowestTemp();
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    DriestToHumidDay();
                    break;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    MoldRisk();
                    break;
                case ConsoleKey.D6:
                case ConsoleKey.NumPad6:
                    MeteorologicalAutumn(new DateTime());
                    break;
                case ConsoleKey.D7:
                case ConsoleKey.NumPad7:
                    MeteorologicalWinter(new DateTime());
                    break;
                case ConsoleKey.D8:
                case ConsoleKey.NumPad8:
                    Environment.Exit(1);
                    break;
                default:
                    Console.Clear();
                    Console.WriteLine("Invalid key. Press any key to try again..");
                    Console.ReadKey();
                    StartUpMenu();
                    break;
            }
        }

        private static void AddDataToDB()
        {
            const string csvFile = @"Data\TemperaturData.csv";          // Ändra till copy if newer på File Properties, då behövs inte hela filepath skrivas in

            var lines = File.ReadLines(csvFile).Select(x => x.Split(','));

            var items = from t in lines
                        select new Weather
                        {
                            Date = DateTime.Parse(t[0]),
                            Location = t[1],                                                 // Connectar varje prop till sitt motvärde i csv filen
                            Temperature = double.Parse(t[2].Replace(".", ",")),
                            Humidity = double.Parse(t[3])
                        };

            using (var db = new EFContext())
            {
                if (db.Temperatures.Count() > 0) return;                                           // Om csv filen redan finns i databasen kommer den inte att läggas till igen

                items.Take(1).ToList().ForEach(t => db.Temperatures.AddRange(items));             // Tar all data från items och lägger in det i databasen 

                db.SaveChanges();
            }
        }

        private static void AverageTemp(DateTime date)
        {
            using (var db = new EFContext())
            {
                Console.Clear();

                Console.WriteLine("Enter a date between 2016-05-31 - 2017-01-10 (YYYY-MM-DD):");
                DateTime userDateInput;
                if (DateTime.TryParse(Console.ReadLine(), out userDateInput))       // Inmatning från användaren, ger date samma värde som userDateInput
                    date = userDateInput;

                Console.Clear();
                Console.WriteLine("Choose an alternative: \n\n" +
                    "Press [1] for average temperature indoors\n" +
                    "Press [2] for average temperature outdoors");

                ConsoleKeyInfo consoleKey = Console.ReadKey(true);

                try
                {
                    switch (consoleKey.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            var avgTempIn = db.Temperatures
                                         .Where(x => x.Date.Date == date.Date && x.Location == "Inne")    // Här kopplas date (som har userDateInput värdet) med prop Date i databasen
                                         .Average(x => x.Temperature);
                            Console.Clear();
                            Console.WriteLine($"Average temperature indoors for {date:yyyy-MM-dd} is {Math.Round(avgTempIn)} degrees");
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            var avgTempOut = db.Temperatures
                                            .Where(x => x.Date.Date == date.Date && x.Location == "Ute")
                                            .Average(x => x.Temperature);
                            Console.Clear();
                            Console.WriteLine($"Average temperature outdoors for {date:yyyy-MM-dd} is {Math.Round(avgTempOut)} degrees");
                            break;
                        default:
                            Console.Clear();
                            Console.WriteLine("Invalid key. Press any key to try again..");
                            Console.ReadKey();
                            AverageTemp(date);
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.WriteLine("The date does not exist in database, enter a valid date... \n\n" +
                        "Press any key to return to main menu");
                    Console.ReadKey();
                    StartUpMenu();
                }
                Console.Write("\nPress any key to return to main menu");
                Console.ReadKey();
                StartUpMenu();
            }
        }

        private static void HighestToLowestTemp()
        {
            using (var db = new EFContext())
            {
                Console.Clear();
                Console.WriteLine("Choose an alternative: \n\n" +
                    "Press [1] to sort from highest to lowest temperature indoors\n" +
                    "Press [2] to sort from highest to lowest temperature outdoors");

                ConsoleKeyInfo consoleKey = Console.ReadKey(true);
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:

                        var highestToLowestTempIn = db.Temperatures.Where(l => l.Location == "Inne")
                        .GroupBy(d => d.Date.Date)
                        .Select(da => new { Date = da.Key, AverageTemp = da.Average(t => t.Temperature) }).ToList()    
                        .OrderByDescending(x => x.AverageTemp);

                        Console.Clear();

                        highestToLowestTempIn.ToList().ForEach(t => Console.WriteLine($"{t.Date:yyyy-MM-dd} = {Math.Round(t.AverageTemp)} degrees"));

                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:

                        var highestToLowestTempOut = db.Temperatures.Where(l => l.Location == "Ute")
                        .GroupBy(d => d.Date.Date)
                        .Select(da => new { Date = da.Key, AverageTemp = da.Average(t => t.Temperature) }).ToList()
                        .OrderByDescending(x => x.AverageTemp);

                        Console.Clear();

                        highestToLowestTempOut.ToList().ForEach(t => Console.WriteLine($"{t.Date:yyyy-MM-dd} = {Math.Round(t.AverageTemp)} degrees"));

                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid key. Press any key to try again..");
                        Console.ReadKey();
                        HighestToLowestTemp();
                        break;
                }
                Console.Write("\nPress any key to return to main menu");
                Console.ReadKey();
                StartUpMenu();
            }
        }

        private static void DriestToHumidDay()
        {
            using (var db = new EFContext())
            {
                Console.Clear();
                Console.WriteLine("Choose an alternative: \n\n" +
                    "Press [1] to sort by driest to most humid day indoors\n" +
                    "Press [2] to sort by driest to most humid day outdoors");
                ConsoleKeyInfo consoleKey = Console.ReadKey(true);
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:

                        var highestToLowestHumIn = db.Temperatures.Where(l => l.Location == "Inne")
                        .GroupBy(d => d.Date.Date)
                        .Select(da => new { Date = da.Key, AverageHum = da.Average(h => h.Humidity) }).ToList()
                        .OrderBy(x => x.AverageHum);

                        Console.Clear();

                        highestToLowestHumIn.ToList().ForEach(t => Console.WriteLine($"{t.Date:yyyy-MM-dd} -> {Math.Round(t.AverageHum)}"));

                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:

                        var highestToLowestHumOut = db.Temperatures.Where(l => l.Location == "Ute")
                        .GroupBy(d => d.Date.Date)
                        .Select(da => new { Date = da.Key, AverageHum = da.Average(h => h.Humidity) }).ToList()
                        .OrderBy(x => x.AverageHum);

                        Console.Clear();

                        highestToLowestHumOut.ToList().ForEach(t => Console.WriteLine($"{t.Date:yyyy-MM-dd} -> {Math.Round(t.AverageHum)}"));

                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid key. Press any key to try again..");
                        Console.ReadKey();
                        DriestToHumidDay();
                        break;
                }
                Console.Write("\nPress any key to return to main menu..");
                Console.ReadKey();
                StartUpMenu();
            }
        }

        private static void MoldRisk()
        {
            using (var db = new EFContext())
            {
                Console.Clear();
                Console.WriteLine("Choose an alternative: \n\n" +
                    "Press [1] to sort by moldrisk indoors\n" +
                    "Press [2] to sort by moldrisk outdoors");
                ConsoleKeyInfo consoleKey = Console.ReadKey(true);
                Console.Clear();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:

                        var moldRiskIn = db.Temperatures.Where(l => l.Location == "Inne")
                    .GroupBy(d => d.Date.Date)
                    .Select(m => new
                    {
                        Date = m.Key,
                        AverageTemp = m.Average(c => c.Temperature / 15),
                        AverageHum = m.Average(c => c.Humidity - 78)
                    }).ToList().OrderByDescending(x => x.AverageHum * x.AverageTemp / 0.22);

                        moldRiskIn.ToList().ForEach(x =>
                        {
                            double moldrisk = x.AverageHum * x.AverageTemp / 0.22;

                            if (moldrisk < 0)
                            {
                                moldrisk = 0;
                            }
                            else if (moldrisk > 100)
                            {
                                moldrisk = 100;
                            }

                            Console.WriteLine($"{x.Date:yyyy-MM-dd} = {Math.Round((decimal)moldrisk)} %");
                        });

                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:

                        var moldRiskOut = db.Temperatures.Where(l => l.Location == "Ute")
                    .GroupBy(d => d.Date.Date)
                    .Select(m => new
                    {
                        Date = m.Key,
                        AverageTemp = m.Average(c => c.Temperature / 15),
                        AverageHum = m.Average(c => c.Humidity - 78),
                    }).ToList().OrderByDescending(x => x.AverageHum * x.AverageTemp / 0.22);

                        moldRiskOut.ToList().ForEach(x =>
                        {
                            double moldrisk = x.AverageHum * x.AverageTemp / 0.22;

                            if (moldrisk < 0)
                            {
                                moldrisk = 0;
                            }
                            else if (moldrisk > 100)
                            {
                                moldrisk = 100;
                            }
                            Console.WriteLine($"{x.Date:yyyy-MM-dd} = {Math.Round((decimal)moldrisk)} %");
                        });

                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid key. Press any key to try again..");
                        Console.ReadKey();
                        MoldRisk();
                        break;
                }
                Console.Write("\nPress any key to return to main menu");
                Console.ReadKey();
                StartUpMenu();
            }
        }

        private static void MeteorologicalAutumn(DateTime date)
        {
            using (var db = new EFContext())
            {
                var autumn = db.Temperatures.Where(l => l.Location == "Ute" && l.Date > DateTime.Parse("2016-06-11"))
                        .GroupBy(d => d.Date.Date)
                        .Select(da => new { Date = da.Key, AverageTemp = da.Average(c => c.Temperature) }).ToList()
                        .OrderBy(x => x.Date);

                Console.Clear();

                // Om temperaturen understiger 10 grader 5 dagar i rad räknas den första datumen som meteorologisk höst

                int dayCount = 0;

                autumn.ToList().ForEach(x =>
                {
                    if (x.AverageTemp <= 10)
                    {
                        if (dayCount == 0)
                        {
                            date = x.Date;
                        }
                        else if (dayCount == 5)
                        {
                            Console.WriteLine($"Meteorological autumn starts at {date:yyyy-MM-dd}");
                        }
                        dayCount++;
                    }
                    else if (x.AverageTemp! <= 10)
                    {
                        dayCount = 0;
                    }
                });
                Console.Write("\nPress any key to return to main menu");
                Console.ReadKey();
                StartUpMenu();
            }
        }

        private static void MeteorologicalWinter(DateTime date) 
        {
            using (var db = new EFContext())
            {
                var winter = db.Temperatures.Where(l => l.Location == "Ute")
                        .GroupBy(d => d.Date.Date)
                        .Select(da => new { Date = da.Key, AverageTemp = da.Average(c => c.Temperature) }).ToList()
                        .OrderBy(x => x.Date);

                Console.Clear();

                // Om temperaturen understiger 0 grader 5 dagar i rad räknas den första datumen som meteorologisk vinter

                int dayCount = 0;
                
                winter.ToList().ForEach(x =>
                { 
                    if (x.AverageTemp <= 0)
                    {
                        if (dayCount == 5)
                        {
                            Console.WriteLine($"Meteorological winter starts at {date:yyyy-MM-dd}");
                        }
                        else if (dayCount == 0)
                        {
                            date = x.Date;
                        }
                        dayCount++;
                    }
                    else if (x.AverageTemp! <= 0)
                    {
                        dayCount = 0;
                    }
                });

                
                Console.Write("\nPress any key to return to main menu");
                Console.ReadKey();
                StartUpMenu();
            }
        }
    }

}