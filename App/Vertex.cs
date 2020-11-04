namespace App
{
    /// <summary>
    /// Координаты точки в пространстве
    /// </summary>
    public struct Vertex
    {
        private float _x;
        private float _y;
        private float _z;


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

        public int IdInMesh { get; }

        public Vertex(float ix, float iy, float iz)
        {
            IdInMesh = 0;
            _x = ix;
            _y = iy;
            _z = iz;
        }

        public Vertex(Vertex v)
        {
            IdInMesh = v.IdInMesh;
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
        }

        public Vertex(int idInMesh,float ix, float iy, float iz)
        {
            IdInMesh = idInMesh;
            _x = ix;
            _y = iy;
            _z = iz;
        }

        public bool Equals(Vertex o)
        {
            if (_x==o.X && _y==o.Y && _z==o.Z)
                return true;
            return false;
        }

        public void Set(Vertex t1)
        {
            _x = t1.X;
            _y = t1.Y;
            _z = t1.Z;
        }
        public void Sub(Vertex t1)
        {
            _x -= t1.X;
            _y -= t1.Y;
            _z -= t1.Z;
        }

        public void Scale(float s)
        {
            _x *= s;
            _y *= s;
            _z *= s;
        }

        public void Add(Vertex t1)
        {
            _x += t1.X;
            _y += t1.Y;
            _z += t1.Z;
        }
    }
}