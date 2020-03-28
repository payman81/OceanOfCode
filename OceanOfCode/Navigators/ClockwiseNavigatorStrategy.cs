namespace OceanOfCode
{
    public class ClockwiseNavigatorStrategy : NavigatorBase
    {
        public ClockwiseNavigatorStrategy(GameProps gameProps, MapScanner mapScanner) : base(gameProps, mapScanner)
        {
        }


        public override char? Next((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            Map[x, y] = 1;

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
    }
}