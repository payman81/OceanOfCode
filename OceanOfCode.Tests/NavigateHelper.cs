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
        
        //todo: use the logic in the app
        public int[,] ScanMap(int width, int height)
        {
            int[,] map = new int[width, height];
            for (int j = 0; j < height; j++)
            {
                string line = _console.ReadLine();
                char[] rowChars = line.ToCharArray();
                for (int i = 0; i < width; i++)
                {
                    map[i, j] = rowChars[i].Equals('.') ? 0 : 1;
                }
            }

            return map;
        }
    }
}