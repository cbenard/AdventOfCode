using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Day02Puzzle01
{
    class Program
    {
        private static readonly Regex _lineRegex =
            new Regex(@"^(?<Min>\d+)\-(?<Max>\d+) (?<Letter>\w): (?<Password>.+)$");
        static void Main(string[] args)
        {
            using var reader = new StreamReader("input.txt");
            int valid = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (IsValidPuzzle2(line))
                {
                    valid++;
                }
            }

            Console.WriteLine($"Valid passwords: {valid}");
        }

        private static bool IsValidPuzzle1(string line)
        {
            var match = _lineRegex.Match(line.Trim());
            if (!match.Success)
            {
                return false;
            }

            int min = Int32.Parse(match.Groups["Min"].Value);
            int max = Int32.Parse(match.Groups["Max"].Value);
            string letter = match.Groups["Letter"].Value;
            string password = match.Groups["Password"].Value;

            MatchCollection ruleMatches = Regex.Matches(password, letter);

            bool isValid = min <= ruleMatches.Count && ruleMatches.Count <= max;
            if (isValid)
            {
                Console.WriteLine($"Valid password ({ruleMatches.Count}): {line.Trim()}");
            }
            else
            {
                Console.WriteLine($"Invalid password ({ruleMatches.Count}): {line.Trim()}");
            }

            return isValid;
        }

        private static bool IsValidPuzzle2(string line)
        {
            var match = _lineRegex.Match(line.Trim());
            if (!match.Success)
            {
                return false;
            }

            int index1 = Int32.Parse(match.Groups["Min"].Value) - 1;
            int index2 = Int32.Parse(match.Groups["Max"].Value) - 1;
            char letter = match.Groups["Letter"].Value[0];
            string password = match.Groups["Password"].Value;

            char char1 = password[index1];
            char char2 = password[index2];

            bool isValid = (char1 == letter) ^ (char2 == letter);

            if (isValid)
            {
                Console.WriteLine($"Valid password: {line.Trim()}");
            }
            else
            {
                Console.WriteLine($"Invalid password: {line.Trim()}");
            }

            return isValid;
        }
    }
}
