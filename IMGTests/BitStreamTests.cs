using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMGToolLib;

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

			byte b1 = bitStream.ReadUnary();
			byte b2 = bitStream.ReadUnary();
			byte b3 = bitStream.ReadUnary();
			byte b4 = bitStream.ReadByte();

			Assert.Equal((byte)0x01, b1);
			Assert.Equal((byte)0x01, b2);
			Assert.Equal((byte)0x01, b3);
			Assert.Equal((byte)0x4d, b4);
		}
	}
}
