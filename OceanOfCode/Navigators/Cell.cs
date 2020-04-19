namespace OceanOfCode
{
    public class Cell
    {
        public (int, int) Position { get; set; }
        public Move Next { get; set; }
        public Move Previous { get; set; }
    }
}