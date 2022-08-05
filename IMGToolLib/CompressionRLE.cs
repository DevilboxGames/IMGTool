using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToxicRagers.Compression
{
	public class CompressionRLE : ICompressionBase
	{
		private int _sequenceLength = 1;

		public CompressionRLE(int sequenceLength)
		{
			_sequenceLength = sequenceLength;
		}
		public async Task<byte[]> Compress(byte[] data)
		{

			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				byte lastColour = 0;
				int count = 0;

				for (int i = 0; i < data.Length; i++)
				{
					if (data[i] != lastColour || count == 127)
					{
						if (i > 0)
						{
							if (count < 4)
							{
								count = 3;
								count = Math.Min(count, data.Length - i);
								bw.Write((byte)(0x80 | count));
								for (int j = 0; j < count; j++, i++)
								{
									bw.Write(lastColour);
									lastColour = data[i];
								}

							}
							else
							{
								bw.Write((byte)count);
								bw.Write(lastColour);
							}
						}

						lastColour = data[i];
						count = 0;
					}

					count++;
				}

				return ms.ToArray();
			}
		}

		public async Task<byte[]> Decompress(byte[] data)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				for (int i = 0; i + _sequenceLength < data.Length;)
				{
					int count = data[i + 0];
					bool isRepeat = (count & 0x80) == 0;

					if (!isRepeat)
					{
						count = count & 0x7f;
						for (int k = 1; k <= count; k++)
						{
							bw.Write(data[i + k]);
						}

						i += 1 + count;
						continue;
					}

					byte[] colour = new byte[_sequenceLength];

					for (int k = 0; k < _sequenceLength; k++)
					{
						colour[k] = data[i + 1 + k];
					}

					for (int j = 0; j < count; j++)
					{
						for (int k = 0; k < _sequenceLength; k++)
						{
							bw.Write(colour[k]);
						}

					}

					i += 1 + _sequenceLength;
				}

				return ms.ToArray();
			}
        }
	}
}
