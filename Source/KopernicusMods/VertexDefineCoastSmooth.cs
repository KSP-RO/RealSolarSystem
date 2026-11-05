/* 
 * This code is adapted from KopernicusExpansion-Continued
 * Available from https://github.com/StollD/KopernicusExpansion-Continued
 */

using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.Configuration.ModLoader;

namespace RealSolarSystem
{
    public class VertexDefineCoastSmooth : ModLoader<PQSMod_VertexDefineCoastSmooth>
    {
        // Height map offset
        [ParserTarget("minOffset")]
        public NumericParser<double> minHeightOffset
        {
            get { return Mod.minHeightOffset; }
            set { Mod.minHeightOffset = value; }
        }

        // Height map offset
        [ParserTarget("maxOffset")]
        public NumericParser<double> maxHeightOffset
        {
            get { return Mod.maxHeightOffset; }
            set { Mod.maxHeightOffset = value; }
        }

        // Height map offset
        [ParserTarget("slopeScale")]
        public NumericParser<double> slopeScale
        {
            get { return Mod.slopeScale; }
            set { Mod.slopeScale = value; }
        }

        // Target half-width of the transition, in vertex spacings. Positive enables adaptive mode,
        // which sizes the ramp from the height map gradient and ignores slopeScale.
        [ParserTarget("coastSpacings")]
        public NumericParser<double> coastSpacings
        {
            get { return Mod.coastSpacings; }
            set { Mod.coastSpacings = value; }
        }

        // Central difference half-width used to measure that gradient, in height map texels.
        [ParserTarget("gradientStencil")]
        public NumericParser<double> gradientStencil
        {
            get { return Mod.gradientStencil; }
            set { Mod.gradientStencil = value; }
        }
    }
}