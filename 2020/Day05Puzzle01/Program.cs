using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Day05Puzzle01
{
    class Program
    {
        struct Seat : IComparable<Seat>
        {
            public int Row { get; private set; }
            public int Column { get; private set; }

            public int ID => Row * 8 + Column;

            public Seat(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public Seat Next
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

            public static bool operator ==(Seat lhs, Seat rhs) => lhs.Equals(rhs);

            public static bool operator !=(Seat lhs, Seat rhs) => !(lhs == rhs);

            public static bool operator >(Seat lhs, Seat rhs) => lhs.CompareTo(rhs) > 0;

            public static bool operator <(Seat lhs, Seat rhs) => !(lhs > rhs);

            public static bool operator >=(Seat lhs, Seat rhs) => lhs.CompareTo(rhs) >= 0;

            public static bool operator <=(Seat lhs, Seat rhs) => lhs.CompareTo(rhs) <= 0;

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
                return HashCode.Combine(Row, Column);
            }

            public int CompareTo(Seat other) => ID - other.ID;
        }

        private static readonly Regex _seatCodeRegex = new Regex(@"^[FB]{7}[LR]{3}$");

        static void Main(string[] args)
        {
            var seats = new SortedSet<Seat>(ParseSeats());

            int maxSeatID = seats.Max.ID;
            Console.WriteLine($"Max seat ID: {maxSeatID}");

            Seat mySeat = FindSeat(seats);
            Console.WriteLine($"My seat: {mySeat} - ID: {mySeat.ID}");
        }

        static Seat FindSeat(SortedSet<Seat> existingSeats)
        {
            Seat firstSeat = existingSeats.Min;
            Seat lastSeat = existingSeats.Max;

            for (int row = firstSeat.Row; row <= lastSeat.Row; row++)
            {
                for (int column = 0; column <= 7; column++)
                {
                    Seat seat = new Seat(row, column);
                    if (seat <= firstSeat || seat >= lastSeat)
                    {
                        continue;
                    }

                    if (!existingSeats.Contains(seat))
                    {
                        return seat;
                    }
                }
            }

            throw new Exception("Unable to find my seat.");
        }

        static IEnumerable<Seat> ParseSeats()
        {
            var reader = new StreamReader("input.txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Seat seat = ParseSeat(line.Trim());
                // Console.WriteLine($"{line}: row {seat.Row}, column {seat.Column}, seat ID {seat.ID}.");
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
