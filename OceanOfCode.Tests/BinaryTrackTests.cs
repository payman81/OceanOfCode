using System;
using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class BinaryTrackTests
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

            BinaryTrack sut = BinaryTrack.StartEmptyTrack(gameProps);
            int[,] map = sut.ToCartesian();
            Assert.AreEqual(gameProps.Width, map.GetLength(0));
            Assert.AreEqual(gameProps.Height, map.GetLength(1));
            Assert.AreEqual(new[,] {{0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}}, map);
        }

        [Test]
        public void CanRepresentAllIslandCells()
        {
            var gameProps = new GameProps {Width = 4, Height = 4, MyId = 0};

            string[] shape = {
                "xxxx",
                "xxxx",
                "xxxx",
                "xxxx"
            };

            BinaryTrack sut = BinaryTrack.FromString(gameProps, shape);
            int[,] map = sut.ToCartesian();
            Assert.AreEqual(gameProps.Width, map.GetLength(0));
            Assert.AreEqual(gameProps.Height, map.GetLength(1));
            Assert.AreEqual(new[,] {{1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}, {1, 1, 1, 1}}, map);
        }

        [Test]
        public void CanRepresentMixedFreeAndIslandCells()
        {
            var gameProps = new GameProps {Width = 4, Height = 4, MyId = 0};

            string[] shape = {
                "x..x",
                ".xx.",
                "....",
                "...."
            };

            BinaryTrack sut = BinaryTrack.FromString(gameProps, shape);
            int[,] map = sut.ToCartesian();
            Assert.AreEqual(gameProps.Width, map.GetLength(0));
            Assert.AreEqual(gameProps.Height, map.GetLength(1));
            Assert.AreEqual(new[,] {{1, 0, 0, 0}, {0, 1, 0, 0}, {0, 1, 0, 0}, {1, 0, 0, 0}}, map);
        }

        [Test]
        public void Shift_East()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            string[] shape = {
                ".x.............",
                "x..............",
                "...............",
                "..............."
            };

            var mapScanner = new MapScanner(gameProps, _console);

            BinaryTrack sut = BinaryTrack.FromString(gameProps, shape);
            sut.TryShiftEast(out var output);
            Console.WriteLine(output);
        }

        [Test]
        public void Shift_East_edge()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            string[] shape = {
                "..............x",
                ".............x.",
                "...............",
                "..............."
            };
           
            BinaryTrack sut = BinaryTrack.FromString(gameProps, shape);
            bool canMoveRight = sut.TryShiftEast(out var output);
            Console.WriteLine(output);
            Assert.IsFalse(canMoveRight);
            Assert.IsNull(output);
        }

        [Test]
        public void Shift_South_edge()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            string[] shape = {
                "...............",
                "...............",
                ".x.............",
                ".....x........."
            };
           
            BinaryTrack sut = BinaryTrack.FromString(gameProps, shape);
            bool canMoveSouth = sut.TryShiftSouth(out var output);
            Console.WriteLine(output);
            Assert.IsFalse(canMoveSouth);
            Assert.IsNull(output);
        }
    }
}