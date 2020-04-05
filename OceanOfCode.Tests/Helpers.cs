using System.Linq;
using NUnit.Framework;

namespace OceanOfCode.Tests
{
    public class NavigateHelper
    {
        private readonly ConsoleMock _console;

        public NavigateHelper(ConsoleMock console)
        {
            _console = console;
        }
        public void ConsoleRecordMove(int x, int y)
        {
            _console.Record($"{x} {y} -1 -1 -1 -1 -1 -1");
            _console.Record("");
            _console.Record("");
        }
    }
    public static class MapAssert
    {
        public static void AllCoordinatesAreZeroExcept(int[,] map, params (int, int)[] coordinates)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    if (coordinates.Any(c => c.Equals((i, j))))
                    {
                        if (map[i, j] != 1)
                        {
                            Assert.Fail($"Coordinate ({i}, {j}) was {map[i,j]} but expected 1.");
                        }
                        continue;
                    }

                    if (map[i, j] != 0)
                    {
                        Assert.Fail($"Coordinate ({i}, {j}) was {map[i,j]} but expected 0.");
                    }
                }
            }
        }

        public static void AllCoordinatesAreZero(int[,] map)
        {
            AllCoordinatesAreZeroExcept(map);
        }
        
        public static void AllCoordinatesAreOneExcept(int[,] map, params (int, int)[] coordinates)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    if (coordinates.Any(c => c.Equals((i, j))))
                    {
                        if (map[i, j] != 0)
                        {
                            Assert.Fail($"Coordinate ({i}, {j}) was {map[i,j]} but expected 0.");
                        }
                        continue;
                    }

                    if (map[i, j] != 1)
                    {
                        Assert.Fail($"Coordinate ({i}, {j}) was {map[i,j]} but expected 1.");
                    }
                }
            }
        }

        public static void AllCoordinatesAreOne(int[,] map)
        {
            AllCoordinatesAreOneExcept(map);
        }
    }
}