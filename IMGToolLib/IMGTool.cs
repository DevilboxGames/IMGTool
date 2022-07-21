using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToxicRagers.Stainless.Formats;

namespace IMGToolLib
{
    public class IMGTool
    {
        public static void ConvertToIMG(string[] inputPath, string outputPath, bool[] forceNoAlpha = null)
        {

            List<string> paths = new List<string>();
            List<bool> alphas = new List<bool>();
            int imgnum = 0;
            foreach (var path in inputPath)
            {
                if (!paths.Contains(path.ToUpper()))
                {
                    paths.Add(path.ToUpper());
                    if (forceNoAlpha != null)
                    {
                        alphas.Add(forceNoAlpha[imgnum]);
                    }
                }

                imgnum++;
            }


            Parallel.ForEach(paths,
                (s,state,i) =>
                {
                    ConvertToIMG(s, Path.Combine(outputPath, Path.GetFileNameWithoutExtension(s) + ".IMG"),
                        CompressionMethod.Huffman, forceNoAlpha != null && alphas[(int)i]);
                });

        }

        public static void ConvertToIMG(string[] inputPath, string outputPath, CompressionMethod compression, bool[] forceNoAlpha)
        {


            List<string> paths = new List<string>();
            List<bool> alphas = new List<bool>();
            int imgnum = 0;
            foreach (var path in inputPath)
            {
                if (!paths.Contains(path.ToUpper()))
                {
                    paths.Add(path.ToUpper());
                    if (forceNoAlpha != null)
                    {
                        alphas.Add(forceNoAlpha[imgnum]);
                    }
                }

                imgnum++;
            }

            Parallel.ForEach(paths, (s, state, i) => { ConvertToIMG(s, Path.Combine( outputPath, Path.GetFileNameWithoutExtension(s)+".IMG"), compression, forceNoAlpha != null && alphas[(int)i]); });

        }
        public static void ConvertToIMG(string inputPath, string outputPath, CompressionMethod compression, bool forceNoAlpha)
        {
            Console.WriteLine($"Converting {inputPath} to {outputPath}");
            var img = new ToxicRagers.Stainless.Formats.IMG();
            Bitmap bmp = new Bitmap(inputPath);
            img.ImportFromBitmap(bmp, compression, forceNoAlpha);
            img.Save(outputPath);
        }
        public static void ConvertFromIMG(string[] inputPath, string outputPath)
        {

	        List<string> paths = new List<string>();
	        List<bool> alphas = new List<bool>();
	        int imgnum = 0;
	        foreach (var path in inputPath)
	        {
		        if (!paths.Contains(path.ToUpper()))
		        {
			        paths.Add(path.ToUpper());

		        }

		        imgnum++;
	        }


	        Parallel.ForEach(paths,
		        (s, state, i) =>
		        {
			        ConvertFromIMG(s, Path.Combine(outputPath, Path.GetFileNameWithoutExtension(s) + ".PNG"));
		        });

        }
        /*
        public static void ConvertFromIMG(string[] inputPath, string outputPath)
        {


	        List<string> paths = new List<string>();
	        List<bool> alphas = new List<bool>();
	        int imgnum = 0;
	        foreach (var path in inputPath)
	        {
		        if (!paths.Contains(path.ToUpper()))
		        {
			        paths.Add(path.ToUpper());
		        }

		        imgnum++;
	        }

	        Parallel.ForEach(paths, (s, state, i) => { ConvertFromIMG(s, Path.Combine(outputPath, Path.GetFileNameWithoutExtension(s) + ".PNG")); });

        }*/
        public static void ConvertFromIMG(string inputPath, string outputPath)
        {
            Console.WriteLine($"Converting {inputPath} to {outputPath}");
            var img = ToxicRagers.Stainless.Formats.IMG.Load(inputPath);
            
            Bitmap bmp = img.ExportToBitmap();
            bmp.Save(outputPath);
        }
    }
}