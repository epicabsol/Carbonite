using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbonite
{
    /// <summary>
    /// Writes a Carbonite Image to a <see cref="Stream"/>.
    /// </summary>
    public class CarboniteImageWriter : IDisposable
    {
        /// <summary>
        /// The size of a pointer in an image.
        /// </summary>
        public const int PointerSize = sizeof(ulong);

        /// <summary>
        /// The size of an array property in an image.
        /// </summary>
        /// <remarks>
        /// This is the number of bytes written in the property's container, not including the array elements.
        /// </remarks>
        public const int ArraySize = sizeof(ulong) + PointerSize;

        /// <summary>
        /// The size of a string property in an image.
        /// </summary>
        /// <remarks>
        /// This is the number of bytes written in the property's container, not including the character data.
        /// </remarks>
        public const int StringSize = ArraySize;


        /// <summary>
        /// The <see cref="Stream"/> that the image is being written to.
        /// </summary>
        public Stream BaseStream { get; }

        /// <summary>
        /// The user-supplied version number that identifies the format of the objects written to this image.
        /// </summary>
        public uint PayloadFormatVersion { get; }

        /// <summary>
        /// The <see cref="Stream.Position"/> in the <see cref="BaseStream"/> of the header of this image.
        /// </summary>
        private long HeaderOffset { get; }
        private List<ulong> PointerOffsets { get; } = new List<ulong>();
        private List<ulong> RootObjectOffsets { get; } = new List<ulong>();
        private List<CarboniteImagePageWriter> Pages { get; } = new List<CarboniteImagePageWriter>();

        /// <summary>
        /// Creates a writer for writing a new Carbonite Image to the given stream.
        /// </summary>
        /// <param name="baseStream">The <see cref="Stream"/> to write the new image to.</param>
        /// <param name="payloadFormatVersion">The user-supplied version number that identifies the format of the objects written to this image.</param>
        public CarboniteImageWriter(Stream baseStream, uint payloadFormatVersion)
        {
            this.BaseStream = baseStream;
            this.PayloadFormatVersion = payloadFormatVersion;
            this.HeaderOffset = this.BaseStream.Position;

            // Reserve space in the stream for the header
            for (int i = 0; i < CarboniteImageHeader.Size; i++)
            {
                this.BaseStream.WriteByte(0);
            }
        }

        /// <summary>
        /// Appends a new page with the given length to the image.
        /// </summary>
        /// <param name="length">The length of the new page, in bytes.</param>
        /// <returns>A <see cref="CarboniteImageWriter"/> for writing to the new page.</returns>
        internal CarboniteImagePageWriter AppendPage(int length)
        {
            long startOffset = this.BaseStream.Position - this.HeaderOffset;

            // Reserve space in the stream for the page
            this.BaseStream.SetLength(startOffset + length);
            this.BaseStream.Position = startOffset + length;

            CarboniteImagePageWriter newPage = new CarboniteImagePageWriter(this, startOffset, length);
            this.Pages.Add(newPage);
            return newPage;
        }

        /// <summary>
        /// Adds the given pointer offset to the list of pointer offsets to be transformed into pointers when the image is loaded.
        /// </summary>
        /// <param name="pointerOffset">The offset in the <see cref="BaseStream"/> where the pointer is located.</param>
        internal void AddPointerOffset(ulong pointerOffset)
        {
            this.PointerOffsets.Add(pointerOffset);
        }

        /// <summary>
        /// Writes the given freezable object to the image as a root object.
        /// </summary>
        /// <typeparam name="TFreezable">The type of object to write.</typeparam>
        /// <param name="value">The object to write.</param>
        public void WriteRootObject<TFreezable>(TFreezable value) where TFreezable : IFreezable<TFreezable>
        {
            CarboniteImagePageWriter newPage = this.AppendPage(TFreezable.FrozenSize);
            TFreezable.Freeze(newPage, 0, in value);
            this.RootObjectOffsets.Add((ulong)newPage.StartOffset);
        }

        public void Dispose()
        {
            using (BinaryWriter writer = new BinaryWriter(this.BaseStream, Encoding.UTF8, true))
            {
                // Go back and write the header
                CarboniteImageHeader header = new CarboniteImageHeader(CarboniteImageHeader.CarboniteImageMagic, this.PayloadFormatVersion, 
                    (ulong)this.PointerOffsets.Count, (ulong)this.BaseStream.Position, 
                    (ulong)this.RootObjectOffsets.Count, (ulong)(this.BaseStream.Position + this.PointerOffsets.Count * CarboniteImageWriter.PointerSize));
                this.BaseStream.Position = this.HeaderOffset;
                header.Write(writer);

                // Write the pages
                foreach (CarboniteImagePageWriter page in this.Pages)
                {
                    System.Diagnostics.Debug.Assert(this.BaseStream.Position == page.StartOffset);
                    writer.Write(page.Memory.Span);
                    page.Dispose();
                }

                // Write the pointer table
                System.Diagnostics.Debug.Assert(this.BaseStream.Position == (long)header.PointerTableOffset);
                foreach (ulong pointerOffset in this.PointerOffsets)
                {
                    writer.Write(pointerOffset);
                }

                // Write the root object table
                System.Diagnostics.Debug.Assert(this.BaseStream.Position == (long)header.RootObjectTableOffset);
                foreach (ulong rootObjectOffset in this.RootObjectOffsets)
                {
                    writer.Write(rootObjectOffset);
                }
            }
        }
    }
}