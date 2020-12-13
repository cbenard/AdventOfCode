using System;
using System.Collections.Generic;
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
            long sequentialScheduleTimestamp = FindSequentialScheduleTimestamp(schedule);
            Console.WriteLine($"Sequential Schedule Timestamp: {sequentialScheduleTimestamp}");
        }

        // Adapted from https://github.com/constb/aoc2020/blob/main/13/index2.js
        // because I didn't really understand the Chinese Remainder Theorem
        static long FindSequentialScheduleTimestamp(Schedule schedule)
        {
            List<ModulusResult> modulusResults = new();

            for (int i = 0; i < schedule.BusIDs.Length; i++)
            {
                if (!schedule.BusIDs[i].HasValue) continue;

                int busID = schedule.BusIDs[i].Value;
                modulusResults.Add(new ModulusResult
                {
                    Modulo = busID,
                    Remainder = (busID - (i % busID)) % busID,
                });
            }

            var largestFirst = modulusResults.OrderByDescending(x => x.Modulo).ToArray();

            long val = 0;
            long step = 1;

            for (long i = 0; i < largestFirst.Length; i++)
            {
                while (val % largestFirst[i].Modulo != largestFirst[i].Remainder) val += step;
                step *= largestFirst[i].Modulo;
            }
            
            return val;
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

            long timestamp = (schedule.Timestamp / busID) * busID;
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

    struct ModulusResult
    {
        public int Modulo;
        public int Remainder;
    }

    struct Schedule
    {
        public int Timestamp;
        public int?[] BusIDs;
    }

    struct Departure
    {
        public int BusID;
        public long Timestamp;
    }
}
