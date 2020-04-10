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
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);

            var firstPosition = sut.First();
            Assert.AreEqual((0,0), firstPosition);

            Assert.AreEqual('E', sut.Next((0,0)).Direction);
            Assert.AreEqual('E', sut.Next((2,0)).Direction);
            Assert.AreEqual('S', sut.Next((3,0)).Direction);
            Assert.AreEqual('S', sut.Next((3,2)).Direction);
            Assert.AreEqual('W', sut.Next((3,3)).Direction);
            Assert.AreEqual('N', sut.Next((0,3)).Direction);
            Assert.AreEqual('E', sut.Next((0,1)).Direction);
            Assert.AreEqual('S', sut.Next((2,1)).Direction);
            Assert.AreEqual('W', sut.Next((2,2)).Direction);
            Assert.IsNull( sut.Next((1,2)));
        }
        
        [Test]
        public void MustReturnCorrectOrderOfPositions_ReversedMode()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, true, gameProps);

            var firstPosition = sut.First();
            Assert.AreEqual((1,2), firstPosition);

            Assert.AreEqual('E', sut.Next((1,2)).Direction);
            Assert.AreEqual('N', sut.Next((2,2)).Direction);
            Assert.AreEqual('W', sut.Next((2,1)).Direction);
            Assert.AreEqual('W', sut.Next((1,1)).Direction);
            Assert.AreEqual('S', sut.Next((0,1)).Direction);
            Assert.AreEqual('N', sut.Next((3,3)).Direction);
            Assert.AreEqual('W', sut.Next((3,0)).Direction);
            Assert.AreEqual('W', sut.Next((2,0)).Direction);
            Assert.AreEqual('W', sut.Next((1,0)).Direction);
            Assert.IsNull( sut.Next((0,0)));
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
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);

            var firstPosition = sut.First();
            Assert.AreEqual((0,0), firstPosition);
            Assert.AreEqual('S', sut.Next((0,0)).Direction);
            Assert.AreEqual('S', sut.Next((0,1)).Direction);
            Assert.AreEqual('E', sut.Next((0,2)).Direction);
            Assert.AreEqual('E', sut.Next((1,2)).Direction);
            Assert.AreEqual('S', sut.Next((3,2)).Direction);
            Assert.AreEqual('W', sut.Next((3,3)).Direction);
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
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);

            var firstPosition = sut.First();
            Assert.AreEqual((0,0), firstPosition);
            Assert.AreEqual('E', sut.Next((0,0)).Direction);
            Assert.AreEqual('S', sut.Next((3,0)).Direction);
            Assert.AreEqual('S', sut.Next((3,2)).Direction);
            Assert.AreEqual('N', sut.Next((0,3)).Direction);
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
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);


            Assert.IsNull(sut.Next((0,1)));
            sut.Reset();
            
            Assert.AreEqual('S', sut.Next((0,1)).Direction);
            Assert.AreEqual('S', sut.Next((0,2)).Direction);
            Assert.AreEqual('E', sut.Next((0,3)).Direction);
            Assert.AreEqual('E', sut.Next((1,3)).Direction);
            Assert.AreEqual('E', sut.Next((2,3)).Direction);
            Assert.AreEqual('N', sut.Next((3,3)).Direction);
            Assert.AreEqual('N', sut.Next((3,2)).Direction);
            Assert.AreEqual('N', sut.Next((3,1)).Direction);
            Assert.AreEqual('W', sut.Next((3,0)).Direction);
            Assert.AreEqual('W', sut.Next((2,0)).Direction);
            Assert.AreEqual('W', sut.Next((1,0)).Direction);
            Assert.IsNull(sut.Next((0,0)));
            sut.Reset();
            Assert.AreEqual('E', sut.Next((0,0)).Direction);
        }
        
        [Test]
        public void MustAvoidDeadEnd_MovingEast()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};
            
            _console.Record("....");
            _console.Record("..xx");
            _console.Record("....");
            _console.Record("....");
            
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);
            
            
            Assert.AreEqual('S', sut.Next((1,0)).Direction);
            Assert.AreEqual((1,1), sut.Next((1,0)).Position);
        }
        
        [Test]
        public void MustAvoidDeadEnd_MovingSouth()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};
            
            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            _console.Record("..x.");
            
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);
            
            
            Assert.AreEqual('W', sut.Next((3,2)).Direction);
            Assert.AreEqual((2,2), sut.Next((3,2)).Position);
        }
        
        [Test]
        public void MustAvoidDeadEnd_MovingWest()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};
            
            _console.Record("....");
            _console.Record("....");
            _console.Record("xx..");
            _console.Record("....");
            
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);
            
            
            Assert.AreEqual('W', sut.Next((3,3)).Direction);
            Assert.AreEqual((2,3), sut.Next((3,3)).Position);
            
            Assert.AreEqual('N', sut.Next((2,3)).Direction);
            Assert.AreEqual((2,2), sut.Next((2,3)).Position);
        }
        [Test]
        public void MustAvoidDeadEnd_MovingNorth()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};
            
            _console.Record("....");
            _console.Record(".x..");
            _console.Record("....");
            _console.Record("....");
            
            var mapScanner = new MapScanner(gameProps, _console);
            PreComputedSpiralNavigator sut = new PreComputedSpiralNavigator(mapScanner, _console, false, gameProps);
            
            
            Assert.AreEqual('E', sut.Next((0,2)).Direction);
            Assert.AreEqual((1,2), sut.Next((0,2)).Position);
        }
    }
}