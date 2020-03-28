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
    }
}