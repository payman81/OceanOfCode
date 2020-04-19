using System.Collections.Generic;
using System.Linq;

namespace OceanOfCode
{
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
}