 using System;
 using System.Globalization;
 using System.Runtime.InteropServices;
 using UnityEngine;

 /// <summary>Represents a color, stored in the KML AABBGGRR format.</summary>
 [StructLayout(LayoutKind.Explicit)]
    public struct UINTColor32 : IComparable<UINTColor32>, IEquatable<UINTColor32>
    {
        [FieldOffset(0)]
        private uint _rgba;

        [FieldOffset(0)] public byte r;
        [FieldOffset(1)] public byte g;
        [FieldOffset(2)] public byte b;
        [FieldOffset(3)] public byte a;

        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.r;
                    case 1:
                        return this.g;
                    case 2:
                        return this.b;
                    case 3:
                        return this.a;
                    default:
                        throw new IndexOutOfRangeException("Invalid Color index(" + index.ToString() + ")!");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the Color32 struct to the specified
        /// color components.
        /// </summary>
        /// <param name="alpha">The alpha component value.</param>
        /// <param name="blue">The blue component value.</param>
        /// <param name="green">The green component value.</param>
        /// <param name="red">The red component value.</param>
        public UINTColor32(byte red, byte green, byte blue, byte alpha)
        {
            _rgba = (uint)(red) | (uint)(green << 8) | (uint)(blue << 16) | (uint)alpha << 24;
            
            this.r = red;
            this.g = green;
            this.b = blue;
            this.a = alpha;
        }
        
        /// <summary>
        /// Initializes a new instance of the Color32 struct to the specified
        /// ABGR value.
        /// </summary>
        /// <param name="abgr">An integer containing the ABGR color value.</param>
        public UINTColor32(int rgba)
        {
            _rgba = (uint)rgba;
            r = (byte)(_rgba);
            g = (byte)(_rgba >> 8);
            b = (byte)(_rgba >> 16);
            a = (byte)(_rgba >> 24);
        }
        
        public uint Rgba
        {
            get
            {
                return _rgba;
            }
        }

        public void SetRGBA(uint rgba)
        {
            _rgba = rgba;
            r = (byte)(_rgba);
            g = (byte)(_rgba >> 8);
            b = (byte)(_rgba >> 16);
            a = (byte)(_rgba >> 24);
        }

        /// <summary>Gets the red component value.</summary>

        /// <summary>
        /// Determines whether two specified Color32s have the same value.
        /// </summary>
        /// <param name="colorA">The first Color32 to compare.</param>
        /// <param name="colorB">The second Color32 to compare.</param>
        /// <returns>
        /// true if the value of the two colors is the same; otherwise, false.
        /// </returns>
        public static bool operator ==(UINTColor32 colorA, UINTColor32 colorB)
        {
            // Can't use (a == null) because that would call this method again!
            if (object.ReferenceEquals(colorA, null))
            {
                return object.ReferenceEquals(colorB, null);
            }
            return colorA.Equals(colorB);
        }

        /// <summary>
        /// Determines whether two specified Color32s have different values.
        /// </summary>
        /// <param name="colorA">The first Color32 to compare.</param>
        /// <param name="colorB">The second Color32 to compare.</param>
        /// <returns>
        /// true if the value of the colors is different; otherwise, false.
        /// </returns>
        public static bool operator !=(UINTColor32 colorA, UINTColor32 colorB)
        {
            return !(colorA == colorB);
        }

        /// <summary>
        /// Determines whether the first specified Color32 is less than the second.
        /// </summary>
        /// <param name="colorA">The first Color32 to compare.</param>
        /// <param name="colorB">The second Color32 to compare.</param>
        /// <returns>
        /// true is the value of colorA is less than the value of colorB;
        /// otherwise, false.
        /// </returns>
        public static bool operator <(UINTColor32 colorA, UINTColor32 colorB)
        {
            return colorA.CompareTo(colorB) < 0;
        }

        /// <summary>
        /// Determines whether the first specified Color32 is greater than the second.
        /// </summary>
        /// <param name="colorA">The first Color32 to compare.</param>
        /// <param name="colorB">The second Color32 to compare.</param>
        /// <returns>
        /// true is the value of colorA is greater than the value of colorB;
        /// otherwise, false.
        /// </returns>
        public static bool operator >(UINTColor32 colorA, UINTColor32 colorB)
        {
            return colorA.CompareTo(colorB) > 0;
        }

        /// <summary>
        /// Converts the string representation of a color hex value to a Color32.
        /// </summary>
        /// <param name="value">
        /// A string containing a hex color value to convert.
        /// </param>
        /// <returns>A Color32 representing the value parameter.</returns>
        public static UINTColor32 Parse(string value)
        {
            if (value == null)
            {
                return new UINTColor32(0);
            }
            uint converted = 0;
            int max = Math.Min(value.Length, 8); // We consider only the first eight characters significant.
            for (int i = 0; i < max; ++i)
            {
                // Always increase the color, even if the char isn't a valid number
                converted <<= 4; // Move along one hex - 2^4
                string letter = value[i].ToString();
                uint number;
                if (uint.TryParse(letter, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number))
                {
                    converted += number;
                }
            }
            return new UINTColor32((int)converted);
        }

        /// <summary>Creates a copy of this instance.</summary>
        /// <returns>
        /// A new Color32 instance with the same value of this instance.
        /// </returns>
        public UINTColor32 Clone()
        {
            return new UINTColor32((int)_rgba);
        }

        /// <summary>
        /// Compares this instance with a specified Color32 and indicates
        /// whether this instance precedes, follows, or appears in the same
        /// position in the sort order as the specified Color32.
        /// </summary>
        /// <param name="other">The Color32 to compare with this instance.</param>
        /// <returns>
        /// <list type="table"><item>
        /// <term>Less than zero</term>
        /// <description>This instance proceeds the value parameter.</description>
        /// </item><item>
        /// <term>Zero</term>
        /// <description>
        /// This instance has the same position in the sort order as the
        /// value parameter.
        /// </description>
        /// </item><item>
        /// <term>Greater than zero</term>
        /// <description>
        /// This instance follows the value parameter or the value parameter is null.
        /// </description>
        /// </item></list>
        /// </returns>
        public int CompareTo(UINTColor32 other)
        {
            return _rgba.CompareTo(other._rgba);
        }

        /// <summary>
        /// Determines whether this instance and the specified Color32 have the
        /// same value.
        /// </summary>
        /// <param name="other">The Color32 to compare to this instance.</param>
        /// <returns>
        /// true if the value of the value parameter is the same as this instance;
        /// otherwise, false.
        /// </returns>
        public bool Equals(UINTColor32 other)
        {
            return _rgba == other._rgba;
        }

        /// <summary>
        /// Determines whether this instance and the specified object have the
        /// same value.
        /// </summary>
        /// <param name="obj">
        /// An object, which must be a Color32, to compare to this instance.
        /// </param>
        /// <returns>
        /// true if the object is a Color32 and the value of the object is the
        /// same as this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            UINTColor32? color = obj as UINTColor32?;
            return color.HasValue && this.Equals(color.Value);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return _rgba.GetHashCode();
        }

        /// <summary>
        /// Converts this instance to a string representing the hex value in
        /// ABGR format.
        /// </summary>
        /// <returns>
        /// The string representation of this instance in ABGR format.
        /// </returns>
        public override string ToString()
        {
            return _rgba.ToString("x8", CultureInfo.InvariantCulture);
        }
        
        public static implicit operator UINTColor32(Color c) => new UINTColor32((byte) Mathf.Round(Mathf.Clamp01(c.r) * (float) byte.MaxValue), (byte) Mathf.Round(Mathf.Clamp01(c.g) * (float) byte.MaxValue), (byte) Mathf.Round(Mathf.Clamp01(c.b) * (float) byte.MaxValue), (byte) Mathf.Round(Mathf.Clamp01(c.a) * (float) byte.MaxValue));
        
        public static implicit operator UINTColor32(Color32 c) => new UINTColor32(c.r, c.g, c.b, c.a);

        public static implicit operator Color(UINTColor32 c) => new Color((float) c.r / (float) byte.MaxValue, (float) c.g / (float) byte.MaxValue, (float) c.b / (float) byte.MaxValue, (float) c.a / (float) byte.MaxValue);
    }