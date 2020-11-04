using System;

namespace App
{
    public struct StlNormal
    {
        private float _x;
        private float _y;
        private float _z;

        public bool IsZero => X == 0.0f && Y == 0.0f && Z == 0.0f;

        public float X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        public float Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }

        public float Z
        {
            get
            {
                return _z;
            }

            set
            {
                _z = value;
            }
        }

        public StlNormal(float x, float y, float z): this()
        {
            _x = x;
            _y = y;
            _z = z;
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
