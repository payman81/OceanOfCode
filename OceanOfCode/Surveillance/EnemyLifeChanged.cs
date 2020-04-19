namespace OceanOfCode.Surveillance
{
    public class EnemyLifeChanged
    {
        public int PreviousLife { get; set; }
        public int CurrentLife { get; set; }
        public string EnemyOrder { get; set; }

        public bool HasSurfaced()
        {
            return EnemyOrder?.Contains("SURFACE") ?? false;
        }
    }
}