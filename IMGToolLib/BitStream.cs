using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMGToolLib
{
	public class BitStream
	{
		private byte _bitsLeft;
		private byte _mask;
		private int _currentByteIndex;
		private byte _currentByte;
		private byte[] _data;
		private bool _eof;

		public bool EOF
		{
			get => _eof;
		}

		public BitStream(byte[] data)
		{
			_data = data;
			MoveToNextByte(true);
		}

		public void MoveToNextByte(bool firstByte = false)
		{

			_currentByteIndex = firstByte ? 0 : _currentByteIndex + 1;
			if (_currentByteIndex >= _data.Length)
			{
				_eof = true;
			}
			else
			{
				_currentByte = _data[_currentByteIndex];
				_bitsLeft = 8;
				_mask = (byte)0xff;
			}

		}

		public byte ReadBits(byte numBits)
		{
			byte output = 0;
			if (_bitsLeft >= numBits)
			{
				_bitsLeft -= numBits;
				output = (byte)((_currentByte & _mask) >> (_bitsLeft));
				_mask >>= numBits;
				
			}
			else
			{
				byte nextBits = (byte)(numBits - _bitsLeft);
				output = (byte)((_currentByte & _mask) << nextBits);
				numBits -= nextBits;
				MoveToNextByte();
				
				_bitsLeft -= numBits;
				output |= (byte)((_currentByte & _mask) >> (_bitsLeft));
				_mask >>= numBits;

			}

			if (_bitsLeft == 0)
			{
				MoveToNextByte();
			}
			return output;
		}

		public byte ReadUnary()
		{
			byte output = ReadBits(1);
			while((output & 1) == 0)
			{
				output = (byte)(output << 1);
				output |= ReadBits(1);
			} 

			return output;
		}

		public byte ReadByte()
		{
			return ReadBits(8);
		}

		public short ReadShort()
		{
			return BitConverter.ToInt16(new[] { ReadBits(8), ReadBits(8) }, 0);
		}
		public ushort ReadUShort()
		{
			return BitConverter.ToUInt16(new[] { ReadBits(8), ReadBits(8) }, 0);
		}
		public int ReadInt()
		{
			return BitConverter.ToInt32(new[] { ReadBits(8), ReadBits(8), ReadBits(8), ReadBits(8) }, 0);
		}
		public uint ReadUInt()
		{
			return BitConverter.ToUInt32(new[] { ReadBits(8), ReadBits(8), ReadBits(8), ReadBits(8) }, 0);
		}
	}
}
