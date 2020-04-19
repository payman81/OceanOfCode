namespace OceanOfCode.Surveillance
{
    public class EnemyAttacked
    {
        public (int, int)? TriggeredMinePosition { get; set; }
        public (int, int)? TorpedoTargetPosition { get; set; }
    }
}