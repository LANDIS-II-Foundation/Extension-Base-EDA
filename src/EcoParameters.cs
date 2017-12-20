//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// Extra Ecoregion Paramaters
    /// </summary>
    public interface IEcoParameters
    {
        double EcoModifier{get;set;}
    }
}


namespace Landis.Extension.BaseEDA
{
    public class EcoParameters
        : IEcoParameters
    {
        private double ecoModifier;

        //---------------------------------------------------------------------
        /// <summary>
        /// </summary>
        public double EcoModifier{
            get{
                return ecoModifier;
            }
            set {
                if (value < -10.0 || value > 10.0)
                    throw new InputValueException(value.ToString(),
                        "Value must be > -10 and < 10.");
                ecoModifier = value;
            }
        }
        //---------------------------------------------------------------------
        public EcoParameters()
        {
        }

    }
}
