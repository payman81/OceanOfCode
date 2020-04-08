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


        public class BinaryTrackShiftTests
        {
            
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
        }
        public class BinaryTrackMoveTests
        {
            private GameProps _gameProps;

            [SetUp]
            public void SetUp()
            {
                _gameProps = new GameProps {Width = 15, Height = 4, MyId = 0};

            }
            [Test]
            public void Move_Initial()
            {
                BinaryTrack sut = BinaryTrack.StartEmptyTrack(_gameProps);
            
                Assert.AreEqual((0,0), sut.Head);
                var map = sut.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (0, 0));
            }

            [Test]
            public void Move_East_Initial()
            {
                BinaryTrack sut = BinaryTrack.StartEmptyTrack(_gameProps);
            
                var output = sut.MoveEast();
            
                Assert.AreEqual((1,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (0, 0), (1, 0));
            }

            [Test]
            public void Move_East_Before_Edge()
            {
                var shape = new[]
                {
                    ".............X.",
                    "...............",
                    "...............",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveEast();
            
                Assert.AreEqual((14,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (13,0), (14,0));
            }
            
            
            [Test]
            public void Move_East_On_Edge()
            {
                var shape = new[]
                {
                    ".............xX",
                    ".............x.",
                    "...............",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveEast();
            
                Assert.AreEqual((14,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (12, 1),(12, 0), (13,0), (14,0));
            }
            
            [Test]
            public void Move_East_NoShiftNeeded_CanMove()
            {
                var shape = new[]
                {
                    "............X.x",
                    "............xxx",
                    "...............",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveEast();
            
                Assert.AreEqual((13,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (12, 0),(13, 0), (14,0), (12,1), (13, 1), (14, 1));
            }
            
            [Test]
            public void Move_East_ShiftNeeded()
            {
                var shape = new[]
                {
                    "............xxX",
                    "............xx.",
                    "...............",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveEast();
            
                Assert.AreEqual((14,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (11,0),(12, 0),(13, 0), (14,0), (11,1), (12, 1));
            }
            
            [Test]
            public void Move_South()
            {
                BinaryTrack sut = BinaryTrack.StartEmptyTrack(_gameProps);
            
                var output = sut.MoveSouth();
            
                Assert.AreEqual((0,1), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (0, 0), (0, 1));
            }
            
            
            [Test]
            public void Move_South_One_Before_Edge()
            {
                var shape = new[]
                {
                    "...............",
                    "............xx.",
                    ".............X.",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveSouth();
            
                Assert.AreEqual((13, 3), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (12, 1), (13, 1), (13, 2), (13, 3));
            }
            
            [Test]
            public void Move_South_On_Edge()
            {
                var shape = new[]
                {
                    "...............",
                    "............xx.",
                    ".............x.",
                    ".............X.",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveSouth();
            
                Assert.AreEqual((13, 3), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (12, 0), (13, 0), (13, 1), (13, 2), (13, 3));
            }
            [Test]
            public void Move_South_NoShiftNeeded()
            {
                var shape = new[]
                {
                    "...............",
                    ".............xx",
                    ".............Xx",
                    "..............x",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveSouth();
            
                Assert.AreEqual((13, 3), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (13, 1), (14, 1), (13, 2), (14, 2), (13,3), (14, 3));
            }
            
            [Test]
            public void Move_West_Initial()
            {
                BinaryTrack sut = BinaryTrack.StartEmptyTrack(_gameProps);
            
                var output = sut.MoveWest();
            
                Assert.AreEqual((0,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (0, 0), (1, 0));
            }
            
            [Test]
            public void Move_West()
            {
                var shape = new[]
                {
                    "............Xx.",
                    ".............x.",
                    "...............",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveWest();
            
                Assert.AreEqual((11,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (11,0), (12,0), (13, 0), (13, 1));
            }
            
            
            [Test]
            public void Move_West_NoShiftNeeded_CanMove()
            {
                var shape = new[]
                {
                    "...............",
                    "...............",
                    "x.X............",
                    "xxx............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveWest();
            
                Assert.AreEqual((1,2), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (0,2), (0,3), (1, 2), (1, 3), (2,2), (2,3));
            }
            
            [Test]
            public void Move_West_ShiftNeeded()
            {
                var shape = new[]
                {
                    ".xx............",
                    "Xxx............",
                    "...............",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveWest();
            
                Assert.AreEqual((0,1), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (2,0), (3,0), (0, 1), (1, 1), (2,1), (3,1));
            }
            
            [Test]
            public void Move_North_Initial()
            {
                BinaryTrack sut = BinaryTrack.StartEmptyTrack(_gameProps);
            
                var output = sut.MoveNorth();
            
                Assert.AreEqual((0,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (0, 0), (0, 1));
            }
            
            [Test]
            public void Move_North()
            {
                var shape = new[]
                {
                    "xx.............",
                    ".xX............",
                    "...............",
                    "...............",
                };
                BinaryTrack sut = BinaryTrack.FromString(_gameProps, shape);
            
                var output = sut.MoveNorth();
            
                Assert.AreEqual((2,0), output.Head);
                var map = output.ToCartesian();
                MapAssert.AllCoordinatesAreZeroExcept(map, (0, 0), (1, 0), (1,1),(2,1),(2,0));
            }
        }
        
    }
}