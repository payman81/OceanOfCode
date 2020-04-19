using System.Collections.Generic;
using OceanOfCode.Attack;
using OceanOfCode.Surveillance;

namespace OceanOfCode
{
    class Submarine
    {
        readonly List<string> _actions = new List<string>();
        private INavigator _navigator;
        private readonly IEnemyTracker _enemyTracker;
        private readonly IConsole _console;
        private readonly AttackController _attackController;
        private readonly ChargeController _chargeController;
        private NavigationResult _lastNavigationResult;

        public Submarine(INavigator navigator, IEnemyTracker enemyTracker, IConsole console,
            AttackController attackController, ChargeController chargeController )
        {
            _navigator = navigator;
            _enemyTracker = enemyTracker;
            _console = console;
            _attackController = attackController;
            _chargeController = chargeController;
        }

        public void Start()
        {
            var firstPosition = _navigator.First();
            Start(firstPosition);
            ExecuteActions();

        }
        public void Next(MoveProps moveProps)
        {
            _enemyTracker.Next(moveProps);
            _attackController.NextStart(moveProps, _lastNavigationResult);


            if (_lastNavigationResult != null)
            {
                //Before we move
                if (_attackController.TryFireTorpedo(_lastNavigationResult, out var torpedoTarget1))
                {
                    Torpedo(torpedoTarget1.Value);
                }
            }
            
            var next = _navigator.Next(moveProps.MyPosition);
            if (next != null && moveProps.SilenceCooldown == 0)
            {
                Silence(next);
                
                //After silence
                if (_attackController.TryFireTorpedo(next, out var torpedoTarget2))
                {
                    Torpedo(torpedoTarget2.Value);
                }

                next = _navigator.Next(next.Position);
            }
            
            if (_attackController.TryTriggerMine(moveProps, next, out var position))
            {
                TriggerMine(position.Value);
            }

            if (next == null)
            {
                Surface();
            }
            else
            {
                Move(next.Direction, moveProps);
                if (_attackController.TryFireTorpedo(next, out var torpedoTarget))
                {
                    Torpedo(torpedoTarget.Value);
                }
            
                if(_attackController.TryDropMine(moveProps, next, out var dropMineDirection))
                {
                    DropMine(dropMineDirection);
                }
            }

            ExecuteActions();
            _lastNavigationResult = next;
            _attackController.NextEnd();
        }

        private void Start((int, int) startPosition)
        {
            var (x, y) = startPosition;
            _actions.Add($"{x} {y}");
        }

        private void Move(char direction, MoveProps moveProps)
        {
            _actions.Add($"MOVE {direction} {_chargeController.NextPowerToCharge(moveProps)}");
        }

        private void Torpedo((int, int) coordinate)
        {
            var (x, y) = coordinate;
            _actions.Add($"TORPEDO {x} {y}");
        }

        private void Silence(NavigationResult next)
        {
            _actions.Add($"SILENCE {next.Direction} 1");
        }

        private void Surface()
        {
            Reset();
            _actions.Add("SURFACE");
        }
        
        private void DropMine(in char direction)
        {
            _actions.Add($"MINE {direction}");
        }

        private void TriggerMine((int, int) position)
        {
            var (x, y) = position;
            _actions.Add($"TRIGGER {x} {y}");
        }

        private void Reset()
        {
            _actions.Clear();
            _navigator.Reset();
        }

        private void ExecuteActions()
        {
            string actions = string.Join("|", _actions);
            _console.Debug($"Send Actions: {actions}");
            _console.WriteLine(actions); 
            _actions.Clear();
        }
    }
}