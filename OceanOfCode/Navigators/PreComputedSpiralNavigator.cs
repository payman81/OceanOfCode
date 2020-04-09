using System;
using System.Collections.Generic;
using System.Linq;

namespace OceanOfCode
{
    public class Cell
    {
        public (int, int) Position { get; set; }
        public Move Next { get; set; }
        public Move Previous { get; set; }
    }

    public class Move
    {
        public char Direction { get; set; }
        public Cell Source { get; set; }
        public Cell Destination { get; set; }
    }

    class PathTimeMachine
    {
        class Node
        {
            public int Weight { get; set; }
            public List<Node> Neighbours { get; } = new List<Node>();
        }

        private readonly int[,] _map;
        private readonly GameProps _gameProps;
        private readonly Node[,] _weightedMap;
        private Move _lastMoveWithMoreTurnOptions;

        public PathTimeMachine(int[,] map, GameProps gameProps)
        {
            _map = map;
            _gameProps = gameProps;
            _weightedMap = new Node[_gameProps.Width, _gameProps.Height];
            BuildWeightedMap();
        }

        public Move GetLastMoveWithOtherChoiceOfTurn()
        {
            return _lastMoveWithMoreTurnOptions;
        }

        public void AddMove(Move nextMove)
        {
            var position = nextMove.Destination.Position;
            UpdateNeighboursWeight(position);
            var (x, y) = position;
            if (_weightedMap[x, y].Weight > 1)
            {
                _lastMoveWithMoreTurnOptions = nextMove;
            }
        }

        private void BuildWeightedMap()
        {
            for (int j = 0; j < _gameProps.Height; j++)
            {
                for (int i = 0; i < _gameProps.Width; i++)
                {
                    _weightedMap[i, j] = new Node();
                }
            }

            for (int j = 0; j < _gameProps.Height; j++)
            {
                for (int i = 0; i < _gameProps.Width; i++)
                {
                    var neighbourPositions = GetNeighbourPositions((i, j));
                    _weightedMap[i, j].Neighbours
                        .AddRange(neighbourPositions.Select(p => _weightedMap[p.Item1, p.Item2]));
                    _weightedMap[i, j].Weight = neighbourPositions.Sum(p => _map[p.Item1, p.Item2] == 0 ? 1 : 0);
                }
            }
        }

        private List<(int, int)> GetNeighbourPositions((int x, int y) position)
        {
            var (x, y) = position;
            var neighbourPositions = new List<(int, int)>();
            if (x > 0)
            {
                neighbourPositions.Add((x - 1, y));
            }

            if (x < _gameProps.Width - 1)
            {
                neighbourPositions.Add((x + 1, y));
            }

            if (y > 0)
            {
                neighbourPositions.Add((x, y - 1));
            }

            if (y < _gameProps.Height - 1)
            {
                neighbourPositions.Add((x, y + 1));
            }

            return neighbourPositions;
        }

        private void UpdateNeighboursWeight((int, int) position)
        {
            var neighbourPositions = GetNeighbourPositions(position);
            foreach (var neighbourPosition in neighbourPositions)
            {
                var (x, y) = neighbourPosition;
                _weightedMap[x, y].Weight--;
            }
        }
    }

    public class ComputedPath
    {
        private readonly int[,] _map;
        private readonly int[,] _originalMap;
        private readonly IConsole _console;
        private readonly Dictionary<(int, int), Cell> _path = new Dictionary<(int, int), Cell>();
        private (int, int) _firstPosition;
        private (int, int) _lastPosition;
        private bool _reversedModeOn;
        private readonly GameProps _gameProps;
        readonly PathTimeMachine _timeMachine;

        public ComputedPath(int[,] map, IConsole console, bool reversedModeOn, GameProps gameProps)
        {
            _console = console;
            _gameProps = gameProps;
            _map = map.CloneMap();
            _originalMap = map;
            _timeMachine = new PathTimeMachine(_map, gameProps);
            PreComputePath();
            _reversedModeOn = reversedModeOn;
        }

        private void PreComputePath()
        {
            _firstPosition = GetFirstPosition();
            var currentCell = new Cell {Position = _firstPosition};
            _path[_firstPosition] = currentCell;
            Cell nextCell = null;
            do
            {
                _map[currentCell.Position.Item1, currentCell.Position.Item2] = 1;
                var nextCellAndDirection = TryFindNextCell(currentCell, currentCell.Previous?.Destination);
                nextCell = nextCellAndDirection?.Item1;
                if (nextCell == null && ShouldAvoidDeadEnds())
                {
                    Move moveAtLastTurn = _timeMachine.GetLastMoveWithOtherChoiceOfTurn();
                    currentCell = moveAtLastTurn.Destination;
                    nextCellAndDirection = TryFindNextCell(currentCell, currentCell.Previous?.Destination,
                        directionToAvoid: moveAtLastTurn.Direction);
                    nextCell = nextCellAndDirection?.Item1;
                }

                if (nextCell != null)
                {
                    var nextMove = new Move
                        {Source = currentCell, Destination = nextCell, Direction = nextCellAndDirection.Value.Item2};
                    currentCell.Next = nextMove;
                    nextCell.Previous = new Move
                    {
                        Source = nextCell, Destination = currentCell,
                        Direction = nextCellAndDirection.Value.Item2.ToOpposite()
                    };
                    _path[nextCell.Position] = nextCell;
                    currentCell = nextCell;
                    _lastPosition = nextCell.Position;

                    _timeMachine.AddMove(nextMove);
                }
            } while (nextCell != null);
        }

