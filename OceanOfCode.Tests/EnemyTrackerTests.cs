using System;
using System.Linq;
using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class EnemyTrackerTests
    {
        private ConsoleMock _console;

        [SetUp]
        public void Setup()
        {
            _console = new ConsoleMock();
        }
        
        [Test]
        public void FetchPossibleMatches()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            _console.Record(".............xx");
            _console.Record(".............xx");
            _console.Record("......xx.......");
            _console.Record("......xx.......");
            var mapScanner = new MapScanner(gameProps, _console);

            var shape = new[]
            {
                "xxxxxxxxxxX....",
                "x..............",
                "x..............",
                "..............."
            };

            BinaryTrack trackBinary = BinaryTrack.FromString(gameProps, shape);
            EnemyTracker et = new EnemyTracker(gameProps, mapScanner.GetMapOrScan());
            var possibleMatches = et.PossibleMatches(trackBinary).ToList();

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
        public void FirstPossibleShape()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            _console.Record(".............xx");
            _console.Record(".............xx");
            _console.Record("......xx.......");
            _console.Record("......xx.......");
            var mapScanner = new MapScanner(gameProps, _console);
            
            EnemyTracker et = new EnemyTracker(gameProps, mapScanner.GetMapOrScan());
            
            et.OnMove(Direction.East);

            BinaryTrack result = et.FirstPossibleTrack();
            Assert.AreEqual((1, 0), result.Head);
            
            var cartesianMap = result.ToCartesian();
            Assert.AreEqual(1, cartesianMap[0,0]);
            Assert.AreEqual(1, cartesianMap[1,0]);
        }
    }
}