using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Day12
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadLines("input.txt")
                .Select(x => x.Trim())
                .ToArray();

            Stopwatch watch = new();
            watch.Start();
            int relativeMovementDistance =
                CalculateRelativeMovementDistance(lines, CardinalDirection.East);
            watch.Stop();
            Console.WriteLine($"Relative movement distance: {relativeMovementDistance} - Elapsed: {watch.Elapsed}");

            watch.Reset();
            watch.Start();
            int waypointFollowingDistance =
                CalculateWaypointFollowingDistance(lines, 10, -1);
            watch.Stop();
            Console.WriteLine($"Waypoint following distance: {waypointFollowingDistance} - Elapsed: {watch.Elapsed}");
        }

        static int CalculateWaypointFollowingDistance(
            string[] lines, int waypointX, int waypointY)
        {
            int x = 0;
            int y = 0;

            foreach (string line in lines)
            {
                // Console.WriteLine($"Waypoint: {waypointX}, {waypointY} - Position: {x}, {y}");
                // Console.WriteLine(line);

                char prefix = line[0];
                if (prefix is 'F')
                {
                    int units = Int32.Parse(line.Substring(1));
                    // Console.WriteLine($"Previous position: {x}, {y}. Moving: {waypointX * units}, {waypointY * units}");
                    x += waypointX * units;
                    y += waypointY * units;
                    // Console.WriteLine($"Moved to: {x}, {y}");
                }
                else if (prefix is 'L' or 'R')
                {
                    // Console.WriteLine($"Previous waypoint: {waypointX}, {waypointY}");
                    RelativeDirection relativeDirection = prefix.ToRelativeDirection();
                    int degrees = Int32.Parse(line.Substring(1));
                    (waypointX, waypointY) = RotatePoints(waypointX, waypointY, relativeDirection, degrees);
                    // Console.WriteLine($"Rotating waypoint {relativeDirection} {degrees} degrees. New waypoint: {waypointX}, {waypointY}");
                }
                else if (prefix is 'N' or 'S' or 'E' or 'W')
                {
                    var direction = prefix.ToCardinalDirection();
                    (int xIncrement, int yIncrement) = direction.ToCoordinateIncrements();
                    int units = Int32.Parse(line.Substring(1));

                    // Console.WriteLine($"Moving waypoint {direction} {units} units");

                    waypointX += units * xIncrement;
                    waypointY += units * yIncrement;

                    // Console.WriteLine($"New waypoint: {waypointX}, {waypointY}");
                }
            }

            // Console.WriteLine($"X: {x} - Y: {y}");
            int manhattanDistance = Math.Abs(x) + Math.Abs(y);
            return manhattanDistance;
        }

        static (int waypointX, int waypointY) RotatePoints(
            int x, int y, RelativeDirection relativeDirection, int degrees)
        {
            if (relativeDirection == RelativeDirection.Left)
            {
                degrees *= -1;
            }

            return degrees switch
            {
                0 => (x, y),
                -90 or 270 => (y, -x),
                180 or -180 => (-x, -y),
                90 or -270 => (-y, x),
                _ => throw new Exception($"Unhandled degrees: {degrees}"),
            };
        }

        static int CalculateRelativeMovementDistance(
            string[] lines, CardinalDirection initialDirection)
        {
            CardinalDirection direction = initialDirection;
            int x = 0;
            int y = 0;

            foreach (string line in lines)
            {
                // Console.WriteLine($"Facing direction: {direction}");
                // Console.WriteLine(line);

                char prefix = line[0];
                if (prefix is 'L' or 'R')
                {
                    RelativeDirection relative = prefix.ToRelativeDirection();
                    int degrees = Int32.Parse(line.Substring(1));
                    direction = relative.ToCardinalDirection(direction, degrees);
                    // Console.WriteLine($"Turning {relative} {degrees} degrees. New direction: {direction}");
                }
                else if (prefix is 'N' or 'S' or 'E' or 'W' or 'F')
                {
                    var absoluteDirection = direction;
                    if (prefix != 'F')
                    {
                        absoluteDirection = prefix.ToCardinalDirection();
                    }

                    (int xIncrement, int yIncrement) = absoluteDirection.ToCoordinateIncrements();
                    int units = Int32.Parse(line.Substring(1));

                    // Console.WriteLine($"Moving {absoluteDirection} {units} units");

                    x += units * xIncrement;
                    y += units * yIncrement;
                }
            }

            // Console.WriteLine($"X: {x} - Y: {y}");
            int manhattanDistance = Math.Abs(x) + Math.Abs(y);
            return manhattanDistance;
        }
    }

    enum CardinalDirection
    {
        North,
        East,
        South,
        West,
    }

    enum RelativeDirection
    {
        Left,
        Right,
        Forward,
    }

    static class DirectionExtensions
    {
        public static (int x, int y) ToCoordinateIncrements(this CardinalDirection direction)
        {
            return direction switch
            {
                CardinalDirection.North => (0, -1),
                CardinalDirection.South => (0, 1),
                CardinalDirection.East => (1, 0),
                CardinalDirection.West => (-1, 0),
                _ => throw new Exception($"Unhandled direction: {direction}"),
            };
        }
        public static int ToDegrees(this CardinalDirection direction)
        {
            return direction switch
            {
                CardinalDirection.North => 0,
                CardinalDirection.South => 180,
                CardinalDirection.East => 90,
                CardinalDirection.West => 270,
                _ => throw new Exception($"Unhandled direction: {direction}"),
            };
        }
        public static CardinalDirection ToCardinalDirectionFromDegrees(this int degrees)
        {
            return degrees switch
            {
                0 => CardinalDirection.North,
                180 => CardinalDirection.South,
                90 => CardinalDirection.East,
                270 => CardinalDirection.West,
                _ => throw new Exception($"Unhandled degrees: {degrees}"),
            };
        }

        public static CardinalDirection ToCardinalDirection(
            this RelativeDirection direction,
            CardinalDirection previousDirection,
            int degrees)
        {
            int existingDegrees = previousDirection.ToDegrees();
            int newDegrees;

            if (direction == RelativeDirection.Right)
            {
                newDegrees = (existingDegrees + degrees) % 360;
                return newDegrees.ToCardinalDirectionFromDegrees();
            }
            else if (direction == RelativeDirection.Left)
            {
                bool isNegative = degrees > existingDegrees;
                newDegrees = (existingDegrees - degrees) % 360;
                if (isNegative)
                {
                    newDegrees = 360 + newDegrees;
                }
            }
            else
            {
                throw new Exception($"Unhandled direction: {direction}");
            }

            return newDegrees.ToCardinalDirectionFromDegrees();
        }

        public static RelativeDirection ToRelativeDirection(this char prefix)
        {
            return prefix switch
            {
                'L' => RelativeDirection.Left,
                'R' => RelativeDirection.Right,
                'F' => RelativeDirection.Forward,
                _ => throw new Exception($"Unhandled prefix: {prefix}"),
            };
        }

        public static CardinalDirection ToCardinalDirection(this char prefix)
        {
            return prefix switch
            {
                'N' => CardinalDirection.North,
                'S' => CardinalDirection.South,
                'E' => CardinalDirection.East,
                'W' => CardinalDirection.West,
                _ => throw new Exception($"Unhandled prefix: {prefix}"),
            };
        }
    }
}
