using System;

namespace AppLib
{
    public struct StlNormal
    {
        public bool IsZero => X == 0.0f && Y == 0.0f && Z == 0.0f;

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public StlNormal(float x, float y, float z): this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public StlNormal Normalize()
        {
            var length = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            if (length == 0.0f)
                return new StlNormal();
            return new StlNormal(X / length, Y / length, Z / length);
        }
    }
}
