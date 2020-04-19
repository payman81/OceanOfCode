using System.Linq;
using OceanOfCode.Attack;
using OceanOfCode.Surveillance;

namespace OceanOfCode
{
    public class GameController
    {
        private readonly IConsole _console;
        private INavigator _navigator;
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
            _navigator = new PreComputedSpiralNavigator(_mapScanner, _console, reversedModeOn:true, _gameProps);
            var headPositionReducer = new HeadPositionReducer(_gameProps, _mapScanner);
            _enemyTracker = new EnemyTracker(_gameProps, _mapScanner.GetMapOrScan(), console, headPositionReducer);
            var torpedoController = new AttackController(_gameProps, _enemyTracker, _mapScanner, _console, headPositionReducer, _navigator);
            
            var chargeController = new ChargeController(_enemyTracker);
            _submarine = new Submarine(_navigator, _enemyTracker, _console, torpedoController, chargeController);
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
}