using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Materials
{
    public class MaterialStorage
    {
        public static List<Material> Materials
        {
            get
            {
                List<Material> materials = new List<Material>()
                {
                    //new Material(1, "cortical bone", 18350, 0.3, 2.020e-6),
                    new Material(1, "cortical bone", 18350e6, 0.3, 2020),
                    new Material(2, "cancellous bone", 350, 0.2, 2.020e-6),
                    //new Material(2, "cancellous bone", 350e6, 0.3, 2020),
                    new Material(3, "intervertebral disc", 57, 0.4, 1.0903e-6),
                    //new Material(3, "intervertebral disc", 57e6, 0.4, 1090),
                    new Material(4, "steel", 2e5, 0.3, 7.85e-6),
                    new Material(5, "plastic", 2.2059e5, 0.22, 2.3e-6),
                    //new Material(6, "steel", 2.1e11, 0.28, 7700)
                    new Material(6, "alloy steel", 2.1e5, 0.28, 7.7e-6),
                    new Material(7, "ground", 9e9, 0.3, 7700)
                };
                return materials;
            }
        }
    }
}
