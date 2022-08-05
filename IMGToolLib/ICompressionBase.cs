using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToxicRagers.Compression
{
	public interface ICompressionBase
	{
		Task<byte[]> Compress(byte[] data);
		Task<byte[]> Decompress(byte[] data);
	}
}
