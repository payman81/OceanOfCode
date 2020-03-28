using NUnit.Framework;

namespace OceanOfCode.Tests
{
    public class PreComputedSpiralNavigatorTests
    {
        private ConsoleMock _console;
        private NavigateHelper _navigateHelper;

        [SetUp]
        public void Setup()
        {
            _console = new ConsoleMock();
            _navigateHelper = new NavigateHelper(_console);
            
        }
        
        [Test]
        public void MustReturnCorrectOrderOfPositions()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console);

            var firstPosition = sut.First();
            Assert.AreEqual((0,0), firstPosition);

            Assert.AreEqual('E', sut.Next((0,0)));
            Assert.AreEqual('E', sut.Next((2,0)));
            Assert.AreEqual('S', sut.Next((3,0)));
            Assert.AreEqual('S', sut.Next((3,2)));
            Assert.AreEqual('W', sut.Next((3,3)));
            Assert.AreEqual('N', sut.Next((0,3)));
            Assert.AreEqual('E', sut.Next((0,1)));
            Assert.AreEqual('S', sut.Next((2,1)));
            Assert.AreEqual('W', sut.Next((2,2)));
            Assert.IsNull( sut.Next((1,2)));
        }

        [Test]
        public void MustAvoidIslandsWhenMovingEast()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record(".xxx");
            _console.Record(".xxx");
            _console.Record("....");
            _console.Record("....");
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console);

            var firstPosition = sut.First();
            Assert.AreEqual((0,0), firstPosition);
            Assert.AreEqual('S', sut.Next((0,0)));
            Assert.AreEqual('S', sut.Next((0,1)));
            Assert.AreEqual('E', sut.Next((0,2)));
            Assert.AreEqual('E', sut.Next((1,2)));
            Assert.AreEqual('S', sut.Next((3,2)));
            Assert.AreEqual('W', sut.Next((3,3)));
            Assert.IsNull(sut.Next((0,3)));
        }
        
        [Test]
        public void MustAvoidIslandsInMiddle()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record(".xx.");
            _console.Record(".xx.");
            _console.Record("....");
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console);

            var firstPosition = sut.First();
            Assert.AreEqual((0,0), firstPosition);
            Assert.AreEqual('E', sut.Next((0,0)));
            Assert.AreEqual('S', sut.Next((3,0)));
            Assert.AreEqual('S', sut.Next((3,2)));
            Assert.AreEqual('N', sut.Next((0,3)));
            Assert.IsNull(sut.Next((0,1)));
        }

        [Test]
        public void MustReversePathAfterReset()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record(".xx.");
            _console.Record(".xx.");
            _console.Record("....");
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console);


            Assert.IsNull(sut.Next((0,1)));
            sut.Reset();
            
            Assert.AreEqual('S', sut.Next((0,1)));
            Assert.AreEqual('S', sut.Next((0,2)));
            Assert.AreEqual('E', sut.Next((0,3)));
            Assert.AreEqual('E', sut.Next((1,3)));
            Assert.AreEqual('E', sut.Next((2,3)));
            Assert.AreEqual('N', sut.Next((3,3)));
            Assert.AreEqual('N', sut.Next((3,2)));
            Assert.AreEqual('N', sut.Next((3,1)));
            Assert.AreEqual('W', sut.Next((3,0)));
            Assert.AreEqual('W', sut.Next((2,0)));
            Assert.AreEqual('W', sut.Next((1,0)));
            Assert.IsNull(sut.Next((0,0)));
            sut.Reset();
            Assert.AreEqual('E', sut.Next((0,0)));
        }
    }
}