using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day08
{
    class Program
    {
        static void Main(string[] args)
        {
            bool success;
            int acc;

            List<Instruction> instructions = ParseInstructions("input.txt");
            (success, acc) = ExecuteInstructions(instructions);
            Console.WriteLine($"Exited normally: {success} - Accumulator value: {acc}");

            (success, acc) = FixAndExecuteInstructions(instructions);
            Console.WriteLine($"Exited normally: {success} - Accumulator value: {acc}");
        }

        private static (bool success, int acc) FixAndExecuteInstructions(List<Instruction> instructions)
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                var origInstruction = instructions[i];
                if (origInstruction.OpCode != "nop" && origInstruction.OpCode != "jmp") continue;

                var newInstruction = new Instruction(origInstruction.OpCode == "nop" ? "jmp" : "nop", origInstruction.Argument);
                instructions[i] = newInstruction;

                (bool success, int acc) = ExecuteInstructions(instructions);
                
                if (success)
                {
                    return (true, acc);
                }

                instructions[i] = origInstruction;
            }

            return (false, -1);
        }

        private static (bool success, int acc) ExecuteInstructions(List<Instruction> instructions)
        {
            HashSet<int> visited = new();
            int acc = 0;

            for (int i = 0; i < instructions.Count; i++)
            {
                if (visited.Contains(i))
                {
                    return (false, acc);
                }

                visited.Add(i);
                var instruction = instructions[i];

                switch (instruction.OpCode)
                {
                    case "nop":
                        continue;
                    case "jmp":
                        i += instruction.Argument - 1;
                        continue;
                    case "acc":
                        acc += instruction.Argument;
                        continue;
                }
            }
            
            return (true, acc);
        }

        private static List<Instruction> ParseInstructions(string v)
        {
            List<Instruction> instructions = new();

            foreach (string line in File.ReadLines("input.txt").Select(x => x.Trim()))
            {
                string[] parts = line.Split(' ', 2);
                instructions.Add(new Instruction(parts[0], Int32.Parse(parts[1])));
            }

            return instructions;
        }

        internal record Instruction
        {
            public string OpCode { get; }
            public int Argument { get; }

            public Instruction(string opCode, int argument)
                => (OpCode, Argument) = (opCode, argument);
        }
    }
}
