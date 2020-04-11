

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OceanOfCode.Surveillance;

namespace OceanOfCode
{
    public static class MapExtensions
    {
        public static int[,] CloneMap(this int[,] source)
        {
            int dimension1Length = source.GetLength(0);
            int dimension2Length = source.GetLength(1);
            int[,] clone = new int[dimension1Length, dimension2Length];
            Array.Copy(source, clone, dimension1Length * dimension2Length);
            return clone;
        }

        public static int AvailableCellCount(this int[,] source)
        {
            int availableCellCount = 0;
            for (int j = 0; j < source.GetLength(1); j++)
            {
                for (int i = 0; i < source.GetLength(0); i++)
                {
                    if (source[i, j] == 0)
                    {
                        availableCellCount++;
                    }
                }
            }

            return availableCellCount;
        }
        
        public static (int, int) FindPositionWhenIMove(this (int, int) currentPosition, char direction)
        {
            var (x, y) = currentPosition;
            switch (direction)
            {
                case Direction.East:
                    return (x + 1, y);
                case Direction.South:
                    return (x, y + 1);
                case Direction.West:
                    return (x - 1, y);
                case Direction.North:
                    return (x, y - 1);
            }

            throw new Exception("Incorrect direction given");
        }

        public static char ToOpposite(this char direction)
        {
            switch (direction)
            {
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                case Direction.North:
                    return Direction.South;
                default:
                    return default;
            }
        }
        
        public static List<(int,int)> FindNeighbouringCells(this (int, int) position, GameProps gameProps)
        {
            var neighbours = new Dictionary<(int,int),(int,int)>();
            var (x, y) = position;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    SafeAddPositionsInRange(neighbours, (x + i, y + j), gameProps);
                }
            }

