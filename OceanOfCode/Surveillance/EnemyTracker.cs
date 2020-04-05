using System.Collections.Generic;

namespace OceanOfCode.Surveillance
{
    /* MVP:
    * - (done) check for possibilities 
    * - (done) keep track of the head
    * - transfer series of directions to BinaryMap
     * - collect enemy's commands
     * - Torpedo if charged and single possibility is within reach
     *         /*
         * TORPEDO 5 2|MOVE E
         * TORPEDO 4 1|MOVE E
         * NA
         * SURFACE 7
         * SILENCE
         * SONAR 4
         *
    * Next:
    * watch my life to detect I was hit by torpedo to limit possible area 
    * watch for opponent surface to limit possible areas
    * Use sonar to limit possible area
    * When silence is used, reset the tracker but limit the area
    */
    public class EnemyTracker
    {
        private readonly GameProps _gameProps;
        private readonly int[,] _cartesianMap;
        private readonly BinaryTrack _binaryTrack;
        
        private List<char> _lastMoves = new List<char>();
        private BinaryTrack _startingPosition;
        

        public EnemyTracker(GameProps gameProps, int[,] map)
        {
            _gameProps = gameProps;
            _cartesianMap = map.CloneMap();
            _binaryTrack = BinaryTrack.FromCartesian(gameProps, map);
            _startingPosition = BinaryTrack.StartEmptyTrack(gameProps);
        }

        public IEnumerable<BinaryTrack> PossibleMatches(BinaryTrack currentPossibleTrack)
        {
            BinaryTrack nextPossibleTrack = currentPossibleTrack;
            do
            {
                currentPossibleTrack = nextPossibleTrack;
                do
                {
                    if (!nextPossibleTrack.HasCollisionWith(_binaryTrack))
                    {
                        yield return nextPossibleTrack;
                    }
                } while (nextPossibleTrack.TryShiftEast(out nextPossibleTrack));

                nextPossibleTrack = currentPossibleTrack;
            } while (nextPossibleTrack.TryShiftSouth(out nextPossibleTrack));
        }

        public void OnMove(char direction)
        {
            _lastMoves.Add(direction);
            
        }

        public void OnSilence()
        {
            _lastMoves = new List<char>();
        }

        public BinaryTrack FirstPossibleTrack()
        {
            throw new System.NotImplementedException();
        }
    }
}