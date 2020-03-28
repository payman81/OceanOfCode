using System.Collections.Generic;
using System.Linq;

namespace OceanOfCode
{
    public class Node
    {
        public int Weight { get; set; }
        public List<Node> Neighbours { get; } = new List<Node>();
    }
    public class NeighbourAnalysisNavigatorStrategy : NavigatorBase
    {
        private readonly Node[,] _weightedMap;
        public NeighbourAnalysisNavigatorStrategy(int[,] map, GameProps gameProps) : base(map, gameProps)
        {
            _weightedMap = new Node[gameProps.Width, gameProps.Height];
            BuildWeightedMap();
        }

        private void BuildWeightedMap()
        {
            for (int j = 0; j < GameProps.Height; j++)
            {
                for (int i = 0; i < GameProps.Width; i++)
                {
                    _weightedMap[i, j] = new Node();
                }
            }
            for (int j = 0; j < GameProps.Height; j++)
            {
                for (int i = 0; i < GameProps.Width; i++)
                {
                    var neighbourPositions = GetNeighbourPositions((i,j));
                    _weightedMap[i, j].Neighbours.AddRange(neighbourPositions.Select(p => _weightedMap[p.Item1, p.Item2]));
                    _weightedMap[i, j].Weight = neighbourPositions.Sum(p => Map[p.Item1, p.Item2] == 0 ? 1 : 0);
                }
            }
        }

        private List<(int, int)> GetNeighbourPositions((int x, int y) position)
        {
            var (x, y) = position;
            var neighbourPositions = new List<(int,int)>();
            if (x > 0)
            {
                neighbourPositions.Add((x - 1, y));
            }

            if (x < GameProps.Width - 1)
            {
                neighbourPositions.Add((x + 1, y));
            }
            
            if (y > 0)
            {
                neighbourPositions.Add((x, y - 1));
            }

            if (y < GameProps.Height - 1)
            {
                neighbourPositions.Add((x, y + 1));
            }

            return neighbourPositions;
        }
        
        public override char? Next((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            Map[x, y] = 1;
            UpdateNeighboursWeight(currentPosition);

            if (CanMoveEast(currentPosition))
            {
                return Direction.East;
            }

            if (CanMoveSouth(currentPosition))
            {
                return Direction.South;
            }

            if (CanMoveWest(currentPosition))
            {
                return Direction.West;
            }

            if (CanMoveNorth(currentPosition))
            {
                return Direction.North;
            }

            return null;
        }

        public override (int, int) First()
        {
            var position = base.First();
            UpdateNeighboursWeight(position);

            return position;
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

        protected override bool CanMoveEast((int, int) currentPosition)
        {
            var canMove = base.CanMoveEast(currentPosition);
            if (canMove)
            {
                var (x, y) = currentPosition;
                canMove = !IsDeadEnd((x+1, y));
            }

            return canMove;
        }

        protected override bool CanMoveSouth((int, int) currentPosition)
        {
            var canMove = base.CanMoveSouth(currentPosition);
            if (canMove)
            {
                var (x, y) = currentPosition;
                canMove = !IsDeadEnd((x, y + 1));
            }

            return canMove;
        }

        protected override bool CanMoveWest((int, int) currentPosition)
        {
            var canMove = base.CanMoveWest(currentPosition);
            if (canMove)
            {
                var (x, y) = currentPosition;
                canMove = !IsDeadEnd((x - 1, y));
            }
            return canMove;
        }

        protected override bool CanMoveNorth((int, int) currentPosition)
        {
            var canMove = base.CanMoveNorth(currentPosition);
            if (canMove)
            {
                var (x, y) = currentPosition;
                canMove = !IsDeadEnd((x, y - 1));
            }
            return canMove;
        }

        public override void Reset()
        {
            base.Reset();
            BuildWeightedMap();
        }

        private bool IsDeadEnd((int, int y) position)
        {
            var (x, y) = position;
            if (_weightedMap[x,y].Weight < 1)
            {
                return true;
            }

            return false;
        }

        public Node[,] WeightedMap => _weightedMap;
    }
}