        private bool ShouldAvoidDeadEnds()
        {
            var availableCellCount = _map.AvailableCellCount();
            var shouldAvoidDeadEnds = (double) availableCellCount / (_gameProps.Width * _gameProps.Height) > .05;
            _console.Debug(shouldAvoidDeadEnds
                ? $"Avoiding dead ends as there are {availableCellCount} free cells."
                : $"Not avoiding dead ends as there are only {availableCellCount} free cells.");

            return shouldAvoidDeadEnds;
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

        private (Cell, char)? TryFindNextCell(Cell currentCell, Cell previousCell, char? directionToAvoid = null)
        {
            if (previousCell == null)
            {
                return TryFindNextCellBasedOnOrderedDirections(
                    new[] {Direction.East, Direction.South, Direction.West, Direction.North},
                    currentCell);
            }

            List<char> directions = GetDirectionsInTheRightOrder(currentCell.Position, previousCell.Position).ToList();
            if (directionToAvoid.HasValue)
            {
                directions.Remove(directionToAvoid.Value);
            }

            ;
            return TryFindNextCellBasedOnOrderedDirections(directions.ToArray(), currentCell);
        }

        private char[] GetDirectionsInTheRightOrder((int, int) currentPosition, (int, int) previousPosition)
        {
            var directionMap = new Dictionary<char, char[]>
            {
                {Direction.None, new[] {Direction.East, Direction.South, Direction.West, Direction.North}},
                {Direction.East, new[] {Direction.North, Direction.East, Direction.South}},
                {Direction.South, new[] {Direction.East, Direction.South, Direction.West}},
                {Direction.West, new[] {Direction.South, Direction.West, Direction.North}},
                {Direction.North, new[] {Direction.West, Direction.North, Direction.East}},
            };
            int deltaX = previousPosition.Item1 - currentPosition.Item1;
            int deltaY = previousPosition.Item2 - currentPosition.Item2;

            var delta = (deltaX, deltaY);

            if (delta.Equals((-1, 0)))
            {
                return directionMap[Direction.East];
            }

            if (delta.Equals((0, -1)))
            {
                return directionMap[Direction.South];
            }

            if (delta.Equals((1, 0)))
            {
                return directionMap[Direction.West];
            }

            if (delta.Equals((0, 1)))
            {
                return directionMap[Direction.North];
            }

            return directionMap[Direction.None];
        }

        private (Cell, char)? TryFindNextCellBasedOnOrderedDirections(char[] directions, Cell currentCell)
        {
            foreach (var direction in directions)
            {
                bool canMove;
                switch (direction)
                {
                    case Direction.East:
                        canMove = CanMoveEast(currentCell.Position);
                        break;
                    case Direction.South:
                        canMove = CanMoveSouth(currentCell.Position);
                        break;
                    case Direction.West:
                        canMove = CanMoveWest(currentCell.Position);
                        break;
                    case Direction.North:
                        canMove = CanMoveNorth(currentCell.Position);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction given");
                }

                if (canMove)
                {
                    return (new Cell {Position = currentCell.Position.FindPositionWhenIMove(direction)}, direction);
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
            return _reversedModeOn ? _path[_lastPosition] : _path[_firstPosition];
        }

        public Move Next((int, int) currentPosition)
        {
            return _reversedModeOn ? _path[currentPosition].Previous : _path[currentPosition].Next;
        }

        public void Reset()
        {
            _reversedModeOn = !_reversedModeOn;
        }
    }

    public class PreComputedSpiralNavigator : INavigator
    {
        private readonly ComputedPath _path;

        public PreComputedSpiralNavigator(MapScanner mapScanner, IConsole console, bool reversedModeOn,
            GameProps gameProps)
        {
            _path = new ComputedPath(mapScanner.GetMapOrScan(), console, reversedModeOn, gameProps);
        }

        public char? Next((int, int) currentPosition)
        {
            return _path.Next(currentPosition)?.Direction;
        }

        public void Reset()
        {
            _path.Reset();
        }

        public (int, int) First()
        {
            return _path.Next().Position;
        }
    }
}