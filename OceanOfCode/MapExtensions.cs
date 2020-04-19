using System;
using System.Collections.Generic;
using System.Linq;
using OceanOfCode.Surveillance;

namespace OceanOfCode
{
    public static class MapExtensions
    {
        public static int[,] CloneMap(this int[,] source)
        {
            int dimension1Length = source.GetLength(0);
            int dimension2Length = source.GetLength(1);
            int[,] clone = new int[dimension1Length, dimension2Length];
            Array.Copy(source, clone, dimension1Length * dimension2Length);
            return clone;
        }

        public static int AvailableCellCount(this int[,] source)
        {
            int availableCellCount = 0;
            for (int j = 0; j < source.GetLength(1); j++)
            {
                for (int i = 0; i < source.GetLength(0); i++)
                {
                    if (source[i, j] == 0)
                    {
                        availableCellCount++;
                    }
                }
            }

            return availableCellCount;
        }
        
        public static (int, int) FindPositionWhenIMove(this (int, int) currentPosition, char direction)
        {
            var (x, y) = currentPosition;
            switch (direction)
            {
                case Direction.East:
                    return (x + 1, y);
                case Direction.South:
                    return (x, y + 1);
                case Direction.West:
                    return (x - 1, y);
                case Direction.North:
                    return (x, y - 1);
            }

            throw new Exception("Incorrect direction given");
        }

        public static char ToOpposite(this char direction)
        {
            switch (direction)
            {
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                case Direction.North:
                    return Direction.South;
                default:
                    return default;
            }
        }
        
        public static List<(int,int)> FindNeighbouringCells(this (int, int) position, GameProps gameProps, int[,] map)
        {
            var neighbours = new Dictionary<(int,int),(int,int)>();
            var (x, y) = position;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    SafeAddPositionsInRange(neighbours, (x + i, y + j), gameProps);
                }
            }

            return neighbours.Select(p => p.Value).Where(positions => map[positions.Item1, positions.Item2] == 0).ToList();
        }

        public static Dictionary<(int, int), (BinaryTrack torpedoTargetMap, BinaryTrack torpedoRangeMap)> CalculateTorpedoRangeWithBinaryTracks(this (int, int) myPosition, GameProps gameProps,
            int[,] map)
        {
            Dictionary<(int, int), (BinaryTrack torpedoTargetMap, BinaryTrack torpedoRangeMap)> result = new Dictionary<(int, int), (BinaryTrack torpedoTargetMap, BinaryTrack torpedoRangeMap)>();
            var torpedoRange = myPosition.CalculateTorpedoRangeNotHittingMyself(gameProps, map);
            foreach (var potentialTorpedoTarget in torpedoRange)
            {
                
                BinaryTrack torpedoTargetMap = BinaryTrack.FromAllZeroExcept(gameProps, new List<(int,int)>{potentialTorpedoTarget});
                BinaryTrack torpedoRangeMap = BinaryTrack.FromAllZeroExcept(gameProps, potentialTorpedoTarget.FindNeighbouringCells(gameProps, map));
                result[potentialTorpedoTarget] = (torpedoTargetMap, torpedoRangeMap);
            }
            return result;
        }

        public static List<(int, int)> CalculateTorpedoRangeNotHittingMyself(this (int, int) myPosition, GameProps gameProps, int[,] map)
        {
            var (x, y) = myPosition;
            var positionsInRange = new Dictionary<(int,int),(int,int)>();
            int i,j;
            for (int max = 4; max > 0; max--)
            {
                for (i = 0; i <= max; i++)
                {
                    j = max - i;
                    SafeAddPositionsInRange(positionsInRange, (x + i, y + j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x + i, y - j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x - i, y + j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x - i, y - j), gameProps);
                }
            }

            var neighbours = myPosition.FindNeighbouringCells(gameProps, map);
            return positionsInRange.Values.Where(positions => map[positions.Item1, positions.Item2] == 0).Where(p => !neighbours.Any(n => n.Equals(p))).ToList();
        }
        
        public static List<(int, int)> CalculateTorpedoRange(this (int, int) myPosition, GameProps gameProps, int[,] map)
        {
            var (x, y) = myPosition;
            var positionsInRange = new Dictionary<(int,int),(int,int)>();
            int i,j;
            for (int max = 4; max > 0; max--)
            {
                for (i = 0; i <= max; i++)
                {
                    j = max - i;
                    SafeAddPositionsInRange(positionsInRange, (x + i, y + j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x + i, y - j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x - i, y + j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x - i, y - j), gameProps);
                }
            }

            return positionsInRange.Values.Where(positions => map[positions.Item1, positions.Item2] == 0).ToList();
        }

        private static void SafeAddPositionsInRange(Dictionary<(int, int), (int, int)> positionsInRange, (int, int) position, GameProps gameProps)
        {
            var (x1, y1) = position;
            if (x1 >= 0 && x1 < gameProps.Width && y1 >= 0 && y1 < gameProps.Height)
            {
                positionsInRange[position] = position;
            }
        }
        
    }
}