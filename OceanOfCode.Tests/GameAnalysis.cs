using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    /*
     *
     *
     *
     * MoveProps Input: x:8, y:9, TorpedoCooldown:0, OpponentOrders:MOVE S
Torpedo skipped. Too many candidates. 
Possible enemy positions are:(0,1), (1,1), (2,1), (3,1), (4,1), (5,1), (6,1), (13,1), (0,2), (1,2), (2,2), (3,2), (4,2), (5,2), (6,2), (13,2), (0,3), (1,3), (2,3), (3,3), (4,3), (5,3), (6,3), (7,3), (8,3), (9,3), (10,3), (11,3), (12,3), (13,3), (0,4), (4,4), (5,4), (6,4), (7,4), (8,4), (9,4), (10,4), (11,4), (12,4), (13,4), (0,5), (4,5), (5,5), (6,5), (7,5), (8,5), (9,5), (10,5), (11,5), (12,5), (13,5), (0,6), (4,6), (5,6), (6,6), (7,6), (8,6), (9,6), (10,6), (11,6), (12,6), (13,6), (4,7), (5,7), (6,7), (7,7), (8,7), (9,7), (10,7), (11,7), (12,7), (13,7), (5,8), (6,8), (7,8), (8,8), (9,8), (10,8), (11,8), (12,8), (13,8), (5,9), (6,9), (7,9), (8,9), (9,9), (10,9), (11,9), (12,9), (13,9), (5,10), (6,10), (7,10), (8,10), (9,10), (10,10), (11,10), (12,10), (13,10), (0,11), (1,11), (2,11), (3,11), (4,11), (5,11), (6,11), (7,11), (8,11), (9,11), (10,11), (11,11), (12,11), (13,11), (0,12), (1,12), (2,12), (3,12), (4,12), (5,12), (6,12), (7,12), (8,12), (9,12), (10,12), (11,12), (12,12), (13,12), (0,13), (1,13), (2,13), (3,13), (4,13), (5,13), (6,13), (7,13), (8,13), (9,13), (10,13), (11,13), (12,13), (13,13), (0,14), (1,14), (2,14), (3,14), (4,14), (5,14), (6,14), (7,14), (8,14), (9,14), (10,14), (11,14), (12,14), (13,14), 
EnemyTracker state: 
binaryMap:Head: None, data= new short[]{108,108,0,0,6144,6144,0,14336,15360,15360,0,0,0,0,0,} 
opponentTrack:Head:(0, 1), data= new short[]{24576,24576,0,0,0,0,0,0,0,0,0,0,0,0,0,} 
Send Actions: MOVE S TORPEDO
Standard Output Stream:
MOVE S TORPEDO
008
364
Standard Output Stream:
TORPEDO 10 7|MOVE S TORPEDO
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
                {108,108,0,0,6144,6144,0,14336,15360,15360,0,0,0,0,0,} ; 
            BinaryTrack binaryMap = BinaryTrack.FromDebug(_gameProps, mapData, null);

            var currentData = new short[] {24576,24576,0,0,0,0,0,0,0,0,0,0,0,0,0,} ;
            BinaryTrack currentTrack = BinaryTrack.FromDebug(_gameProps, currentData, (0,1));
            
//            var exactData = new short[] {4032,4032,8176,7216,7216,7216,4159,4159,4351,5119,4735,4735,5107,4595,8179,};
//            BinaryTrack exactTrack = BinaryTrack.FromDebug(_gameProps, exactData, (3,0));
//            
            _sut = EnemyTracker.FromDebug(_gameProps, binaryMap, currentTrack, null, _console, new HeadPositionReducer(_gameProps, binaryMap.ToCartesian()));
            
            _sut.Next(new MoveProps{OpponentOrders = "TORPEDO 10 7|MOVE S TORPEDO"});
            
        }
    }
}