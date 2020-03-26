/*
 * 1- Avoid dead ends by using move availability matrix
 * 2- Reset map after Surface
 * 3- EnemyTracker to scan map to find candidates matching opponent's move history
 * 4- Torpedo must avoid obstacles 
 * */

using System;
using System.Collections.Generic;
using System.Linq;

namespace OceanOfCode
{
    static class ArrayExtensions
    {
        public static int[,] CloneMap(this int[,] source)
        {
            int dimension1Length = source.GetLength(0);
            int dimension2Length = source.GetLength(1);
            int[,] clone = new int[dimension1Length, dimension2Length];
            Array.Copy(source, clone, dimension1Length * dimension2Length);
            return clone;
        }
    }

    class MoveProps
    {
        public (int, int) MyPosition { get; set; }
        public int TorpedoCooldown { get; set; }

        public override string ToString()
        {
            return $"MoveProps Input: x:{MyPosition.Item1}, y:{MyPosition.Item2}, TorpedoCooldown:{TorpedoCooldown}";
        }
    }

    class GameProps
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MyId { get; set; }
        public override string ToString()
        {
            return $"GameProps Input: Width:{Width}, Height:{Height}, MyId:{MyId}";
        }
    }

    class EnemyTracker
    {
        protected readonly int[,] Map;

        public EnemyTracker(int[,] map)
        {
            Map = map.CloneMap();
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

    class Submarine
    {
        readonly List<string> _actions = new List<string>();
        private NavigatorBase _navigator;
        private readonly EnemyTracker _enemyTracker;
        private readonly IConsole _console;

        public Submarine(NavigatorBase navigator, EnemyTracker enemyTracker, IConsole console)
        {
            _navigator = navigator;
            _enemyTracker = enemyTracker;
            _console = console;
        }

        public void Start()
        {
            var firstPosition = _navigator.First();
            Start(firstPosition);
            ExecuteActions();

        }
        public void Next(MoveProps moveProps)
        {
            if (moveProps.TorpedoCooldown == 0)
            {
                Torpedo(_enemyTracker.GuessEnemyLocation(moveProps));
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

    class Direction
    {
        public const char North = 'N';
        public const char South = 'S';
        public const char West = 'W';
        public const char East = 'E';
    }

    abstract class NavigatorBase
    {
        protected int[,] Map;
        private readonly int[,] _originalMap;
        private readonly GameProps _gameProps;

        protected NavigatorBase(int[,] map, GameProps gameProps)
        {
            _originalMap = map;
            _gameProps = gameProps;
            Map = map.CloneMap();
        }

        protected bool CanMoveEast((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (x == _gameProps.Width - 1)
            {
                return false;
            }

            return Map[x + 1, y] == 0;
        }

        protected bool CanMoveSouth((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (y == _gameProps.Height - 1)
            {
                return false;
            }

            return Map[x, y + 1] == 0;
        }

        protected bool CanMoveWest((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (x == 0)
            {
                return false;
            }

            return Map[x - 1, y] == 0;
        }

        protected bool CanMoveNorth((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            if (y == 0)
            {
                return false;
            }

            return Map[x, y - 1] == 0;
        }

        public abstract char? Next((int, int) currentPosition);

        public void Reset()
        {
            Map = _originalMap.CloneMap();
        }

        public (int, int) First()
        {
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int j = Map.GetLength(1) - 1; j >= 0; j--)
                {
                    if (Map[i, j] == 0)
                    {
                        return (i, j);
                    }
                }
            }

            throw new Exception("No first position is available!");
        }
    }

    class ClockwiseNavigatorStrategy : NavigatorBase
    {
        public ClockwiseNavigatorStrategy(int[,] map, GameProps gameProps) : base(map, gameProps)
        {
        }


        public override char? Next((int, int) currentPosition)
        {
            var (x, y) = currentPosition;
            Map[x, y] = 1;

            if (CanMoveEast(currentPosition))
            {
                return Direction.East;
            }

            if (CanMoveSouth(currentPosition))
            {
                return Direction.South;
            }

            if (CanMoveWest(currentPosition))
            {
                return Direction.West;
            }

            if (CanMoveNorth(currentPosition))
            {
                return Direction.North;
            }

            return null;
        }
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
        private ClockwiseNavigatorStrategy _moveStrategy;
        private EnemyTracker _enemyTracker;
        private Submarine _submarine;
        private GameProps _gameProps;
        private int[,] _map;

        public GameController(IConsole console)
        {
            _console = console;
            Init();
        }

        private void Init()
        {
            _map = null;
            string[] inputs;
            inputs = _console.ReadLine().Split(' ');
            _gameProps = new GameProps
            {
                Width = int.Parse(inputs[0]),
                Height = int.Parse(inputs[1]),
                MyId = int.Parse(inputs[2])
            };
            _console.Debug(_gameProps);
         
            _map = ScanMap();
            _console.Debug("Map scanned!");
            _moveStrategy = new ClockwiseNavigatorStrategy(_map, _gameProps);
            _enemyTracker = new EnemyTracker(_map);
            _submarine = new Submarine(_moveStrategy, _enemyTracker, _console);
            _submarine.Start();
        }

        private int[,] ScanMap()
        {
            int[,] map = new int[_gameProps.Width, _gameProps.Height];
            for (int j = 0; j < _gameProps.Height; j++)
            {
                string line = _console.ReadLine();
                char[] rowChars = line.ToCharArray();
                for (int i = 0; i < _gameProps.Width; i++)
                {
                    map[i, j] = rowChars[i].Equals('.') ? 0 : 1;
                }
            }

            return map;
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

                var moveProps = new MoveProps {MyPosition = (x, y), TorpedoCooldown = torpedoCooldown};
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
}