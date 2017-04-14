using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.BaseEDA
{
    public class ExternalClimateVariableValues
    {
        public Dictionary<string, double[]> climateVariableValues;
        //---------------------------------------------------------------------
        public Dictionary<string, double[]> ClimateVariableValues
        {
            get
            {
                return climateVariableValues;
            }
            set
            {
                climateVariableValues = value;
            }
        }
    }
    public class ExternalClimateEcoregion
    {
        public Dictionary<int, ExternalClimateVariableValues> ecoregionClimate;
        //---------------------------------------------------------------------
        public Dictionary<int, ExternalClimateVariableValues> EcoregionClimate
        {
            get
            {
                return ecoregionClimate;
            }
            set
            {
                ecoregionClimate = value;
            }
        }
    }
    public class ExternalClimateYear
    {
        public Dictionary<int, ExternalClimateEcoregion> yearClimate;
        //---------------------------------------------------------------------
        public Dictionary<int, ExternalClimateEcoregion> YearClimate
        {
            get
            {
                return yearClimate;
            }
            set
            {
                yearClimate = value;
            }
        }
    }
    public class ExternalClimateData
    {
        public Dictionary<string, ExternalClimateYear> externalClimateData;
        //---------------------------------------------------------------------
        public Dictionary<string, ExternalClimateYear> ExternalData
        {
            get
            {
                return externalClimateData;
            }
            set
            {
                externalClimateData = value;
            }
        }
    }
}
