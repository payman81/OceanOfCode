using System;
using System.Collections.Generic;

namespace OceanOfCode
{
    public class Cell
    {
        //public char Direction { get; set; }
        public (int, int) Position { get; set; }
        public Move Next { get; set; }
        public Move Previous { get; set; }
    }

    public class Move
    {
        public char Direction { get; set; }
        public Cell Destination { get; set; }
    }

    public class ComputedPath
    {
        private int[,] _map;
        private readonly IConsole _console;
        private readonly Dictionary<(int, int), Cell> _path = new Dictionary<(int, int), Cell>();
        private (int, int) _firstPosition;
        private bool _isReversed;

        public ComputedPath(int[,] map, IConsole console)
        {
            _console = console;
            _map = map.CloneMap();
            PreComputePath();
        }

       

        private void PreComputePath()
        {
            (int, int)? previousPosition = null;
            _firstPosition = GetFirstPosition();

            _map[_firstPosition.Item1, _firstPosition.Item2] = 1;
            var currentCell = new Cell {Position = _firstPosition};
            _path[_firstPosition] = currentCell;
            Cell nextCell;
            do
            {
                var nextCellAndDirection = TryFindNextCell(currentCell.Position, previousPosition);
                nextCell = nextCellAndDirection?.Item1;
                if (nextCell != null)
                {
                    nextCell.Previous = new Move{Destination = currentCell, Direction = nextCellAndDirection.Value.Item2.ToOpposite()};
                    currentCell.Next = new Move{Destination = nextCell, Direction = nextCellAndDirection.Value.Item2};
                    _path[nextCell.Position] = nextCell;
                    previousPosition = currentCell.Position;
                    currentCell = nextCell;
                }
            } while (nextCell != null);
        }

        private (int X, int Y) GetFirstPosition()
        {
            for (int j = 0; j < _map.GetLength(1); j++)
            {
                for (int i = 0; j < _map.GetLength(0); j++)
                {
                    if (_map[i, j] == 0)
                    {
                        return (i, j);
                    }
                }
            }

            var errorMessage = "Cannot find first position";
            _console.Debug(errorMessage);
            throw new Exception(errorMessage);
        }

        private (Cell, char)? TryFindNextCell((int, int) currentPosition, (int, int)? previousPosition)
        {
            var (x, y) = currentPosition;
            _map[x, y] = 1;

            if (!previousPosition.HasValue)
            {
                return TryFindNextCell(new[] {Direction.East, Direction.South, Direction.West, Direction.North},
                    currentPosition);
            }

            char[] directions = GetDirectionsInTheRightOrder(currentPosition, previousPosition.Value);

            return TryFindNextCell(directions, currentPosition);
        }

        private char[] GetDirectionsInTheRightOrder((int, int) currentPosition, (int, int) previousPosition)
        {
            var directionMap = new Dictionary<char, char[]>
            {
                {Direction.East, new[] {Direction.North, Direction.East, Direction.South}},
                {Direction.South, new[] {Direction.East, Direction.South, Direction.West}},
                {Direction.West, new[] {Direction.South, Direction.West, Direction.North}},
                {Direction.North, new[] {Direction.West, Direction.North, Direction.East}},
            };
            int deltaX = previousPosition.Item1 - currentPosition.Item1;
            int deltaY = previousPosition.Item2 - currentPosition.Item2;
            char currentDirection;

            switch (deltaX, deltaY)
            {
                case (-1, 0):
                    currentDirection = Direction.East;
                    break;
                case (0, -1):
                    currentDirection = Direction.South;
                    break;
                case (1, 0):
                    currentDirection = Direction.West;
                    break;
                case (0, 1):
                    currentDirection = Direction.North;
                    break;
                    
                default:
                    throw new Exception("Incorrect positions given. Previous position and current position should be next to each other");
            }

            if (currentDirection == default(char))
            {
                throw new Exception($"Could not deduce direction. previous position={previousPosition} current position={currentDirection}");
            }
            return directionMap[currentDirection];
        }

        private (Cell, char)? TryFindNextCell(char[] directions, (int, int) position)
        {
            foreach (var direction in directions)
            {
                bool canMove;
                switch (direction)
                {
                    case Direction.East:
                        canMove = CanMoveEast(position);
                        break;
                    case Direction.South:
                        canMove = CanMoveSouth(position);
                        break;
                    case Direction.West:
                        canMove = CanMoveWest(position);
                        break;
                    case Direction.North:
                        canMove = CanMoveNorth(position);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction given");
                }

                if (canMove)
                {
                    return (new Cell {Position = position.FindPositionWhenIMove(direction)}, direction);
                }
            }

            return null;
        }

        private bool CanMoveEast((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (x == _map.GetLength(0) - 1)
            {
                return false;
            }

            return _map[x + 1, y] == 0;
        }

        private bool CanMoveSouth((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (y == _map.GetLength(1) - 1)
            {
                return false;
            }

            return _map[x, y + 1] == 0;
        }

        private bool CanMoveWest((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (x == 0)
            {
                return false;
            }

            return _map[x - 1, y] == 0;
        }

        private bool CanMoveNorth((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (y == 0)
            {
                return false;
            }

            return _map[x, y - 1] == 0;
        }


        public Cell Next()
        {
            return _path[_firstPosition];
        }

        public Move Next((int, int) currentPosition)
        {
            return _isReversed? _path[currentPosition].Previous : _path[currentPosition].Next;
        }

        public void Reset()
        {
            _isReversed = !_isReversed;
        }
    }

    public class PreComputedSpiralNavigator : INavigator
    {
        private (int, int)? _previousPosition = null;
        private ComputedPath path;


        public PreComputedSpiralNavigator(MapScanner mapScanner, IConsole console)
        {
            path = new ComputedPath(mapScanner.GetMapOrScan(), console);
        }

        public char? Next((int, int) currentPosition)
        {
            return path.Next(currentPosition)?.Direction;
        }


        public void Reset()
        {
            path.Reset();
        }

        public (int, int) First()
        {
            return path.Next().Position;
        }
        
        
    }

    public static class PositionExtensions
    {
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
    }
}