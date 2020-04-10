using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    /*
     *
     * MoveProps Input: x:14, y:10, TorpedoCooldown:0, OpponentOrders:MOVE N
Torpedo not fired as the opponent isn't within range
Enemy exact location is (3, 0)
EnemyTracker state: 
binaryMap:Head: None, data= new short[]{28720,28720,0,25536,25551,911,3968,3968,3840,0,384,384,3084,3084,0,} 
opponentTrack:Head:(0, 0), data= new short[]{16384,16384,16384,16384,0,0,0,0,0,0,0,0,0,0,0,} 
exactTrack:Head:(3, 0), data= new short[]{4032,4032,8176,7216,7216,7216,4159,4159,4351,5119,4735,4735,5107,4595,8179,}
Send Actions: MOVE S TORPEDO
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
                {28720, 28720, 0, 25536, 25551, 911, 3968, 3968, 3840, 0, 384, 384, 3084, 3084, 0,}; 
            BinaryTrack binaryMap = BinaryTrack.FromDebug(_gameProps, mapData, null);

            var currentData = new short[] {16384, 16384, 16384, 16384, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,};
            BinaryTrack currentTrack = BinaryTrack.FromDebug(_gameProps, currentData, (0,0));
            
            var exactData = new short[] {4032,4032,8176,7216,7216,7216,4159,4159,4351,5119,4735,4735,5107,4595,8179,};
            BinaryTrack exactTrack = BinaryTrack.FromDebug(_gameProps, exactData, (3,0));
            
            _sut = EnemyTracker.FromDebug(_gameProps, binaryMap, currentTrack, exactTrack, _console, new HeadPositionReducer(_gameProps, binaryMap.ToCartesian()));
            
            _sut.OnMove(Direction.East);
            
        }
    }
}