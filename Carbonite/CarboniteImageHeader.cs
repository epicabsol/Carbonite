using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbonite
{
    /// <summary>
    /// The header of a Carbonite Image.
    /// </summary>
    internal struct CarboniteImageHeader
    {
        /// <summary>
        /// The expected value of <see cref="Magic"/> for valid Carbonite Images.
        /// </summary>
        /// <remarks>This value is 'CRBN' when written as little-endian.</remarks>
        public const uint CarboniteImageMagic = 0x4E425243u;

        /// <summary>
        /// The number of bytes that a <see cref="CarboniteImageHeader"/> spans in a Carbonite Image.
        /// </summary>
        public const int Size = sizeof(uint) + sizeof(uint) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong) + sizeof(ulong);


        /// <summary>
        /// The value that identifies this header. Valid images written by Carbonite will have a value of <see cref="CarboniteImageMagic"/>.
        /// </summary>
        public uint Magic { get; set; }

        /// <summary>
        /// A user-supplied version number to identify any changes to the format of the objects that are contained within the image.
        /// </summary>
        public uint PayloadFormatVersion { get; set; }

        /// <summary>
        /// The number of pointers within the Carbonite Image that need to be transformed from relative offsets when the image is loaded.
        /// </summary>
        public ulong PointerCount { get; set; }

        /// <summary>
        /// The offset of the list of offsets of pointers within the image that need to be transformed when the image is loaded, relative to the beginning of this header.
        /// </summary>
        public ulong PointerTableOffset { get; set; }

        /// <summary>
        /// The number of root objects that are contained within the image.
        /// </summary>
        public ulong RootObjectCount { get; set; }

        /// <summary>
        /// The offset of the list of offsets of the root objects within the image.
        /// </summary>
        public ulong RootObjectTableOffset { get; set; }

        /// <summary>
        /// Creates a new header with the given values.
        /// </summary>
        /// <param name="magic">The value that identifies this header as beginning a valid Carbonite Image.</param>
        /// <param name="payloadFormatVersion">A user-supplied version number for the format of the objects in the image.</param>
        /// <param name="pointerCount">The number of pointers that need to be transformed when the image is loaded.</param>
        /// <param name="pointerTableOffset">The offset of the list of pointers to be transformed when the image is loaded.</param>
        /// <param name="rootObjectCount">The number of root objects that are contained within the image.</param>
        /// <param name="rootObjectTableOffset">The offset of the list of root objects that are contained within the image.</param>
        public CarboniteImageHeader(uint magic, uint payloadFormatVersion, ulong pointerCount, ulong pointerTableOffset, ulong rootObjectCount, ulong rootObjectTableOffset)
        {
            this.Magic = magic;
            this.PayloadFormatVersion = payloadFormatVersion;
            this.PointerCount = pointerCount;
            this.PointerTableOffset = pointerTableOffset;
            this.RootObjectCount = rootObjectCount;
            this.RootObjectTableOffset = rootObjectTableOffset;
        }

        /// <summary>
        /// Writes this header to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The writer to write this header to.</param>
        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.Magic);
            writer.Write(this.PayloadFormatVersion);
            writer.Write(this.PointerCount);
            writer.Write(this.PointerTableOffset);
            writer.Write(this.RootObjectCount);
            writer.Write(this.RootObjectTableOffset);
        }
    }
}
