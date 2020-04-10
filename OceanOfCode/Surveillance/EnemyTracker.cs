using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly HeadPositionReducer _headPositionReducer;
        
        Regex _moveRegex = new Regex("^MOVE (.?)");
        Regex _silenceRegex = new Regex("^SILENCE");
        Regex _surfaceRegex = new Regex("^SURFACE (.?)");
        Regex _torpedoRegex = new Regex("^TORPEDO ([0-9]{1,2}) ([0-9]{1,2})");

        private BinaryTrack _currentTrack;
        private BinaryTrack _exactEnemyTrack = null;
        private char _lastMoveDirection = Direction.None;


        private EnemyTracker(GameProps gameProps, BinaryTrack binaryMap, BinaryTrack currentTrack, BinaryTrack exactTrack, IConsole console, HeadPositionReducer headPositionReducer)
        {
            _console = console;
            _headPositionReducer = headPositionReducer;
            _gameProps = gameProps;
            _binaryMap = binaryMap;
            _currentTrack = currentTrack;
            _exactEnemyTrack = exactTrack;
            _cartesianMap = binaryMap.ToCartesian();
        }
        public EnemyTracker(GameProps gameProps, int[,] map, IConsole console, HeadPositionReducer headPositionReducer)
        {
            _gameProps = gameProps;
            _console = console;
            _headPositionReducer = headPositionReducer;
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
            return PossibleTracksWithHeadFilter(_currentTrack, _headPositionReducer.HeadFilter);
        }

        public IEnumerable<BinaryTrack> PossibleTracksWithHeadFilter(BinaryTrack currentTrack, BinaryTrack headFilter)
        {
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
            _lastMoveDirection = direction;
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
            _headPositionReducer.Handle(new SilenceDetected{LastMoveDirection = _lastMoveDirection});
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
            var orders = moveProps.OpponentOrders.Split('|');
            SurfaceDetected surfaceDetected = null;
            TorpedoDetected torpedoDetected = null;
            bool silenceDetected = false;
            
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

            return debug;
        }

        public static EnemyTracker FromDebug(GameProps gameProps, BinaryTrack binaryMap, BinaryTrack currentTrack, BinaryTrack exactTrack, IConsole console, HeadPositionReducer headPositionReducer)
        {
            return new EnemyTracker(gameProps, binaryMap, currentTrack, exactTrack, console, headPositionReducer);
        }
    }

    public class TorpedoDetected
    {
        public (int,int) Target { get; set; }
    }

    public class MoveDetected
    {
        public char Direction { get; set; }
    }

    public class SurfaceDetected
    {
        public int Sector { get; set; }
    }

    public class SilenceDetected
    {
        public char LastMoveDirection { get; set; }
    }

    public class HeadPositionReducer
    {
        private readonly GameProps _gameProps;
        private BinaryTrack _filter;
        private BinaryTrack _mapFilter;
        private int[,] _map; 

        public BinaryTrack HeadFilter => _filter;
        public HeadPositionReducer(GameProps gameProps, MapScanner mapScanner)
        {
            _map = mapScanner.GetMapOrScan();
            _gameProps = gameProps;
            _mapFilter = BinaryTrack.FromCartesian(gameProps, _map);
            
            Reset();
        }

        //For testing
        public HeadPositionReducer(GameProps gameProps, int[,] map)
        {
            _map = map;
            _gameProps = gameProps;
            _mapFilter = BinaryTrack.FromCartesian(gameProps, _map);
            Reset();
        }
        public void Handle(TorpedoDetected torpedoDetected)
        {
            var inRangePositions = torpedoDetected.Target.CalculateTorpedoRange(_gameProps, _map);
            var torpedoRangeFilter = BinaryTrack.FromAllOneExcept(_gameProps, inRangePositions);

            _filter = _filter.BinaryOr(torpedoRangeFilter);
        }

        public void Handle(MoveDetected moveDetected)
        {
            switch (moveDetected.Direction)
            {
                case Direction.East:
                    _filter = ShiftFilterEast();
                    break;
                case Direction.South:
                    _filter = ShiftFilterSouth();
                    break;
                case Direction.West:
                    _filter = ShiftFilterWest();
                    break;
                case Direction.North:
                    _filter = ShiftFilterNorth();
                    break;
            }
        }

       

        private BinaryTrack ShiftFilterEast()
        {
            return _filter
                .ShiftEast(true)
                .BinaryOr(_mapFilter);
        }

        private BinaryTrack ShiftFilterSouth()
        {
            return _filter
                .ShiftSouth(defaultBitsToOne:true)
                .BinaryOr(_mapFilter);
        }
        
        private BinaryTrack ShiftFilterWest()
        {
            return _filter.ShiftWest(true)
                .BinaryOr(_mapFilter);
        }
        
        private BinaryTrack ShiftFilterNorth()
        {
            var track = _filter
                .ShiftNorth(defaultBitsToOne: true);
            return track.BinaryOr(_mapFilter);
        }

        public void Handle(SilenceDetected silenceDetected)
        {
            BinaryTrack nextPaddedEast = _filter;
            BinaryTrack nextPaddedSouth = _filter;
            BinaryTrack nextPaddedWest = _filter;
            BinaryTrack nextPaddedNorth = _filter;
            var directionToAvoidPadding = silenceDetected.LastMoveDirection.ToOpposite();
            for (int i = 0; i < 4; i++)
            {
                if (!directionToAvoidPadding.Equals(Direction.East))
                {
                    nextPaddedEast = nextPaddedEast.ShiftEast(defaultBitsToOne: true);
                    _filter = _filter.BinaryAnd(nextPaddedEast);
                }

                if (!directionToAvoidPadding.Equals(Direction.South))
                {
                    nextPaddedSouth = nextPaddedSouth.ShiftSouth(defaultBitsToOne: true);
                    _filter = _filter.BinaryAnd(nextPaddedSouth);
                }

                if (!directionToAvoidPadding.Equals(Direction.West))
                {
                    nextPaddedWest = nextPaddedWest.ShiftWest(defaultBitsToOne: true);
                    _filter = _filter.BinaryAnd(nextPaddedWest);
                }

                if (!directionToAvoidPadding.Equals(Direction.North))
                {
                    nextPaddedNorth = nextPaddedNorth.ShiftNorth(defaultBitsToOne: true);
                    _filter = _filter.BinaryAnd(nextPaddedNorth);
                }
            }

            _filter = _filter.BinaryOr(_mapFilter);
        }

        private void Reset()
        {
            _filter = BinaryTrack.FromEmpty(_gameProps);

        }

        public void Handle(SurfaceDetected surfaceDetected)
        {
            _filter = _filter
                .BinaryOr(BinaryTrack.FromSector(surfaceDetected.Sector))
                .BinaryOr(_mapFilter);
            
        }
    }
}