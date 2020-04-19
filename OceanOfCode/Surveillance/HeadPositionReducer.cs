using System;
using System.Collections.Generic;

namespace OceanOfCode.Surveillance
{
    public class HeadPositionReducer
    {
        private readonly GameProps _gameProps;
        private BinaryTrack _filter;
        private BinaryTrack _mapFilter;
        private int[,] _map;
        private Nullable<(int, int)> _lastTriggeredMinePosition = null;
        private Nullable<(int, int)> _lastTorpedoPosition = null;

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
        public HeadPositionReducer(GameProps gameProps, int[,] map, BinaryTrack initialFilterTrack)
        {
            _map = map;
            _gameProps = gameProps;
            _mapFilter = BinaryTrack.FromCartesian(gameProps, _map);
            _filter = initialFilterTrack;
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

        public void Handle(EnemyAttacked enemyAttacked)
        {
            _lastTriggeredMinePosition = enemyAttacked.TriggeredMinePosition;
            _lastTorpedoPosition = enemyAttacked.TorpedoTargetPosition;
        }
        public void Handle(EnemyLifeChanged enemyLifeChanged)
        {
            var lifeLost = enemyLifeChanged.PreviousLife - enemyLifeChanged.CurrentLife ;
            var lifeLostExceptSurface = enemyLifeChanged.HasSurfaced() ? lifeLost - 1 : lifeLost;
            bool bothTorpedoAndMineWereTriggered = _lastTorpedoPosition != null && _lastTriggeredMinePosition != null;
            if (lifeLostExceptSurface > 1 && !bothTorpedoAndMineWereTriggered)
            {
                //either torpedo or mine have exactly hit the enemy
                var lastHitPositions = new List<(int,int)>();
                if (_lastTorpedoPosition.HasValue)
                {
                    lastHitPositions.Add(_lastTorpedoPosition.Value);
                }

                if (_lastTriggeredMinePosition.HasValue)
                {
                    lastHitPositions.Add(_lastTriggeredMinePosition.Value);
                }

                _filter = _filter.BinaryOr(BinaryTrack.FromAllOneExcept(_gameProps, lastHitPositions));
            }

            if (lifeLostExceptSurface == 1 || bothTorpedoAndMineWereTriggered)
            {
                var newArea = BinaryTrack.FromAllOneExcept(_gameProps, new List<(int, int)>());
                if (_lastTorpedoPosition.HasValue)
                {
                    newArea = newArea.BinaryAnd(BinaryTrack.FromAllOneExcept(_gameProps,
                        _lastTorpedoPosition.Value.FindNeighbouringCells(_gameProps, _map)));
                }

                if (_lastTriggeredMinePosition.HasValue)
                {
                    newArea = newArea.BinaryAnd(BinaryTrack.FromAllOneExcept(_gameProps,
                        _lastTriggeredMinePosition.Value.FindNeighbouringCells(_gameProps, _map)));
                }

                _filter = _filter
                    .BinaryOr(newArea)
                    .BinaryOr(_mapFilter);
            }
        }
    }
}