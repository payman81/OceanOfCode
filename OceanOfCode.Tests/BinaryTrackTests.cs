using System;
using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class BinaryTrackTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanRepresentAllEmptyCells()
        {
            var gameProps = new GameProps {Width = 4, Height = 4, MyId = 0};

            string[] shape = {
                "....",
                "....",
                "....",
                "...."
            };
            
            BinaryTrack sut = BinaryTrack.FromString(gameProps, shape);
            
            int[,] map = sut.ToCartesian();
            MapAssert.AllCoordinatesAreZero(map);
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
            
            MapAssert.AllCoordinatesAreOne(map);
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
            
            MapAssert.AllCoordinatesAreZeroExcept(map, (0,0), (3,0), (1,1), (2,1));
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

            BinaryTrack sut = BinaryTrack.FromString(gameProps, shape);
            sut.TryShiftEast(out var output);
            Console.WriteLine(output);

            var map = output.ToCartesian();
            MapAssert.AllCoordinatesAreZeroExcept(map, (1,1), (2,0));
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

        [Test]
        public void Move_Initial()
        {
            var gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};
            
            BinaryTrack sut = BinaryTrack.StartEmptyTrack(gameProps);
            
            Assert.AreEqual((0,0), sut.Head);
            var map = sut.ToCartesian();
            Assert.AreEqual(1, map[0,0]);
            MapAssert.AllCoordinatesAreZeroExcept(map, (0, 0));
        }
    }
}