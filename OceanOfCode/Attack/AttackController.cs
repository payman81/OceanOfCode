using System;
using System.Collections.Generic;
using System.Linq;
using OceanOfCode.Surveillance;
// ReSharper disable ConvertNullableToShortForm

namespace OceanOfCode.Attack
{
    public class AttackController
    {
        private readonly GameProps _gameProps;
        private readonly IEnemyTracker _enemyTracker;
        private readonly HeadPositionReducer _headPositionReducer;
        private readonly INavigator _navigator;
        private readonly IConsole _console;
        private int[,] _map;

        private BinaryTrack _possibleEnemyHeadsMap;
        private Dictionary<(int,int), BinaryTrack> _mineMaps = new Dictionary<(int, int), BinaryTrack>();
        private Dictionary<(int, int),(int, int)> _torpedoTarget = new Dictionary<(int, int), (int, int)>();
        private (int, int)? _triggerTarget;
        private char _mineDirection;


        public AttackController(GameProps gameProps, IEnemyTracker enemyTracker, MapScanner mapScanner,
            IConsole console, HeadPositionReducer headPositionReducer, INavigator navigator)
        {
            _gameProps = gameProps;
            _enemyTracker = enemyTracker;
            _console = console;
            _headPositionReducer = headPositionReducer;
            _navigator = navigator;
            _map = mapScanner.GetMapOrScan();
        }
        
        //Debug only
        public AttackController(GameProps gameProps, IEnemyTracker enemyTracker, BinaryTrack mapTrack,  Dictionary<(int,int), BinaryTrack> mineMaps,
            IConsole console, HeadPositionReducer headPositionReducer)
        {
            _gameProps = gameProps;
            _enemyTracker = enemyTracker;
            _console = console;
            _headPositionReducer = headPositionReducer;
            _map = mapTrack.ToCartesian();
            _mineMaps = mineMaps;
        }

        public void NextStart(MoveProps moveProps, NavigationResult currentPosition)
        {
            _possibleEnemyHeadsMap = BinaryTrack.FromAllZeroExcept(_gameProps, _enemyTracker.PossibleEnemyPositions(), null);
            
            _torpedoTarget = ComputeTorpedoTarget(moveProps, currentPosition);
            _triggerTarget = null;
            _mineDirection = Direction.None;
        }

        public void NextEnd()
        {
            _headPositionReducer.Handle(new EnemyAttacked
            {
                TriggeredMinePosition = _triggerTarget,
                TorpedoTargetPosition = _torpedoTarget.Select(x => x.Value).FirstOrDefault()
            });
        }

        public bool TryFireTorpedo(NavigationResult next, out (int, int)? target)
        {
            if (!_torpedoTarget.ContainsKey(next.Position))
            {
                target = null;
                return false;
            }

            target = _torpedoTarget[next.Position];
            return _torpedoTarget.ContainsKey(next.Position);
        }

        public bool TryDropMine(MoveProps moveProps, NavigationResult next,out char dropMineDirection)
        {
            if (_mineDirection != Direction.None)
            {
                dropMineDirection = Direction.None;
                return false;
            }
            _mineDirection = CalculateMineDirection(moveProps, next );

            dropMineDirection = _mineDirection;
            return _mineDirection != Direction.None;
        }

        public bool TryTriggerMine(MoveProps moveProps, NavigationResult next, out (int, int)? target)
        {
            if (_triggerTarget.HasValue)
            {
                target = null;
                return false;
            }
            _triggerTarget = CalculateTriggerTarget(moveProps, next );
            if (_triggerTarget.HasValue)
            {
                _mineMaps.Remove(_triggerTarget.Value);
            }
            target = _triggerTarget;
            return _triggerTarget.HasValue;
        }


