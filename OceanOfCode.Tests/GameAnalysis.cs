using System.Collections.Generic;
using NUnit.Framework;
using OceanOfCode.Attack;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    /*
     *
     MoveProps Input: x:1, y:11, TorpedoCooldown:0, OpponentOrders:SILENCE|SURFACE 5
Opponent silence detected. Resetting enemy's starting position
Opponent surface detected. Resetting enemy's track keeping the head
Torpedo skipped. Too many candidates. 
Possible enemy positions are:(5,5), (6,5), (7,5), (8,5), (9,5), (5,6), (6,6), (7,6), (8,6), (9,6), (5,7), (6,7), (7,7), (8,7), (9,7), (5,8), (6,8), (7,8), (8,8), (9,8), (5,9), (8,9), 
EnemyTracker state: 

binaryMap:Head: None, data= new short[]{0,3867,3867,7680,7168,7168,30720,30720,28672,25008,25008,1,1,24576,24576,} 
opponentTrack:Head:(0, 0), data= new short[]{16384,0,0,0,0,0,0,0,0,0,0,0,0,0,0,} 
HEadFilter:Head: None, data= new short[]{32767,32767,32767,32767,32767,31775,31775,31775,31775,32191,32767,32767,32767,32767,32767,}
lastMoveDirection: S

Send Actions: MOVE E SILENCE
     */
    public class GameAnalysis
    {
        private ConsoleMock _console;
        private GameProps _gameProps;
        private EnemyTracker _sut;


        [Test]
        public void Setup()
        {
            _console = new ConsoleMock();
            _gameProps = new GameProps {Width = 15, Height = 15, MyId = 0};

            var mapData = new short[]
                {0,3867,3867,7680,7168,7168,30720,30720,28672,25008,25008,1,1,24576,24576,}  ; 
            BinaryTrack binaryMap = BinaryTrack.FromDebug(_gameProps, mapData, null);

            var currentData = new short[] {16384,0,0,0,0,0,0,0,0,0,0,0,0,0,0,} ;
            BinaryTrack currentTrack = BinaryTrack.FromDebug(_gameProps, currentData, (0,0));

            var filterData = new short[]
                {32767,32767,32767,32767,32767,31775,31775,31775,31775,32191,32767,32767,32767,32767,32767,};
            BinaryTrack filterTrack = BinaryTrack.FromDebug(_gameProps, filterData, null);
//            var exactData = new short[] {4032,4032,8176,7216,7216,7216,4159,4159,4351,5119,4735,4735,5107,4595,8179,};
//            BinaryTrack exactTrack = BinaryTrack.FromDebug(_gameProps, exactData, (3,0));
//            
            var headPositionReducer = new HeadPositionReducer(_gameProps, binaryMap.ToCartesian(), filterTrack);
            _sut = EnemyTracker.FromDebug(_gameProps, binaryMap, currentTrack, null, _console, headPositionReducer, Direction.South);

            var moveProps = new MoveProps{OpponentOrders = "SILENCE|SURFACE 5", OpponentLife = 5, MyPosition = (2,11), TorpedoCooldown = 0, MineCooldown = 3, SilenceCooldown = 5};
            _sut.Next(moveProps);
            
            Dictionary<(int,int), BinaryTrack> mineMaps = new Dictionary<(int, int), BinaryTrack>();
            mineMaps.Add(
            (3,14), BinaryTrack.FromAllZeroExcept(_gameProps, (3,14).FindNeighbouringCells(_gameProps))
            );
            AttackController attackController = new AttackController(_gameProps, _sut, binaryMap, mineMaps, _console, headPositionReducer);
            attackController.Next(moveProps, new NavigationResult{Direction = Direction.East, Position = (2,11)});

            var nextMoveProps = new MoveProps{OpponentOrders = "MOVE S", OpponentLife = 5, MyPosition = (2,11), TorpedoCooldown = 0, SilenceCooldown = 2, MineCooldown = 1};
            _sut.Next(nextMoveProps);
            attackController.Next(nextMoveProps, new NavigationResult{Direction = Direction.North, Position = (2,10)});

            
        }
    }
}