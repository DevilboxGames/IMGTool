using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IMGTool
{
    public enum IMGCompressionFormat
    {
        None,
        RLE,
        Huffman,
        LIC,
        MAX
    }

    public enum IMGColourFormat
    {
        BZ_IMAGE_FILE_XRGB, // RGB data, padded to 32Bits. This helps with loading.
        BZ_IMAGE_FILE_ARGB,
        BZ_IMAGE_FILE_ALPHA,    // When the image is an alpha only image. Made from gray scale images.
        BZ_IMAGE_FILE_CLUT4,
        BZ_IMAGE_FILE_CLUT8,
        BZ_IMAGE_FILE_XRGB_PLANE,       // XRGB data stored in planes and then RLE compressed for better compression
        BZ_IMAGE_FILE_ARGB_PLANE,       // ARGB data stored in planes and then RLE compressed for better compression
        BZ_IMAGE_FILE_XRGB_ANIMATION,   // XRGB animation - uses frame coherency info for better compression - on loading it is decompressed into a single IMG containing all the frames
        BZ_IMAGE_FILE_ARGB_ANIMATION,   // ARGB animation - uses frame coherency info for better compression - on loading it is decompressed into a single IMG containing all the frames
        BZ_IMAGE_FILE_A8L8
    }
    [Flags]
    public enum ImageFileFlags : ushort
    {
        do_not_compress = 0x01,			 // set if you don't want the textures compressed on the memory card
        compressed_file = 0x02,			 // set if the image file data is compressed (NOTE: used to be rle_encoded) - use bits 8, 9 & 10 to find out what sort of compression
        one_bit_alpha = 0x04,			 // set if the image is 1 bit alpha
        do_not_16bit = 0x08,			 // set if you don't want this down sampled to 16 bit
        attached_data = 0x10,			 // set if there is attached data
        do_not_down_sample = 0x20,		 // set if you don't want the image down sampled to a smaller MIP in low memory situations
        do_not_mipmap = 0x40,			 // set if you don't want MIPs
        is_a_cube_map_face = 0x80,		 // This image is a cube map face, after the first cube map face is found, there should be at least 5 more. 
        // if there are more than the expected 5 faces, then in future there could be more than one cube map in the file,
        // however, this is currently unimplemented
        fc_0 = 0x100,					// file compression LSB
        fc_1 = 0x200,					// file compression
        fc_2 = 0x400,					// file compression MSB - room for 8 styles which should be plenty
        crush_to_jpeg = 0x800,			// set if the IMG file should be compressed as a jpeg when it comes to Crushing it.
        dont_auto_jpeg = 0x1000,			// If set then files without crush_to_jpeg set wont be converted to jpeg by crushes convert all to jpeg option
        is_srgb = 0x2000,					// Image contains sRGB data.
        FREE_BIT_14 = 0x4000,				
        FREE_BIT_15 = 0x8000				
    }

    [Flags]
    public enum ImageDataFlags
    {
        texture,						 // 0=Picture, 1=texture
        locked,						 // 0=No lock, 1=locked and pointer/stride are valid
        alpha_1bit,					 // There is a 1bit alpha channel to define where pixels are drawn and where they are not.
        alpha_nbit,					 // The alpha channel has multiple bits to define translucency (This bit is also set for 4bit alpha formats)
        compressed,					 // texture is compressed using a platform dependent compression schema. Flagged as it can cause access problems.
        render_target,				 // texture is used as a device for rendering into
        do_not_compress,				 // Some textures should not be compress, such as detailed GUI GFX and bump maps.
        do_not_16bit,				 // Do not store RGB/argb textures in a platform 16Bit texture format.
        do_not_save,					 // Lets some tool block saving of textures specific textures.
        do_not_down_sample,			 // Stop fonts etc. getting corrupted (or will do)
        loaded_from_tex,				 // texture was loaded from an old .TEX file - this won't save out, and the getRGB stuff won't work.
        do_not_mipmap,				 // Image will not be mip mapped
        multi_image,					 // Image has been loaded from a multiple image file
        make_linear,					
        lockedsystem,				
        swizzled,					 // Been swizzled by the platform you are running on, added for XBOX.
        main_memory,					 // Loaded in main memory - can't be used by hardware
        do_not_Interpolate_alpha,	 // The alpha channel is not interpolated, such as DXT3
        using_workspace,				 // The texture has not been locked - some workspace has been allocated ready to be copied into the texture
        depth_buffer,				 // Texture is used as a z/stencil buffer
        crush_to_jpeg,				 // Texture will be compressed as JPEG when it gets crushed
        dont_auto_jpeg,				 // Texture will never be compressed as JPEG when it gets crushed (even if crush was told to convert all to JPEG)
        readable_and_renderable,		 // creates a hardware texture that can be read back as well as rendered. slower than non readable but usefull for tools
        dont_create_depth_resolve_target,	 // Tells the system that we dont need to create a texture to resolve the depth buffer into.
        hardware_tex_padded_to_pow2,	 // The hardware image resolution has been padded to a power of 2, when the image is used, it will have to be scaled so it fits properly.
        ready,						 // Threaded processing has finished, and this texture is ready to use
        cleared,						 // Used for shadow stuff
        alpha_only,					 // Set if the texture only contains an alpha channel.
        is_srgb,						 // Image contains sRGB data.
        use_as_shader_resource,		 // Image needs to be bound as the input to a shader
        non_power_of_two			 // Set if texture is not power of two. Used by ipad, as they need special clamp setting.
    }
    public class IMGHeader
    {
        public byte[] Identifier = new byte[8];
        public ushort Version;
        public ushort Rawflags;
        public byte ColourFormat;
        public byte NumImages;
        public uint Size;
        public ushort Width, Height;
        public uint JPGQuality;

        public ImageFileFlags Flags
        {
            get { return (ImageFileFlags) Rawflags; }
            set { Rawflags = (ushort)value; }
        }
    }
    public class IMGFile
    {


    }
}
