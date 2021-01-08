using System;
using System.Numerics;

namespace RadarPlugin {
    public static class FloatEx {
        private const float Deg2RadVal = (float)Math.PI * 2f / 360f;
        private const float Rad2DegVal = 1f / Deg2RadVal;

        public static float Deg2Rad(this float degrees) {
            return degrees * Deg2RadVal;
        }

        public static float Rad2Deg(this float radians) {
            return radians * Rad2DegVal;
        }
    }

    public static class Vector3Ex {
        public static Vector3 Add(this Vector3 lhs, Vector3 rhs) {
            return new Vector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        public static Vector3 Subtract(this Vector3 lhs, Vector3 rhs) {
            return new Vector3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        public static Vector3 Rotate2D(this Vector3 vector3, float angleRadians) {
            return new Vector3 {
                X = vector3.X * (float)Math.Cos(angleRadians) - vector3.Z * (float)Math.Sin(angleRadians),
                Y = vector3.Y,
                Z = vector3.Z * (float)Math.Cos(angleRadians) + vector3.X * (float)Math.Sin(angleRadians)
            };
        }

        public static Vector3 Scale(this Vector3 vector3, float scale) {
            return new Vector3 {
                X = vector3.X * scale,
                Y = vector3.Y * scale,
                Z = vector3.Z * scale
            };
        }

        public static Vector2 ToVector2(this Vector3 vector3) {
            return new Vector2(vector3.X, vector3.Z);
        }
    }
}