using System;
using System.Linq;
using System.Text;
// ReSharper disable ConvertNullableToShortForm

namespace OceanOfCode.Surveillance
{
    public class BinaryTrack
    {
        private readonly GameProps _gameProps;
        private readonly short[] _binaryMap;
        private readonly (int, int)? _head;

        private BinaryTrack(GameProps gameProps, (int, int)? head)
        {
            _gameProps = gameProps;
            _binaryMap = new short[_gameProps.Height];
            _head = head;
        }

        private BinaryTrack(GameProps gameProps, short[] data, (int, int)? head)
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

        public static BinaryTrack operator >>(BinaryTrack bmap, int i)
        {
            Nullable<(int,int)> head = null;
            if (bmap._head.HasValue)
            {
                head = (bmap._head.Value.Item1 + 1, bmap._head.Value.Item2);
            }

            var newBmap = new BinaryTrack(bmap._gameProps, head);
            for (int j = 0; j < bmap._binaryMap.Length; j++)
            {
                newBmap._binaryMap[j] = (short) (bmap._binaryMap[j] >> i);
            }

            return newBmap;
        }

        public static BinaryTrack operator <<(BinaryTrack bmap, int i)
        {
            Nullable<(int,int)> head = null;
            if (bmap._head.HasValue)
            {
                head = (bmap._head.Value.Item1 - 1, bmap._head.Value.Item2);
            }

            var newBmap = new BinaryTrack(bmap._gameProps, head);
            for (int j = 0; j < bmap._binaryMap.Length; j++)
            {
                newBmap._binaryMap[j] = (short) (bmap._binaryMap[j] << i);
            }

            return newBmap;
        }
        
        public bool HasCollisionWith(BinaryTrack target)
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

        public static BinaryTrack StartEmptyTrack(GameProps gameProps)
        {
            short[] binaryMap = new short[gameProps.Height];
            //Find starting position
            binaryMap[0] = Convert.ToInt16("100000000000000", 2);

            return new BinaryTrack(gameProps, binaryMap, (0, 0));
        }

        public BinaryTrack MoveEast()
        {
            BinaryTrack baseline = this;
            if (!CanMoveEast(baseline))
            {
                baseline = this << 1;
            }

            var (currentHeadX, currentHeadY) = baseline.Head.Value;
            var newBinaryMap = new short[baseline._binaryMap.Length];
            Array.Copy(baseline._binaryMap, newBinaryMap, baseline._binaryMap.Length);
            checked
            {
                var newHead = (X: currentHeadX + 1, Y: currentHeadY);
                newBinaryMap[newHead.Y] =
                    (short) (baseline._binaryMap[newHead.Y] | (short) Math.Pow(2, _gameProps.Width - 1 - newHead.X));
                return new BinaryTrack(_gameProps, newBinaryMap, newHead);
            }
        }
        private bool CanMoveEast(BinaryTrack baseline)
        {
            if (!baseline.Head.HasValue)
            {
                return false;
            }

            var (baselineX, baselineY) = baseline.Head.Value;
            if (baselineX >= _gameProps.Width - 1)
            {
                return false;
            }
            int cellToTheEast = (short) Math.Pow(2, _gameProps.Width - baselineX - 2);
            return (baseline._binaryMap[baselineY] & cellToTheEast) == 0;
        }

        public BinaryTrack MoveSouth()
        {
            BinaryTrack baseline = this;
            if (!CanMoveSouth(baseline))
            {
                baseline = ShiftNorth();
            }

            var (currentHeadX, currentHeadY) = baseline.Head.Value;
            var newBinaryMap = new short[baseline._binaryMap.Length];
            Array.Copy(baseline._binaryMap, newBinaryMap, baseline._binaryMap.Length);

            var newHead = (X: currentHeadX, Y: currentHeadY + 1);
            newBinaryMap[newHead.Y] =
                (short) (baseline._binaryMap[newHead.Y] | (short) Math.Pow(2, _gameProps.Width - 1 - newHead.X));
            return new BinaryTrack(_gameProps, newBinaryMap, newHead);
        }
        private bool CanMoveSouth(BinaryTrack baseline)
        {
            if (!baseline.Head.HasValue)
            {
                return false;
            }

            var (_, y) = baseline._head.Value;

            return y < baseline._gameProps.Height - 1;
        }

