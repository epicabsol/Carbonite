using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbonite
{
    /// <summary>
    /// Writes data to a contiguous block of memory in a Carbonite Image.
    /// </summary>
    internal class CarboniteImagePageWriter : ICarboniteImagePageWriter, IDisposable
    {
        /// <summary>
        /// The writer of the Carbonite Image that this page is being written to.
        /// </summary>
        private CarboniteImageWriter ImageWriter { get; }

        /// <summary>
        /// The owner of the memory used by this page.
        /// </summary>
        private IMemoryOwner<byte> MemoryOwner { get; }

        /// <summary>
        /// The data in this page.
        /// </summary>
        public Memory<byte> Memory => this.MemoryOwner.Memory.Slice(0, this.Length);

        /// <summary>
        /// The offset of the beginning of this page relative to the image header.
        /// </summary>
        public long StartOffset { get; }

        /// <summary>
        /// The length of this page in bytes.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Creates a new page writer for the given image.
        /// </summary>
        /// <param name="imageWriter">The writer of the Carbonite Image that the page is being written to.</param>
        /// <param name="startOffset">The offset of the beginning of the page relative to the image header.</param>
        /// <param name="length">The length of this page in bytes.</param>
        public CarboniteImagePageWriter(CarboniteImageWriter imageWriter, long startOffset, int length)
        {
            this.ImageWriter = imageWriter;
            this.MemoryOwner = MemoryPool<byte>.Shared.Rent(length);
            this.StartOffset = startOffset;
            this.Length = length;
        }

        /// <summary>
        /// Writes the offset of the given page (plus the given pointer offset) to the given offset in this page,
        /// and adds the offset to the list of pointers in the image.
        /// </summary>
        /// <param name="pageOffset">The offset in this page to write the pointer at.</param>
        /// <param name="page">The page to write a pointer to. If <c>null</c>, a null pointer is written.</param>
        /// <param name="pointerOffset">An offset into the given page to adjust the pointer written by. Ignored if <c>page</c> is <c>null</c>.</param>
        private void WritePagePointer(int pageOffset, CarboniteImagePageWriter? page, int pointerOffset)
        {
            if (page != null)
            {
                BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), (ulong)(page.StartOffset + pointerOffset));
                this.ImageWriter.AddPointerOffset((ulong)(this.StartOffset + pageOffset));
            }
            else
            {
                BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), 0ul);
            }
        }

        /// <summary>
        /// Writes an array property to this page that points to a new empty page for storing the array elements.
        /// </summary>
        /// <param name="pageOffset">The offset in this page to write the array property at.</param>
        /// <param name="count">The number of elements of the given size that the array will contain.</param>
        /// <param name="elementSize">The size of each of the elements in the array.</param>
        /// <returns>The page writer for the array data page that was created, or <c>null</c> if <c>count</c> was zero.</returns>
        private CarboniteImagePageWriter? WriteArray(int pageOffset, int count, int elementSize)
        {
            // Write the count
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), (ulong)count);

            if (count > 0)
            {
                // Allocate a new page for the array elements
                CarboniteImagePageWriter newPage = this.ImageWriter.AppendPage(elementSize * count);

                // Write the pointer to the current page
                this.WritePagePointer(pageOffset + sizeof(ulong), newPage, 0);

                return newPage;
            }
            else
            {
                this.WritePagePointer(pageOffset + sizeof(ulong), null, 0);
                return null;
            }
        }

        /// <summary>
        /// Writes an array property to this given page with the given primitive elements.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements in the array of primitives to write.</typeparam>
        /// <param name="pageOffset">The offset in this page to write the array property at.</param>
        /// <param name="elements">The elements in the array to write.</param>
        private void WritePrimitiveArray<TElement>(int pageOffset, ReadOnlySpan<TElement> elements) where TElement : unmanaged
        {
            int elementSize = System.Runtime.InteropServices.Marshal.SizeOf<TElement>();
            CarboniteImagePageWriter? newPage = this.WriteArray(pageOffset, elements.Length, elementSize);
            if (newPage != null)
            {
                System.Runtime.InteropServices.MemoryMarshal.AsBytes(elements).CopyTo(newPage.Memory.Span);
            }
        }

        #region ICarboniteImagePageWriter

        public void Write(int pageOffset, sbyte value)
        {
            this.Memory.Span[pageOffset] = (byte)value;
        }

        public void Write(int pageOffset, byte value)
        {
            this.Memory.Span[pageOffset] = value;
        }

        public void Write(int pageOffset, short value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, ushort value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, int value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, uint value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, long value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, ulong value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, float value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, double value)
        {
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), value);
        }

        public void Write(int pageOffset, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value + '\0');

            // Write the byte count (not including the null we appended)
            BitConverter.TryWriteBytes(this.Memory.Span.Slice(pageOffset), (ulong)bytes.Length - 1);

            if (value.Length > 0)
            {
                // Allocate a new page for the character bytes
                CarboniteImagePageWriter newPage = this.ImageWriter.AppendPage(bytes.Length);

                // Write the pointer to the current page
                this.WritePagePointer(pageOffset + sizeof(ulong), newPage, 0);

                // Write the character data
                bytes.CopyTo(newPage.Memory);
            }
            else
            {
                // Write a pointer to the first byte of the length, which has the value zero, which will function as a single null terminating character
                this.WritePagePointer(pageOffset + sizeof(ulong), this, pageOffset);
            }
        }

        public void Write<TFreezable>(int pageOffset, in TFreezable value) where TFreezable : IFreezable<TFreezable>
        {
            if (TFreezable.IsReferenceType)
            {
                this.WriteReference(pageOffset, value);
            }
            else
            {
                TFreezable.Freeze(this, pageOffset, in value);
            }
        }

        private void WriteReference<TFreezable>(int pageOffset, TFreezable value) where TFreezable : IFreezable<TFreezable>
        {
            // Allocate a new page for the value
            CarboniteImagePageWriter newPage = this.ImageWriter.AppendPage(TFreezable.FrozenSize);

            // Write the pointer to the current page
            this.WritePagePointer(pageOffset, newPage, 0);

            // Write the value to the new page
            TFreezable.Freeze(newPage, 0, in value);
        }

        public void Write(int pageOffset, ReadOnlySpan<sbyte> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<byte> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<short> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<ushort> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<int> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<uint> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<long> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<ulong> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<float> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<double> values)
        {
            this.WritePrimitiveArray(pageOffset, values);
        }

        public void Write(int pageOffset, ReadOnlySpan<string> values)
        {
            int elementSize = CarboniteImageWriter.ArraySize;
            ICarboniteImagePageWriter? newPage = this.WriteArray(pageOffset, values.Length, elementSize);
            if (newPage != null)
            {
                // Write the array elements to the new page
                for (int i = 0; i < values.Length; i++)
                {
                    newPage.Write(elementSize * i, values[i]);
                }
            }
        }

        public void Write<TFreezable>(int pageOffset, ReadOnlySpan<TFreezable> values) where TFreezable : IFreezable<TFreezable>
        {
            int elementSize = TFreezable.IsReferenceType ? CarboniteImageWriter.PointerSize : TFreezable.FrozenSize;
            ICarboniteImagePageWriter? newPage = this.WriteArray(pageOffset, values.Length, elementSize);
            if (newPage != null)
            {
                // Write the array elements to the new page
                for (int i = 0; i < values.Length; i++)
                {
                    newPage.Write(TFreezable.FrozenSize * i, values[i]);
                }
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            this.MemoryOwner.Dispose();
        }

        #endregion
    }
}
