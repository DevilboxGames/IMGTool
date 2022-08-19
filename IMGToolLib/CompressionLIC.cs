using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToxicRagers.Compression;

namespace IMGToolLib
{
	public class CompressionLIC : ICompressionBase
	{
		private int _blockSize = 1;
		private int _width, _height;
		
		public CompressionLIC(int blockSize, int width, int height)
		{
			_blockSize = blockSize;
			_width = width;
			_height = height;
		}

		private bool RiceEncode(byte val, byte k, out byte q, out byte r)
		{
			if (k >= 32)
			{
				throw new InvalidDataException($"Rice byte k is too large: {k}");
			}
			
			q = (byte)(val >> k);
			r = (byte)(val & ((1 << k) - 1));

			return true;
		}
		private byte RiceDecode(byte k, byte q, byte r)
		{
			if (k >= 32)
			{
				throw new InvalidDataException($"Rice byte k is too large: {k}");
			}

			return (byte)((q * (1 << k)) + r);
		}
		public async Task<byte[]> Compress(byte[] data)
		{
			BitStream stream = new BitStream();

			byte riceBucketSize = 3;

			// Write the first two raw bytes, they're used for calculating subsequent bits
			stream.WriteByte(data[0]);
			stream.WriteByte(data[1]);

			for (int y = 0, index = 0; y < _height; y++)
			{
				for (int x = 0; x < _width && index < data.Length; x++, index += _blockSize)
				{

					byte n1, n2;
					byte p1 = data[index];

					// Skip these buggers, we already fetched them
					if (y == 0 && x < 2)
					{
						continue;
					}

					if (y == 124 && x == 247)
					{
						var tmp = 0;
					}

					if (y == 0)
					{
						n1 = data[index - 1];
						n2 = data[index - 2];
					}
					else
					{
						if (x == 0)
						{
							n1 = data[index - (_width * _blockSize)];
							n2 = data[index - (_width * _blockSize - 1)];
						}
						else
						{
							n1 = data[index - 1];
							n2 = data[index - (_width * _blockSize)];
						}
					}

					byte low = Math.Min(n1, n2);
					byte high = Math.Max(n1, n2);
					byte delta = (byte)(high - low);

					if (low <= p1 && p1 <= high)
					{
						// output 1 bit to show we are in range
						stream.WriteBits(1, 1);
						byte byteValue = delta;

						byteValue |= 1; // We need at least one bit
						byte bitsNeeded = 0;

						while (byteValue > 0)
						{
							byteValue >>= 1;
							bitsNeeded++;
						}

						//output binary code for how close we are
						//Encode P-L by adjusted binary coding
						stream.WriteBits((byte)(p1 - low), bitsNeeded);
					}
					else if (p1 < low)
					{
						// output 0 to show we are out of range
						stream.WriteBits(0,1);

						// output 0 to show we are below range
						stream.WriteBits(0, 1);

						RiceEncode((byte)((low - 1) - p1), riceBucketSize, out byte q, out byte r);

						stream.WriteUnary(q, 0);
						stream.WriteBits(r, riceBucketSize);
					}
					else
					{
						// output 0 to show we are out of range
						stream.WriteBits(0, 1);

						// output 1 to show we are above range
						stream.WriteBits(1, 1);

						RiceEncode((byte)(p1 - (high + 1)), riceBucketSize, out byte q, out byte r);

						stream.WriteUnary(q, 0);
						stream.WriteBits(r, riceBucketSize);


					}
				}
			}

			return stream.GetData();
		}

		public async Task<byte[]> Decompress(byte[] data)
		{
			BitStream stream = new BitStream(data);
			List<byte> output = new List<byte>();

			byte riceBucketSize = 3;

			output.Add(stream.ReadByte());
			output.Add(stream.ReadByte());

			for (int y = 0, index = 0; y < _height && !stream.EOF; y++)
			{
				for (int x = 0; x < _width && !stream.EOF; x++, index += _blockSize)
				{

					byte n1, n2;
					// Skip these buggers, we already fetched them
					if (y == 0 && x < 2)
					{
						continue;
					}

					if (y == 0)
					{
						n1 = output[index - 1];
						n2 = output[index - 2];
					}
					else
					{
						if (x == 0)
						{
							n1 = output[index - (_width * _blockSize)];
							n2 = output[index - (_width * _blockSize - 1)];
						}
						else
						{
							n1 = output[index - 1];
							n2 = output[index - (_width * _blockSize)];
						}
					}

					if (x == 66 && y == 0)
					{

						byte tmp = 0;
					}
					byte low = Math.Min(n1, n2);
					byte high = Math.Max(n1, n2);
					byte delta =(byte)(high - low);

					byte bitValue = stream.ReadBits(1);
					if (bitValue == 1)
					{
						byte byteVal = delta;
						byteVal |= 1;
						byte bitsNeeded = 0;
						while (byteVal > 0)
						{
							byteVal >>= 1;
							bitsNeeded++;
						}

						byteVal = stream.ReadBits(bitsNeeded);
						//byteVal >>= 8 - bitsNeeded;
						output.Add((byte)(low + byteVal));
					}
					else
					{
						bitValue = stream.ReadBits(1);

						byte q = stream.ReadUnary(0);
						byte r = stream.ReadBits(riceBucketSize);
						byte byteVal = RiceDecode(riceBucketSize, q, r);

						if (bitValue != 0)
						{
							output.Add((byte)(high + 1 + byteVal));
						}
						else
						{
							output.Add((byte)(low - 1 - byteVal));

						}
					}
				}
			}
			return output.ToArray();

		}
	}
}
