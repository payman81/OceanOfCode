namespace OceanOfCode
{
    public class OceanOfCode
    {
        static void Main(string[] args)
        {
            var controller = new GameController(RealConsole.Instance);
            controller.StartLoop();
        }
    }
}