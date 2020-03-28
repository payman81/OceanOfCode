using System;

namespace OceanOfCode
{
    public abstract class NavigatorBase : INavigator
    {
        protected int[,] Map;
        private readonly int[,] _originalMap;
        protected readonly GameProps GameProps;

        protected NavigatorBase(GameProps gameProps, MapScanner mapScanner)
        {
            _originalMap = mapScanner.GetMapOrScan();
            GameProps = gameProps;
            Map = _originalMap.CloneMap();
        }

        protected virtual bool CanMoveEast((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (x == GameProps.Width - 1)
            {
                return false;
            }

            return Map[x + 1, y] == 0;
        }

        protected virtual bool CanMoveSouth((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (y == GameProps.Height - 1)
            {
                return false;
            }

            return Map[x, y + 1] == 0;
        }

        protected virtual bool CanMoveWest((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (x == 0)
            {
                return false;
            }

            return Map[x - 1, y] == 0;
        }

        protected virtual bool CanMoveNorth((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (y == 0)
            {
                return false;
            }

            return Map[x, y - 1] == 0;
        }

        public abstract char? Next((int, int) currentPosition);

        public virtual void Reset()
        {
            Map = _originalMap.CloneMap();
        }

        public virtual (int, int) First()
        {
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int j = Map.GetLength(1) - 1; j >= 0; j--)
                {
                    if (Map[i, j] == 0)
                    {
                        return (i, j);
                    }
                }
            }

            throw new Exception("No first position is available!");
        }
    }
}