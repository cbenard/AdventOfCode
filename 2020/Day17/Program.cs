using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Day17
{
    class Program
    {
        static void Main(string[] args)
        {
            DimensionSet dimension = ParseDimension3D("input.txt");
            Stopwatch watch = new Stopwatch();

            watch.Start();
            ProcessCycles(dimension, 6, is4D: false);
            int activeCount6Cycles3D = dimension.Where(x => x.Value).Count();
            watch.Stop();
            Console.WriteLine($"Active count after 6 cycles in 3D: {activeCount6Cycles3D} - Elapsed: {watch.Elapsed}");

            watch.Reset();
            dimension = ParseDimension3D("input.txt");
            watch.Start();
            ProcessCycles(dimension, 6, is4D: true);
            int activeCount6Cycles4D = dimension.Where(x => x.Value).Count();
            watch.Stop();
            Console.WriteLine($"Active count after 6 cycles in 4D: {activeCount6Cycles4D} - Elapsed: {watch.Elapsed}");
        }

        static void ProcessCycles(DimensionSet dimension, int cycles, bool is4D)
        {
            for (int i = 1; i <= cycles; i++)
            {
                Dictionary<DimensionLocation, bool> newValues = new();
                (DimensionLocation min, DimensionLocation max) = dimension.GetBounds();

                for (int w = min.W - 1; w <= max.W + 1; w++)
                {
                    if (!is4D && w != 0) continue;

                    for (int z = min.Z - 1; z <= max.Z + 1; z++)
                    {
                        for (int y = min.Y - 1; y <= max.Y + 1; y++)
                        {
                            for (int x = min.X - 1; x <= max.X + 1; x++)
                            {
                                var location = new DimensionLocation(x, y, z, w);
                                bool active = dimension.IsActive(location);
                                var neighbors = location.GetNeighbors(is4D);
                                var activeNeighborCount = neighbors
                                    .Where(x => dimension.IsActive(x))
                                    .Count();

                                if (active)
                                {
                                    newValues[location] = activeNeighborCount is 2 or 3;
                                }
                                else
                                {
                                    newValues[location] = activeNeighborCount == 3;
                                }
                            }
                        }
                    }
                }

                foreach (KeyValuePair<DimensionLocation, bool> item in newValues)
                {
                    dimension[item.Key] = item.Value;
                }

                Debug.WriteLine($"Cycle: {i}");
                Debug.WriteLine("");
                Debug.WriteLine(dimension);
                Debug.WriteLine("");
            }
        }

        static DimensionSet ParseDimension3D(string filename)
        {
            DimensionSet dimension = new();

            int y = 0;
            int z = 0;

            foreach (string line in File.ReadLines(filename).Select(x => x.TrimEnd()))
            {
                for (int x = 0; x < line.Length; x++)
                {
                    dimension.Add(new DimensionLocation(x, y, z, 0), line[x] == '#');
                }

                y++;
            }

            return dimension;
        }
    }

    class DimensionSet : Dictionary<DimensionLocation, bool>
    {
        public bool IsActive(int x, int y, int z, int w)
            => IsActive(new DimensionLocation(x, y, z, w));

        public bool IsActive(DimensionLocation location)
        {
            bool exists = ContainsKey(location);
            bool active = exists && this[location];
            return active;
        }

        public void SetActive(int x, int y, int z, int w, bool active)
        {
            var key = new DimensionLocation(x, y, z, w);
            this[key] = active;
        }

        public (DimensionLocation min, DimensionLocation max) GetBounds()
        {
            int activeXMin = int.MaxValue;
            int activeXMax = int.MinValue;
            int activeYMin = int.MaxValue;
            int activeYMax = int.MinValue;
            int activeZMin = int.MaxValue;
            int activeZMax = int.MinValue;
            int activeWMin = int.MaxValue;
            int activeWMax = int.MinValue;

            foreach (DimensionLocation item in this.Keys)
            {
                if (!this[item]) continue;

                if (item.X < activeXMin) activeXMin = item.X;
                if (item.X > activeXMax) activeXMax = item.X;
                if (item.Y < activeYMin) activeYMin = item.Y;
                if (item.Y > activeYMax) activeYMax = item.Y;
                if (item.Z < activeZMin) activeZMin = item.Z;
                if (item.Z > activeZMax) activeZMax = item.Z;
                if (item.W < activeWMin) activeWMin = item.W;
                if (item.W > activeWMax) activeWMax = item.W;
            }

            return (
                new DimensionLocation(activeXMin, activeYMin, activeZMin, activeWMin),
                new DimensionLocation(activeXMax, activeYMax, activeZMax, activeWMax));
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            (DimensionLocation min, DimensionLocation max) = GetBounds();

            for (int w = min.W; w <= max.W; w++)
            {
                for (int z = min.Z; z <= max.Z; z++)
                {
                    sb.AppendLine($"z={z}, w={w}");
                    for (int y = min.Y; y <= max.Y; y++)
                    {
                        for (int x = min.X; x <= max.X; x++)
                        {
                            bool active = IsActive(x, y, z, w);
                            sb.Append(active ? "#" : ".");
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd();
        }
    }

    struct DimensionLocation
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public int W { get; private set; }

        public DimensionLocation(int x, int y, int z, int w)
            => (X, Y, Z, W) = (x, y, z, w);

        public IEnumerable<DimensionLocation> GetNeighbors(bool is4D)
        {
            for (int w = W - 1; w <= W + 1; w++)
            {
                if (!is4D && w != 0) continue;

                for (int z = Z - 1; z <= Z + 1; z++)
                {
                    for (int y = Y - 1; y <= Y + 1; y++)
                    {
                        for (int x = X - 1; x <= X + 1; x++)
                        {
                            var neighbor = new DimensionLocation(x, y, z, w);

                            if (!neighbor.Equals(this))
                            {
                                yield return neighbor;
                            }
                        }
                    }
                }
            }
        }

        public override int GetHashCode()
            => HashCode.Combine(X, Y, Z, W);

        public override bool Equals(object obj)
        {
            if (obj is not DimensionLocation) return base.Equals(obj);

            var other = (DimensionLocation)obj;

            return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
        }
    }
}
