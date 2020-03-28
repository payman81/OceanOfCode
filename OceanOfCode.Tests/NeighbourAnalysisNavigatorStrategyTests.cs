using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace OceanOfCode.Tests
{
    public class NeighbourAnalysisNavigatorStrategyTests
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
        public void MustCalculateCorrectWeight()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record("..x.");
            _console.Record("....");
            _console.Record("....");
            var map = _navigateHelper.ScanMap(gameProps.Width, gameProps.Height);

            NeighbourAnalysisNavigatorStrategy sut = new NeighbourAnalysisNavigatorStrategy(map, gameProps);
            
            Assert.AreEqual(2,sut.WeightedMap[0, 0].Weight);
            Assert.AreEqual(3,sut.WeightedMap[1, 0].Weight);
            Assert.AreEqual(2,sut.WeightedMap[2, 0].Weight);
            Assert.AreEqual(2,sut.WeightedMap[3, 0].Weight);
            Assert.AreEqual(3,sut.WeightedMap[1, 1].Weight);
            Assert.AreEqual(4,sut.WeightedMap[1, 2].Weight);
        }

        [Test]
        public void CanUpdateNeighboursWeight()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record("..x.");
            _console.Record("....");
            _console.Record("....");
            var map = _navigateHelper.ScanMap(gameProps.Width, gameProps.Height);

            NeighbourAnalysisNavigatorStrategy sut = new NeighbourAnalysisNavigatorStrategy(map, gameProps);

            sut.Next((1, 0));
            Assert.AreEqual(1,sut.WeightedMap[0, 0].Weight);
            Assert.AreEqual(1,sut.WeightedMap[2, 0].Weight);
            Assert.AreEqual(2,sut.WeightedMap[1, 1].Weight);
            Assert.AreEqual(3,sut.WeightedMap[1, 0].Weight);
        }

        [Test]
        public void MustAvoidDeadEnd_MovingEast()
        {
            _console.Record("4 4 0");
            
            _console.Record("....");
            _console.Record("...x");
            _console.Record("....");
            _console.Record("....");
            
            _navigateHelper.ConsoleRecordMove(2, 0);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.False(_console.RecordedActions.Last().Contains("MOVE E"));
        }
        
        [Test]
        public void MustAvoidDeadEnd_MovingSouth()
        {
            _console.Record("4 4 0");
            
            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            _console.Record("..x.");
            
            _navigateHelper.ConsoleRecordMove(3, 2);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.False(_console.RecordedActions.Last().Contains("MOVE S"));
        }
        
        [Test]
        public void MustAvoidDeadEnd_MovingWest()
        {
            _console.Record("4 4 0");
            
            _console.Record("....");
            _console.Record("....");
            _console.Record("x...");
            _console.Record("..xx");
            
            _navigateHelper.ConsoleRecordMove(1, 3);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.False(_console.RecordedActions.Last().Contains("MOVE W"));
        }
        
        [Test]
        public void MustRestoreWeightsAfterReset()
        {
            var gameProps = new GameProps{Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record("..x.");
            _console.Record("....");
            _console.Record("....");
            var map = _navigateHelper.ScanMap(gameProps.Width, gameProps.Height);

            NeighbourAnalysisNavigatorStrategy sut = new NeighbourAnalysisNavigatorStrategy(map, gameProps);

            sut.Next((1, 0));
            sut.Reset();
            Assert.AreEqual(2,sut.WeightedMap[0, 0].Weight);
            Assert.AreEqual(3,sut.WeightedMap[1, 0].Weight);
            Assert.AreEqual(2,sut.WeightedMap[2, 0].Weight);
            Assert.AreEqual(2,sut.WeightedMap[3, 0].Weight);
            Assert.AreEqual(3,sut.WeightedMap[1, 1].Weight);
            Assert.AreEqual(4,sut.WeightedMap[1, 2].Weight);
        }
    }

}