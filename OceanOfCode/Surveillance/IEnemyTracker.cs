using System.Collections.Generic;

namespace OceanOfCode.Surveillance
{
    public interface IEnemyTracker
    {
        List<(int, int)> PossibleEnemyPositions();
        List<BinaryTrack> PossibleEnemyTracks();
        void Next(MoveProps moveProps);
        string Debug();
    }
}