namespace OceanOfCode
{
    public class PreComputedSpiralNavigator : INavigator
    {
        private readonly ComputedPath _path;

        public PreComputedSpiralNavigator(MapScanner mapScanner, IConsole console, bool reversedModeOn,
            GameProps gameProps)
        {
            _path = new ComputedPath(mapScanner.GetMapOrScan(), console, reversedModeOn, gameProps);
        }

        public NavigationResult Next((int, int) currentPosition)
        {
            var next = _path.Next(currentPosition);
            if (next == null)
            {
                return null;
            }

            return new NavigationResult {Direction = next.Direction, Position = next.Destination.Position};
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