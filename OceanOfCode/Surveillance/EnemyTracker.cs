using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OceanOfCode.Surveillance
{
    /* MVP:
    * - (done) check for possibilities 
    * - (done) keep track of the head
    * - (done)transfer series of directions to BinaryMap
     * - (done) collect enemy's commands
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
        Regex _moveRegex = new Regex("MOVE (.?)");

        private List<char> _lastMoves = new List<char>();
        private BinaryTrack _currentTrack;


        public EnemyTracker(GameProps gameProps, int[,] map)
        {
            _gameProps = gameProps;
            _cartesianMap = map.CloneMap();
            _binaryTrack = BinaryTrack.FromCartesian(gameProps, map);
            _currentTrack = BinaryTrack.StartEmptyTrack(gameProps);
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

        public IEnumerable<BinaryTrack> PossibleMatches()
        {
            return PossibleMatches(_currentTrack);
        }


        public void OnMove(char direction)
        {
            switch (direction)
            {
                case Direction.East:
                    _currentTrack = _currentTrack.MoveEast();
                    break;
                case Direction.South:
                    _currentTrack = _currentTrack.MoveSouth();
                    break;
                case Direction.West:
                    _currentTrack = _currentTrack.MoveWest();
                    break;
                case Direction.North:
                    _currentTrack = _currentTrack.MoveNorth();
                    break;
            }
        }

        public void OnSilence()
        {
            _lastMoves = new List<char>();
            _currentTrack = BinaryTrack.StartEmptyTrack(_gameProps);
        }

        public BinaryTrack FirstPossibleTrack()
        {
            return _currentTrack;
        }

        public void Next(MoveProps moveProps)
        {
            var orders = moveProps.OpponentOrders.Split('|');
            foreach (var order in orders)
            {
                var regexResult = _moveRegex.Match(order);
                if (regexResult.Groups.Count > 1)
                {
                    char moveDirection = regexResult.Groups[1].Value.ToCharArray()[0];
                    OnMove(moveDirection);
                }
            }
            
         
        }
    }

   
}