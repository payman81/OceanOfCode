using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/* MVP:
 * - (done) check for possibilities 
 * - (done) keep track of the head
 * - transfer series of directions to BinaryMap
 * - Torpedo if charged and single possibility is within reach
 * 
 * Next:
 * watch my life to limit possible area
 * watch for opponent surface to limit possible areas
 * Use sonar to limit possible area
 * When silence is used, reset the tracker but limit the area
 */
namespace OceanOfCode.Tests
{
    public class BinaryMap
    {
        private readonly GameProps _gameProps;
        private readonly short[] _binaryMap;
        private readonly (int, int)? _head;

        public BinaryMap(GameProps gameProps, (int, int)? head)
        {
            _gameProps = gameProps;
            _binaryMap = new short[_gameProps.Height];
            _head = head;
        }

        public BinaryMap(GameProps gameProps, short[] data, (int, int)? head)
        {
            _gameProps = gameProps;
            _binaryMap = data;
            _head = head;
        }

        public (int, int)? Head => _head;

        public override string ToString()
        {
            string output = string.Empty;
            foreach (var row in _binaryMap)
            {
                output += Convert.ToString(row, 2).PadLeft(_gameProps.Width, '0') + Environment.NewLine;
            }

            return output;
        }

        public int[,] ToCartesian()
        {
            int[,] map = new int[_gameProps.Width, _gameProps.Height];
            for (int j = 0; j < _gameProps.Height; j++)
            {
                string row = Convert.ToString(_binaryMap[j], 2).PadLeft(_gameProps.Width, '0');
                for (int i = 0; i < row.Length; i++)
                {
                    map[i, j] = row[i].Equals('0') ? 0 : 1;
                }

                _binaryMap[j] = Convert.ToInt16(row, 2);
            }

            return map;
        }

        public static BinaryMap operator >>(BinaryMap bmap, int i)
        {
            (int, int)? head = null;
            if (bmap._head.HasValue)
            {
                head = (bmap._head.Value.Item1 + 1, bmap._head.Value.Item2);
            }
            var newBmap = new BinaryMap(bmap._gameProps, head);
            for (int j = 0; j < bmap._binaryMap.Length; j++)
            {
                newBmap._binaryMap[j] = (short) (bmap._binaryMap[j] >> i);
            }

            return newBmap;
        }

        public bool TryMoveEast(out BinaryMap output)
        {
            if (CanMoveEast())
            {
                output = this >> 1;
                return true;
            }

            output = null;
            return false;
        }

        public bool TryMoveSouth(out BinaryMap output)
        {
            if (CanMoveSouth())
            {
                (int, int)? head = null;
                if (_head.HasValue)
                {
                    head = (_head.Value.Item1, _head.Value.Item2 + 1);
                }
                
                short[] newBmap = new short[_binaryMap.Length];
                Array.Copy(_binaryMap, 0, newBmap, 1, _binaryMap.Length - 1);
                output = new BinaryMap(_gameProps, newBmap, head);
                return true;
            }

            output = null;
            return false;
        }

        private bool CanMoveEast()
        {
            return _binaryMap.All(row => (row & (short) 1) == 0);
        }

        private bool CanMoveSouth()
        {
            return _binaryMap[^1] == 0;
        }

