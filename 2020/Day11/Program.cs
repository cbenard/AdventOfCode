using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Day11
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = File.ReadLines("input.txt");
            var layout = new SeatLayout(lines);

            Stopwatch watch = new();
            watch.Start();
            int stabilizedCount1 = GetStabilizedOccupiedCount(layout, ApplyRuleSet1);
            watch.Stop();
            Console.WriteLine($"Rule set 1 stabilized Count: {stabilizedCount1} - Elapsed: {watch.Elapsed}");

            watch.Reset();
            watch.Start();
            int stabilizedCount2 = GetStabilizedOccupiedCount(layout, ApplyRuleSet2);
            watch.Stop();
            Console.WriteLine($"Rule set 2 stabilized Count: {stabilizedCount2} - Elapsed: {watch.Elapsed}");
        }

        static int GetStabilizedOccupiedCount(
            SeatLayout layout,
            Action<SeatLayout, SeatLayout> ApplyRules)
        {
            SeatLayout currentLayout = (SeatLayout)layout.Clone();

            // Console.WriteLine("Initial:");
            // Console.WriteLine("-----------------------------");
            // Console.WriteLine(currentLayout);
            // Console.WriteLine();

            // int iterations = 1;

            do
            {
                // Console.WriteLine($"Iteration: {++iterations}");

                SeatLayout lastLayout = currentLayout;
                currentLayout = (SeatLayout)currentLayout.Clone();
                ApplyRules(lastLayout, currentLayout);

                // Console.WriteLine("-----------------------------");
                // Console.WriteLine(currentLayout);
                // Console.WriteLine();
            } while (currentLayout.IsDirty);

            return currentLayout.OccupiedSeatCount;
        }

        static void ApplyRuleSet1(SeatLayout previousLayout, SeatLayout currentLayout)
        {
            for (int row = 0; row < currentLayout.RowCount; row++)
            {
                for (int col = 0; col < currentLayout.ColumnCount; col++)
                {
                    if (previousLayout.IsFloor(row, col))
                    {
                        continue;
                    }
                    else
                    {
                        bool occupied = previousLayout.IsOccupied(row, col);
                        int adjacent = previousLayout.CountAdjacentOccupied(row, col);

                        // If a seat is empty (L) and there are no occupied
                        // seats adjacent to it, the seat becomes occupied.
                        if (!occupied && adjacent == 0)
                        {
                            currentLayout.MarkOccupied(row, col);
                        }
                        // If a seat is occupied (#) and four or more seats
                        // adjacent to it are also occupied, the seat becomes empty.
                        else if (occupied && adjacent >= 4)
                        {
                            currentLayout.MarkEmpty(row, col);
                        }
                    }
                }
            }
        }

        static void ApplyRuleSet2(SeatLayout previousLayout, SeatLayout currentLayout)
        {
            for (int row = 0; row < currentLayout.RowCount; row++)
            {
                for (int col = 0; col < currentLayout.ColumnCount; col++)
                {
                    if (previousLayout.IsFloor(row, col))
                    {
                        continue;
                    }
                    else
                    {
                        bool occupied = previousLayout.IsOccupied(row, col);
                        int withinView = previousLayout.CountWithinViewOccupied(row, col);

                        // If a seat is empty (L) and there are no visible
                        // occupied seats within view, the seat becomes occupied.
                        // The person can only see the first seat encountered in
                        // each direction.
                        if (!occupied && withinView == 0)
                        {
                            currentLayout.MarkOccupied(row, col);
                        }
                        // If a seat is occupied (#) and five or more seats
                        // within view are also occupied, the seat becomes empty.
                        // The person can only see the first seat encountered in
                        // each direction.
                        else if (occupied && withinView >= 5)
                        {
                            currentLayout.MarkEmpty(row, col);
                        }
                    }
                }
            }
        }
    }

    class SeatLayout : ICloneable
    {
        public int RowCount => _seats.Count;
        public int ColumnCount => _seats[0].Count;
        public int OccupiedSeatCount => GetOccupiedSeatCount();
        public bool IsDirty { get; private set; }
        readonly List<List<bool?>> _seats = new();

        public SeatLayout(IEnumerable<string> chartLines)
        {
            foreach (string line in chartLines?.Select(x => x.Trim()))
            {
                List<bool?> lineSeats = new();

                foreach (char c in line)
                {
                    lineSeats.Add(c switch
                    {
                        '#' => true,
                        'L' => false,
                        '.' => null,
                        _ => throw new Exception($"Invalid seat character: {c}")
                    });
                }

                _seats.Add(lineSeats);
            }
        }

        SeatLayout(SeatLayout originalLayout)
        {
            originalLayout._seats.ForEach(x => _seats.Add(x.ToList()));
        }

        public bool IsOccupied(int row, int col) => _seats[row][col] == true;

        public bool IsFloor(int row, int col) => _seats[row][col] == null;

        public int CountAdjacentOccupied(int row, int col)
        {
            var _adjacentSeats = new[] {
                (row - 1, col - 1),
                (row - 1, col),
                (row - 1, col + 1),
                (row, col - 1),
                (row, col + 1),
                (row + 1, col - 1),
                (row + 1, col),
                (row + 1, col + 1),
            };

            return _adjacentSeats
                .Where(x => x.Item1 >= 0 && x.Item1 < RowCount
                    && x.Item2 >= 0 && x.Item2 < ColumnCount)
                .Where(x => IsOccupied(x.Item1, x.Item2))
                .Count();
        }

        int IsDirectionOccupied(IEnumerable<(int, int)> seats)
        {
            foreach (var seat in seats)
            {
                (int row, int col) = seat;
                if (IsFloor(row, col)) continue;
                return IsOccupied(row, col) ? 1 : 0;
            }

            // All were floor spaces or we are at an edge
            return 0;
        }

        IEnumerable<(int, int)> GenerateDirection(
            int row,
            int col,
            int rowIncrement,
            int colIncrement)
        {
            int curRow = row + rowIncrement;
            int curCol = col + colIncrement;

            while (curRow >= 0 && curRow < RowCount
                && curCol >= 0 && curCol < ColumnCount)
            {
                yield return (curRow, curCol);

                curRow += rowIncrement;
                curCol += colIncrement;
            }
        }

        public int CountWithinViewOccupied(int row, int col)
        {
            var directionIncrements = new[]
            {
                (-1, 0),  // Up
                (-1, 1),  // Up + Right
                (0, 1),   // Right
                (1, 1),   // Down + Right
                (1, 0),   // Down
                (1, -1),  // Down + Left
                (0, -1),  // Left
                (-1, -1), // Left + Up
            };

            int occupied = directionIncrements
                .Select(x => GenerateDirection(row, col, x.Item1, x.Item2))
                .Select(x => IsDirectionOccupied(x))
                .Sum();

            return occupied;
        }

        int GetOccupiedSeatCount()
        {
            int count = 0;

            for (int row = 0; row < _seats.Count; row++)
            {
                for (int col = 0; col < _seats[row].Count; col++)
                {
                    if (IsOccupied(row, col))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public void MarkOccupied(int row, int col)
        {
            _seats[row][col] = true;
            IsDirty = true;
        }

        public void MarkEmpty(int row, int col)
        {
            _seats[row][col] = false;
            IsDirty = true;
        }

        public object Clone()
        {
            return new SeatLayout(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            for (int row = 0; row < _seats.Count; row++)
            {
                for (int col = 0; col < _seats[row].Count; col++)
                {
                    if (IsFloor(row, col))
                    {
                        sb.Append('.');
                    }
                    else
                    {
                        sb.Append(IsOccupied(row, col) ? '#' : 'L');
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }
    }
}
