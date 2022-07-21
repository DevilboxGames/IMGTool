using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using ToxicRagers.Helpers;
using ToxicRagers.Stainless.Formats;

namespace IMGTool
{
    class Program
    {
        private static Version ConverterVersion = new Version(0,1,1);
        private static string outputPath = "";
        private static string inputPath = "";
        private static bool convertFolder = false;
        private static bool convertRecursive = false;
        private static bool convertToIMG = true;
        private static bool forceNoAlpha = false;
        private static bool fromTif = false;
        private static CompressionMethod Compression = CompressionMethod.None;
        private static bool scrapeInfo = false;
        static void Main(string[] args)
        {


            Console.WriteLine($"IMG File converter v{ConverterVersion.ToString()}");
            if (args.Length == 0)
            {
                Console.WriteLine("Todo: write help text!");
                return;
            }

            for (int i =0; i < args.Length;i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "--output":
                    case "-o":
                        outputPath = args[i+1];
                        i++;
                        break;
                    case "--input":
                    case "-i":
                        inputPath = args[i+1];
                        i++;
                        break;
                    case "--no-alpha":
                    case "-n":
                        forceNoAlpha = true;
                        break;
                    case "--convert-folder":
                    case "-cf":
                        convertFolder = true;
                        break;
                    case "--convert-folder-recursive":
                    case "-cfr":
                        convertFolder = true;
                        convertRecursive = true;
                        break;
                    case "--scrape-info":
                    case "-csv":
	                    scrapeInfo = true;
                        break;
                    case "--to-png":
                    case "-png":
                        convertToIMG = false;
                        break;
                    case "--from-tif":
                    case "-tif":
                        fromTif = true;
                        break;
                    case "--to-img":
                    case "-img":
                        convertToIMG = true;
                        break;
                    case "--huffman":
                    case "-h":
                        Compression = CompressionMethod.Huffman;
                        break;
                    case "--RLE":
                    case "-r":
                        Compression = CompressionMethod.RLE;
                        break;
                    case "--LIC":
                    case "-l":
                        Compression = CompressionMethod.LIC;
                        break;
                }
            }

            if (scrapeInfo)
            {
                ScrapeIMGFormats(Path.GetDirectoryName(Path.GetFullPath(inputPath)));
                return; 
            }
            string imageFormat = fromTif ? "TIF" : "PNG";

            if (outputPath == "")
            {
                outputPath = Path.ChangeExtension(inputPath, convertToIMG? "IMG":imageFormat);
            }
            var outputFolder = Path.GetDirectoryName(Path.GetFullPath(outputPath));

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputPath);
            }
            List<string> inputFiles = new List<string>();
            if (!convertFolder)
            {
                convertToIMG = Path.GetExtension(inputPath).ToUpper() != ".IMG";
                //var fileAttribs = File.GetAttributes(outputPath);
                var outputFilename = Path.GetFileNameWithoutExtension(outputPath) + (convertToIMG ? ".IMG" : $".{imageFormat}");
                if (convertToIMG)
                {
                    ConvertPNGToIMG(inputPath, outputFilename);
                }
                else
                {
                    ConverIMGToPng(inputPath, outputFilename);
                }
            }
            else
            {
                inputPath = Path.GetFullPath(inputPath);
                inputFiles.AddRange(Directory.GetFiles(inputPath,convertToIMG?$"*.{imageFormat}":"*.IMG", convertRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                foreach (var file in inputFiles)
                {
                    if (convertToIMG)
                    {
                        ConvertPNGToIMG(file, Path.Combine(outputPath,Path.GetFileNameWithoutExtension(file)+ (convertToIMG ? ".IMG" : $".{imageFormat}")));
                    }
                }
            }

        }

        static void ConvertPNGToIMG(string inputPath, string outputPath)
        {
            Console.WriteLine($"Converting {inputPath} to {outputPath}");
            var img = new ToxicRagers.Stainless.Formats.IMG();
            Bitmap bmp = new Bitmap(inputPath);
            img.ImportFromBitmap(bmp, Compression, forceNoAlpha);
            img.Save(outputPath);
        }
        static void ConverIMGToPng(string inputPath, string outputPath)
        {
            Console.WriteLine($"Converting {inputPath} to {outputPath}");
            //var img = new ToxicRagers.Stainless.Formats.IMG();
            //Bitmap bmp = new Bitmap(inputPath);
            //img.ImportFromBitmap(bmp, Compression, forceNoAlpha);
            //img.Save(outputPath);
            IMGToolLib.IMGTool.ConvertFromIMG(inputPath, outputPath);
        }

        static void ScrapeIMGFormats(string folder)
        {
	        var files = Directory.EnumerateFiles(folder, "*.img", SearchOption.AllDirectories);
	        StringBuilder output = new StringBuilder();

	        output.Append(
		        $"path,imgversion,imgbasicFlags,imgadvancedFlags,imgwidth,imgheight,imgimageFormat,fileSize\n");
            foreach (string path in files)
	        {
		        using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
		        using (BinaryReader br = new BinaryReader(ms))
		        {
			        if (br.ReadByte() != 0x49 || // I
			            br.ReadByte() != 0x4D || // M
			            br.ReadByte() != 0x41 || // A
			            br.ReadByte() != 0x47 || // G
			            br.ReadByte() != 0x45 || // E
			            br.ReadByte() != 0x4D || // M
			            br.ReadByte() != 0x41 || // A
			            br.ReadByte() != 0x50) // P
			        {
				        Logger.LogToFile(Logger.LogLevel.Error, "{0} isn't a valid IMG file", path);
				        continue;
			        }

			        byte minor = br.ReadByte();
			        byte major = br.ReadByte();

			        var imgversion = new Version(major, minor);
			        var imgbasicFlags = (IMG.BasicFlags)br.ReadByte();
			        var imgadvancedFlags = (IMG.AdvancedFlags)br.ReadByte();
			        var imgimageFormat = (IMG.ImageFormat)br.ReadUInt32();
			        int fileSize = (int)br.ReadUInt32();
			        var imgwidth = br.ReadUInt16();
			        var imgheight = br.ReadUInt16();

			        output.Append(
				        $"{path},{imgversion},{imgbasicFlags.ToString().Replace(',',';')},{imgadvancedFlags.ToString().Replace(',', ';')},{imgwidth},{imgheight},{imgimageFormat},{fileSize}\n");
		        }
	        }
            File.WriteAllText("img_scrape.csv",output.ToString());
        }
    }
}
