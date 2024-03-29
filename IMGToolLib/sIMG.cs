﻿using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ToxicRagers.Compression.Huffman;
using ToxicRagers.Generics;
using ToxicRagers.Helpers;

namespace ToxicRagers.Stainless.Formats
{
    public enum CompressionMethod
    {
        None = 0,
        RLE = 1,
        Huffman = 2,
        LIC = 3
    }

    public class IMG : Texture
    {
        [Flags]
        public enum BasicFlags : byte
        {
            DisableCompressInTextureMemory = 0x01,
            Compressed = 0x02,
            OneBitAlpha = 0x04,
            Disable16bit = 0x08,
            AttachedDataSize = 0x10,
            DisableDownSample = 0x20,
            DisableMipMaps = 0x40,
            IsCubemap = 0x80
        }

        [Flags]
        public enum AdvancedFlags : byte
        {
            Huffman = 0x01,
            LIC = 0x02,
            UnknownCompression = 0x04,
            CrushToJPEG = 0x08,
            DontAutoJPEG = 0x10,
            SRGB = 0x20
        }

        public enum ImageFormat
        {
            XRGB = 0,
            ARGB = 1,
            Alpha = 2,
            CLUT4 = 3,
            CLUT8 = 4,
            PlaneXRGB = 5,
            PlaneARGB = 6,
            XRGBAnimation = 7,
            ARGBAnimation = 8,
            A8L8 = 9
        }

        Version version;
        BasicFlags basicFlags;
        AdvancedFlags advancedFlags;
        ImageFormat imageFormat;

        int width;
        int height;
        ConcurrentBag<Plane> planes = new ConcurrentBag<Plane>();

        public IMG()
            : base()
        {
            extension = "IMG";
        }

        public static IMG Load(string path)
        {
            Logger.LogToFile(Logger.LogLevel.Info, "{0}", path);
            IMG img = new IMG() { Name = Path.GetFileNameWithoutExtension(path) };

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
                    br.ReadByte() != 0x50)   // P
                {
                    Logger.LogToFile(Logger.LogLevel.Error, "{0} isn't a valid IMG file", path);
                    return null;
                }

                byte minor = br.ReadByte();
                byte major = br.ReadByte();

                img.version = new Version(major, minor);
                img.basicFlags = (BasicFlags)br.ReadByte();
                img.advancedFlags = (AdvancedFlags)br.ReadByte();
                img.imageFormat = (ImageFormat)br.ReadUInt32();
                int fileSize = (int)br.ReadUInt32();
                img.width = br.ReadUInt16();
                img.height = br.ReadUInt16();

                Logger.LogToFile(Logger.LogLevel.Info, "{0} : {1} : {2} : {3}", img.version, img.basicFlags, img.advancedFlags, img.imageFormat);

                if (img.version.Minor == 1) { int jpgQuality = (int)br.ReadUInt32(); }

                int planeCount = img.imageFormat != ImageFormat.PlaneXRGB && img.imageFormat != ImageFormat.PlaneARGB ? 1 : img.advancedFlags.HasFlag(AdvancedFlags.Huffman) && img.imageFormat == ImageFormat.PlaneXRGB ? 3 : 4;
                if (planeCount > 1)
                {
	                for (int i = 0; i < planeCount; i++)
	                {
		                img.planes.Add(new Plane(i) { Output = new byte[br.ReadUInt32()] });
	                }
                }
                else
                {

	                img.planes.Add(new Plane(0) { Output = new byte[img.width * img.height * 4], rleBytes = 4});
                }

                for (int i = 0; i < img.planes.Count; i++)
                {
                    Plane plane = img.planes.First(p => p.Index == i);
                    plane.Output = br.ReadBytes(plane.Output.Length);
                    if (img.basicFlags.HasFlag(BasicFlags.Compressed))
                    {
                        plane.Decompress((img.advancedFlags.HasFlag(AdvancedFlags.Huffman) ? CompressionMethod.Huffman : CompressionMethod.RLE));
                    }
                }
            }

