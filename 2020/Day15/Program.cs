using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Day15
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] startingNumbers = GetStartingNumbers();
            int lastSpokenNumber;
            Stopwatch watch = new Stopwatch();

            watch.Start();
            lastSpokenNumber = PlayMemoryGame(startingNumbers, playThrough: 2020);
            watch.Stop();
            Console.WriteLine($"2020th number spoken: {lastSpokenNumber} - Elapsed: {watch.Elapsed}");
            
            watch.Reset();
            watch.Start();
            lastSpokenNumber = PlayMemoryGame(startingNumbers, playThrough: 30000000);
            watch.Stop();
            Console.WriteLine($"30000000th number spoken: {lastSpokenNumber} - Elapsed: {watch.Elapsed}");
        }

        private static int PlayMemoryGame(int[] startingNumbers, int playThrough)
        {
            Dictionary<int, List<int>> history = new();

            for (int turn = 1; turn <= startingNumbers.Length; turn++)
            {
                history[startingNumbers[turn - 1]] = new() { turn };
            }

            int lastSpoken = startingNumbers[^1];

            for (int turn = startingNumbers.Length + 1; turn <= playThrough; turn++)
            {
                int nowSpoken;

                List<int> historyForLastSpoken = history[lastSpoken];
                if (historyForLastSpoken.Count == 1)
                {
                    nowSpoken = 0;
                }
                else
                {
                    nowSpoken = historyForLastSpoken[^1] - historyForLastSpoken[^2];
                }

                if (history.TryGetValue(nowSpoken, out List<int> historyForNowSpoken))
                {
                    historyForNowSpoken.Add(turn);
                }
                else
                {
                    history[nowSpoken] = new() { turn };
                }

                lastSpoken = nowSpoken;
            }

            return lastSpoken;
        }

        static int[] GetStartingNumbers()
        {
            int[] numbers = File.ReadAllLines("input.txt")[0]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Int32.Parse(x))
                .ToArray();

            return numbers;
        }
    }
}
