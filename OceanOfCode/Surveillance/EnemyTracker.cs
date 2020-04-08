using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OceanOfCode.Surveillance
{
    public interface IEnemyTracker
    {
        IEnumerable<(int, int)> PossibleEnemyPositions();
        void Next(MoveProps moveProps);
        string Debug();
    }

    public class EnemyTracker : IEnemyTracker
    {
        private readonly GameProps _gameProps;
        private readonly IConsole _console;
        private readonly int[,] _cartesianMap;
        private readonly BinaryTrack _binaryMap;
        
        Regex _moveRegex = new Regex("^MOVE (.?)");
        Regex _silenceRegex = new Regex("^SILENCE");
        Regex _surfaceRegex = new Regex("^SURFACE (.?)");

        private BinaryTrack _currentTrack;
        private BinaryTrack _exactEnemyTrack = null;


        private EnemyTracker(GameProps gameProps, BinaryTrack binaryMap, BinaryTrack currentTrack, IConsole console)
        {
            _console = console;
            _gameProps = gameProps;
            _binaryMap = binaryMap;
            _currentTrack = currentTrack;
            _cartesianMap = binaryMap.ToCartesian();
        }
        public EnemyTracker(GameProps gameProps, int[,] map, IConsole console)
        {
            _gameProps = gameProps;
            _console = console;
            _cartesianMap = map.CloneMap();
            _binaryMap = BinaryTrack.FromCartesian(gameProps, map);
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
                    if (!nextPossibleTrack.HasCollisionWith(_binaryMap))
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

        public IEnumerable<BinaryTrack> PossibleTracksWithHeadFilter(BinaryTrack headFilter)
        {
            BinaryTrack currentPossibleTrack = BinaryTrack.FromAnotherBinaryTrack(_currentTrack);
            BinaryTrack nextPossibleTrack = currentPossibleTrack;
            do
            {
                currentPossibleTrack = nextPossibleTrack;
                do
                {
                    if (!nextPossibleTrack.HasCollisionWith(_binaryMap))
                    {
                        if (!nextPossibleTrack.HasHeadCollisionWith(headFilter))
                        {
                            yield return nextPossibleTrack;
                        }
                        
                    }
                } while (nextPossibleTrack.TryShiftEast(out nextPossibleTrack));

                nextPossibleTrack = currentPossibleTrack;
            } while (nextPossibleTrack.TryShiftSouth(out nextPossibleTrack));
        }
        
        public IEnumerable<(int, int)> PossibleEnemyPositions()
        {
            if (_exactEnemyTrack != null)
            {
                return new List<(int, int)>{_exactEnemyTrack.Head.Value};
            }
            var possibleTracks = PossibleTracks().ToList();
            if (possibleTracks.Count == 1)
            {
                _exactEnemyTrack = possibleTracks.Single();
            }
            return possibleTracks.Where(x => x.Head.HasValue).Select(x => x.Head.Value);
        }



        public void OnMove(char direction)
        {
            switch (direction)
            {
                case Direction.East:
                    _currentTrack = _currentTrack.MoveEast();
                    _exactEnemyTrack = _exactEnemyTrack?.MoveEast();
                    break;
                case Direction.South:
                    _currentTrack = _currentTrack.MoveSouth();
                    _exactEnemyTrack = _exactEnemyTrack?.MoveSouth();
                    break;
                case Direction.West:
                    _currentTrack = _currentTrack.MoveWest();
                    _exactEnemyTrack = _exactEnemyTrack?.MoveWest();
                    break;
                case Direction.North:
                    _currentTrack = _currentTrack.MoveNorth();
                    _exactEnemyTrack = _exactEnemyTrack?.MoveNorth();
                    break;
            }
        }

        public void OnSilence()
        {
            _console.Debug("Opponent silence detected. Resetting enemy's starting position");
            
            _currentTrack = BinaryTrack.StartEmptyTrack(_gameProps);
            _exactEnemyTrack = null;
        }
        
        private void OnSurface(object SurfaceDetected)
        {
            _console.Debug("Opponent surface detected. Resetting enemy's track keeping the head");
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
                else if (_surfaceRegex.Match(order).Success)
                {
                    OnSurface(null);
                }
            }
        }


        public string Debug()
        {
            return $"binaryMap:{_binaryMap.Debug()} opponentTrack:{_currentTrack.Debug()}";
        }

        public static EnemyTracker FromDebug(GameProps gameProps, BinaryTrack binaryMap, BinaryTrack currentTrack, IConsole console)
        {
            return new EnemyTracker(gameProps, binaryMap, currentTrack, console);
        }

    }
}