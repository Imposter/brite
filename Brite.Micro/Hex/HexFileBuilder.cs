using System.Collections.Generic;
using System.Linq;

namespace Brite.Micro.Hex
{
    public class HexFileBuilder
    {
        private HexFile _hf = new HexFile();
        private List<byte> _currentData = new List<byte>();
        private HexFileLine _line;
        private bool _newLine;
        private int _offset;

        public void SetAddress(int val)
        {
            _offset = val;
        }

        public void WriteByte(byte? bt)
        {
            if (bt.HasValue)
            {
                WriteByte(bt.Value);
            }
            else
            {
                ++_offset;
                FlushLine();
            }
        }

        public void WriteByte(byte bt)
        {
            if (!CheckCurrentLine(_offset))
                FlushLine();
            if (_line == null)
                EnsureLine(_offset);
            var index = _offset - _line.Address;
            while (_currentData.Count <= index)
                _currentData.Add(byte.MaxValue);
            _currentData[index] = bt;
            ++_offset;
        }

        private bool CheckCurrentLine(int adr)
        {
            if (_line == null)
                return false;
            var num = adr - _line.Address;
            return num >= 0 && num < 16 && _currentData.Count >= num;
        }

        private void EnsureLine(int adr)
        {
            _line = _hf.Lines.FirstOrDefault(item => item.Type == HexFileLineType.Data && item.Address >> 4 == adr >> 4);
            if (_line == null)
            {
                _line = new HexFileLine
                {
                    Address = (ushort)_offset,
                    Type = HexFileLineType.Data,
                    Data = new byte[0]
                };
                _newLine = true;
            }
            else
                _newLine = false;
            _currentData = new List<byte>(_line.Data);
        }

        private void FlushLine()
        {
            if (_line == null || !_newLine || _currentData.Count <= 0)
                return;
            _line.Data = _currentData.ToArray();
            _hf.Lines.Add(_line);
            _line = null;
            _newLine = false;
        }

        public HexFile Build()
        {
            FlushLine();
            _hf.Lines.Add(new HexFileLine { Type = HexFileLineType.Eof });
            var hf = _hf;
            _hf = new HexFile();
            _offset = 0;
            return hf;
        }
    }
}
