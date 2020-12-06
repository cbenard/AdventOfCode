using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Day04Puzzle01
{
    class Program
    {
        private static readonly string[] _requiredFields = new[] { "byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid" };
        private static readonly string[] _validEyeColors = new[] { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" };
        private static readonly Regex _hairColorRegex = new Regex(@"^#[0-9a-f]{6}$");
        private static readonly Regex _passportIDRegex = new Regex(@"^\d{9}$");
        private static readonly Regex _heightRegex = new Regex(@"^(?<Value>\d+)(?<Unit>(cm|in))$");

        static void Main(string[] args)
        {
            int validPassportCount = GetValidPassportCount();
            Console.WriteLine($"Valid passports: {validPassportCount}");
        }

        static int GetValidPassportCount()
        {
            StringBuilder sb = new();
            using var reader = new StreamReader("input.txt");
            int validCount = 0;

            while (true)
            {
                string line = reader.ReadLine();

                if (!String.IsNullOrWhiteSpace(line))
                {
                    sb.Append($"{line.Trim()} ");
                }
                else
                {
                    bool isValid = IsPassportValid(sb.ToString());
                    validCount += Convert.ToInt32(isValid);
                    sb.Clear();
                }

                if (line == null) break;
            }

            return validCount;
        }

        static bool IsPassportValid(string passport)
        {
            if (String.IsNullOrWhiteSpace(passport)) return false;

            Dictionary<string, string> dict = GetPassportValues(passport);

            bool isValid;
            isValid = dict.Keys.Intersect(_requiredFields).Count() == _requiredFields.Length;
            isValid &= ValidatePassportFields(dict);

            return isValid;
        }

        static Dictionary<string, string> GetPassportValues(string passport)
        {
            Dictionary<string, string> dict = new();
            string[] pairs = passport.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                string[] elements = pair.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                if (elements.Length != 2) continue;
                dict.Add(elements[0], elements[1]);
            }

            return dict;
        }

        static bool ValidatePassportFields(Dictionary<string, string> dict)
        {
            return dict.All(x => ValidatePassportField(x));
        }

        static bool ValidatePassportField(KeyValuePair<string, string> field)
        {
            return field.Key switch
            {
                "byr" => Int32.TryParse(field.Value, out int birthYear) && birthYear >= 1920 && birthYear <= 2002,
                "iyr" => Int32.TryParse(field.Value, out int issYear) && issYear >= 2010 && issYear <= 2020,
                "eyr" => Int32.TryParse(field.Value, out int expYear) && expYear >= 2020 && expYear <= 2030,
                "hgt" => ValidateHeight(field.Value),
                "hcl" => _hairColorRegex.IsMatch(field.Value),
                "ecl" => _validEyeColors.Contains(field.Value),
                "pid" => _passportIDRegex.IsMatch(field.Value),
                _ => true
            };
        }

        static bool ValidateHeight(string height)
        {
            var match = _heightRegex.Match(height);
            if (!match.Success) return false;

            int value = Int32.Parse(match.Groups["Value"].Value);
            string unit = match.Groups["Unit"].Value;

            return unit switch
            {
                "cm" => 150 <= value && value <= 193,
                "in" => 59 <= value && value <= 76,
                _ => false
            };
        }
    }
}
