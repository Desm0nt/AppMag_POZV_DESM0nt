using System.Collections.Generic;
using System.IO;

namespace AppLib
{
    public class StlFile
    {
        public string SolidName { get; set; }

        public List<MyTriangle> Triangles { get; set; }

        public StlFile()
        {
            Triangles = new List<MyTriangle>();
        }

        public void Save(Stream stream, bool asAscii = true)
        {
            var writer = new StlWriter();
            writer.Write(this, stream, asAscii);
        }

    }
}