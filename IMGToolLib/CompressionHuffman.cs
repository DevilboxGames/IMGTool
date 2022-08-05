using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToxicRagers.Compression.Huffman;

namespace ToxicRagers.Compression
{
	public class CompressionHuffman : ICompressionBase
	{
		public string TreeJson { get; set; }
		public async Task<byte[]> Compress(byte[] data)
		{
			Tree tree = new Tree();

			tree.BuildTree(data);
			TreeJson = tree.Json;
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				byte[] huffmanTable = tree.ToByteArray();

				bw.Write((byte)0x42); // B
				bw.Write((byte)0x54); // T
				bw.Write((byte)0x54); // T
				bw.Write((byte)0x42); // B
				bw.Write(32);
				bw.Write(1573120);
				bw.Write(huffmanTable.Length);
				bw.Write((short)4);
				bw.Write((short)1);
				bw.Write(tree.LeafCount);
				bw.Write(1);
				bw.Write(8);
				bw.Write(huffmanTable);
				bw.Write(tree.Encode(data));

				return ms.ToArray();
			}
		}

		public async Task<byte[]> Decompress(byte[] data)
		{
			Tree tree = new Tree();

			using (MemoryStream msIn = new MemoryStream(data))
			using (BinaryReader br = new BinaryReader(msIn))
			using (MemoryStream msOut = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(msOut))
			{
				byte[] tmpbyes = br.ReadBytes(4);    // BTTB
				int tmp1 = (int)br.ReadUInt32();    // 32
				int tmp2 = (int)br.ReadUInt32();    // 1573120
				int huffmanTableLength = (int)br.ReadUInt32();
				short tmp3 = (short)br.ReadUInt16();    // 4
				short tmp4 = (short)br.ReadUInt16();    // 1
				int leafCount = (int)br.ReadUInt32();
				int tmp5 = (int)br.ReadUInt32();    // 1
				int tmp6 = (int)br.ReadUInt32();    // 8

				tree.FromByteArray(br.ReadBytes(huffmanTableLength), leafCount);

				bw.Write(tree.Decode(br.ReadBytes(data.Length - (int)br.BaseStream.Position)));

				return msOut.ToArray();
			}
		}
	}
}
