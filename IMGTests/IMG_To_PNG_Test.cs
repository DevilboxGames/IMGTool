using System.Drawing;
using ToxicRagers.Stainless.Formats;

namespace IMGTests
{
	public class IMG_To_PNG_Test
	{
		[Theory]
		[InlineData("assets/dump.img","converted/dump.png")]
		public void TestConvertLICPlaneXRGBToPNG(string inputPath, string outputPath)
		{
			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/screwie.img", "converted/screwie.png")]
		public void TestConvertLICPlaneARGBToPNG(string inputPath, string outputPath)
		{
			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/ulitbot.img", "converted/ulitbot.png")]
		public void TestConvertLICARGBToPNG(string inputPath, string outputPath)
		{
			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}
		[Theory]
		[InlineData("assets/chevrn.img","converted/chevrn.png")]
		public void TestConvertHuffmanPlaneXRGBToPNG(string inputPath, string outputPath)
		{
			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/checkpnt.img", "converted/checkpnt.png")]
		public void TestConvertHuffmanPlaneARGBToPNG(string inputPath, string outputPath)
		{
			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/craneleg.img","converted/craneleg.png")]
		public void TestConvertHuffmanXRGBToPNG(string inputPath, string outputPath)
		{
			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/ctrl_ffw.img", "converted/ctrl_ffw.png")]
		public void TestConvertHuffmanARGBToPNG(string inputPath, string outputPath)
		{
			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/newcheck.img", "converted/newcheck.png")]
		public void TestConvertRLEPlaneXRGBToPNG(string inputPath, string outputPath)
		{

			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/spark.img", "converted/spark.png")]
		public void TestConvertRLEPlaneARGBToPNG(string inputPath, string outputPath)
		{

			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}
		[Theory]
		[InlineData("assets/arrowon.img", "converted/arrowon.png")]
		public void TestConvertRLEXRGBToPNG(string inputPath, string outputPath)
		{

			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}

		[Theory]
		[InlineData("assets/chevrn.img", "converted/chevrn.png")]
		public void TestConvertRLEARGBToPNG(string inputPath, string outputPath)
		{

			if (!Directory.Exists("converted"))
			{
				Directory.CreateDirectory("converted");
			}
			var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);

			Bitmap bmp = img.ExportToBitmap();
			bmp.Save(outputPath);

		}
	}
}