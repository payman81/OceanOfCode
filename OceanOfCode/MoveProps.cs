namespace OceanOfCode
{
    public class MoveProps
    {
        public (int, int) MyPosition { get; set; }
        public int TorpedoCooldown { get; set; }
        public string OpponentOrders { get; set; }
        public int MyLife { get; set; }
        public int OpponentLife { get; set; }
        public int SilenceCooldown { get; set; }
        public int MineCooldown { get; set; }

        public override string ToString()
        {
            return $"MoveProps Input: x:{MyPosition.Item1}, y:{MyPosition.Item2}, TorpedoCooldown:{TorpedoCooldown}, OpponentOrders:{OpponentOrders}";
        }
    }
}