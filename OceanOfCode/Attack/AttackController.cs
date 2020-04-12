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
        private readonly IConsole _console;
        private int[,] _map;

        private BinaryTrack _possibleEnemyHeadsMap;
        private Dictionary<(int,int), BinaryTrack> _mineMaps = new Dictionary<(int, int), BinaryTrack>();
        private (int, int)? _torpedoTarget;
        private (int, int)? _triggerTarget;
        private char _mineDirection;
        private NavigationResult _lastNavigationResult;


        public AttackController(GameProps gameProps, IEnemyTracker enemyTracker, MapScanner mapScanner,
            IConsole console, HeadPositionReducer headPositionReducer)
        {
            _gameProps = gameProps;
            _enemyTracker = enemyTracker;
            _console = console;
            _headPositionReducer = headPositionReducer;
            _map = mapScanner.GetMapOrScan();
        }

        public void Next(MoveProps moveProps, NavigationResult next)
        {
            _possibleEnemyHeadsMap = BinaryTrack.FromAllZeroExcept(_gameProps, _enemyTracker.PossibleEnemyPositions(), null);

            _torpedoTarget = CalculateTorpedoTarget(moveProps, next ?? _lastNavigationResult);
            _mineDirection = CalculateMineDirection(moveProps, next ?? _lastNavigationResult);
            
            
            _triggerTarget = CalculateTriggerTarget(moveProps, next ?? _lastNavigationResult);
            if (_triggerTarget.HasValue)
            {
                _mineMaps.Remove(_triggerTarget.Value);
                _headPositionReducer.Handle(new MineTriggered{Position = _triggerTarget.Value});
                
            }
            
            if (next != null)
            {
                _lastNavigationResult = next;
            }
        }

        public bool TryFireTorpedo(out (int, int)? target)
        {
            target = _torpedoTarget;
            return _torpedoTarget.HasValue;
        }

        public bool TryDropMine(out char dropMineDirection)
        {
            dropMineDirection = _mineDirection;
            return _mineDirection != Direction.None;
        }

        public bool TryTriggerMine(out (int, int)? target)
        {
            target = _triggerTarget;
            return _triggerTarget.HasValue;
        }

        private (int,int)? CalculateTorpedoTarget(MoveProps moveProps, NavigationResult navigationResult)
        {
            Nullable<(int, int)> target;
            if (moveProps.TorpedoCooldown != 0)
            {
                _console.Debug("Torpedo not charged. Skipped.");
                var positionsDebug = _enemyTracker.PossibleEnemyPositions();
                Log(positionsDebug);
                return null;
            }

            var positions = _enemyTracker.PossibleEnemyPositions();
            if (positions.Count > 1)
            {
                string debugMessage = "Torpedo skipped. Too many candidates. ";
                
                _console.Debug(debugMessage);
                Log(positions);
                return null;
            }

            if (positions.Count == 0)
            {
                _console.Debug("Torpedo not fired as there is no possible enemy location");
                Log(positions);
                return null;
            }
            
            var positionsWithinRange = navigationResult.Position.CalculateTorpedoRange(_gameProps, _map);

            var commonPositions = positions.Intersect(positionsWithinRange).ToList();
            if (!commonPositions.Any())
            {
                var neighbouringCells = positions.First().FindNeighbouringCells(_gameProps);
                commonPositions = neighbouringCells.Intersect(positionsWithinRange).ToList();
                if (commonPositions.Any())
                {
                    target = commonPositions.First();
                    return target;
                }
                _console.Debug($"Torpedo not fired as the opponent isn't within range");
                Log(positions);
                return null;
            }

            target = commonPositions.First();
            return target;
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
                _mineMaps.Add(next.Position, BinaryTrack.FromAllZeroExcept(_gameProps, next.Position.FindNeighbouringCells(_gameProps), null));
            }
            return dropMineDirection;
        }
        private (int, int)? CalculateTriggerTarget(MoveProps moveProps, NavigationResult lastNavigationResult)
        {
            var collisionResults = new List<(CollisionResult, (int, int))>();
            var myNeighbourCells = moveProps.MyPosition.FindNeighbouringCells(_gameProps);
            myNeighbourCells.Add(moveProps.MyPosition);
            
            foreach (var mineMap in _mineMaps.Where(x => myNeighbourCells.All(n => !n.Equals(x.Key))))
            {
                var collisionResult = _possibleEnemyHeadsMap.CalculateCollision(mineMap.Value);
                collisionResults.Add((collisionResult, mineMap.Key));
            }
            
            //Find any guaranteed aim
            var guaranteedAttackMine = collisionResults.Where(x => x.Item1.NotCollidingCount == 0).ToList();
            if (guaranteedAttackMine.Any())
            {
                _console.Debug($"Trigger guaranteed mine at {guaranteedAttackMine.First().Item2}");
                return guaranteedAttackMine.First().Item2;
            }

            var mineWithHighestChanceOfLimitingHeadPositions =
                collisionResults.OrderByDescending(x => x.Item1.CollidingCount).Where(x => x.Item1.CollidingCount > 0).ToList();
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
    }
}