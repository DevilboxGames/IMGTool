using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
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
        private static CompressionMethod Compression = CompressionMethod.None;

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
                    case "--convert-folder":
                    case "-cf":
                        convertFolder = true;
                        break;
                    case "--convert-folder-recursive":
                    case "-cfr":
                        convertFolder = true;
                        convertRecursive = true;
                        break;
                    case "--to-png":
                    case "-png":
                        convertToIMG = false;
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

            if (outputPath == "")
            {
                outputPath = Path.ChangeExtension(inputPath, convertToIMG? "IMG":"PNG");
            }
            var outputFolder = Path.GetDirectoryName(Path.GetFullPath(outputPath));

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputPath);
            }
            List<string> inputFiles = new List<string>();
            if (!convertFolder)
            {
                convertToIMG = Path.GetExtension(inputPath).ToUpper() == (convertToIMG ? ".IMG" : ".PNG");
                var fileAttribs = File.GetAttributes(outputPath);
                var outputFilename = Path.GetFileNameWithoutExtension(outputPath) + (convertToIMG ? ".IMG" : ".PNG");
                if (convertToIMG)
                {
                    ConvertPNGToIMG(inputPath, outputFilename);
                }
            }
            else
            {
                inputPath = Path.GetFullPath(inputPath);
                inputFiles.AddRange(Directory.GetFiles(inputPath,convertToIMG?"*.PNG":"*.IMG", convertRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                foreach (var file in inputFiles)
                {
                    if (convertToIMG)
                    {
                        ConvertPNGToIMG(file, Path.Combine(outputPath,Path.GetFileNameWithoutExtension(file)+ (convertToIMG ? ".IMG" : ".PNG")));
                    }
                }
            }

        }

        static void ConvertPNGToIMG(string inputPath, string outputPath)
        {
            Console.WriteLine($"Converting {inputPath} to {outputPath}");
            var img = new ToxicRagers.Stainless.Formats.IMG();
            Bitmap bmp = new Bitmap(inputPath);
            img.ImportFromBitmap(bmp, Compression);
            img.Save(outputPath);
        }

    }
}
