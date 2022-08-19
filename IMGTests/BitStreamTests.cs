using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMGToolLib;
using ToxicRagers.Helpers;

namespace IMGTests
{
	public class BitStreamTests
	{
		[Fact]
		public void BitStream_ReadBits()
		{
			byte[] data = new[] { (byte)0x49, (byte)0x4D, (byte)0x41, (byte)0x47, (byte)0x45, (byte)0x4D, (byte)0x41, (byte)0x50 };

			BitStream bitStream = new BitStream(data);

			byte b1 = bitStream.ReadBits(8);
			byte b2 = bitStream.ReadBits(4);
			byte b3 = bitStream.ReadBits(8);
			byte b4 = bitStream.ReadBits(4);

			Assert.Equal((byte)0x49,b1);
			Assert.Equal((byte)0x04,b2);
			Assert.Equal((byte)0xD4,b3);
			Assert.Equal((byte)0x01, b4);
		}
		[Fact]
		public void BitStream_ReadByte()
		{
			byte[] data = new[] { (byte)0x49, (byte)0x4D, (byte)0x41, (byte)0x47, (byte)0x45, (byte)0x4D, (byte)0x41, (byte)0x50 };

			BitStream bitStream = new BitStream(data);

			byte b1 = bitStream.ReadByte();
			byte b2 = bitStream.ReadByte();
			byte b3 = bitStream.ReadByte();
			byte b4 = bitStream.ReadByte();

			Assert.Equal((byte)0x49,b1);
			Assert.Equal((byte)0x4D,b2);
			Assert.Equal((byte)0x41,b3);
			Assert.Equal((byte)0x47, b4);
		}
		[Fact]
		public void BitStream_ReadShort()
		{
			byte[] data = new[] { (byte)0x49, (byte)0x4D, (byte)0x41, (byte)0x47, (byte)0x45, (byte)0x4D, (byte)0x41, (byte)0x50 };

			BitStream bitStream = new BitStream(data);

			short b1 = bitStream.ReadShort();
			short b2 = bitStream.ReadShort();
			short b3 = bitStream.ReadShort();
			short b4 = bitStream.ReadShort();

			Assert.Equal((short)0x4D49,b1);
			Assert.Equal((short)0x4741,b2);
			Assert.Equal((short)0x4d45,b3);
			Assert.Equal((short)0x5041, b4);
		}
		[Fact]
		public void BitStream_ReadUShort()
		{
			byte[] data = new[] { (byte)0x49, (byte)0x4D, (byte)0x41, (byte)0x47, (byte)0x45, (byte)0x4D, (byte)0x41, (byte)0x50 };

			BitStream bitStream = new BitStream(data);

			ushort b1 = bitStream.ReadUShort();
			ushort b2 = bitStream.ReadUShort();
			ushort b3 = bitStream.ReadUShort();
			ushort b4 = bitStream.ReadUShort();

			Assert.Equal((ushort)0x4D49,b1);
			Assert.Equal((ushort)0x4741,b2);
			Assert.Equal((ushort)0x4d45,b3);
			Assert.Equal((ushort)0x5041, b4);
		}
		[Fact]
		public void BitStream_ReadInt()
		{
			byte[] data = new[] { (byte)0x49, (byte)0x4D, (byte)0x41, (byte)0x47, (byte)0x45, (byte)0x4D, (byte)0x41, (byte)0x50 };

			BitStream bitStream = new BitStream(data);

			int b1 = bitStream.ReadInt();
			int b2 = bitStream.ReadInt();

			Assert.Equal((int)0x47414D49, b1);
			Assert.Equal((int)0x50414d45, b2);
		}
		[Fact]
		public void BitStream_ReadUInt()
		{
			byte[] data = new[] { (byte)0x49, (byte)0x4D, (byte)0x41, (byte)0x47, (byte)0x45, (byte)0x4D, (byte)0x41, (byte)0x50 };

			BitStream bitStream = new BitStream(data);

			uint b1 = bitStream.ReadUInt();
			uint b2 = bitStream.ReadUInt();

			Assert.Equal((uint)0x47414D49, b1);
			Assert.Equal((uint)0x50414d45, b2);
		}
		[Fact]
		public void BitStream_ReadUnaray()
		{
			byte[] data = new[] { (byte)0x49, (byte)0x4D, (byte)0x41, (byte)0x47, (byte)0x45, (byte)0x4D, (byte)0x41, (byte)0x50 };

			BitStream bitStream = new BitStream(data);

			byte b1 = bitStream.ReadUnary(1);
			byte b2 = bitStream.ReadUnary(1);
			byte b3 = bitStream.ReadUnary(1);
			byte b4 = bitStream.ReadByte();

			Assert.Equal((byte)0x01, b1);
			Assert.Equal((byte)0x02, b2);
			Assert.Equal((byte)0x02, b3);
			Assert.Equal((byte)0x4d, b4);
		}

		[Fact]
		public void BitStream_WriteBits()
		{
			BitStream bitStream = new BitStream();

			bitStream.WriteBits(10, 4);
			bitStream.WriteBits(123, 4, BitStream.WriteBitsFrom.HighBit);
			bitStream.WriteBits(123, 4, BitStream.WriteBitsFrom.LowBit);
			bitStream.WriteBits(1,2);
			bitStream.WriteBits(179,8);
			bitStream.WriteBits(2, 2);

			byte[] output = bitStream.GetData();

			Assert.Equal(167, output[0]);
			Assert.Equal(182, output[1]);
			Assert.Equal(206, output[2]);


		}

		[Fact]
		public void BitStream_WriteBits2()
		{
			byte[] data = new byte[] { 0xB9, 0xB4, 0x16, 0x00, 0x45, 0x14, 0xAD, 0xB4, 0x10, 0xC5, 0xDC, 0x06 };

			BitStream bitStream = new BitStream();
			bitStream.WriteByte(data[0]);
			bitStream.WriteByte(data[1]);
			bitStream.WriteBits(0, 1);
			bitStream.WriteBits(0, 1);
			bitStream.WriteBits(0, 4);

			byte[] output = bitStream.GetData();
			for (int i = 0; i < data.Length; i++)
			{
				Assert.Equal(data[i], output[i]);
			}
		}
	}
}
