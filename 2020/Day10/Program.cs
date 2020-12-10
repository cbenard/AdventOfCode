using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day10
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> adapters = GetAdapters();
            (int jolt1Count, int jolt2Count, int jolt3Count) = CountAdaptersUsed(adapters);

            Console.WriteLine($"Jolt Counts - 1: {jolt1Count}, 2: {jolt2Count}, 3: {jolt3Count}");
            Console.WriteLine($"1-Jolt differences * 3-Jolt differences: {jolt1Count * jolt3Count}");

            ulong combinations = CountAdapterPossibleCombinations(adapters);
            Console.WriteLine($"Total adapter combinations: {combinations}");
        }

        static ulong CountAdapterPossibleCombinations(List<int> adapters)
        {
            Dictionary<int, ulong> visited = new();
            adapters.ForEach(x => visited[x] = 0);
            visited[adapters[0]] = 1;

            for (int i = 0; i < adapters.Count - 1; i++)
            {
                int adapterJoltRating = adapters[i];

                for (int j = 1; j <= 3 && adapters.Count > i + j && adapters[i + j] - adapterJoltRating <= 3; j++)
                {
                    int nextAdapter = adapters[i + j];
                    visited[nextAdapter] += visited[adapterJoltRating];
                }
            }

            return visited[adapters[^1]];
        }

        static (int jolt1Count, int jolt2Count, int jolt3Count) CountAdaptersUsed(List<int> adapters)
        {
            Dictionary<int, int> joltCounts = new();
            joltCounts[1] = 0;
            joltCounts[2] = 0;
            joltCounts[3] = 0;
            int currentJoltRating = 0;

            for (int i = 1; i < adapters.Count; i++)
            {
                int adapterJoltRating = adapters[i];
                joltCounts[adapterJoltRating - currentJoltRating]++;
                currentJoltRating = adapterJoltRating;
            }

            return (joltCounts[1], joltCounts[2], joltCounts[3]);
        }

        static List<int> GetAdapters()
        {
            List<int> adapters = File.ReadLines("input.txt")
                .Select(x => Int32.Parse(x.Trim()))
                .ToList();
            adapters.Sort();

            // Seat has a rating of 0
            adapters.Insert(0, 0);
            // Built-in adapter has a rating of the max adapter + 3
            adapters.Add(adapters[^1] + 3);

            return adapters;
        }
    }
}
