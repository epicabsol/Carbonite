using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbonite
{
    /// <summary>
    /// Provides a mechanism for writing values to a contiguous block of memory in a Carbonite Image.
    /// </summary>
    public interface ICarboniteImagePageWriter
    {
        /// <summary>
        /// Writes the given <see cref="SByte"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, sbyte value);

        /// <summary>
        /// Writes the given <see cref="Byte"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, byte value);

        /// <summary>
        /// Writes the given <see cref="Int16"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, short value);

        /// <summary>
        /// Writes the given <see cref="UInt16"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, ushort value);

        /// <summary>
        /// Writes the given <see cref="Int32"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, int value);

        /// <summary>
        /// Writes the given <see cref="UInt32"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, uint value);

        /// <summary>
        /// Writes the given <see cref="Int64"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, long value);

        /// <summary>
        /// Writes the given <see cref="UInt64"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, ulong value);

        /// <summary>
        /// Writes the given <see cref="Single"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, float value);

        /// <summary>
        /// Writes the given <see cref="Double"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, double value);

        /// <summary>
        /// Writes the given <see cref="String"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write(int pageOffset, string value);

        /// <summary>
        /// Writes the given <see cref="IFreezable{TSelf}"/> at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the value at.</param>
        /// <param name="value">The value to write.</param>
        public void Write<TFreezable>(int pageOffset, in TFreezable value) where TFreezable : IFreezable<TFreezable>;

        /// <summary>
        /// Writes an array of <see cref="SByte"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<sbyte> values);

        /// <summary>
        /// Writes an array of <see cref="Byte"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<byte> values);

        /// <summary>
        /// Writes an array of <see cref="Int16"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<short> values);

        /// <summary>
        /// Writes an array of <see cref="UInt16"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<ushort> values);

        /// <summary>
        /// Writes an array of <see cref="Int32"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<int> values);

        /// <summary>
        /// Writes an array of <see cref="UInt32"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<uint> values);

        /// <summary>
        /// Writes an array of <see cref="Int64"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<long> values);

        /// <summary>
        /// Writes an array of <see cref="UInt64"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<ulong> values);

        /// <summary>
        /// Writes an array of <see cref="Single"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<float> values);

        /// <summary>
        /// Writes an array of <see cref="Double"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<double> values);

        /// <summary>
        /// Writes an array of <see cref="String"/>s at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write(int pageOffset, ReadOnlySpan<string> values);

        /// <summary>
        /// Writes an array of freezable values at the given offset.
        /// </summary>
        /// <param name="pageOffset">The offset into the page to write the array at.</param>
        /// <param name="values">The array elements to write.</param>
        public void Write<TFreezable>(int pageOffset, ReadOnlySpan<TFreezable> values) where TFreezable : IFreezable<TFreezable>;
    }
}