        private Dictionary<(int, int),(int,int)> ComputeTorpedoTarget(MoveProps moveProps, NavigationResult currentNavigationResult)
        {
            Dictionary<(int, int),(int,int)> target = new Dictionary<(int, int), (int, int)>();
            
            if (moveProps.TorpedoCooldown != 0)
            {
                _console.Debug("Torpedo not charged. Skipped.");
                var positionsDebug = _enemyTracker.PossibleEnemyPositions();
                Log(positionsDebug);
                return target;
            }
            var positions = _enemyTracker.PossibleEnemyPositions();
            if (positions.Count == 0)
            {
                _console.Debug("Torpedo not fired as there is no possible enemy location");
                Log(positions);
                return target;
            }

            var myNextPositions = GetMyNextPositions(moveProps, currentNavigationResult);
            var torpedoTargetsPair = FindAllTorpedoTargetsForAllMyNextPositions(myNextPositions);

            //1- Do I have exact target?
            if (positions.Count == 1)
            {
                //1.1 - Can I hit the exact position?
                foreach (var torpedoTargetPair in torpedoTargetsPair)
                {
                    if (_possibleEnemyHeadsMap.HasCollisionWith(torpedoTargetPair.Value.torpedoTargetMap))
                    {
                        //I can hit the exact position
                        _console.Debug($"Torpedo aimed at exact position at {torpedoTargetPair.Key}. My position when firing torpedo is {torpedoTargetPair.Value.myNextPositions}");
                        Log(positions);
                        target[torpedoTargetPair.Value.myNextPositions] = torpedoTargetPair.Key;
                        return target;
                    }
                }
                
                //1.2 - Can I hit any neighbours of the exact position?
                foreach (var torpedoTargetPair in torpedoTargetsPair)
                {
                    if (_possibleEnemyHeadsMap.HasCollisionWith(torpedoTargetPair.Value.torpedoRangeMap))
                    {
                        //I can hit exact position's neighbour
                        _console.Debug($"Torpedo aimed at neighbour of exact position at {torpedoTargetPair.Key}. My position when firing torpedo is {torpedoTargetPair.Value.myNextPositions}");
                        Log(positions);
                        target[torpedoTargetPair.Value.myNextPositions] = torpedoTargetPair.Key;
                        return target;
                    }
                }
            }
                
            //2- Do I have more than one enemy positions
            if (positions.Count > 1)
            {
                int maxCollidingCount = 0;
                //2.1 Find a hit position with maximum shared cells with possible enemy positions
                TorpedoPositionGroup bestMatch = null;
                foreach (var torpedoTargetPair in torpedoTargetsPair)
                {
                    var collision = _possibleEnemyHeadsMap.CalculateCollision(torpedoTargetPair.Value.torpedoRangeMap);
                    if (collision.CollidingCount > maxCollidingCount)
                    {
                        maxCollidingCount = collision.CollidingCount;
                        bestMatch = new TorpedoPositionGroup
                        {
                            MyPosition = torpedoTargetPair.Value.myNextPositions,
                            TorpedoPosition = torpedoTargetPair.Key,
                            TorpedoTargetMap = torpedoTargetPair.Value.torpedoTargetMap,
                            TorpedoRangeMap = torpedoTargetPair.Value.torpedoRangeMap
                        };
                    }
                }

                if (bestMatch != null)
                {
                    _console.Debug($"Torpedo aimed at possible positions at {bestMatch.TorpedoPosition}. My position when firing torpedo is {bestMatch.MyPosition} and total enemy possible positions are {positions.Count}");
                    Log(positions);
                    target[bestMatch.MyPosition] = bestMatch.TorpedoPosition;
                }
            }
            
            return target;
        }

        private Dictionary<(int, int), ((int, int) myNextPositions, BinaryTrack torpedoTargetMap, BinaryTrack torpedoRangeMap)> FindAllTorpedoTargetsForAllMyNextPositions(List<NavigationResult> myNextPositions)
        {
            Dictionary<(int, int), ((int, int) myNextPositions, BinaryTrack torpedoTargetMap, BinaryTrack torpedoRangeMap)> data
                = new Dictionary<(int, int), ((int, int) myNextPositions, BinaryTrack torpedoTargetMap, BinaryTrack
                    torpedoRangeMap)>();
            foreach (var myNextPosition in myNextPositions)
            {
                var torpedoTargets = myNextPosition.Position.CalculateTorpedoRangeWithBinaryTracks(_gameProps, _map);
                foreach (var torpedoTarget in torpedoTargets)
                {
                    if (data.ContainsKey(torpedoTarget.Key))
                    {
                        continue;
                    }

                    data[torpedoTarget.Key] = (myNextPosition.Position, torpedoTarget.Value.torpedoTargetMap,
                        torpedoTarget.Value.torpedoRangeMap);
                }
            }

            return data;
        }

