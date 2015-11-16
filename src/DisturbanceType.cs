//  Copyright 2007-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.BaseEDA
{
    //This slash type is used for all disturbance fuel types
    public interface IDisturbanceType
    {
        double SHIModifier {get;set; } //site host index (SHI) modifier
        int ImpactDuration {get;set;}  
        List<string> PrescriptionNames{get;set;}
    }

    /// <summary>
    /// A disturbance type.
    /// </summary>
    public class DisturbanceType
        : IDisturbanceType
    {
        private double shiMod;
        private int impactDuration;
        private List<string> prescriptionNames;

        //---------------------------------------------------------------------

        /// <summary>
        /// Index
        /// </summary>
        public double SHIModifier  //CHECK TO MAKE SURE WE DON'T NEED TO ADD ANY CONDITIONAL CHECK FOR HV < 0
        {
            get {
                return shiMod;
            }
            set {
                shiMod = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Maximum duration of impact for a disturbance (in years)
        /// </summary>
        public int ImpactDuration   //changed from MaxAge
        {
            get {
                return impactDuration;
            }
            set {
                if (value <= 0)
                    throw new InputValueException(value.ToString(),"Value must be > 0.");
                impactDuration = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// A prescription name (for harvest)
        /// </summary>
        public List<string> PrescriptionNames
        {
            get {
                return prescriptionNames;
            }
            set {
                if (value != null)
                    prescriptionNames = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public DisturbanceType()
        {
            prescriptionNames = new List<string>();
        }

    }
}
