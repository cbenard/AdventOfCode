using System;
using System.IO;
using System.Linq;

namespace Day13
{
    class Program
    {
        static void Main(string[] args)
        {
            Schedule schedule = ParseSchedule("input.txt");
            Departure departure = FindDeparture(schedule);
            Console.WriteLine($"Departure: Bus {departure.BusID} at {departure.Timestamp}");
            Console.WriteLine($"Minutes Waited ({departure.Timestamp - schedule.Timestamp}) * Bus ID ({departure.BusID}) = {(departure.Timestamp - schedule.Timestamp) * departure.BusID}");
        }
        static Departure FindDeparture(Schedule schedule)
        {
            int busID = schedule.BusIDs
                .Where(x => x.HasValue)
                .Select(x => (x.Value, (schedule.Timestamp / (decimal)x.Value) - Math.Floor(schedule.Timestamp / (decimal)x.Value)))
                .OrderByDescending(x => x.Item2 == 0)
                .ThenByDescending(x => x.Item2)
                .Select(x => x.Value)
                .First();
            
            int timestamp = (schedule.Timestamp / busID) * busID;
            if (timestamp < schedule.Timestamp)
            {
                timestamp += busID;
            }

            return new Departure
            {
                Timestamp = timestamp,
                BusID = busID,
            };
        }

        static Schedule ParseSchedule(string filename)
        {
            string[] lines = File.ReadLines(filename)
                .Take(2)
                .Select(x => x.TrimEnd())
                .ToArray();

            return new Schedule
            {
                Timestamp = Int32.Parse(lines[0]),
                BusIDs = lines[1]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Select(x => Int32.TryParse(x, out int parsed) ? (int?)parsed : null)
                    .ToArray(),
            };
        }
    }

    struct Schedule
    {
        public int Timestamp;
        public int?[] BusIDs;
    }

    struct Departure
    {
        public int BusID;
        public int Timestamp;
    }
}
