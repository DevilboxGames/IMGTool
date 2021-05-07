﻿using System;
using System.ComponentModel;

namespace ToxicRagers.Helpers
{
    [TypeConverter(typeof(Vector2Converter))]
    public class Vector2 : IEquatable<Vector2>
    {
        float _x;
        float _y;

        public Vector2(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public float X
        {
            get => _x;
            set => _x = value;
        }

        public float Y
        {
            get => _y;
            set => _y = value;
        }

        public static Vector2 Zero => new Vector2(0, 0);

        public static Vector2 Parse(string v)
        {
            v = v.Replace(" ", "");

            string[] s = v.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return new Vector2(s[0].ToSingle(), s[1].ToSingle());
        }

        public override string ToString()
        {
            return string.Format("{{X: {0,15:F9} Y: {1,15:F9} }}", _x, _y);
        }

        public static Vector2 operator *(Vector2 x, float y)
        {
            return new Vector2(x._x * y, x._y * y);
        }

        public static bool operator ==(Vector2 x, Vector2 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Vector2 x, Vector2 y)
        {
            return !x.Equals(y);
        }

        public bool Equals(Vector2 other)
        {
            return (X == other.X && Y == other.Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }
            return Equals(obj as Vector2);
        }

        public float Length => (float)Math.Sqrt(Dot(this, this));
        public Vector2 Normalised => new Vector2(X / Length, Y / Length);

        public static float Dot(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }
    }

    public class Vector2Converter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Vector2)) { return true; }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector2)
            {
                Vector2 v = value as Vector2;

                return v.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}