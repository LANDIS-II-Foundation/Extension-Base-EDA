//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using System.Data;

namespace Landis.Extension.BaseEDA
{

    public enum SHImode { max, mean };  //maybe add something new here, like weighted by biomass, or mean for each cohort?
    public enum DispersalType { STATIC, DYNAMIC };
    public enum DispersalTemplate { PowerLaw, NegExp };

    /// <summary>
    /// Interface to the Parameters for the BaseEDA extension
    /// </summary>
    public interface IAgent
    {
        //agent name
        string AgentName { get; set; }

        int StartYear { get; set; }
        int EndYear { get; set; }

        //site host index mode (mean, max)
        SHImode SHImode { get; set; }

        // - Climate - PLACEHOLDER FOR CLIMATE INPUTS
        List<IClimateVariableDefinition> ClimateVars { get; set; }
        List<IDerivedClimateVariable> DerivedClimateVars { get; set; }
        IFormula VarFormula { get; set; }
        List<string> WeatherIndexVars { get; set; }
        DataTable ClimateDataTable { get; set; }
        WeatherIndex AnnualWeatherIndex { get; set; }
        double[] EcoWeatherIndexNormal { get; set; }

        //- Transmission -
        double TransmissionRate { get; set; }  //beta0 = Mean rate at which an infected cell infects another cell (per time step)
        double AcquisitionRate { get; set; }  //rD = Rate of acquisition of detectable symptoms (per time step)
        string InitEpiMap { get; set; }   //initial map of infected cells (0=non infected, 1=infected)
        DispersalType DispersalType { get; set; }
        DispersalTemplate DispersalKernel { get; set; }
        int DispersalMaxDist { get; set; }
        double AlphaCoef { get; set; }

        //List of tree species to be ignored
        IEnumerable<ISpecies> NegSppList { get; set; }

        ISppParameters[] SppParameters { get; set; }
        IEcoParameters[] EcoParameters { get; set; }
        List<IDisturbanceType> DisturbanceTypes { get; }

    }
}


namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class AgentParameters
        : IAgent
    {

        //agent name
        private string agentName;

        private int startYear;
        private int endYear;

        //site host index mode (mean, max)
        private SHImode shiMode;

        // - Climate - PLACEHOLDER FOR CLIMATE INPUTS
        private List<IClimateVariableDefinition> climateVarDefn;
        private List<IDerivedClimateVariable> derivedClimateVars;
        private IFormula varFormula;
        private List<string> weatherIndexVars;
        private DataTable climateDataTable;
        private WeatherIndex annualWeatherIndex;
        private double[] ecoWeatherIndexNormal;

        //-- Transmission -------------
        private double transmissionRate { get; set; }  //beta0 = Mean rate at which an infected cell infects another cell (per time step)
        private double acquisitionRate { get; set; }  //rD = Rate of acquisition of detectable symptoms (per time step)
        private string initEpiMap { get; set; }   //initial map of infected cells (0=non infected, 1=infected)
        private DispersalTemplate dispersalKernel { get; set; }
        private DispersalType dispersalType { get; set; }
        private int dispersalMaxDist { get; set; }
        private double alphaCoef { get; set; }

        private IEnumerable<ISpecies> negSppList;  //List of tree species to be ignored

        private ISppParameters[] sppParameters;
        private IEcoParameters[] ecoParameters;
        private List<IDisturbanceType> disturbanceTypes;

        //---------------------------------------------------------------------
        public string AgentName
        {
            get
            {
                return agentName;
            }
            set
            {
                agentName = value;
            }
        }
        //---------------------------------------------------------------------
        public int StartYear
        {
            get
            {
                return startYear;
            }
            set
            {
                startYear = value;
            }
        }
        //---------------------------------------------------------------------
        public int EndYear
        {
            get
            {
                return endYear;
            }
            set
            {
                endYear = value;
            }
        }
        //---------------------------------------------------------------------
        public SHImode SHImode
        {
            get
            {
                return shiMode;
            }
            set
            {
                shiMode = value;
            }
        }
        //---------------------------------------------------------------------
        // - Climate - PLACEHOLDER FOR CLIMATE INPUTS

        public List<IClimateVariableDefinition> ClimateVars
        {
            get { return climateVarDefn; }
            set { climateVarDefn = value; }

        }
        //---------------------------------------------------------------------
        public List<IDerivedClimateVariable> DerivedClimateVars
        {
            get { return derivedClimateVars; }
            set { derivedClimateVars = value; }
        }
        //---------------------------------------------------------------------
        public IFormula VarFormula
        {
            get
            {
                return varFormula;
            }
            set
            {
                varFormula = value;
            }
        }
        //---------------------------------------------------------------------
        public List<string> WeatherIndexVars
        {
            get { return weatherIndexVars; }
            set { weatherIndexVars = value; }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Climate Data Table.
        /// </summary>
        public DataTable ClimateDataTable
        {
            get
            {
                return climateDataTable;
            }
            set
            {
                climateDataTable = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Weather Index.
        /// </summary>
        public WeatherIndex AnnualWeatherIndex
        {
            get
            {
                return annualWeatherIndex;
            }
            set
            {
                annualWeatherIndex = value;
            }
        }
        //---------------------------------------------------------------------
        //-- Transmission -------------
        public double TransmissionRate
        {
            get
            {
                return transmissionRate;
            }
            set
            {
                transmissionRate = value;
            }
        }
        //---------------------------------------------------------------------
        public double AcquisitionRate
        {
            get
            {
                return acquisitionRate;
            }
            set
            {
                acquisitionRate = value;
            }
        }
        //---------------------------------------------------------------------
        public string InitEpiMap
        {
            get { return initEpiMap; }
            set { initEpiMap = value; }
        }
        //---------------------------------------------------------------------
        public DispersalType DispersalType
        {
            get
            {
                return dispersalType;
            }
            set
            {
                dispersalType = value;
            }
        }
        //---------------------------------------------------------------------
        public DispersalTemplate DispersalKernel
        {
            get
            {
                return dispersalKernel;
            }
            set
            {
                dispersalKernel = value;
            }
        }
        //---------------------------------------------------------------------
        public int DispersalMaxDist
        {
            get
            {
                return dispersalMaxDist;
            }
            set
            {
                dispersalMaxDist = value;
            }
        }
        //---------------------------------------------------------------------
        public double AlphaCoef
        {
            get
            {
                return alphaCoef;
            }
            set
            {
                alphaCoef = value;
            }
        }
        //---------------------------------------------------------------------
        public ISppParameters[] SppParameters
        {
            get
            {
                return sppParameters;
            }
            set
            {
                sppParameters = value;
            }
        }
        //---------------------------------------------------------------------
        public IEcoParameters[] EcoParameters
        {
            get
            {
                return ecoParameters;
            }
            set
            {
                ecoParameters = value;
            }
        }
        //---------------------------------------------------------------------
        public double[] EcoWeatherIndexNormal
        {
            get
            {
                return ecoWeatherIndexNormal;
            }
            set
            {
                ecoWeatherIndexNormal = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Disturbances that can alter the SHI value
        /// </summary>
        public List<IDisturbanceType> DisturbanceTypes
        {
            get
            {
                return disturbanceTypes;
            }
        }
        //---------------------------------------------------------------------
        public IEnumerable<ISpecies> NegSppList
        {
            get
            {
                return negSppList;
            }
            set
            {
                negSppList = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Objects and Lists must be initialized.
        /// </summary>
        public AgentParameters(int sppCount, int ecoCount)
        {
            SppParameters = new ISppParameters[sppCount];
            EcoParameters = new IEcoParameters[ecoCount];
            EcoWeatherIndexNormal = new double[ecoCount];
            disturbanceTypes = new List<IDisturbanceType>();
            negSppList = new List<ISpecies>();
            climateVarDefn = new List<IClimateVariableDefinition>();
            derivedClimateVars = new List<IDerivedClimateVariable>();
            weatherIndexVars = new List<string>();
            climateDataTable = new DataTable();

            for (int i = 0; i < sppCount; i++)
                SppParameters[i] = new SppParameters();
            for (int i = 0; i < ecoCount; i++)
                EcoParameters[i] = new EcoParameters();
        }

    }
    
}
