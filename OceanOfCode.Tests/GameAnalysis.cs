using System.Collections.Generic;
using NUnit.Framework;
using OceanOfCode.Attack;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class GameAnalysis
    {
        private ConsoleMock _console;
        private GameProps _gameProps;
        private EnemyTracker _sut;


        [Test, Ignore("End-to-end test for debug pueposes")]
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
            var headPositionReducer = new HeadPositionReducer(_gameProps, binaryMap.ToCartesian(), filterTrack);
            _sut = EnemyTracker.FromDebug(_gameProps, binaryMap, currentTrack, null, _console, headPositionReducer, Direction.South);

            var moveProps = new MoveProps{OpponentOrders = "SILENCE|SURFACE 5", OpponentLife = 5, MyPosition = (2,11), TorpedoCooldown = 0, MineCooldown = 3, SilenceCooldown = 5};
            _sut.Next(moveProps);
            
            Dictionary<(int,int), BinaryTrack> mineMaps = new Dictionary<(int, int), BinaryTrack>();
            mineMaps.Add(
            (3,14), BinaryTrack.FromAllZeroExcept(_gameProps, (3,14).FindNeighbouringCells(_gameProps))
            );
            AttackController attackController = new AttackController(_gameProps, _sut, binaryMap, mineMaps, _console, headPositionReducer);
            attackController.NextStart(moveProps, new NavigationResult());

            var nextMoveProps = new MoveProps{OpponentOrders = "MOVE S", OpponentLife = 5, MyPosition = (2,11), TorpedoCooldown = 0, SilenceCooldown = 2, MineCooldown = 1};
            _sut.Next(nextMoveProps);
        }
    }
}