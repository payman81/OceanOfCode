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
        public void CorrectRangeIsReturned()
        {
            var moveProps = new MoveProps{MyPosition = (9,4)};
            _sut.Handle(new TorpedoDetected{Target = (7,4), MoveProps = moveProps});
            Console.WriteLine(_sut.HeadFilter);

            string[] expected =
            {
                "xxxxxxx.xxxxxxx",
                "xxxxxx...xxxxxx",
                "xxxxx.xx..xxxxx",
                "xxxx..xx...xxxx",
                "xxx....x.x..xxx",
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
    }
}