using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Day14
{
    class Program
    {
        private static readonly Regex _cmdRegex =
            new Regex(@"(?<Command>(?:mask|mem))(?:\[(?<Location>\d+)\])? = (?<Value>[\dX]+)");

        static void Main(string[] args)
        {
            IEnumerable<Command> commands = ParseCommands("input.txt");

            Stopwatch watch = new();

            watch.Start();
            Dictionary<ulong, ulong> mem1 = ExecuteCommandsVersion1(commands);
            watch.Stop();
            Console.WriteLine($"Sum of memory version 1: {mem1.Sum(x => (long)x.Value)} - Elapsed: {watch.Elapsed}");

            watch.Reset();
            watch.Start();
            Dictionary<ulong, ulong> mem2 = ExecuteCommandsVersion2(commands);
            watch.Stop();
            Console.WriteLine($"Sum of memory version 2: {mem2.Sum(x => (long)x.Value)} - Elapsed: {watch.Elapsed}");
        }

        private static Dictionary<ulong, ulong> ExecuteCommandsVersion1(IEnumerable<Command> commands)
        {
            Mask mask = null;
            Dictionary<ulong, ulong> mem = new();

            foreach (Command cmd in commands)
            {
                if (cmd is MaskCommand)
                {
                    mask = ((MaskCommand)cmd).Mask;
                }
                else if (cmd is MemSetCommand)
                {
                    if (mask == null) throw new Exception($"Mask is null when processing: {cmd}");

                    var memCmd = (MemSetCommand)cmd;
                    mem[memCmd.Location] = mask.GetMaskedValue(memCmd.Value);
                }
                else throw new Exception($"Invalid command type: {cmd.GetType()}");
            }

            return mem;
        }

        private static Dictionary<ulong, ulong> ExecuteCommandsVersion2(IEnumerable<Command> commands)
        {
            Mask mask = null;
            Dictionary<ulong, ulong> mem = new();

            foreach (Command cmd in commands)
            {
                if (cmd is MaskCommand)
                {
                    mask = ((MaskCommand)cmd).Mask;
                }
                else if (cmd is MemSetCommand)
                {
                    if (mask == null) throw new Exception($"Mask is null when processing: {cmd}");

                    var memCmd = (MemSetCommand)cmd;
                    IEnumerable<ulong> locations = mask.GetMaskedMemoryLocations(memCmd.Location);
                    foreach (ulong location in locations)
                    {
                        mem[location] = memCmd.Value;
                    }
                }
                else throw new Exception($"Invalid command type: {cmd.GetType()}");
            }

            return mem;
        }

        private static IEnumerable<Command> ParseCommands(string filename)
        {
            foreach (string line in File.ReadLines(filename).Select(x => x.TrimEnd()))
            {
                Match m = _cmdRegex.Match(line);
                if (!m.Success) throw new Exception($"Invalid command line: {line}");

                if (m.Groups["Command"].Value == "mask")
                {
                    yield return new MaskCommand(m.Groups["Value"].Value);
                }
                else if (m.Groups["Command"].Value == "mem")
                {
                    yield return new MemSetCommand(m.Groups["Location"].Value, m.Groups["Value"].Value);
                }
                else throw new Exception($"Invalid regex command '{m.Groups["Command"].Value}' for line: {line}");
            }
        }
    }

    class MemSetCommand : Command
    {
        public ulong Location { get; private set; }
        public ulong Value { get; private set; }

        public MemSetCommand(string locationText, string valueText)
        {
            Location = UInt64.Parse(locationText);
            Value = UInt64.Parse(valueText);
        }

        public override string ToString()
        {
            return $"mem[{Location}] = {Value}";
        }
    }

    class MaskCommand : Command
    {
        public Mask Mask { get; private set; }

        public MaskCommand(string maskText)
        {
            Mask = new Mask(maskText);
        }

        public override string ToString()
        {
            return $"mask = {Mask}";
        }
    }

    abstract class Command { }

    class Mask
    {
        private string MaskText { get; set; }

        public Mask(string maskText) =>
            MaskText = maskText ?? throw new Exception("Mask text cannot be null");

        public override string ToString()
        {
            return MaskText;
        }

        public ulong GetMaskedValue(ulong value)
        {
            char[] longBits = CreateMaskForValue(value);

            for (int i = 0; i < MaskText.Length; i++)
            {
                if (MaskText[i] == 'X') continue;

                longBits[i] = MaskText[i];
            }

            ulong maskedValue = CreateValueForMask(longBits);
            return maskedValue;
        }

        private static ulong CreateValueForMask(char[] longBits)
        {
            return Convert.ToUInt64(new string(longBits), fromBase: 2);
        }

        public IEnumerable<ulong> GetMaskedMemoryLocations(ulong location)
        {
            char[] longBits = CreateMaskForValue(location);

            for (int i = 0; i < longBits.Length; i++)
            {
                longBits[i] = MaskText[i] switch
                {
                    '1' => '1',
                    'X' => 'X',
                    _ => longBits[i]
                };
            }

            List<string> masks = CreateAllMasks(new string(longBits));

            foreach (string mask in masks)
            {
                ulong maskedValue = CreateValueForMask(mask.ToCharArray());
                yield return maskedValue;
            }
        }

        private List<string> CreateAllMasks(string maskText)
        {
            List<string> masks = new List<string>();
            masks.Add(maskText);

            for (int i = 0; i < maskText.Length; i++)
            {
                for (int j = 0; j < masks.Count; j++)
                {
                    if (masks[j][i] == 'X')
                    {
                        string mask1 = masks[j].Remove(i, 1).Insert(i, "0");
                        string mask2 = masks[j].Remove(i, 1).Insert(i, "1");
                        masks[j] = mask1;
                        masks.Insert(j + 1, mask2);
                        j++;
                    }
                }
            }

            return masks;
        }

        private char[] CreateMaskForValue(ulong value)
        {
            return Convert.ToString((long)value, toBase: 2)
                .PadLeft(MaskText.Length, '0')
                .ToCharArray();
        }
    }
}
