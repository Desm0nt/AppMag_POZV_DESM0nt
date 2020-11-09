namespace AppLib
{
    /// <summary>
    /// Координаты точки в пространстве
    /// </summary>
    public struct Vertex
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public int IdInMesh { get; }

        public Vertex(float ix, float iy, float iz)
        {
            IdInMesh = 0;
            X = ix;
            Y = iy;
            Z = iz;
        }

        public Vertex(Vertex v)
        {
            IdInMesh = v.IdInMesh;
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public Vertex(int idInMesh,float ix, float iy, float iz)
        {
            IdInMesh = idInMesh;
            X = ix;
            Y = iy;
            Z = iz;
        }

        public bool Equals(Vertex o)
        {
            if (X==o.X && Y==o.Y && Z==o.Z)
                return true;
            return false;
        }

        public void Set(Vertex t1)
        {
            X = t1.X;
            Y = t1.Y;
            Z = t1.Z;
        }
        public void Sub(Vertex t1)
        {
            X -= t1.X;
            Y -= t1.Y;
            Z -= t1.Z;
        }

        public void Scale(float s)
        {
            X *= s;
            Y *= s;
            Z *= s;
        }

        public void Add(Vertex t1)
        {
            X += t1.X;
            Y += t1.Y;
            Z += t1.Z;
        }
    }
}