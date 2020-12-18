using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Day18
{
    class Program
    {
        static void Main()
        {
            Stopwatch watch = new();
            long sum;

            watch.Start();
            sum = SolveProblems("input.txt", additionHigherThanMultiplication: false);
            watch.Stop();
            Console.WriteLine($"Sum of all problems (+ and * equal precedence): {sum} - Elapsed: {watch.Elapsed}");

            watch.Reset();
            watch.Start();
            sum = SolveProblems("input.txt", additionHigherThanMultiplication: true);
            watch.Stop();
            Console.WriteLine($"Sum of all problems (+ higher precedence than *): {sum} - Elapsed: {watch.Elapsed}");
        }

        static long SolveProblems(string filename, bool additionHigherThanMultiplication)
        {
            long sum = 0;

            foreach (string line in File.ReadLines(filename).Select(x => x.TrimEnd()))
            {
                long result = SolveProblem(line, additionHigherThanMultiplication);
                Debug.WriteLine($"{line} = {result}");
                sum += result;
            }

            return sum;
        }

        private static long SolveProblem(string line, bool additionHigherThanMultiplication)
        {
            SymbolList symbols = new SymbolList(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            // Preprocess parens to add them as symbols
            for (int i = 0; i < symbols.Count; i++)
            {
                string curSymbol = symbols[i];
                if (curSymbol.Length > 1 && curSymbol.StartsWith("("))
                {
                    symbols[i] = curSymbol.Substring(1);
                    symbols.Insert(i, "(");
                }
                else if (curSymbol.Length > 1 && curSymbol.EndsWith(")"))
                {
                    symbols[i] = curSymbol[0..^1];
                    symbols.Insert(i + 1, ")");
                    i--;
                }
            }

            while (symbols.Count > 1)
            {
                for (int i = 1; i < symbols.Count; i++)
                {
                    // Consolidate single value in paren
                    if (symbols[i - 1] is "(" && symbols[i + 1] is ")")
                    {
                        symbols[i - 1] = symbols[i];
                        symbols.RemoveRange(i, 2);
                        i = 0;
                        continue;
                    }

                    // Part 2: + happens before *
                    if (additionHigherThanMultiplication
                        && symbols[i] is "*" && symbols.Contains("+") && symbols[i + 1] is not "(")
                    {
                        // Are we inside parens, if so ignore *
                        Stack<int> openParens = new();
                        var preceedingSymbols = symbols.GetRange(0, i);
                        for (int j = 0; j < preceedingSymbols.Count; j++)
                        {
                            if (preceedingSymbols[j] is "(")
                            {
                                openParens.Push(j);
                            }
                            else if (preceedingSymbols[j] is ")")
                            {
                                openParens.Pop();
                            }
                        }

                        if (openParens.Count > 0)
                        {
                            int lastOpenParen = openParens.Pop();
                            int closeParen = symbols.IndexOf(")", lastOpenParen + 1);
                            if (symbols.GetRange(lastOpenParen + 1, closeParen - lastOpenParen - 1).Contains("+"))
                            {
                                // We are in a paren that has + that needs to be done first
                                continue;
                            }
                        }
                        else
                        {
                            // We aren't in a paren but + needs to be done first
                            continue;
                        }
                    }

                    if (symbols[i] is "+" or "*")
                    {
                        // Ignore paren until processed
                        if (symbols[i - 1] is "(" or ")" || symbols[i + 1] is "(" or ")") continue;

                        // Regular values calculate and consolidate (no order of operations per puzzle)
                        symbols[i - 1] = symbols[i] switch
                        {
                            "+" => (Int64.Parse(symbols[i - 1]) + Int64.Parse(symbols[i + 1])).ToString(),
                            "*" => (Int64.Parse(symbols[i - 1]) * Int64.Parse(symbols[i + 1])).ToString(),
                            _ => throw new Exception($"Invalid operator: {symbols[i]}")
                        };

                        symbols.RemoveRange(i, 2);
                        i = 0;
                    }
                }
            }

            return Int64.Parse(symbols[0]);
        }

        private class SymbolList : List<string>
        {
            public new string this[int index]
            {
                get
                {
                    if (index < 0 || index > Count - 1) return string.Empty;

                    return base[index];
                }
                set
                {
                    base[index] = value;
                }
            }

            public SymbolList(IEnumerable<string> items) : base(items) { }
        }
    }
}
