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
		public enum BitStreamReadWriteMode
		{
			Read,
			Write
		}
		private byte _bitsLeft;
		private byte _mask;
		private int _currentByteIndex;
		private byte _currentByte;
		private byte[] _data;
		private bool _eof;
		private BitStreamReadWriteMode _readWriteMode;
		private long _bytesWritten;

		public bool EOF
		{
			get => _currentByteIndex >= _data.Length;
		}

		public BitStream(byte[] data)
		{
			_data = data;
			_readWriteMode = BitStreamReadWriteMode.Read;
			MoveToNextByte(true);
		}

		public BitStream(int numBytes = 16)
		{
			_data = new byte[numBytes];
			_readWriteMode = BitStreamReadWriteMode.Write;
			MoveToNextByte();
		}

		public void MoveToNextByte(bool firstByte = false)
		{
			_currentByteIndex = firstByte ? 0 : _currentByteIndex + 1;

			if (_readWriteMode == BitStreamReadWriteMode.Write)
			{
				if (_bytesWritten > 0)
				{
					_data[_bytesWritten - 1] = _currentByte;
				}
				if (_currentByteIndex >= _data.Length)
				{
					Array.Resize(ref _data, _data.Length + 16);
				}
			}
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
		
		public enum WriteBitsFrom
		{
			HighBit,
			LowBit
		}

		public byte[] GetData()
		{
			if (_readWriteMode == BitStreamReadWriteMode.Write && _data.Length > _bytesWritten)
			{
				if (_bytesWritten > 0 && _bitsLeft < 8)
				{
					_data[_bytesWritten - 1] = _currentByte;
				}
				Array.Resize(ref _data, (int)_bytesWritten);
			}

			return _data;
		}

		public void WriteBits(byte data, byte numBits, WriteBitsFrom writeBitsFrom = WriteBitsFrom.LowBit)
		{
			if (_bitsLeft == 8 && numBits > 0)
			{
				_bytesWritten++;
			}
			if (_bitsLeft >= numBits)
			{
				byte sourceBits = (byte)(writeBitsFrom == WriteBitsFrom.HighBit ? data >> (8 - numBits) : data & (0xff >> (8 - numBits)));
				_currentByte |= (byte)(sourceBits << (_bitsLeft - numBits));
				_bitsLeft -= numBits;
			}
			else
			{
				byte sourceBits = (byte)(data & (0xff >> (8-numBits)));
				byte leftOverBits = (byte)(numBits - _bitsLeft);
				_currentByte |= (byte)((sourceBits >> leftOverBits) );
				MoveToNextByte();
				_bytesWritten++;
				_currentByte |= (byte)((sourceBits & (0xFF >> (8 - leftOverBits))) << (8 - leftOverBits));
				_bitsLeft -= leftOverBits;
			}


			if (_bitsLeft == 0)
			{
				MoveToNextByte();
			}
		}

		public void WriteByte(byte data)
		{
			WriteBits(data, 8);
		}
		public void WriteShort(short data)
		{
			WriteBits((byte)(data >> 8), 8);
			WriteBits((byte)(data  & 0xff), 8);
		}
		public void WriteInt(int data)
		{
			WriteBits((byte)(data >> 24), 8);
			WriteBits((byte)(data >> 16), 8);
			WriteBits((byte)(data >> 8), 8);
			WriteBits((byte)(data  & 0xff), 8);
		}

		public void WriteUnary(byte count, byte terminator)
		{
			for (int i = 0; i < count; i++)
			{
				WriteBits((byte)(terminator == 1 ? 0 : 1), 1);
			}
			WriteBits(terminator, 1);
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
				byte leftOverBits = (byte)(numBits - _bitsLeft);

				output = (byte)((_currentByte & _mask) << leftOverBits);
				numBits -= _bitsLeft;
				MoveToNextByte();
				
				_bitsLeft -= leftOverBits;
				output |= (byte)((_currentByte & _mask) >> (_bitsLeft));
				_mask >>= numBits;

			}

			if (_bitsLeft == 0)
			{
				MoveToNextByte();
			}
			return output;
		}

		public byte ReadUnary(int terminator)
		{
			byte b = ReadBits(1);
			byte output = 0;
			while(b != terminator && !EOF)
			{
				output++;// = (byte)((output << 1) | b);
				b = ReadBits(1);
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
