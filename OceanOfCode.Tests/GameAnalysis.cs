using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
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

            var mapData = new short[]{56,62,62,48,0,0,4032,4032,3968,0,768,768,0,1536,1536,};
            BinaryTrack binaryMap = BinaryTrack.FromDebug(_gameProps, mapData, null);

            var opponentData = new short[]{32704,18368,17344,16384,16384,16384,16384,16384,16384,16384,16384,16384,20480,28672,0,};
            BinaryTrack opponentTrack = BinaryTrack.FromDebug(_gameProps, opponentData, (2,12));
            
            _sut = EnemyTracker.FromDebug(_gameProps, binaryMap, opponentTrack, _console, new HeadPositionReducer(_gameProps));
            
            _sut.OnMove(Direction.West);
            
        }
    }
}