        public bool HasCollisionWith(BinaryMap target)
        {
            for (int j = 0; j < _binaryMap.Length; j++)
            {
                if (_binaryMap[j] > 0 && (_binaryMap[j] & target._binaryMap[j]) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static BinaryMap FromCartesian(GameProps gameProps, int[,] cartesianMap)
        {
            short[] binaryMapData = new short[gameProps.Height];
            for (int j = 0; j < gameProps.Height; j++)
            {
                string row = string.Empty;
                for (int i = 0; i < gameProps.Width; i++)
                {
                    row += cartesianMap[i, j];
                }

                binaryMapData[j] = Convert.ToInt16(row, 2);
            }

            return new BinaryMap(gameProps, binaryMapData, null);
        }

        //For testing only
        public static BinaryMap FromString(GameProps gameProps, string[] shape)
        {
            (int, int)? head = null;
            
            short[] binaryMapData = new short[gameProps.Height];
            for (int j = 0; j < gameProps.Height; j++)
            {
                string row = string.Empty;
                int positionOfHead = shape[j].IndexOf('X');
                if (positionOfHead >= 0)
                {
                    head = (positionOfHead, j);
                }
                row = shape[j]
                    .Replace('x', '1')
                    .Replace('X', '1')
                    .Replace('.', '0');
                binaryMapData[j] = Convert.ToInt16(row, 2);
            }

            return new BinaryMap(gameProps, binaryMapData, head);
        }
    }

    public class EnemyTracker
    {
        private readonly GameProps _gameProps;
        private readonly int[,] _cartesianMap;
        private readonly BinaryMap _binaryMap;

        public EnemyTracker(GameProps gameProps, int[,] map)
        {
            _gameProps = gameProps;
            _cartesianMap = map.CloneMap();
            _binaryMap = BinaryMap.FromCartesian(gameProps, map);
        }

        public IEnumerable<BinaryMap> PossibleMatches(BinaryMap currentPossible)
        {
            BinaryMap nextPossible = currentPossible;
            do
            {
                currentPossible = nextPossible;
                do
                {
                    if (!nextPossible.HasCollisionWith(_binaryMap))
                    {
                        yield return nextPossible;
                    }
                } while (nextPossible.TryMoveEast(out nextPossible));

                nextPossible = currentPossible;
            } while (nextPossible.TryMoveSouth(out nextPossible));
        }
    }

    public class EnemyTrackerTests
    {
        private ConsoleMock _console;

        [SetUp]
        public void Setup()
        {
            _console = new ConsoleMock();
        }

        [Test]
        public void CanRepresentAllEmptyCells()
        {
            var gameProps = new GameProps {Width = 4, Height = 4, MyId = 0};

            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            _console.Record("....");
            var mapScanner = new MapScanner(gameProps, _console);

            BinaryMap sut = new BinaryMap(gameProps, null);
            int[,] map = sut.ToCartesian();
            Assert.AreEqual(gameProps.Width, map.GetLength(0));
            Assert.AreEqual(gameProps.Height, map.GetLength(1));
            Assert.AreEqual(new[,] {{0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}}, map);
        }

        [Test]
        public void CanRepresentAllIslandCells()
        {
            var gameProps = new GameProps {Width = 4, Height = 4, MyId = 0};

            _console.Record("xxxx");
            _console.Record("xxxx");
            _console.Record("xxxx");
            _console.Record("xxxx");

            var mapScanner = new MapScanner(gameProps, _console);

            BinaryMap sut = BinaryMap.FromCartesian(gameProps, mapScanner.GetMapOrScan());
            int[,] map = sut.ToCartesian();
            Assert.AreEqual(gameProps.Width, map.GetLength(0));
            Assert.AreEqual(gameProps.Height, map.GetLength(1));
            Assert.AreEqual(new[,] {{1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}}, map);
        }

        [Test]
        public void CanRepresentMixedFreeAndIslandCells()
        {
            var gameProps = new GameProps {Width = 4, Height = 4, MyId = 0};

            _console.Record("x..x");
            _console.Record(".xx.");
            _console.Record("....");
            _console.Record("....");

            var mapScanner = new MapScanner(gameProps, _console);

            BinaryMap sut = BinaryMap.FromCartesian(gameProps, mapScanner.GetMapOrScan());
            int[,] map = sut.ToCartesian();
            Assert.AreEqual(gameProps.Width, map.GetLength(0));
            Assert.AreEqual(gameProps.Height, map.GetLength(1));
            Assert.AreEqual(new[,] {{1, 0, 0, 0}, {0, 1, 0, 0}, {0, 1, 0, 0}, {1, 0, 0, 0}}, map);
        }

        [Test]
        public void Shift_East()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            _console.Record(".x.............");
            _console.Record("x..............");
            _console.Record("...............");
            _console.Record("...............");

            var mapScanner = new MapScanner(gameProps, _console);

            BinaryMap bmap = BinaryMap.FromCartesian(gameProps, mapScanner.GetMapOrScan());
            bmap.TryMoveEast(out var output);
            Console.WriteLine(output);
        }

        [Test]
        public void Shift_East_edge()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            _console.Record("..............x");
            _console.Record(".............x.");
            _console.Record("...............");
            _console.Record("...............");

            var mapScanner = new MapScanner(gameProps, _console);

            BinaryMap bmap = BinaryMap.FromCartesian(gameProps, mapScanner.GetMapOrScan());
            bool canMoveRight = bmap.TryMoveEast(out var output);
            Console.WriteLine(output);
            Assert.IsFalse(canMoveRight);
            Assert.IsNull(output);
        }

        [Test]
        public void Shift_South_edge()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            _console.Record("...............");
            _console.Record("...............");
            _console.Record(".x.............");
            _console.Record(".....x.........");

            var mapScanner = new MapScanner(gameProps, _console);

            BinaryMap bmap = BinaryMap.FromCartesian(gameProps, mapScanner.GetMapOrScan());
            bool canMoveSouth = bmap.TryMoveSouth(out var output);
            Console.WriteLine(output);
            Assert.IsFalse(canMoveSouth);
            Assert.IsNull(output);
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

            BinaryMap shapeBinary = BinaryMap.FromString(gameProps, shape);
            EnemyTracker et = new EnemyTracker(gameProps, mapScanner.GetMapOrScan());
            var possibleMatches = et.PossibleMatches(shapeBinary).ToList();

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
    }
}