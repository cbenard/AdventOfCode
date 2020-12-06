using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Day05Puzzle01
{
    class Program
    {
        struct Seat
        {
            public int Row { get; private set; }
            public int Column { get; private set; }

            public int ID => Row * 8 + Column;

            public Seat(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public Seat Before
            {
                get
                {
                    int row = Row;
                    int col = Column;

                    if (col == 0)
                    {
                        col = 7;
                        row--;
                    }
                    else
                    {
                        col--;
                    }

                    return new Seat(row, col);
                }
            }

            public Seat After
            {
                get
                {
                    int row = Row;
                    int col = Column;

                    if (col == 7)
                    {
                        col = 0;
                        row++;
                    }
                    else
                    {
                        col++;
                    }

                    return new Seat(row, col);
                }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Seat)) return false;

                var otherSeat = (Seat)obj;
                return otherSeat.Row == Row && otherSeat.Column == Column;
            }

            public override string ToString()
            {
                return $"{{ Seat - Row: {Row}, Columns: {Column} }}";
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Row, Column, ID, Before, After);
            }
        }

        private static readonly Regex _seatCodeRegex = new Regex(@"^[FB]{7}[LR]{3}$");

        static void Main(string[] args)
        {
            List<Seat> seats = ParseSeats().ToList();

            int maxSeatID = seats.Max(x => x.ID);
            Console.WriteLine($"Max seat ID: {maxSeatID}");

            Seat mySeat = FindSeat(seats);
            Console.WriteLine($"My seat: {mySeat} - ID: {mySeat.ID}");
        }

        static Seat FindSeat(List<Seat> existingSeats)
        {
            int minRow = existingSeats.Min(x => x.Row);
            int maxRow = existingSeats.Max(x => x.Row);
            var allRows = Enumerable.Range(minRow, maxRow - minRow + 1);

            Seat? mySeat = null;

            Console.WriteLine("    0123 4567");

            for (int row = minRow; row < maxRow; row++)
            {
                Console.Write(row.ToString().PadLeft(3) + " ");

                for (int column = 0; column <= 7; column++)
                {
                    if (column == 4)
                    {
                        Console.Write(" ");
                    }

                    Seat seat = new Seat(row, column);

                    //if (!existingSeats.Any(x => x.Row == row && x.Column == column))
                    if (!existingSeats.Contains(seat))
                    {
                        Seat before = seat.Before;
                        Seat after = seat.After;
                        if (existingSeats.Contains(before) && existingSeats.Contains(after))
                        {
                            mySeat = seat;
                            Console.Write("~");
                        }
                        else
                        {
                            Console.Write(".");
                        }
                    }
                    else
                    {
                        Console.Write("#");
                    }
                }

                Console.WriteLine();
            }

            if (!mySeat.HasValue)
            {
                throw new Exception("Unable to find my seat.");
            }

            return mySeat.Value;
        }

        static IEnumerable<Seat> ParseSeats()
        {
            var reader = new StreamReader("input.txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Seat seat = ParseSeat(line.Trim());
                Console.WriteLine($"{line}: row {seat.Row}, column {seat.Column}, seat ID {seat.ID}.");
                yield return seat;
            }
        }

        static Seat ParseSeat(string seatCode)
        {
            ValidateCode(seatCode);
            int row = ParseSeatRow(seatCode.Substring(0, 7));
            int column = ParseSeatColumn(seatCode.Substring(7, 3));
            return new Seat(row, column);
        }

        static int ParseSeatRow(string rowCode) => ParsePartition(rowCode, 127, 'F', 'B');
        static int ParseSeatColumn(string columnCode) => ParsePartition(columnCode, 7, 'L', 'R');

        static int ParsePartition(string partitionCode, int maxNumber, char lowChar, char highChar)
        {
            int min = 0;
            int max = maxNumber;

            for (int i = 0; i < partitionCode.Length; i++)
            {
                char c = partitionCode[i];

                if (c == lowChar)
                {
                    max = (max - min) / 2 + min;
                }
                else if (c == highChar)
                {
                    min = (max - min) / 2 + 1 + min;
                }
                else
                {
                    throw new ArgumentException($"Code '{partitionCode}' character '{c}' is not the low ('{lowChar}') or high ('{highChar}') character.");
                }
            }

            if (min != max)
            {
                throw new Exception($"Code '{partitionCode}' resulted in min {min} and max {max} which are different. Unable to find the correct number.");
            }

            return min;
        }

        static void ValidateCode(string seatCode)
        {
            if (!_seatCodeRegex.IsMatch(seatCode))
            {
                throw new ArgumentException($"Invalid Seat Code: {seatCode}", nameof(seatCode));
            }
        }
    }
}
