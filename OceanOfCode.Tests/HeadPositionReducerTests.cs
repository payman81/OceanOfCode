using System;
using Moq;
using NUnit.Framework;
using OceanOfCode.Surveillance;

namespace OceanOfCode.Tests
{
    public class HeadPositionReducerTests
    {
        private ConsoleMock _console;
        private GameProps _gameProps;
        private MapScanner _mapScanner;
        private HeadPositionReducer _sut;

        [SetUp]
        public void Setup()
        {
            _console = new ConsoleMock();
            _gameProps = new GameProps {Width = 15, Height = 15, MyId = 0};

            _console.Record(".............xx");
            _console.Record(".............xx");
            _console.Record("......xx.......");
            _console.Record("......xx.......");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            _console.Record("...............");
            
            _mapScanner = new MapScanner(_gameProps, _console);
            _sut = new HeadPositionReducer(_gameProps, _mapScanner);
        }

        [Test]
        public void TorpedoDetected_CorrectRangeIsReturned()
        {
            _sut.Handle(new TorpedoDetected{Target = (7,4)});
            Console.WriteLine(_sut.HeadFilter);

            string[] expected =
            {
                "xxxxxxx.xxxxxxx",
                "xxxxxx...xxxxxx",
                "xxxxx.xx..xxxxx",
                "xxxx..xx...xxxx",
                "xxx....x....xxx",
                "xxxx.......xxxx",
                "xxxxx.....xxxxx",
                "xxxxxx...xxxxxx",
                "xxxxxxx.xxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx"
            }; 
            
            MapAssert.MatchesShape(_gameProps, _sut.HeadFilter, expected) ;
        }

        [Test]
        public void TorpedoDetected_ThenMoveEastDetected_CorrectRangeIsReturned()
        {
            _sut.Handle(new TorpedoDetected{Target = (7,4)});
            _sut.Handle(new MoveDetected{Direction = Direction.East});
            Console.WriteLine(_sut.HeadFilter);

            string[] expected =
            {
                "xxxxxxxx.xxxxxx",
                "xxxxxxx...xxxxx",
                "xxxxxxxxx..xxxx",
                "xxxxx.xxx...xxx",
                "xxxx....x....xx",
                "xxxxx.......xxx",
                "xxxxxx.....xxxx",
                "xxxxxxx...xxxxx",
                "xxxxxxxx.xxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx"
            }; 
            
            MapAssert.MatchesShape(_gameProps, _sut.HeadFilter, expected) ;
        }
        
        [Test]
        public void TorpedoDetected_ThenMoveSouthDetected_CorrectRangeIsReturned()
        {
            _sut.Handle(new TorpedoDetected{Target = (7,4)});
            _sut.Handle(new MoveDetected{Direction = Direction.South});
            Console.WriteLine(_sut.HeadFilter);

            string[] expected =
            {
                "xxxxxxxxxxxxxxx",
                "xxxxxxx.xxxxxxx",
                "xxxxxxxx.xxxxxx",
                "xxxxx.xx..xxxxx",
                "xxxx..xx...xxxx",
                "xxx....x....xxx",
                "xxxx.......xxxx",
                "xxxxx.....xxxxx",
                "xxxxxx...xxxxxx",
                "xxxxxxx.xxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx"
            }; 
            
            MapAssert.MatchesShape(_gameProps, _sut.HeadFilter, expected) ;
        }
        
        [Test]
        public void TorpedoDetected_ThenMoveWestDetected_CorrectRangeIsReturned()
        {
            _sut.Handle(new TorpedoDetected{Target = (7,4)});
            _sut.Handle(new MoveDetected{Direction = Direction.West});
            Console.WriteLine(_sut.HeadFilter);

            string[] expected =
            {
                "xxxxxx.xxxxxxxx",
                "xxxxx...xxxxxxx",
                "xxxx.xxx.xxxxxx",
                "xxx..xxx..xxxxx",
                "xx....x....xxxx",
                "xxx.......xxxxx",
                "xxxx.....xxxxxx",
                "xxxxx...xxxxxxx",
                "xxxxxx.xxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx"
            }; 
            
            MapAssert.MatchesShape(_gameProps, _sut.HeadFilter, expected) ;
        }
        [Test]
        public void TorpedoDetected_ThenMoveNorthDetected_CorrectRangeIsReturned_()
        {
            _sut.Handle(new TorpedoDetected{Target = (7,4)});
            _sut.Handle(new MoveDetected{Direction = Direction.North});
            Console.WriteLine(_sut.HeadFilter);

            string[] expected =
            {
                "xxxxxx...xxxxxx",
                "xxxxx.xx..xxxxx",
                "xxxx..xx...xxxx",
                "xxx...xx....xxx",
                "xxxx.......xxxx",
                "xxxxx.....xxxxx",
                "xxxxxx...xxxxxx",
                "xxxxxxx.xxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx"
            }; 
            
            MapAssert.MatchesShape(_gameProps, _sut.HeadFilter, expected) ;
        }

        [Test]
        public void SilenceDetected_LastMoveDirectionIsNorth_PadsTheFilterBy4InAllDirectionsExceptSouth()
        {
            _sut.Handle(new TorpedoDetected{Target = (7,4)});
            _sut.Handle(new SilenceDetected{LastMoveDirection = Direction.North});
            Console.WriteLine(_sut.HeadFilter);
            
            string[] expected =
            {
                "xxx.........xxx",
                "xx...........xx",
                "x.....xx......x",
                "......xx.......",
                "...............",
                "...............",
                "x.............x",
                "xx...........xx",
                "xxx.........xxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx",
                "xxxxxxxxxxxxxxx"
            };

            MapAssert.MatchesShape(_gameProps, _sut.HeadFilter, expected) ;
        }
        
    }
}