        public BinaryTrack MoveWest()
        {
            BinaryTrack baseline = this;
            if (!CanMoveWest(baseline))
            {
                baseline = this >> 1;
            }

            var (currentHeadX, currentHeadY) = baseline.Head.Value;
            var newBinaryMap = new short[baseline._binaryMap.Length];
            Array.Copy(baseline._binaryMap, newBinaryMap, baseline._binaryMap.Length);
            checked
            {
                var newHead = (X: currentHeadX - 1, Y: currentHeadY);
                newBinaryMap[newHead.Y] =
                    (short) (baseline._binaryMap[newHead.Y] | (short) Math.Pow(2, _gameProps.Width - 1 - newHead.X));
                return new BinaryTrack(_gameProps, newBinaryMap, newHead);
            }

            
        }
        private bool CanMoveWest(BinaryTrack baseline)
        {
            if (!baseline.Head.HasValue)
            {
                return false;
            }
            
            var (baselineX, baselineY) = baseline.Head.Value;
            if (baselineX <= 0)
            {
                return false;
            }
            int cellToTheWest = (short) Math.Pow(2, _gameProps.Width - baselineX);

            return (baseline._binaryMap[baselineY] & cellToTheWest) == 0;
        }
        
        public BinaryTrack MoveNorth()
        {
            BinaryTrack baseline = this;
            if (!CanMoveNorth(baseline))
            {
                TryShiftSouth(out baseline);
            }

            var (currentHeadX, currentHeadY) = baseline.Head.Value;
            var newBinaryMap = new short[baseline._binaryMap.Length];
            Array.Copy(baseline._binaryMap, newBinaryMap, baseline._binaryMap.Length);

            var newHead = (X: currentHeadX, Y: currentHeadY - 1);
            newBinaryMap[newHead.Y] =
                (short) (baseline._binaryMap[newHead.Y] | (short) Math.Pow(2, _gameProps.Width - 1 - newHead.X));
            return new BinaryTrack(_gameProps, newBinaryMap, newHead);

            
        }
        private bool CanMoveNorth(BinaryTrack baseline)
        {
            if (!baseline.Head.HasValue)
            {
                return false;
            }

            var (_, y) = baseline._head.Value;

            return y > 0;
        }

        public bool TryShiftEast(out BinaryTrack output)
        {
            if (CanShiftEast())
            {
                output = this >> 1;
                return true;
            }

            output = null;
            return false;
            
        }
        private bool CanShiftEast()
        {
            return _binaryMap.All(row => (row & (short) 1) == 0);
        }

        public bool TryShiftSouth(out BinaryTrack output)
        {
            if (CanShiftSouth())
            {
                Nullable<(int,int)> head = null;
                if (_head.HasValue)
                {
                    head = (_head.Value.Item1, _head.Value.Item2 + 1);
                }

                short[] newBmap = new short[_binaryMap.Length];
                Array.Copy(_binaryMap, 0, newBmap, 1, _binaryMap.Length - 1);
                output = new BinaryTrack(_gameProps, newBmap, head);
                return true;
            }

            output = null;
            return false;
            
        }
        private bool CanShiftSouth()
        {
            return _binaryMap[_binaryMap.Length - 1] == 0;
        }
        
        private BinaryTrack ShiftNorth()
        {
            Nullable<(int,int)> head = null;
            if (_head.HasValue)
            {
                head = (_head.Value.Item1, _head.Value.Item2 - 1);
            }

            short[] newBmap = new short[_binaryMap.Length];
            Array.Copy(_binaryMap, 1, newBmap, 0, _binaryMap.Length - 1);
            return new BinaryTrack(_gameProps, newBmap, head);
        }

        public static BinaryTrack FromCartesian(GameProps gameProps, int[,] cartesianMap)
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

            return new BinaryTrack(gameProps, binaryMapData, null);
        }

        //For testing only
        public static BinaryTrack FromString(GameProps gameProps, string[] shape)
        {
            Nullable<(int,int)> head = null;

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

            return new BinaryTrack(gameProps, binaryMapData, head);
        }

        public string Debug()
        {
            StringBuilder sb = new StringBuilder();
            if (_head.HasValue)
            {
                sb.Append($"Head:{_head.Value}");
            }
            else
            {
                sb.Append("Head: None");
            }

            sb.Append(", data= new short[]{");
            foreach (var row in _binaryMap)
            {
                sb.Append($"{row},");
            }

            sb.Append("}");

            return sb.ToString();
        }

        public static BinaryTrack FromDebug(GameProps gameProps, short[] data, (int,int)? head)
        {
            return new BinaryTrack(gameProps, data, head);
        }
    }
}