        private List<NavigationResult> GetMyNextPositions(MoveProps moveProps, NavigationResult currentNavigationResult)
        {
            var myNextPositions = new List<NavigationResult>();
            myNextPositions.Add(currentNavigationResult);
            var next = _navigator.Next(currentNavigationResult.Position);
            if (next != null && moveProps.SilenceCooldown == 0)
            {
                myNextPositions.Add(next);
                next = _navigator.Next(next.Position);
            }

            if (next != null)
            {
                myNextPositions.Add(next);
            }

            return myNextPositions;
        }
        
        private char CalculateMineDirection(MoveProps moveProps, NavigationResult next)
        {
            bool shouldDropMine = 
                next != null && 
                moveProps.MineCooldown == 0 && 
                !_mineMaps.ContainsKey(next.Position);
            //todo: Do not drop mine too close to an exisiting mine
            
            var dropMineDirection = shouldDropMine ? next.Direction : Direction.None;
            if (shouldDropMine)
            {
                var mineArea = next.Position.FindNeighbouringCells(_gameProps, _map);
                var minePosition = next.Position.FindPositionWhenIMove(next.Direction);
                mineArea.Add(minePosition);
                _console.Debug($"Dropping mine at position {minePosition}");
                _mineMaps.Add(minePosition, BinaryTrack.FromAllZeroExcept(_gameProps, mineArea, null));
            }
            return dropMineDirection;
        }
        private (int, int)? CalculateTriggerTarget(MoveProps moveProps, NavigationResult lastNavigationResult)
        {
            var collisionResults = new List<(CollisionResult, (int, int))>();
            var myNeighbourCells = moveProps.MyPosition.FindNeighbouringCells(_gameProps, _map);
            myNeighbourCells.Add(moveProps.MyPosition);
            
            foreach (var mineMap in _mineMaps.Where(x => myNeighbourCells.All(n => !n.Equals(x.Key))))
            {
                var collisionResult = _possibleEnemyHeadsMap.CalculateCollision(mineMap.Value);
                collisionResults.Add((collisionResult, mineMap.Key));
            }
            
            //Find any guaranteed aim
            var guaranteedAttackMine = collisionResults
                .Where(x => x.Item1.CollidingCount > 0)
                .Where(x => x.Item1.NotCollidingCount == 0).ToList();
            if (guaranteedAttackMine.Any())
            {
                _console.Debug($"Trigger guaranteed mine at {guaranteedAttackMine.First().Item2}");
                return guaranteedAttackMine.First().Item2;
            }

            //Couldn't find guaranteed mine. Now try to find one that will reduce the possible head positions
            var degreeOfCertainty = 20; //the higher this is the more frequent (more inaccurate) we trigger mines
            var mineWithHighestChanceOfLimitingHeadPositions =
                collisionResults
                    .OrderByDescending(x => x.Item1.CollidingCount)
                    .Where(x => x.Item1.CollidingCount > 0)
                    .Where(x => x.Item1.NotCollidingCount < degreeOfCertainty).ToList();
            if(mineWithHighestChanceOfLimitingHeadPositions.Any())
            {
                _console.Debug($"Trigger mine to reduce head positions by {mineWithHighestChanceOfLimitingHeadPositions.First().Item1.CollidingCount} at {mineWithHighestChanceOfLimitingHeadPositions.First().Item2}");
                return mineWithHighestChanceOfLimitingHeadPositions.First().Item2;
            }

            return null;
        }
        private void Log(List<(int, int)> positions)
        {
            if (positions.Count == 1)
            {
                _console.Debug($"Enemy exact location is {positions.First()}");
            }

            if (positions.Count > 1)
            {
                var debugMessage = "Possible enemy positions are:";
                foreach (var p in positions)
                {
                    debugMessage = $"{debugMessage}({p.Item1},{p.Item2}), ";
                }
                _console.Debug(debugMessage);
            }
            _console.Debug("EnemyTracker state: " +  _enemyTracker.Debug());
        }
        
        class TorpedoPositionGroup
        {
            public (int,int) TorpedoPosition { get; set; }
            public (int, int) MyPosition { get; set; }
            public BinaryTrack TorpedoTargetMap { get; set; }
            public BinaryTrack TorpedoRangeMap { get; set; }
        }

    }
}