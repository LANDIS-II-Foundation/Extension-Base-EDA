//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo

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
                ecoModifier = value;
            }
        }

        //---------------------------------------------------------------------
        public EcoParameters()
        {
        }

    }
}
