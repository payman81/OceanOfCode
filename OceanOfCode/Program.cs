

using System;
using System.Collections.Generic;
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

    class EnemyTracker_OldStrategy
    {
        protected readonly int[,] Map;

        public EnemyTracker_OldStrategy(MapScanner mapScanner)
        {
            Map = mapScanner.GetMapOrScan().CloneMap();
        }

        public (int, int) GuessEnemyLocation(MoveProps props)
        {
            var (x, y) = props.MyPosition;

            if (y >= 4)
            {
                return (x, y - 4);
            }

            return (x, y + 4);
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

        public bool TryFireTorpedo(MoveProps moveProps, out (int, int)? target)
        {
            target = null;
            if (moveProps.TorpedoCooldown != 0)
            {
                _console.Debug("Torpedo not charged. Skipped.");
                return false;
            }
            var positions = _enemyTracker.PossibleEnemyPositions().ToList();
            if (positions.Count > 1)
            {
                string debugMessage = "Torpedo skipped. Too many candidates: ";
                foreach (var p in positions)
                {
                    debugMessage = $"{debugMessage}({p.Item1},{p.Item2}), ";
                }
                _console.Debug(debugMessage);
                return false;
            }

            if (positions.Count == 0)
            {
                _console.Debug($"Torpedo not fired as there is no possible enemy location {_enemyTracker.Debug()}");
                return false;
            }
            
            var positionsWithinRange = moveProps.MyPosition.CalculateTorpedoRange(_gameProps, _map);

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
                _console.Debug($"Torpedo not fired as the opponent isn't within range. Enemy position is ({positions.First()}). Debug:{_enemyTracker.Debug()}");
                return false;
            }
            

            target = commonPositions.First();
            return true;
        }

        
    }
    class Submarine
    {
        readonly List<string> _actions = new List<string>();
        private INavigator _navigator;
        private readonly IEnemyTracker _enemyTracker;
        private readonly IConsole _console;
        private readonly TorpedoController _torpedoController;

        public Submarine(INavigator navigator, IEnemyTracker enemyTracker, IConsole console,
            TorpedoController torpedoController)
        {
            _navigator = navigator;
            _enemyTracker = enemyTracker;
            _console = console;
            _torpedoController = torpedoController;
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

            if (_torpedoController.TryFireTorpedo(moveProps, out var torpedoTarget))
            {
                Torpedo(torpedoTarget.Value);
            }

            var (x, y) = moveProps.MyPosition;
            var next = _navigator.Next((x, y));
            if (next == null)
            {
                Surface();
            }
            else
            {
                Move(next.Value);
            }

            ExecuteActions();
        }

        private void Start((int, int) startPosition)
        {
            var (x, y) = startPosition;
            _actions.Add($"{x} {y}");
        }

        private void Move(char direction)
        {
            _actions.Add($"MOVE {direction} TORPEDO");
        }

        private void Torpedo((int, int) coordinate)
        {
            var (x, y) = coordinate;
            _actions.Add($"TORPEDO {x} {y}");
        }

        private void Surface()
        {
            Reset();
            _actions.Add("SURFACE");
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

    public class Direction
    {
        public const char North = 'N';
        public const char South = 'S';
        public const char West = 'W';
        public const char East = 'E';
        public const char None = default;
    }

    public interface INavigator
    {
        char? Next((int, int) currentPosition);
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
            var headPositionReducer = new HeadPositionReducer(_gameProps);
            _enemyTracker = new EnemyTracker(_gameProps, _mapScanner.GetMapOrScan(), console, headPositionReducer);
            var torpedoController = new TorpedoController(_gameProps, _enemyTracker, _mapScanner, _console);
            _submarine = new Submarine(_moveStrategy, _enemyTracker, _console, torpedoController);
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
                    OpponentOrders = opponentOrders
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