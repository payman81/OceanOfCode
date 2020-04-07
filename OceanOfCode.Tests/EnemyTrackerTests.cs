using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class EnemyTrackerTests
    {
        public class PossibleMatchesTests
        {
            private ConsoleMock _console;
            private GameProps _gameProps;
            private EnemyTracker _sut;

            [SetUp]
            public void Setup()
            {
                _console = new ConsoleMock();
                _gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

                _console.Record(".............xx");
                _console.Record(".............xx");
                _console.Record("......xx.......");
                _console.Record("......xx.......");
                var mapScanner = new MapScanner(_gameProps, _console);
                _sut = new EnemyTracker(_gameProps, mapScanner.GetMapOrScan(), _console);
            }
        
            [Test]
            public void FetchPossibleMatches_StartFromDifferentPath()
            {
                var shape = new[]
                {
                    "xxxxxxxxxxX....",
                    "x..............",
                    "x..............",
                    "..............."
                };

                BinaryTrack trackBinary = BinaryTrack.FromString(_gameProps, shape);
                var possibleMatches = _sut.PossibleTracks(trackBinary).ToList();

                Console.WriteLine($"Total number of matches: {possibleMatches.Count}");
                foreach (var possibleMatch in possibleMatches)
                {
                    Console.WriteLine();
                    Console.WriteLine(possibleMatch);
                }

                Assert.AreEqual(6, possibleMatches.Count);
                CollectionAssert.AreEquivalent(possibleMatches.Select(x => x.Head), 
                    new []{(10, 0), (11, 0), (12, 0), (10, 1), (11, 1), (12,1)});
            }
            
            [Test]
            public void FetchPossibleMatches()
            {
                _sut.OnMove(Direction.North);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.South);
                _sut.OnMove(Direction.South);
                _sut.OnMove(Direction.East);
                
                var possibleMatches = _sut.PossibleTracks().ToList();

                Console.WriteLine($"Total number of matches: {possibleMatches.Count}");
                foreach (var possibleMatch in possibleMatches)
                {
                    Console.WriteLine();
                    Console.WriteLine(possibleMatch);
                }
                
                Assert.AreEqual(14, possibleMatches.Count);
            }
        }

        public class FindingFirstTrackTests
        {
            private ConsoleMock _console;
            private EnemyTracker _sut;
            private GameProps _gameProps;

            [SetUp]
            public void Setup()
            {
                _console = new ConsoleMock();
                _gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

                _console.Record(".............xx");
                _console.Record(".............xx");
                _console.Record("......xx.......");
                _console.Record("......xx.......");
                var mapScanner = new MapScanner(_gameProps, _console);
            
                _sut = new EnemyTracker(_gameProps, mapScanner.GetMapOrScan(), _console);
            }
            
            [Test]
            public void Move_East()
            {
                _sut.OnMove(Direction.East);

                BinaryTrack result = _sut.FirstPossibleTrack();
                var map = result.ToCartesian();
            
                Assert.AreEqual((1, 0), result.Head);
                MapAssert.AllCoordinatesAreZeroExcept(map, (0,0), (1,0));
            }
            
            [Test]
            public void Move_SeveralDirection()
            {
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.South);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.North);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.East);
                _sut.OnMove(Direction.South);
                

                BinaryTrack result = _sut.FirstPossibleTrack();
                var map = result.ToCartesian();
            
                Assert.AreEqual((9,1), result.Head);
                MapAssert.AllCoordinatesAreZeroExcept(map, (0,0), (1,0), (1,1), (2,1), (2,0), (3,0), (4,0), (5,0),(6,0), (7,0), (8,0), (9,0), (9,1));
            }
        }
        
    }

    public class EnemyOrderTests
    {
        /*
         *
         * TORPEDO 5 2|MOVE E
         * TORPEDO 4 1|MOVE E
         * NA
         * SURFACE 7
         * SILENCE
         * SONAR 4
         *
         */
        [Test]
        public void Test_Move_Regex()
        {
            //Torpedo
            //Regex r = new Regex("TORPEDO ([0-9]{1,2}) ([0-9]{1,2})");
            
            //move
            Regex r = new Regex("MOVE (.?)");
            var x = r.Match("MOVE E");

            foreach (var g in x.Groups)
            {
                Console.WriteLine(g);
            }
        }

        [Test]
        public void Test_Silence_Regex()
        {
            Regex r = new Regex("SILENCE");
            var x = r.Match("SILENCE");

            foreach (Group g in x.Groups)
            {
                Console.WriteLine(g);
            }

            var match = r.Match("MOVE E");
            foreach (var g in match.Groups)
            {
                Console.WriteLine(g);
            }
        }
    }
}