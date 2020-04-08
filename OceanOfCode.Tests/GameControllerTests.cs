using System;
using System.Linq;
using NUnit.Framework;

namespace OceanOfCode.Tests
{
    [TestFixture]
    public class GameControllerTests
    {
        private ConsoleMock _console;
        private NavigateHelper _navigateHelper;

        [SetUp]
        public void Setup()
        {
            _console = new ConsoleMock();
            _navigateHelper = new NavigateHelper(_console);
        }
        
        [Test, Ignore("Must update")]
        public void MustRemainWithinMapBoundary_TopRightCorner()
        {
            _console.Record("15 15 0");
            
            _console.Record("....");
            _console.Record("....");
            
            _navigateHelper.ConsoleRecordMove(3, 0);
            _console.Record("exit");
            
            GameController controller = new GameController(_console);
            controller.StartLoop();
            
            Assert.True(_console.RecordedActions.Last().Contains("MOVE S"));
        }

        [Test]
        public void TestTupleToString()
        {
            Console.WriteLine((1,2).ToString());
        }
    }
}