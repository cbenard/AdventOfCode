using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day07
{
    class Program
    {
        static readonly Regex _ruleRegex = new Regex(@"(?<Count>\d+) (?<Name>.+?) (?:bag|bags)", RegexOptions.ExplicitCapture);

        static void Main(string[] args)
        {
            Dictionary<string, List<(int, string)>> bagRules = GetBagRules("input.txt");
            string bagType = "shiny gold";

            int bagsThatCanContainBagCount = GetBagsThatCanContainBagCount(bagRules, bagType);
            Console.WriteLine($"{bagType} can be contained in {bagsThatCanContainBagCount} rules.");
            int mustContainBagCount = GetMustContainBagCount(bagRules, bagType);
            Console.WriteLine($"{bagType} must contain {mustContainBagCount} bags.");
        }

        static int GetBagsThatCanContainBagCount(
            Dictionary<string, List<(int, string)>> bagRules, string bagType)
        {
            int count = 0;

            foreach (KeyValuePair<string, List<(int, string)>> item in bagRules)
            {
                Dictionary<string, int> flattened = new();
                FlattenBagRules(bagRules, flattened, item.Value);
                if (flattened.ContainsKey(bagType))
                {
                    count++;
                }
            }

            return count;
        }

        static int GetMustContainBagCount(
            Dictionary<string, List<(int, string)>> bagRules,
            string bagType,
            int multiplier = 1)
        {
            List<(int, string)> rules = bagRules[bagType];
            if (rules?.Any() != true) return 0;

            int count = 0;

            foreach ((int subCount, string subBagType) in rules)
            {
                count += subCount * multiplier;
                count += GetMustContainBagCount(bagRules, subBagType, subCount * multiplier);
            }

            return count;
        }

        static void FlattenBagRules(
            Dictionary<string, List<(int, string)>> bagRules,
            Dictionary<string, int> flattened,
            List<(int, string)> rules,
            HashSet<string> processedBagTypes = null,
            int multiplier = 1)
        {
            if (rules?.Any() != true) return;
            processedBagTypes ??= new();

            foreach (var item in rules)
            {
                int bagCount = item.Item1;
                string containedBagType = item.Item2;

                if (processedBagTypes.Contains(containedBagType))
                {
                    continue;
                }

                processedBagTypes.Add(containedBagType);

                if (flattened.ContainsKey(containedBagType))
                {
                    flattened[containedBagType] += bagCount * multiplier;
                }
                else
                {
                    flattened[containedBagType] = bagCount * multiplier;
                }

                var subRules = bagRules[containedBagType];

                FlattenBagRules(bagRules, flattened, subRules, processedBagTypes);
            }
        }

        static Dictionary<string, List<(int, string)>> GetBagRules(string inputFile)
        {
            Dictionary<string, List<(int, string)>> rules = new();
            using var reader = new StreamReader(inputFile);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] ruleParts = line.Trim().Split(" bags contain ", 2);

                if (ruleParts[1] == "no other bags.")
                {
                    rules.Add(ruleParts[0], null);
                    continue;
                }

                MatchCollection matches = _ruleRegex.Matches(ruleParts[1]);
                if (matches.Count == 0) throw new Exception("Couldn't match rule: " + line);

                List<ValueTuple<int, string>> containedBags = new();
                foreach (Match match in matches)
                {
                    containedBags.Add((Int32.Parse(match.Groups["Count"].Value), match.Groups["Name"].Value));
                }

                rules.Add(ruleParts[0], containedBags);
            }

            return rules;
        }
    }
}
