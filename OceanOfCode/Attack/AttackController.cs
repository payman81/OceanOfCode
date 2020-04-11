using System.Collections.Generic;
using System.Linq;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Attack
{
    public class AttackController
    {
        private readonly GameProps _gameProps;
        private readonly IEnemyTracker _enemyTracker;
        private readonly IConsole _console;
        private int[,] _map;
        private List<BinaryTrack> _possibleEnemyTracks;

        public AttackController(GameProps gameProps, IEnemyTracker enemyTracker, MapScanner mapScanner,
            IConsole console)
        {
            _gameProps = gameProps;
            _enemyTracker = enemyTracker;
            _console = console;
            _map = mapScanner.GetMapOrScan();
        }

        public bool TryFireTorpedo(MoveProps moveProps, (int, int) myNextPosition, out (int, int)? target)
        {
            target = null;
            if (moveProps.TorpedoCooldown != 0)
            {
                _console.Debug("Torpedo not charged. Skipped.");
                var positionsDebug = _enemyTracker.PossibleEnemyPositions();
                Log(positionsDebug);
                return false;
            }

            var positions = _enemyTracker.PossibleEnemyPositions();
            if (positions.Count > 1)
            {
                string debugMessage = "Torpedo skipped. Too many candidates. ";
                
                _console.Debug(debugMessage);
                Log(positions);
                return false;
            }

            if (positions.Count == 0)
            {
                _console.Debug("Torpedo not fired as there is no possible enemy location");
                Log(positions);
                return false;
            }
            
            var positionsWithinRange = myNextPosition.CalculateTorpedoRange(_gameProps, _map);

            var commonPositions = positions.Intersect(positionsWithinRange).ToList();
            if (!commonPositions.Any())
            {
                var neighbouringCells = positions.First().FindNeighbouringCells(_gameProps);
                commonPositions = neighbouringCells.Intersect(positionsWithinRange).ToList();
                if (commonPositions.Any())
                {
                    target = commonPositions.First();
                    return true;
                }
                _console.Debug($"Torpedo not fired as the opponent isn't within range");
                Log(positions);
                return false;
            }
            

            target = commonPositions.First();
            return true;
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

        public bool TryDropMine(MoveProps moveProps, NavigationResult next, out char dropMineDirection)
        {
            bool shouldDropMine = next != null && moveProps.MineCooldown == 0;
            dropMineDirection = shouldDropMine ? next.Direction : Direction.None;
            return shouldDropMine;
        }

        public bool TryTriggerMine(out (int, int) valueTuple)
        {
            valueTuple = (0, 0);
            return false;
        }
    }
}