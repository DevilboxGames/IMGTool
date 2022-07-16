using System.Drawing;
using ToxicRagers.Stainless.Formats;

namespace IMGTests
{
	public class UnitTest1
	{
		[Fact]
		public void TestHuffmanTreeUnpack()
		{
			Bitmap bmp = new Bitmap(Bitmap.FromFile("assets/agntintk.png"));
			IMG img = new IMG();
			img.ImportFromBitmap(bmp, CompressionMethod.Huffman, true);

		}
	}
}