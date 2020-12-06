using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day06
{
    class Program
    {
        static void Main(string[] args)
        {
            int totalAnyoneYesCount = GetTotalAnyoneYesCount();
            Console.WriteLine($"Total anyone yes count: {totalAnyoneYesCount}");
            int totalEveryoneYesCount = GetTotalEveryoneYesCount();
            Console.WriteLine($"Total everyone yes count: {totalEveryoneYesCount}");
        }

        static int GetTotalAnyoneYesCount()
        {
            return GetCustomDeclarationAnyoneYesCounts().Sum();
        }

        static IEnumerable<int> GetCustomDeclarationAnyoneYesCounts()
        {
            foreach (var item in GetCustomDeclarationYesAnswers())
            {
                yield return item.SelectMany(x => x).Distinct().Count();
            }
        }

        static int GetTotalEveryoneYesCount()
        {
            return GetCustomDeclarationEveryoneYesCounts().Sum();
        }

        static IEnumerable<int> GetCustomDeclarationEveryoneYesCounts()
        {
            foreach (var item in GetCustomDeclarationYesAnswers())
            {
                IEnumerable<char> partyYesAnswers = item[0];

                foreach (var personYesAnswers in item)
                {
                    partyYesAnswers = partyYesAnswers.Intersect(personYesAnswers);
                }

                yield return partyYesAnswers.Count();
            }
        }

        static IEnumerable<List<HashSet<char>>> GetCustomDeclarationYesAnswers()
        {
            List<HashSet<char>> declaration = new();
            using var reader = new StreamReader("input.txt");

            while (true)
            {
                string line = reader.ReadLine();

                if (!String.IsNullOrWhiteSpace(line))
                {
                    var questions = new HashSet<char>(line.Trim());
                    declaration.Add(questions);
                }
                else
                {
                    var returnDeclaration = declaration;
                    declaration = new();
                    yield return returnDeclaration;
                }

                if (line == null) break;
            }
        }
    }
}
