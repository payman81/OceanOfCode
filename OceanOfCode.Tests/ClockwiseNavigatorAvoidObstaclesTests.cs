using System.Linq;
using NUnit.Framework;

namespace OceanOfCode.Tests
{
    public class ClockwiseNavigatorAvoidObstaclesTests
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
        public void MustRemainWithinMapBoundary_TopRightCorner()
        {
            _console.Record("4 2 0");
            
            _console.Record("....");
            _console.Record("....");
            
            _navigateHelper.ConsoleRecordMove(3, 0);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.True(_console.RecordedActions.Last().Contains("MOVE S"));
        }
        
        [Test]
        public void MustRemainWithinMapBoundary_BottomRightCorner()
        {
            _console.Record("4 2 0");
            
            _console.Record("....");
            _console.Record("....");
            
            _navigateHelper.ConsoleRecordMove(3, 1);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.True(_console.RecordedActions.Last().Contains("MOVE W"));
        }
        
        [Test]
        public void MustRemainWithinMapBoundary_BottomLeftCorner()
        {
            _console.Record("4 2 0");
            
            _console.Record("....");
            _console.Record("....");    
            
            _navigateHelper.ConsoleRecordMove(0, 1);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.True(_console.RecordedActions.Last().Contains("MOVE E"));
        }
        
        [Test]
        public void MustRemainWithinMapBoundary_TopLeftCorner()
        {
            _console.Record("4 2 0");
            
            _console.Record("....");
            _console.Record("....");
            
            _navigateHelper.ConsoleRecordMove(0, 0);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.True(_console.RecordedActions.Last().Contains("MOVE E"));
        }
    }
}