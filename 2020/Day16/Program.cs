using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day16
{
    class Program
    {
        static readonly Regex _ruleRegex = new Regex(@"^(?<Field>.+?): (?<Start1>\d+)\-(?<End1>\d+) or (?<Start2>\d+)\-(?<End2>\d+)");

        static void Main(string[] args)
        {
            string filename = "input.txt";
            Dictionary<string, int[]> rules = ParseRules(filename);
            IEnumerable<int[]> nearbyTickets = ParseTickets(filename, "nearby tickets:");
            int[] myTicket = ParseTickets(filename, "your ticket:").First();

            Stopwatch watch = new();
            watch.Start();
            (List<int[]> validTickets, long errorRate) = CountErrorRate(rules, nearbyTickets);
            watch.Stop();
            Console.WriteLine($"Error rate: {errorRate} - Elapsed: {watch.Elapsed}");

            watch.Reset();
            watch.Start();
            Dictionary<string, int> myDecodedTicket = DecodeMyTicket(myTicket, rules, validTickets);
            long departureMultiple = CalculateDepartureMultiple(myDecodedTicket);
            watch.Stop();
            Console.WriteLine($"Departure multiple: {departureMultiple} - Elapsed: {watch.Elapsed}");
        }

        static long CalculateDepartureMultiple(Dictionary<string, int> myDecodedTicket)
        {
            return myDecodedTicket
                .Where(x => x.Key.StartsWith("departure"))
                .Select(x => (long)x.Value)
                .Aggregate((lhs, rhs) => lhs * rhs);
        }

        static Dictionary<string, int> DecodeMyTicket(
            int[] myTicket, Dictionary<string, int[]> rules, List<int[]> validTickets)
        {
            Dictionary<string, int> ruleMap = CreateRuleMap(rules, validTickets);
            Dictionary<string, int> decodedTicket = new();

            foreach (KeyValuePair<string, int> mapEntry in ruleMap)
            {
                decodedTicket[mapEntry.Key] = myTicket[mapEntry.Value];
            }

            return decodedTicket;
        }

        static Dictionary<string, int> CreateRuleMap(Dictionary<string, int[]> rules, List<int[]> validTickets)
        {
            Dictionary<string, int> ruleMap = new();

            int[][] columns = new int[validTickets[0].Length][];
            for (int i = 0; i < validTickets[0].Length; i++)
            {
                columns[i] = validTickets.Select(x => x[i]).ToArray();
            }

            while (ruleMap.Count < rules.Count)
            {
                for (int column = 0; column < columns.Length; column++)
                {
                    int[] values = columns[column];
                    string possibleRuleKey = null;

                    foreach (KeyValuePair<string, int[]> rule in rules)
                    {
                        if (ruleMap.ContainsKey(rule.Key)) continue;

                        if (values.All(x => IsValidNumberForRule(x, rule.Value)))
                        {
                            if (possibleRuleKey != null)
                            {
                                possibleRuleKey = null;
                                break;
                            }
                            possibleRuleKey = rule.Key;
                        }
                    }

                    if (possibleRuleKey != null)
                    {
                        ruleMap.Add(possibleRuleKey, column);
                    }
                }
            }
            
            return ruleMap;
        }

        static bool IsValidNumberForRule(int number, int[] ranges)
        {
            return (number >= ranges[0] && number <= ranges[1])
                || (number >= ranges[2] && number <= ranges[3]);
        }

        static (List<int[]> validTickets, long errorRate) CountErrorRate(
            Dictionary<string, int[]> rules,
            IEnumerable<int[]> nearbyTickets)
        {
            HashSet<int> validNumbers = GetValidNumbers(rules);
            List<int[]> validTickets = new();
            long errorRate = 0;

            foreach (int[] ticket in nearbyTickets)
            {
                var invalidNumbers = ticket.Where(x => !validNumbers.Contains(x));
                errorRate += invalidNumbers.Sum();

                if (!invalidNumbers.Any())
                {
                    validTickets.Add(ticket);
                }
            }

            return (validTickets, errorRate);
        }

        static HashSet<int> GetValidNumbers(Dictionary<string, int[]> rules)
        {
            HashSet<int> validNumbers = new();
            foreach (int[] rule in rules.Values)
            {
                for (int i = rule[0]; i <= rule[1]; i++)
                {
                    validNumbers.Add(i);
                }

                for (int i = rule[2]; i <= rule[3]; i++)
                {
                    validNumbers.Add(i);
                }
            }

            return validNumbers;
        }

        static Dictionary<string, int[]> ParseRules(string filename)
        {
            Dictionary<string, int[]> rules = new();

            foreach (string line in File.ReadLines(filename).Select(x => x.TrimEnd()))
            {
                if (line.Length == 0) break;

                Match m = _ruleRegex.Match(line);
                if (!m.Success) throw new Exception($"Can't parse line: {line}");

                rules[m.Groups["Field"].Value] = new[]
                {
                    Int32.Parse(m.Groups["Start1"].Value),
                    Int32.Parse(m.Groups["End1"].Value),
                    Int32.Parse(m.Groups["Start2"].Value),
                    Int32.Parse(m.Groups["End2"].Value),
                };
            }

            return rules;
        }

        static IEnumerable<int[]> ParseTickets(string filename, string prefix)
        {
            Dictionary<string, int[]> rules = new();

            bool ready = false;
            foreach (string line in File.ReadLines(filename).Select(x => x.TrimEnd()))
            {
                if (!ready)
                {
                    if (line == prefix) ready = true;
                    continue;
                }

                if (line.Length == 0) break;

                yield return LineToIntArray(line);
            }
        }

        static int[] LineToIntArray(string line)
        {
            return line.Split(',').Select(x => Int32.Parse(x)).ToArray();
        }
    }
}