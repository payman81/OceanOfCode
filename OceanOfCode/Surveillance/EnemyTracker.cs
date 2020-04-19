using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
// ReSharper disable ConvertNullableToShortForm

namespace OceanOfCode.Surveillance
{
    public class EnemyTracker : IEnemyTracker
    {
        private readonly GameProps _gameProps;
        private readonly IConsole _console;
        private readonly BinaryTrack _binaryMap;
        private readonly HeadPositionReducer _headPositionReducer;
        
        Regex _moveRegex = new Regex("^MOVE (.?)");
        Regex _silenceRegex = new Regex("^SILENCE");
        Regex _surfaceRegex = new Regex("^SURFACE (.?)");
        Regex _torpedoRegex = new Regex("^TORPEDO ([0-9]{1,2}) ([0-9]{1,2})");

        private List<BinaryTrack> _possibleEnemyTracks = new List<BinaryTrack>();
        private BinaryTrack _currentTrack;
        private BinaryTrack _exactEnemyTrack;
        private char _lastMoveMoveDirection = Direction.None;
        private int _lastEnemyLife = 6;


        private EnemyTracker(GameProps gameProps, BinaryTrack binaryMap, BinaryTrack currentTrack,
            BinaryTrack exactTrack, IConsole console, HeadPositionReducer headPositionReducer, char lastMoveDirection)
        {
            _console = console;
            _headPositionReducer = headPositionReducer;
            _gameProps = gameProps;
            _binaryMap = binaryMap;
            _currentTrack = currentTrack;
            _exactEnemyTrack = exactTrack;
            _lastMoveMoveDirection = lastMoveDirection;
            binaryMap.ToCartesian();
        }
        public EnemyTracker(GameProps gameProps, int[,] map, IConsole console, HeadPositionReducer headPositionReducer)
        {
            _gameProps = gameProps;
            _console = console;
            _headPositionReducer = headPositionReducer;
            map.CloneMap();
            _binaryMap = BinaryTrack.FromCartesian(gameProps, map);
            _currentTrack = BinaryTrack.StartEmptyTrack(gameProps);
        }

        public List<BinaryTrack> PossibleEnemyTracks()
        {
            return _possibleEnemyTracks;
        }

        public List<BinaryTrack> PossibleTracksWithHeadFilter(BinaryTrack currentTrack, BinaryTrack headFilter)
        {
            List<BinaryTrack> possibleTracks = new List<BinaryTrack>();
            BinaryTrack currentPossibleTrack = BinaryTrack.FromAnotherBinaryTrack(currentTrack);
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
                            possibleTracks.Add(nextPossibleTrack);
                        }
                        
                    }
                } while (nextPossibleTrack.TryShiftEast(out nextPossibleTrack));

                nextPossibleTrack = currentPossibleTrack;
            } while (nextPossibleTrack.TryShiftSouth(out nextPossibleTrack));

            return possibleTracks;
        }
        
        public List<(int, int)> PossibleEnemyPositions()
        {
            return PossibleEnemyTracks().Where(x => x.Head.HasValue).Select(x => x.Head.Value).ToList();
        }

        public void OnMove(char direction)
        {
            _lastMoveMoveDirection = direction;
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
            _headPositionReducer.Handle(new MoveDetected{Direction = direction});
        }

        public void OnSilence()
        {
            _console.Debug("Opponent silence detected. Resetting enemy's starting position");
            
            _currentTrack = BinaryTrack.StartEmptyTrack(_gameProps);
            _exactEnemyTrack = null;
            _headPositionReducer.Handle(new SilenceDetected{LastMoveDirection = _lastMoveMoveDirection});
        }
        
        private void OnSurface(SurfaceDetected surfaceDetected)
        {
            _console.Debug("Opponent surface detected. Resetting enemy's track keeping the head");
            _currentTrack = BinaryTrack.StartEmptyTrack(_gameProps);
            if (_exactEnemyTrack != null)
            {
                _exactEnemyTrack = BinaryTrack.FromAllZeroExcept(_gameProps,
                    new List<(int, int)> {_exactEnemyTrack.Head.Value}, _exactEnemyTrack.Head);
            }
            
            _headPositionReducer.Handle(surfaceDetected);
        }

        public BinaryTrack FirstPossibleTrack()
        {
            return _currentTrack;
        }

        public void Next(MoveProps moveProps)
        {
            if (moveProps.OpponentLife != _lastEnemyLife)
            {
                _headPositionReducer.Handle(new EnemyLifeChanged{PreviousLife = _lastEnemyLife, CurrentLife = moveProps.OpponentLife, EnemyOrder = moveProps.OpponentOrders});
                _lastEnemyLife = moveProps.OpponentLife;
            }
            var orders = moveProps.OpponentOrders.Split('|');
            SurfaceDetected surfaceDetected;
            TorpedoDetected torpedoDetected;
            bool silenceDetected;
            
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
                    silenceDetected = true;
                    if (silenceDetected)
                    {
                        OnSilence();
                    }
                }
                else if (_surfaceRegex.Match(order).Success)
                {
                    var sectorString = _surfaceRegex.Match(order).Groups[1].Value;
                    surfaceDetected = new SurfaceDetected{Sector = int.Parse(sectorString)};
                    if (surfaceDetected != null)
                    {
                        OnSurface(surfaceDetected);
                    }
                }
                else if(_torpedoRegex.Match(order).Success)
                {
                    var torpedoRegex = _torpedoRegex.Match(order);
                    var x = int.Parse(torpedoRegex.Groups[1].Value);
                    var y = int.Parse(torpedoRegex.Groups[2].Value);
                    torpedoDetected = new TorpedoDetected {Target = (x, y)};
                    if (torpedoDetected != null)
                    {
                        OnTorpedo(torpedoDetected);
                    }
                }
            }
            GeneratePossibleEnemyTracks();
        }

        private void OnTorpedo(TorpedoDetected torpedoDetected)
        {
            _headPositionReducer.Handle(torpedoDetected);
        }


        public string Debug()
        {
            var debug = $"binaryMap:{_binaryMap.Debug()} opponentTrack:{_currentTrack.Debug()} ";
            if (_exactEnemyTrack != null)
            {
                debug = debug + $"exactTrack:{_exactEnemyTrack.Debug()}";
            }

            debug = debug + $"HEadFilter:{_headPositionReducer.HeadFilter.Debug()}";
            debug = debug + $"lastMoveDirection: {_lastMoveMoveDirection}";

            return debug;
        }

        public static EnemyTracker FromDebug(GameProps gameProps, BinaryTrack binaryMap, BinaryTrack currentTrack, BinaryTrack exactTrack, IConsole console, HeadPositionReducer headPositionReducer, char lastDirection)
        {
            return new EnemyTracker(gameProps, binaryMap, currentTrack, exactTrack, console, headPositionReducer, lastDirection);
        }

        public bool DoWeHaveExactEnemyLocation()
        {
            return _exactEnemyTrack != null;
        }

        private void GeneratePossibleEnemyTracks()
        {
            var possibleTracks = PossibleTracksWithHeadFilter(_currentTrack, _headPositionReducer.HeadFilter).ToList();
            if (possibleTracks.Count == 1)
            {
                _exactEnemyTrack = possibleTracks.Single();
            }

            _possibleEnemyTracks = possibleTracks;
        }
    }
}