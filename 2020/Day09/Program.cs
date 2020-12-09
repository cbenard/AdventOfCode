using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day09
{
    class Program
    {
        static void Main(string[] args)
        {
            List<long> numbers = File.ReadLines("input.txt")
                .Select(x => Int64.Parse(x.Trim()))
                .ToList();

            // Example uses 5 as the lookback. Actual uses 25.
            int preambleLength = numbers.Count == 20 ? 5 : 25;
            long invalidNextNumber = FindInvalidNextNumber(numbers, preambleLength);

            Console.WriteLine($"Invalid next number: {invalidNextNumber}");

            List<long> contiguousSet = FindContiguousSet(numbers, invalidNextNumber);

            Console.WriteLine($"Contiguous set for {invalidNextNumber}: {String.Join(", ", contiguousSet)}");
            long smallest = contiguousSet.Min();
            long largest = contiguousSet.Max();
            long sum = smallest + largest;
            Console.WriteLine($"Sum of smallest ({smallest}) and largest ({largest}): {sum}");
        }

        private static List<long> FindContiguousSet(List<long> numbers, long invalidNextNumber)
        {
            for (int i = 0; i < numbers.Count - 1; i++)
            {
                List<long> set = new();

                for (int j = i; j < numbers.Count && set.Sum() < invalidNextNumber; j++)
                {
                    set.Add(numbers[j]);
                }

                if (set.Count >= 2 && set.Sum() == invalidNextNumber)
                {
                    return set;
                }
            }

            throw new Exception($"Unable to find contiguous set adding up to {invalidNextNumber}.");
        }

        private static long FindInvalidNextNumber(List<long> numbers, int preambleLength)
        {
            for (int i = preambleLength; i < numbers.Count; i++)
            {
                long currentNumber = numbers[i];
                var priorNumbers = numbers.Skip(i - preambleLength).Take(preambleLength);
                if (!priorNumbers.Any(x => priorNumbers.Any(y => (x != y) && (x + y == currentNumber))))
                {
                    return currentNumber;
                }
            }

            throw new Exception("Unable to find invalid next number.");
        }
    }
}
