using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Day19
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<int, string> ruleLines;
            List<string> messages;
            Dictionary<int, Regex> rules;
            int count;

            Stopwatch watch = new();
            watch.Start();
            (ruleLines, messages) = ParseFile("input.txt");
            rules = ParseRules(ruleLines);
            count = GetMessageCountMatchingRule(rules[0], messages);
            watch.Stop();
            Console.WriteLine($"Messages matching rule 0: {count} - Elapsed: {watch.Elapsed}");

            // watch.Reset();
            // watch.Start();
            // Dictionary<int, string> overrideRules = new Dictionary<int, string>
            // {
            //     { 8, "42 | 42 8" },
            //     { 11, "42 31 | 42 11 31" },
            // };
            // rules = ParseRules(ruleLines, overrideRules);
            // count = GetMessageCountMatchingRule(rules[0], messages);
            // watch.Stop();
            // Console.WriteLine($"Messages matching rule 0 with overrides: {count} - Elapsed: {watch.Elapsed}");
        }

        static int GetMessageCountMatchingRule(Regex regex, List<string> messages)
        {
            int count = 0;
            messages.AsParallel().ForAll(x =>
            {
                if (regex.IsMatch(x))
                {
                    Interlocked.Increment(ref count);
                }
            });

            return count;
        }

        static (Dictionary<int, string> ruleLines, List<string> messages) ParseFile(string filename)
        {
            using var reader = new StreamReader(filename);

            Dictionary<int, string> ruleLines = new();
            string line;
            while ((line = reader.ReadLine()).TrimEnd().Length > 0)
            {
                ruleLines.Add(
                    Int32.Parse(line.Substring(0, line.IndexOf(":"))),
                    line.Substring(line.IndexOf(":") + 2).TrimEnd());
            }

            List<string> messages = new();
            while ((line = reader.ReadLine()) != null)
            {
                messages.Add(line.TrimEnd());
            }

            return (ruleLines, messages);
        }

        static Dictionary<int, Regex> ParseRules(
            Dictionary<int, string> ruleLines,
            Dictionary<int, string> overrideRules = null)
        {
            if (overrideRules != null)
            {
                foreach (KeyValuePair<int, string> item in overrideRules)
                {
                    ruleLines[item.Key] = item.Value;
                }
            }

            Dictionary<int, Regex> rules = new();

            foreach (KeyValuePair<int, string> item in ruleLines)
            {
                string regexString = ConvertToRegex(item.Value, ruleLines);
                rules[item.Key] = new Regex($"^{regexString}$");
            }

            return rules;
        }

        static string ConvertToRegex(
            string ruleText,
            Dictionary<int, string> ruleLines)
        {
            string regexString;

            // String literal
            if (ruleText.StartsWith("\""))
            {
                regexString = ruleText.Substring(1, ruleText.IndexOf("\"", 1) - 1);
            }
            // Logical OR combinations of references to other rules by number
            else if (ruleText.Contains("|"))
            {
                string[] parts = ruleText.Split('|');
                regexString = String.Concat(
                    "(",
                    String.Join('|', parts.Select(x => ConvertToRegex(x, ruleLines))),
                    ")"
                );
            }
            // One or more references to other rules by number
            else
            {
                IEnumerable<int> parts = ruleText
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Int32.Parse(x));
                regexString = String.Join(String.Empty, parts.Select(x => ConvertToRegex(ruleLines[x], ruleLines)));
            }

            return regexString;
        }
    }
}
