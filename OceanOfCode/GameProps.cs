namespace OceanOfCode
{
    public class GameProps
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MyId { get; set; }
        public override string ToString()
        {
            return $"GameProps Input: Width:{Width}, Height:{Height}, MyId:{MyId}";
        }
    }
}