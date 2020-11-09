namespace AppLib
{
    public class MyTriangle
    {
        public StlNormal Normal { get; }

        public Vertex Vertex1 { get; }

        public Vertex Vertex2 { get; }

        public Vertex Vertex3 { get; }

        public MyTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
            Normal = GetValidNormal();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            MyTriangle p = (MyTriangle)obj;
            return Equals(p);
        }

        private bool Equals(MyTriangle p)
        {
            bool flag = false;
            if (Vertex1.Equals(p.Vertex1))
            {
                if (Vertex2.Equals(p.Vertex2))
                {
                    if (Vertex3.Equals(p.Vertex3))
                    {
                        flag = true;
                    }
                }
                if (Vertex2.Equals(p.Vertex3))
                {
                    if (Vertex3.Equals(p.Vertex2))
                    {
                            flag = true;   
                    }
                }
            }
            if (Vertex1.Equals(p.Vertex2))
            {
                if (Vertex2.Equals(p.Vertex1))
                {
                    if (Vertex3.Equals(p.Vertex3))
                    {
                            flag = true;
                    }
                }
                if (Vertex2.Equals(p.Vertex3))
                {
                    if (Vertex3.Equals(p.Vertex1))
                    {
                            flag = true;
                    }
                }
            }
            if (Vertex1.Equals(p.Vertex3))
            {
                if (Vertex2.Equals(p.Vertex1))
                {
                    if (Vertex3.Equals(p.Vertex2))
                    {
                        flag = true;
                    }
                }
                if (Vertex2.Equals(p.Vertex2))
                {
                    if (Vertex3.Equals(p.Vertex1))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Normal.GetHashCode();
                hashCode = (hashCode*397) ^ Vertex1.GetHashCode();
                hashCode = (hashCode*397) ^ Vertex2.GetHashCode();
                hashCode = (hashCode*397) ^ Vertex3.GetHashCode();
                return hashCode;
            }
        }

        private StlNormal GetValidNormal()
        {
            if (!Normal.IsZero) return Normal;
            var u1 = Vertex2.X - Vertex1.X;
            var u2 = Vertex2.Y - Vertex1.Y;
            var u3 = Vertex2.Z - Vertex1.Z;
            var v1 = Vertex3.X - Vertex1.X;
            var v2 = Vertex3.Y - Vertex1.Y;
            var v3 = Vertex3.Z - Vertex1.Z;

            var i = u2 * v3 - u3 * v2;
            var j = u3 * v1 - u1 * v3;
            var k = u1 * v2 - u2 * v1;

            return new StlNormal(i, j, k);
        }
    }
}