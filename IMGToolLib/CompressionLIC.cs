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
		private int _sequenceLength = 1;
		private int _width, _height;
		private BitStream _bitStream;
		public CompressionLIC(int sequenceLength, int width, int height)
		{
			_sequenceLength = sequenceLength;
			_width = width;
			_height = height;
		}

		private bool RiceEncode(byte val, byte k, ref byte q, ref byte r)
		{
			if (k >= 32)
			{
				throw new InvalidDataException($"Rice byte k is too large: {k}");
			}

			if (q == 0 || r == 0)
			{
				return false;
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
			throw new NotImplementedException();

		}

		public async Task<byte[]> Decompress(byte[] data)
		{
			throw new NotImplementedException();
		}
	}
}
