﻿using System;
using System.ComponentModel;

namespace ToxicRagers.Helpers
{
    [TypeConverter(typeof(Vector3Converter))]
    public class Vector3 : IEquatable<Vector3>
    {
        private float _x;
        private float _y;
        private float _z;

        public float X { get => _x; set => _x = value; }
        public float Y { get => _y; set => _y = value; }
        public float Z { get => _z; set => _z = value; }

        public Vector3(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Vector3(Vector3 v)
        {
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
        }

        public static Vector3 Up => new Vector3(0, 1, 0);
        public static Vector3 Zero => new Vector3(0, 0, 0);
        public Vector3 Normalised
        {
            get
            {
                Vector3 v = new Vector3(this);
                v.Normalise();
                return v;
            }
        }

        public override string ToString()
        {
            return string.Format("{{X: {0,15:F9} Y: {1,15:F9} Z: {2,15:F9} }}", _x, _y, _z);
        }

        public float Sum()
        {
            return _x + _y + _z;
        }

        public static Vector3 TransformVector(Vector3 v, Matrix3D m)
        {
            return v * m;
        }

        public static Vector3 Parse(string v)
        {
            v = v.Replace(" ", "");
            string[] s = v.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return new Vector3(s[0].ToSingle(), s[1].ToSingle(), s[2].ToSingle());
        }

        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(
                v1.Y * v2.Z - v1.Z * v2.Y,
                v1.Z * v2.X - v1.X * v2.Z,
                v1.X * v2.Y - v1.Y * v2.X
            );
        }

        public static float Distance(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v2.X - v1.X, 2) + Math.Pow(v2.Y - v1.Y, 2) + Math.Pow(v2.Z - v1.Z, 2));
        }

        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public float Length => (float)Math.Sqrt(LengthSquared);

        public float LengthSquared => _x * _x + _y * _y + _z * _z;

        public void Normalise()
        {
            float l = Length;
            _x /= l;
            _y /= l;
            _z /= l;
        }

        public static Vector3 operator *(float y, Vector3 x) { return x * y; }

        public static Vector3 operator *(Vector3 x, float y)
        {
            return new Vector3(x._x * y, x._y * y, x._z * y);
        }

        public static Vector3 operator *(Vector3 x, Vector3 y)
        {
            return new Vector3(x._x * y.X, x._y * y.Y, x._z * y.Z);
        }

        public static Vector3 operator -(Vector3 x, Vector3 y)
        {
            return new Vector3(x._x - y.X, x._y - y.Y, x._z - y.Z);
        }

        public static Vector3 operator -(Vector3 x)
        {
            return new Vector3(-x._x, -x._y, -x._z);
        }

        public static Vector3 operator +(Vector3 x, Vector3 y)
        {
            return new Vector3(x._x + y.X, x._y + y.Y, x._z + y.Z);
        }

        public static Vector3 operator /(Vector3 x, float y)
        {
            return new Vector3(x._x / y, x._y / y, x._z / y);
        }

        public static Vector3 operator *(Vector3 x, Matrix3D y)
        {
            Vector3 r = new Vector3(0, 0, 0)
            {
                X = (x.X * y.M11) + (x.Y * y.M21) + (x.Z * y.M31),
                Y = (x.X * y.M12) + (x.Y * y.M22) + (x.Z * y.M32),
                Z = (x.X * y.M13) + (x.Y * y.M23) + (x.Z * y.M33)
            };

            r += y.Position;

            return r;
        }

        public static bool operator ==(Vector3 x, Vector3 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Vector3 x, Vector3 y)
        {
            return !x.Equals(y);
        }

        public bool Equals(Vector3 other)
        {
            return (X == other.X && Y == other.Y && Z == other.Z);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }
            return Equals(obj as Vector3);
        }
    }

    public class Vector3Converter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Vector3)) { return true; }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector3)
            {
                Vector3 v = value as Vector3;

                return v.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}