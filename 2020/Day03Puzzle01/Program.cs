using System;
using System.IO;
using System.Linq;

namespace Day03Puzzle01
{
    class Program
    {
        static void Main(string[] args)
        {
            int treesEncountered = GetTreesEncountered(3, 1);

            Console.WriteLine($"Trees encountered: {treesEncountered}");

            var slopes = new[]
            {
                new[] { 1, 1 },
                new[] { 3, 1 },
                new[] { 5, 1 },
                new[] { 7, 1 },
                new[] { 1, 2 },
            };

            var aggregate = slopes
                .Select(x => (uint)GetTreesEncountered(x[0], x[1]))
                .Aggregate((left, right) => left * right);

            Console.WriteLine($"Aggregate trees encountered: {aggregate}");
        }

        static int GetTreesEncountered(int right, int down)
        {
            using var reader = new StreamReader("input.txt");

            int treesEncountered = 0;
            int horizontalIndex = 0;
            string line = reader.ReadLine();
            do
            {
                for (int i = 0; i < down; i++)
                {
                    line = reader.ReadLine();
                }

                if (line == null)
                {
                    break;
                }

                horizontalIndex += right;

                char currentChar = GetCharFromLine(line, horizontalIndex);
                if (IsTree(currentChar)) {
                    treesEncountered++;
                }
            }
            while (true);

            return treesEncountered;
        }

        static char GetCharFromLine(string line, int pos) => line[pos % line.Length];

        static bool IsTree(char input) => input == '#';
    }
}
