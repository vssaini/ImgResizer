using System;

namespace ImgResizer.Imaging
{
    /// <summary>
    ///     Represents the alignment of the watermark image
    /// </summary>
    public class Alignment
    {
        /// <summary>
        ///     Creates a new Alignment object
        /// </summary>
        public Alignment() : this(HorizontalAlignment.Center, VerticalAlignment.Middle)
        {
        }

        /// <summary>
        ///     Creates a new Alignment object
        /// </summary>
        /// <param name="horizontalAlignment">The horizontal alighment</param>
        /// <param name="verticalAlignment">The vertical alignment</param>
        public Alignment(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
        }

        /// <summary>
        ///     The horizontal alighment
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        ///     The vertical alignment
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var a = obj as Alignment;
            if ((Object) a == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (HorizontalAlignment == a.HorizontalAlignment) && (VerticalAlignment == a.VerticalAlignment);
        }

        public bool Equals(Alignment a)
        {
            // If parameter is null return false:
            if ((object) a == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (HorizontalAlignment == a.HorizontalAlignment) && (VerticalAlignment == a.VerticalAlignment);
        }

        public override int GetHashCode()
        {
            return (int) HorizontalAlignment ^ (int) VerticalAlignment;
        }

        public static bool operator ==(Alignment a, Alignment b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.HorizontalAlignment == b.HorizontalAlignment && a.VerticalAlignment == b.VerticalAlignment;
        }

        public static bool operator !=(Alignment a, Alignment b)
        {
            return !(a == b);
        }
    }
}