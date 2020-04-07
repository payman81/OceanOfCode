using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OceanOfCode.Surveillance
{
    /* MVP:
    * - (done) check for possibilities 
    * - (done) keep track of the head
    * - (done)transfer series of directions to BinaryMap
     * - (done) collect enemy's commands
     * - (done)Torpedo if charged and single possibility is within reach
     * Reset after Silence and 
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
    public interface IEnemyTracker
    {
        IEnumerable<(int, int)> PossibleEnemyPositions();
        void Next(MoveProps moveProps);
    }

    public class EnemyTracker : IEnemyTracker
    {
        private readonly GameProps _gameProps;
        private readonly IConsole _console;
        private readonly int[,] _cartesianMap;
        private readonly BinaryTrack _binaryTrack;
        Regex _moveRegex = new Regex("MOVE (.?)");
        Regex _silenceRegex = new Regex("SILENCE");

        private BinaryTrack _currentTrack;


        public EnemyTracker(GameProps gameProps, int[,] map, IConsole console)
        {
            _gameProps = gameProps;
            _console = console;
            _cartesianMap = map.CloneMap();
            _binaryTrack = BinaryTrack.FromCartesian(gameProps, map);
            _currentTrack = BinaryTrack.StartEmptyTrack(gameProps);
        }

        public IEnumerable<BinaryTrack> PossibleTracks(BinaryTrack currentPossibleTrack)
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

        public IEnumerable<BinaryTrack> PossibleTracks()
        {
            return PossibleTracks(_currentTrack);
        }

        public IEnumerable<(int, int)> PossibleEnemyPositions()
        {
            return PossibleTracks().Where(x => x.Head.HasValue).Select(x => x.Head.Value);
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
            _console.Debug("Opponent silence detected. Resetting enemy's starting position");
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

                else if (_silenceRegex.Match(order).Success)
                {
                    OnSilence();
                }
            }
        }
    }
}