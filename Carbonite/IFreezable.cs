using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbonite
{
    /// <summary>
    /// A type that can be frozen to an <see cref="ICarboniteImagePageWriter"/>.
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    public interface IFreezable<TSelf> where TSelf : IFreezable<TSelf>
    {
        /// <summary>
        /// Freezes the given value to the given Carbonite page writer.
        /// </summary>
        /// <param name="pageWriter">The page writer to freeze to.</param>
        /// <param name="pageOffset">The offset into the given page writer to freeze at.</param>
        /// <param name="value">The value to be frozen.</param>
        public static abstract void Freeze(ICarboniteImagePageWriter pageWriter, int pageOffset, in TSelf value);

        /// <summary>
        /// The size of values of this type when frozen to a Carbonite Image.
        /// </summary>
        public static abstract int FrozenSize { get; }

        /// <summary>
        /// Whether values of this type should be frozen into their own page in the image.
        /// </summary>
        public static abstract bool IsReferenceType { get; }
    }
}
