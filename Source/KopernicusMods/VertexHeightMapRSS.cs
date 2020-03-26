/* 
 * This code is adapted from KopernicusExpansion-Continued
 * Available from https://github.com/StollD/KopernicusExpansion-Continued
 */

using System;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.Configuration.ModLoader;

namespace RealSolarSystem
{
    public class VertexHeightMapRSS : ModLoader<PQSMod_VertexHeightMapRSS>
    {    
        // The map texture for the planet
        [ParserTarget("map")]
        public MapSOParserLarge<MapSO> heightMap
        {
            get { return Mod.heightMap; }
            set { Mod.heightMap = value; }
        }

        // Height map offset
        [ParserTarget("offset")]
        public NumericParser<Double> heightMapOffset 
        {
            get { return Mod.heightMapOffset; }
            set { Mod.heightMapOffset = value; }
        }

        // Height map offset
        [ParserTarget("deformity")]
        public NumericParser<Double> heightMapDeformity
        {
            get { return Mod.heightMapDeformity; }
            set { Mod.heightMapDeformity = value; }
        }

        // Height map offset
        [ParserTarget("scaleDeformityByRadius")]
        public NumericParser<Boolean> scaleDeformityByRadius
        {
            get { return Mod.scaleDeformityByRadius; }
            set { Mod.scaleDeformityByRadius = value; }
        }
    }
}