            return neighbours.Select(p => p.Value).ToList();
        }

        public static List<(int, int)> CalculateTorpedoRange(this (int, int) myPosition, GameProps gameProps, int[,] map)
        {
            var (x, y) = myPosition;
            var positionsInRange = new Dictionary<(int,int),(int,int)>();
            int i,j;
            for (int max = 4; max > 0; max--)
            {
                for (i = 0; i <= max; i++)
                {
                    j = max - i;
                    SafeAddPositionsInRange(positionsInRange, (x + i, y + j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x + i, y - j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x - i, y + j), gameProps);
                    SafeAddPositionsInRange(positionsInRange, (x - i, y - j), gameProps);
                }
            }

            return positionsInRange.Values.Where(positions => map[positions.Item1, positions.Item2] == 0).ToList();
        }

        private static void SafeAddPositionsInRange(Dictionary<(int, int), (int, int)> positionsInRange, (int, int) position, GameProps gameProps)
        {
            var (x1, y1) = position;
            if (x1 >= 0 && x1 < gameProps.Width && y1 >= 0 && y1 < gameProps.Height)
            {
                positionsInRange[position] = position;
            }
        }
        
    }

    public class MoveProps
    {
        public (int, int) MyPosition { get; set; }
        public int TorpedoCooldown { get; set; }
        public string OpponentOrders { get; set; }
        public int MyLife { get; set; }
        public int OpponentLife { get; set; }
        public int SilenceCooldown { get; set; }
        public int MineCooldown { get; set; }

        public override string ToString()
        {
            return $"MoveProps Input: x:{MyPosition.Item1}, y:{MyPosition.Item2}, TorpedoCooldown:{TorpedoCooldown}, OpponentOrders:{OpponentOrders}";
        }
    }

    public class GameProps
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MyId { get; set; }
        public override string ToString()
        {
            return $"GameProps Input: Width:{Width}, Height:{Height}, MyId:{MyId}";
        }
    }

    
    
    public class TorpedoController
    {
        private readonly GameProps _gameProps;
        private readonly IEnemyTracker _enemyTracker;
        private readonly IConsole _console;
        private int[,] _map;

        public TorpedoController(GameProps gameProps, IEnemyTracker enemyTracker, MapScanner mapScanner,
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
                var positionsDebug = _enemyTracker.PossibleEnemyPositions().ToList();
                Log(positionsDebug);
                return false;
            }
            var positions = _enemyTracker.PossibleEnemyPositions().ToList();
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
    }
    class Submarine
    {
        readonly List<string> _actions = new List<string>();
        private INavigator _navigator;
        private readonly IEnemyTracker _enemyTracker;
        private readonly IConsole _console;
        private readonly TorpedoController _torpedoController;
        private readonly ChargeController _chargeController;

        public Submarine(INavigator navigator, IEnemyTracker enemyTracker, IConsole console,
            TorpedoController torpedoController, ChargeController chargeController )
        {
            _navigator = navigator;
            _enemyTracker = enemyTracker;
            _console = console;
            _torpedoController = torpedoController;
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
            
            var next = _navigator.Next(moveProps.MyPosition);
            if (next != null && moveProps.SilenceCooldown == 0)
            {
                Silence(next);
                next = _navigator.Next(next.Position);
            }
            
            if (_torpedoController.TryFireTorpedo(moveProps, next?.Position ?? moveProps.MyPosition, out var torpedoTarget))
            {
                Torpedo(torpedoTarget.Value);
            }
            
            if (next != null && moveProps.MineCooldown == 0)
            {
                Mine(next.Direction);
            }

            if (next == null)
            {
                Surface();
            }
            else
            {
                Move(next.Direction, moveProps);
                
            }

            ExecuteActions();
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
        
        private void Mine(in char direction)
        {
            _actions.Add($"MINE {direction}");
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

    public class ChargeController
    {
        private readonly EnemyTracker _enemyTracker;
        private int _moveCounter;
        List<(int, string)> _charges = new  List<(int, string)>();

        public ChargeController(EnemyTracker enemyTracker)
        {
            _enemyTracker = enemyTracker;
        }

        class Charge
        {
            public static string Torpedo = "TORPEDO";
            public static string Silence = "SILENCE";
            public static string Mine = "MINE";
            public static string Sonar = "SONAR";
            
        }
        public string NextPowerToCharge(MoveProps move)
        {
            _moveCounter++;
            string chosenCharge;
            
            if (move.TorpedoCooldown > 0 && _moveCounter <= 3)
            {
                chosenCharge = Charge.Torpedo;
            }else if (_enemyTracker.DoWeHaveExactEnemyLocation())
            {
                if (move.TorpedoCooldown > 0)
                {
                    chosenCharge = Charge.Torpedo;
                }else if (move.SilenceCooldown > 0 && _charges.Take(6).Select(x => x.Item2).Any(c => !c.Equals(Charge.Silence)))
                {
                    chosenCharge = Charge.Silence;
                }else if (move.MineCooldown > 0)
                {
                    chosenCharge = Charge.Mine;
                }
                else
                {
                    chosenCharge = Charge.Silence;
                }

            }else if (move.SilenceCooldown > 0 && _charges.Take(6).Select(x => x.Item2).Any(c => !c.Equals(Charge.Silence)))
            {
                chosenCharge = Charge.Silence;
            }else if (move.MineCooldown > 0)
            {
                chosenCharge = Charge.Mine;
            }
            else
            {
                chosenCharge = Charge.Torpedo;
            }

            _charges.Add((_moveCounter, chosenCharge));
            return chosenCharge;
        }
    }

    public class Direction
    {
        public const char North = 'N';
        public const char South = 'S';
        public const char West = 'W';
        public const char East = 'E';
        public const char None = default;
    }

    public class NavigationResult
    {
        public char Direction { get; set; }
        public (int,int) Position { get; set; }
    }
    public interface INavigator
    {
        NavigationResult Next((int, int) currentPosition);
        void Reset();
        (int, int) First();
    }

    public interface IConsole
    {
        string ReadLine();
        void WriteLine(object output);
        void Debug(object obj);
    }

    public class GameController
    {
        private readonly IConsole _console;
        private INavigator _moveStrategy;
        private EnemyTracker _enemyTracker;
        private Submarine _submarine;
        private GameProps _gameProps;
        private MapScanner _mapScanner;

        public GameController(IConsole console)
        {
            _console = console;
            string[] inputs;
            inputs = _console.ReadLine().Split(' ');
            _gameProps = new GameProps
            {
                Width = int.Parse(inputs[0]),
                Height = int.Parse(inputs[1]),
                MyId = int.Parse(inputs[2])
            };
            _console.Debug(_gameProps);
            _mapScanner = new MapScanner(_gameProps, _console);
            _moveStrategy = new PreComputedSpiralNavigator(_mapScanner, _console, reversedModeOn:true, _gameProps);
            var headPositionReducer = new HeadPositionReducer(_gameProps, _mapScanner);
            _enemyTracker = new EnemyTracker(_gameProps, _mapScanner.GetMapOrScan(), console, headPositionReducer);
            var torpedoController = new TorpedoController(_gameProps, _enemyTracker, _mapScanner, _console);
            
            var chargeController = new ChargeController(_enemyTracker);
            _submarine = new Submarine(_moveStrategy, _enemyTracker, _console, torpedoController, chargeController);
            _submarine.Start();
        }
        
        public void StartLoop()
        {
            while (true)
            {
                var inputs = _console.ReadLine().Split(' ');
                if (ExitCommandRequested(inputs))
                {
                    break;
                }
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int myLife = int.Parse(inputs[2]);
                int oppLife = int.Parse(inputs[3]);
                int torpedoCooldown = int.Parse(inputs[4]);
                int sonarCooldown = int.Parse(inputs[5]);
                int silenceCooldown = int.Parse(inputs[6]);
                int mineCooldown = int.Parse(inputs[7]);
                string sonarResult = _console.ReadLine();
                string opponentOrders = _console.ReadLine();

                var moveProps = new MoveProps
                {
                    MyPosition = (x, y), 
                    TorpedoCooldown = torpedoCooldown,
                    OpponentOrders = opponentOrders,
                    MyLife = myLife,
                    OpponentLife = oppLife,
                    SilenceCooldown = silenceCooldown,
                    MineCooldown = mineCooldown
                };
                _console.Debug(moveProps);
                _submarine.Next(moveProps);
            }
        }

        private bool ExitCommandRequested(string[] inputs)
        {
            bool exitRequested = inputs.Any(input => input.Equals("exit"));
            if (exitRequested)
            {
                _console.Debug("Exit input detected. Quitting.");
            }

            return exitRequested;
        }
    }

    public class OceanOfCode
    {
        static void Main(string[] args)
        {
            var controller = new GameController(RealConsole.Instance);
            controller.StartLoop();
        }
        sealed class RealConsole : IConsole
        {
            private static readonly Lazy<RealConsole> Lazy = new Lazy<RealConsole>(() => new RealConsole());

            public static RealConsole Instance => Lazy.Value;

            private RealConsole()
            {
            
            }

            public string ReadLine()
            {
                return System.Console.ReadLine();
            }
            public void WriteLine(object output)
            {
                System.Console.WriteLine(output);
            }
            public void Debug(object obj)
            {
                System.Console.Error.WriteLine(obj);
            }
        }
    }

    public class MapScanner
    {
        private readonly GameProps _gameProps;
        private readonly IConsole _console;
        private int[,] _map;

        public MapScanner(GameProps gameProps, IConsole console)
        {
            _gameProps = gameProps;
            _console = console;
        }
        public int[,] GetMapOrScan()
        {
            if (_map != null)
            {
                return _map;
            }
            _map = new int[_gameProps.Width, _gameProps.Height];
            for (int j = 0; j < _gameProps.Height; j++)
            {
                string line = _console.ReadLine();
                char[] rowChars = line.ToCharArray();
                for (int i = 0; i < _gameProps.Width; i++)
                {
                    _map[i, j] = rowChars[i].Equals('.') ? 0 : 1;
                }
            }
            _console.Debug("Map scanned!");
            return _map;
        }
    }
}