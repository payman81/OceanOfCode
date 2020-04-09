using Moq;
using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class TorpedoControllerTests
    {
        private ConsoleMock _console;
        private EnemyTracker _enemyTracker;
        private GameProps _gameProps;
        private MapScanner _mapScanner;

        [SetUp]
        public void Setup()
        {
            _console = new ConsoleMock();
            _gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            _console.Record(".............xx");
            _console.Record(".............xx");
            _console.Record("......xx.......");
            _console.Record("......xx.......");
            _mapScanner = new MapScanner(_gameProps, _console);
            
            var headPositionReducer = new HeadPositionReducer(_gameProps);
            _enemyTracker = new EnemyTracker(_gameProps, _mapScanner.GetMapOrScan(), _console, headPositionReducer);
        }
        
        [Test]
        public void CalculateCorrectRange_NoIslands()
        {
            var positionsInRange = (0, 0).CalculateTorpedoRange(_gameProps, _mapScanner.GetMapOrScan());
            
            CollectionAssert.AreEquivalent(new[]
            {
                (1,0), (2,0), (3,0), (4,0),
                (0,1), (1,1), (2,1), (3,1),
                (0,2), (1,2), (2,2),
                (0,3), (1,3)
            }, positionsInRange);
        }
        
        [Test]
        public void CalculateCorrectRange_MustAvoidIslands()
        {
            var positionsInRange = (8, 2).CalculateTorpedoRange(_gameProps, _mapScanner.GetMapOrScan());
            
            CollectionAssert.DoesNotContain(positionsInRange, (6,2));
            CollectionAssert.DoesNotContain(positionsInRange, (6,3));
            CollectionAssert.DoesNotContain(positionsInRange, (7,2));
            CollectionAssert.DoesNotContain(positionsInRange, (7,3));
        }
        
        [Test, Ignore("Must change range calculation to pass this test")]
        public void CalculateCorrectRange_MustNotReturnPositionsIfJumpedOverIslands()
        {

            var positionsInRange = (8, 2).CalculateTorpedoRange(_gameProps, _mapScanner.GetMapOrScan());
            CollectionAssert.DoesNotContain(positionsInRange, (5,2));
        }

        [Test]
        public void FireTorpedo_ExactEnemyLocationKnown_EnemyLocationWithinReach_ShouldFireTorpedo()
        {
            var myPosition = (0, 0);
            var enemyPossibleLocations = new[] {(3, 1)};
            
            var enemyTracker = new Mock<IEnemyTracker>();
            TorpedoController sut = new TorpedoController(_gameProps, enemyTracker.Object, _mapScanner, _console);

            enemyTracker.Setup(x => x.PossibleEnemyPositions()).Returns(enemyPossibleLocations);

            sut.TryFireTorpedo(new MoveProps {MyPosition = myPosition, TorpedoCooldown = 0}, out var target);
            Assert.AreEqual((3,1), target);
        }
        
        [Test]
        public void FireTorpedo_ExactEnemyLocationKnown_OnlyCellNextToEnemyLocationIsWithinReach_ShouldFireTorpedoToNeighbouringCell()
        {
            var myPosition = (0, 0);
            var enemyPossibleLocations = new[] {(5, 0)};
            
            var enemyTracker = new Mock<IEnemyTracker>();
            TorpedoController sut = new TorpedoController(_gameProps, enemyTracker.Object, _mapScanner, _console);

            enemyTracker.Setup(x => x.PossibleEnemyPositions()).Returns(enemyPossibleLocations);

            sut.TryFireTorpedo(new MoveProps {MyPosition = myPosition, TorpedoCooldown = 0}, out var target);
            Assert.AreEqual((4, 0), target);
        }
        
        [Test]
        public void FireTorpedo_NoExactEnemyLocationKnown_DoNotFireTorpedo()
        {
            var myPosition = (0, 0);
            var enemyPossibleLocations = new[] {(4, 0), (5,0)};
            
            var enemyTracker = new Mock<IEnemyTracker>();
            TorpedoController sut = new TorpedoController(_gameProps, enemyTracker.Object, _mapScanner, _console);

            enemyTracker.Setup(x => x.PossibleEnemyPositions()).Returns(enemyPossibleLocations);

            sut.TryFireTorpedo(new MoveProps {MyPosition = myPosition, TorpedoCooldown = 0}, out var target);
            Assert.IsNull(target);
        }
    }
}