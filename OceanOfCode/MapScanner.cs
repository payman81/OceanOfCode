namespace OceanOfCode
{
    public class MapScanner
    {
        private readonly GameProps _gameProps;
        private readonly IConsole _console;
        private int[,] _map;

        public MapScanner(GameProps gameProps, IConsole console)
        {
            _gameProps = gameProps;
            _console = console;
        }
        public int[,] GetMapOrScan()
        {
            if (_map != null)
            {
                return _map;
            }
            _map = new int[_gameProps.Width, _gameProps.Height];
            for (int j = 0; j < _gameProps.Height; j++)
            {
                string line = _console.ReadLine();
                char[] rowChars = line.ToCharArray();
                for (int i = 0; i < _gameProps.Width; i++)
                {
                    _map[i, j] = rowChars[i].Equals('.') ? 0 : 1;
                }
            }
            _console.Debug("Map scanned!");
            return _map;
        }
    }
}