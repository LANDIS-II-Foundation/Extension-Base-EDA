//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.BaseEDA
{

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
        public double SHIModifier  
        {
            get {
                return shiMod;
            }
            set
            {
                if (value < -1.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                        "Value must be > -1 and < 1.");
                shiMod = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Maximum duration of impact for a disturbance (in years)
        /// </summary>
        public int ImpactDuration   
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
