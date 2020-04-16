using System.Linq;
using Moq;
using NUnit.Framework;
using OceanOfCode.Attack;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class AttackControllerTests
    {
        public class TorpedoAttackTests
        {
            private ConsoleMock _console;
            private GameProps _gameProps;
            private MapScanner _mapScanner;
            private HeadPositionReducer _headPositionReducer;
            private Mock<INavigator> _navigator;

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
                _headPositionReducer = new HeadPositionReducer(_gameProps, _mapScanner);
                _navigator = new Mock<INavigator>();
            }

            [Test]
            public void CalculateTorpedoRangeNotHittingMyself_NoIslands()
            {
                var positionsInRange = (0, 0).CalculateTorpedoRangeNotHittingMyself(_gameProps, _mapScanner.GetMapOrScan());

                CollectionAssert.AreEquivalent(new[]
                {
                    (2, 0), (3, 0), (4, 0),
                    (2, 1), (3, 1),
                    (0, 2), (1, 2), (2, 2),
                    (0, 3), (1, 3)
                }, positionsInRange);
            }

            [Test]
            public void CalculateTorpedoRangeNotHittingMyself_MustAvoidIslands()
            {
                var positionsInRange = (8, 2).CalculateTorpedoRangeNotHittingMyself(_gameProps, _mapScanner.GetMapOrScan());

                CollectionAssert.DoesNotContain(positionsInRange, (6, 2));
                CollectionAssert.DoesNotContain(positionsInRange, (6, 3));
                CollectionAssert.DoesNotContain(positionsInRange, (7, 2));
                CollectionAssert.DoesNotContain(positionsInRange, (7, 3));
            }

            [Test, Ignore("Must change range calculation to pass this test")]
            public void CalculateCorrectRange_MustNotReturnPositionsIfJumpedOverIslands()
            {
                var positionsInRange = (8, 2).CalculateTorpedoRange(_gameProps, _mapScanner.GetMapOrScan());
                CollectionAssert.DoesNotContain(positionsInRange, (5, 2));
            }

            [Test]
            public void FireTorpedo_ExactEnemyLocationKnown_EnemyLocationWithinReach_ShouldFireTorpedo()
            {
                var myPosition = (0, 0);
                var enemyPossibleLocations = new[] {(3, 1)};
                var enemyTracker = new Mock<IEnemyTracker>();
                AttackController sut = new AttackController(_gameProps, enemyTracker.Object, _mapScanner, _console, _headPositionReducer, _navigator.Object);
                enemyTracker.Setup(x => x.PossibleEnemyPositions()).Returns(enemyPossibleLocations.ToList());
                sut.NextStart(new MoveProps{TorpedoCooldown = 0}, new NavigationResult{Position = myPosition});
                
                sut.TryFireTorpedo(new NavigationResult{Position = myPosition}, out var target);
                sut.NextEnd();
                
                Assert.AreEqual((3, 1), target);
            }

            [Test]
            public void
                FireTorpedo_ExactEnemyLocationKnown_OnlyCellNextToEnemyLocationIsWithinReach_ShouldFireTorpedoToNeighbouringCell()
            {
                var myPosition = (0, 0);
                var enemyPossibleLocations = new[] {(5, 0)};
                var enemyTracker = new Mock<IEnemyTracker>();
                AttackController sut = new AttackController(_gameProps, enemyTracker.Object, _mapScanner, _console, _headPositionReducer, _navigator.Object);
                enemyTracker.Setup(x => x.PossibleEnemyPositions()).Returns(enemyPossibleLocations.ToList());
                sut.NextStart(new MoveProps{TorpedoCooldown = 0}, new NavigationResult{Position = myPosition});
                    
                
                sut.TryFireTorpedo(new NavigationResult{Position = myPosition}, out var target);
                sut.NextEnd();
                
                Assert.AreEqual((4, 0), target);
            }

            [Test]
            public void FireTorpedo_NoExactEnemyLocationKnown_FireTorpedo()
            {
                var myPosition = (0, 0);
                var enemyPossibleLocations = new[] {(4, 0), (5, 0)};
                var enemyTracker = new Mock<IEnemyTracker>();
                AttackController sut = new AttackController(_gameProps, enemyTracker.Object, _mapScanner, _console, _headPositionReducer, _navigator.Object);
                enemyTracker.Setup(x => x.PossibleEnemyPositions()).Returns(enemyPossibleLocations.ToList());
                sut.NextStart(new MoveProps{TorpedoCooldown = 0}, new NavigationResult{Position = myPosition});
                
                sut.TryFireTorpedo(new NavigationResult{Position = myPosition}, out var target);
                sut.NextEnd();
                
                Assert.IsNotNull(target);
            }
        }
    }
}