            return img;
        }

        public void Save(string path)
        {
	        int planeNum = 0;
	        foreach(Plane plane in planes)
            {
	            File.WriteAllText($"{Path.GetFileNameWithoutExtension(path)}_plane{planeNum}_tree.json", plane.TreeJson);
	            planeNum++;
            }

            using (BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                BasicFlags basic = 0;
                AdvancedFlags compression = 0;
                int dataSize = 0;

                if (imageFormat == ImageFormat.ARGB)
                {
                    dataSize = planes.First().Output.Length;
                }
                else
                {
                    basic = BasicFlags.Compressed;

                    if (planes.Any(p => p.Method == CompressionMethod.RLE && p.PoorCompression))
                    {
                        Parallel.ForEach(
                            planes,
                            p =>
                            {
                                p.Compress(CompressionMethod.Huffman);
                            }
                        );

                        compression = AdvancedFlags.Huffman;
                    }
                    else if (planes.Any(p => p.Method == CompressionMethod.Huffman))
                    {
                        compression = AdvancedFlags.Huffman;
                    }

                    dataSize = (planes.Count * 4) + planes.Sum(p => p.DataSize);
                }

                bw.WriteString("IMAGEMAP");
                bw.Write(new byte[] { 0x1, 0x1 }); // version 1.1
                bw.Write((byte)(basic | BasicFlags.DisableDownSample | BasicFlags.DisableMipMaps));
                bw.Write((byte)(compression | AdvancedFlags.DontAutoJPEG));
                bw.Write((int)imageFormat);
                bw.Write(dataSize);
                bw.Write((short)width);
                bw.Write((short)height);
                bw.Write(0x64);

                if (imageFormat == ImageFormat.PlaneARGB || imageFormat == ImageFormat.PlaneXRGB)
                {
                    foreach (Plane plane in planes.OrderByDescending(p => p.Index))
                    {
                        bw.Write(plane.DataSize);
                    }
                    foreach (Plane plane in planes.OrderByDescending(p => p.Index))
                    {
	                    bw.Write(plane.Output);
                    }
                }

            }
        }

        public void ImportFromBitmap(Bitmap bitmap, CompressionMethod compression = CompressionMethod.RLE, bool forceNoAlpha = false)
        {
            if (bitmap.Width <= 32 || bitmap.Height <= 32) { bitmap = bitmap.Resize(64, 64); }

            width = bitmap.Width;
            height = bitmap.Height;

            int planeCount = !forceNoAlpha ? Image.GetPixelFormatSize(bitmap.PixelFormat) / 8 : 3;

            if (bitmap.Width < 8 && bitmap.Height < 8) { planeCount = 1; }

            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] iB = new byte[4 * bmpdata.Width * bmpdata.Height];
            Marshal.Copy(bmpdata.Scan0, iB, 0, bmpdata.Stride * bmpdata.Height);
            bitmap.UnlockBits(bmpdata);

            //Parallel.For(0, planeCount,
                //i =>
                for (int i = 0; i < planeCount; i++)
                {
                    Plane plane = new Plane(i) { Data = iB.ToList().Every(planeCount > 1 ? 4 : planeCount, i).ToArray() };
                    plane.Compress(planeCount > 1 ? compression : CompressionMethod.None);
                    planes.Add(plane);
                }
            //);

            switch (planeCount)
            {
                case 1:
                    imageFormat = ImageFormat.ARGB;
                    break;

                case 3:
                    imageFormat = ImageFormat.PlaneXRGB;
                    break;

                case 4:
                    imageFormat = ImageFormat.PlaneARGB;
                    break;
            }
        }

        public Bitmap ExportToBitmap()
        {
            Bitmap bmp = new Bitmap(width, height);
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte[] oB = new byte[4 * width * height];

            if (imageFormat == ImageFormat.PlaneARGB || imageFormat == ImageFormat.PlaneXRGB)
            {
	            int pixelSize = 4;
	            var sortedPlanes = planes.OrderBy(p => p.Index).ToList();


                /*
                for (int i = 0, j = 0; i + pixelSize < oB.Length && j + pixelSize < planes.First().Data.Length; i += 4, j += pixelSize)
	            {
		            for (int k = 0; k < sortedPlanes.Count; k++)
		            {

			            int planeIndex = sortedPlanes[k].Index;
			            if (planes.Count == 3)
			            {
				            planeIndex++;
			            }
                        oB[i + (3 - planeIndex)] = sortedPlanes[k].Data[j + k];
		            }
	            }
                /*/
                foreach (Plane plane in planes.OrderBy(p => p.Index))
	            {
		            if (plane.Data == null)
		            {
			            continue;
		            }

		            int planeIndex = plane.Index;
		            if (planes.Count == 3)
		            {
			            planeIndex++;
		            }

		            for (int i = 0; i < plane.Data.Length && (i * 4) + (3 - plane.Index) < 4 * width * height; i++)
		            {
			            oB[(i * 4) + (3 - planeIndex)] = plane.Data[i];
		            }
	            }
                
	            if (planes.Count == 3)
	            {
		            for (int i = 0; i < planes.First().Data.Length && (i * 4) + (3) < 4 * width * height; i++)
		            {
			            oB[(i * 4) + (3)] = 255;
		            }
	            }
            }
            else if (imageFormat == ImageFormat.XRGB || imageFormat == ImageFormat.ARGB)
            {
	            Plane plane = planes.First();
	            int pixelSize = 4;

                for (int i = 0, j = 0; i + pixelSize < oB.Length && j + pixelSize < plane.Data.Length; i += 4, j += pixelSize)
	            {
			            oB[i + 3] = plane.Data[j + 0];
			            oB[i + 2] = plane.Data[j + 1];
			            oB[i + 1] = plane.Data[j + 2];
			            oB[i + 0] = plane.Data[j + 3];
	            }
            }

            Marshal.Copy(oB, 0, bmpdata.Scan0, bmpdata.Stride * bmpdata.Height);
            bmp.UnlockBits(bmpdata);

            return bmp;
        }
    }

    public class Plane
    {
        int index;
        byte[] data;
        CompressionMethod compressionMethod = CompressionMethod.None;
        byte[] output;
        public string TreeJson { get; set; }
        public int rleBytes { get; set; } = 1;
        public bool PoorCompression => output.Length / (data.Length * 1.0f) > 0.5f;

        public CompressionMethod Method
        {
            get => compressionMethod;
            set => compressionMethod = value;
        }

        public byte[] Data
        {
            get => data;
            set => data = value;
        }

        public byte[] Output
        {
            get => output;
            set => output = value;
        }

        public int DataSize => output.Length;
        public int Index => index;

        public Plane(int index)
        {
            this.index = index;
        }

        public void Compress(CompressionMethod method)
        {
            switch (method)
            {
                case CompressionMethod.None:
                    reorderData();
                    break;

                case CompressionMethod.RLE:
                    compressRLE();
                    break;

                case CompressionMethod.Huffman:
                    compressHuffman();
                    break;
            }
        }

        private void reorderData()
        {
            compressionMethod = CompressionMethod.None;

            output = new byte[data.Length];

            for (int i = 0; i + 3 < data.Length; i += 4)
            {
                output[i + 0] = data[i + 3];
                output[i + 1] = data[i + 2];
                output[i + 2] = data[i + 1];
                output[i + 3] = data[i + 0];
            }
        }

        private void compressRLE()
        {
            compressionMethod = CompressionMethod.RLE;

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
                            bw.Write((byte)count);
                            bw.Write(lastColour);
                        }

                        lastColour = data[i];
                        count = 0;
                    }

                    count++;
                }

                output = ms.ToArray();
            }
        }

        public byte[] BuildTreeToByteArray()
        {
	        Tree tree = new Tree();
	        tree.BuildTree(data);
	        return tree.ToByteArray();
        }
        private void compressHuffman()
        {
            compressionMethod = CompressionMethod.Huffman;

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

                output = ms.ToArray();
            }
        }

        public void Decompress(CompressionMethod method)
        {
            switch (method)
            {
                case CompressionMethod.None:
                    //deorderData();
                    break;

                case CompressionMethod.RLE:
                    decompressRLE();
                    break;

                case CompressionMethod.Huffman:
                    decompressHuffman();
                    break;
            }
        }

        private void decompressRLE()
        {
            compressionMethod = CompressionMethod.RLE;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                for (int i = 0; i+rleBytes < output.Length; )
                {
                    int count = output[i + 0];
                    bool isRepeat = (count & 0x80) == 0;

                    if (!isRepeat)
                    {
	                    count = count & 0x7f;
	                    for (int k = 1; k <= count; k++)
	                    {
							bw.Write(output[i + k]);
	                    }

	                    i += 1 + count;
                        continue;
                    }

                    byte[] colour = new byte[rleBytes];

                    for (int k = 0; k < rleBytes; k++)
                    {
	                    colour[k] = output[i + 1 + k];
                    }

                    for (int j = 0; j < count; j++)
                    {
	                    for (int k = 0; k < rleBytes; k++)
	                    {
							bw.Write(colour[k]);
	                    }
                        
                    }

                    i += 1 + rleBytes;
                }

                data = ms.ToArray();
            }
        }

        private void decompressHuffman()
        {
            compressionMethod = CompressionMethod.Huffman;

            Tree tree = new Tree();

            using (MemoryStream msIn = new MemoryStream(output))
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
                
                File.WriteAllText($"decompress_plane{index}_tree.json", TreeJson);
	             
                bw.Write(tree.Decode(br.ReadBytes(output.Length - (int)br.BaseStream.Position)));

                data = msOut.ToArray();
            }
        }
    }
}