﻿#region License
/*
MIT License

Copyright(c) 2017-2018 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Globalization;

namespace MeshDecimator.Math
{
    /// <summary>
    /// A single precision 2D vector.
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>
    {
        #region Static Read-Only
        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector2 zero = new Vector2(0, 0);
        #endregion

        #region Consts
        /// <summary>
        /// The vector epsilon.
        /// </summary>
        public const float Epsilon = 9.99999944E-11f;
        #endregion

        #region Fields
        /// <summary>
        /// The x component.
        /// </summary>
        public float x;
        /// <summary>
        /// The y component.
        /// </summary>
        public float y;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the magnitude of this vector.
        /// </summary>
        public float Magnitude
        {
            get { return (float)System.Math.Sqrt(x * x + y * y); }
        }

        /// <summary>
        /// Gets the squared magnitude of this vector.
        /// </summary>
        public float MagnitudeSqr
        {
            get { return (x * x + y * y); }
        }

        /// <summary>
        /// Gets a normalized vector from this vector.
        /// </summary>
        public Vector2 Normalized
        {
            get
            {
                Vector2 result;
                Normalize(ref this, out result);
                return result;
            }
        }

        /// <summary>
        /// Gets or sets a specific component by index in this vector.
        /// </summary>
        /// <param name="index">The component index.</param>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new vector with one value for all components.
        /// </summary>
        /// <param name="value">The value.</param>
        public Vector2(float value)
        {
            this.x = value;
            this.y = value;
        }

        /// <summary>
        /// Creates a new vector.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        /// <summary>
        /// Scales the vector uniformly.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The scaling value.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector2 operator *(Vector2 a, float d)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        /// <summary>
        /// Scales the vector uniformly.
        /// </summary>
        /// <param name="d">The scaling value.</param>
        /// <param name="a">The vector.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector2 operator *(float d, Vector2 a)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        /// <summary>
        /// Divides the vector with a float.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The dividing float value.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector2 operator /(Vector2 a, float d)
        {
            return new Vector2(a.x / d, a.y / d);
        }

        /// <summary>
        /// Subtracts the vector from a zero vector.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }

        /// <summary>
        /// Returns if two vectors equals eachother.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>If equals.</returns>
        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return (lhs - rhs).MagnitudeSqr < Epsilon;
        }

        /// <summary>
        /// Returns if two vectors don't equal eachother.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>If not equals.</returns>
        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return (lhs - rhs).MagnitudeSqr >= Epsilon;
        }

        /// <summary>
        /// Explicitly converts from a double-precision vector into a single-precision vector.
        /// </summary>
        /// <param name="v">The double-precision vector.</param>
        public static explicit operator Vector2(Vector2d v)
        {
            return new Vector2((float)v.x, (float)v.y);
        }

        /// <summary>
        /// Implicitly converts from an integer vector into a single-precision vector.
        /// </summary>
        /// <param name="v">The integer vector.</param>
        public static implicit operator Vector2(Vector2i v)
        {
            return new Vector2(v.x, v.y);
        }
        #endregion

        #region Public Methods
        #region Instance
        /// <summary>
        /// Set x and y components of an existing vector.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        public void Set(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Multiplies with another vector component-wise.
        /// </summary>
        /// <param name="scale">The vector to multiply with.</param>
        public void Scale(ref Vector2 scale)
        {
            x *= scale.x;
            y *= scale.y;
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        public void Normalize()
        {
            float mag = this.Magnitude;
            if (mag > Epsilon)
            {
                x /= mag;
                y /= mag;
            }
            else
            {
                x = y = 0;
            }
        }

        /// <summary>
        /// Clamps this vector between a specific range.
        /// </summary>
        /// <param name="min">The minimum component value.</param>
        /// <param name="max">The maximum component value.</param>
        public void Clamp(float min, float max)
        {
            if (x < min) x = min;
            else if (x > max) x = max;

            if (y < min) y = min;
            else if (y > max) y = max;
        }
        #endregion

        #region Object
        /// <summary>
        /// Returns a hash code for this vector.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2;
        }

        /// <summary>
        /// Returns if this vector is equal to another one.
        /// </summary>
        /// <param name="other">The other vector to compare to.</param>
        /// <returns>If equals.</returns>
        public override bool Equals(object other)
        {
            if (!(other is Vector2))
            {
                return false;
            }
            Vector2 vector = (Vector2)other;
            return (x == vector.x && y == vector.y);
        }

        /// <summary>
        /// Returns if this vector is equal to another one.
        /// </summary>
        /// <param name="other">The other vector to compare to.</param>
        /// <returns>If equals.</returns>
        public bool Equals(Vector2 other)
        {
            return (x == other.x && y == other.y);
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return string.Format("({0}, {1})",
                x.ToString("F1", CultureInfo.InvariantCulture),
                y.ToString("F1", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        /// <param name="format">The float format.</param>
        /// <returns>The string.</returns>
        public string ToString(string format)
        {
            return string.Format("({0}, {1})",
                x.ToString(format, CultureInfo.InvariantCulture),
                y.ToString(format, CultureInfo.InvariantCulture));
        }
        #endregion

        #region Static
        /// <summary>
        /// Dot Product of two vectors.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        public static float Dot(ref Vector2 lhs, ref Vector2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }
        
        /// <summary>
        /// Performs a linear interpolation between two vectors.
        /// </summary>
        /// <param name="a">The vector to interpolate from.</param>
        /// <param name="b">The vector to interpolate to.</param>
        /// <param name="t">The time fraction.</param>
        /// <param name="result">The resulting vector.</param>
        public static void Lerp(ref Vector2 a, ref Vector2 b, float t, out Vector2 result)
        {
            result = new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        /// <summary>
        /// Multiplies two vectors component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <param name="result">The resulting vector.</param>
        public static void Scale(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result = new Vector2(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        /// Normalizes a vector.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <param name="result">The resulting normalized vector.</param>
        public static void Normalize(ref Vector2 value, out Vector2 result)
        {
            float mag = value.Magnitude;
            if (mag > Epsilon)
            {
                result = new Vector2(value.x / mag, value.y / mag);
            }
            else
            {
                result = Vector2.zero;
            }
        }
        #endregion
        #endregion
    }
}