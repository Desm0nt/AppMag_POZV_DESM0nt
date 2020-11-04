using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Materials
{
    /// <summary>
    /// Contains a physical characteristics of materials
    /// </summary>
    public class Material
    {
        #region Properties
        /// <summary>
        /// Id of the material
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the material
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Elastic modulus, Pa
        /// </summary>
        public double ElasticModulus { get; set; }

        /// <summary>
        /// Poisson's ratio
        /// </summary>
        public double PoissonsRatio { get; set; }

        /// <summary>
        /// Density of the material, kg / m^3
        /// </summary>
        public double Density { get; set; }

        /// <summary>
        /// Coefficient of the internal friction
        /// </summary>
        public double InternalFriction { get; set; }

        /// <summary>
        /// Coefficient of the thermal conductivity, Vt / (m * K)
        /// </summary>
        public double ThermalConductivity { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Empty material (without characteristics)
        /// </summary>
        public Material()
        { }

        /// <summary>
        /// Complete description of the physical characteristics of the material
        /// </summary>
        /// <param name="id">Id of the material</param>
        /// <param name="name">Name of the material</param>
        /// <param name="elasticModulus">Elastic modulus, Pa</param>
        /// <param name="poissonsRatio">Poisson's ratio</param>
        /// <param name="density">Density of the material, kg / m^3</param>
        /// <param name="friction">Coefficient of the internal friction</param>
        /// <param name="thermalConductivity">Coefficient of the thermal conductivity, Vt / (m * K)</param>
        public Material(int id, string name, double elasticModulus, double poissonsRatio, double density, double friction, double thermalConductivity)
        {
            Id = id;
            Name = name;
            ElasticModulus = elasticModulus;
            PoissonsRatio = poissonsRatio;
            Density = density;
            InternalFriction = friction;
            ThermalConductivity = thermalConductivity;
        }

        /// <summary>
        /// Description of the physical characteristics of the material (without friction and thermal conductivity)
        /// </summary>
        /// <param name="id">Id of the material</param>
        /// <param name="name">Name of the material</param>
        /// <param name="elasticModulus">Elastic modulus, Pa</param>
        /// <param name="poissonsRatio">Poisson's ratio</param>
        /// <param name="density">Density of the material, kg / m^3</param>
        public Material(int id, string name, double elasticModulus, double poissonsRatio, double density)
        {
            Id = id;
            Name = name;
            ElasticModulus = elasticModulus;
            PoissonsRatio = poissonsRatio;
            Density = density;
        }
        #endregion
    }
}
