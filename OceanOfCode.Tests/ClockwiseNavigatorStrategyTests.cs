using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace OceanOfCode.Tests
{
    public class ClockwiseNavigatorStrategyTests
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
        public void MustAvoidDeadEnd()
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

    }

}