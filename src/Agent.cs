//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using System.Data;

namespace Landis.Extension.BaseEDA
{

    public enum SHImode {max, mean};  //maybe add something new here, like weighted by biomass, or mean for each cohort?
    public enum DispersalTemplate { PowerLaw, NegExp };

    /// <summary>
    /// Interface to the Parameters for the BaseEDA extension
    /// </summary>
    public interface IAgent
    {
        string AgentName{get;set;}
        int StartYear { get; set; }
        int EndYear { get; set; }

        SHImode SHImode { get; set; }

        // - Climate - PLACEHOLDER FOR CLIMATE INPUTS
        //VariableNames {get; set;}
        //StartMonth {get; set;}
        //EndMonth {get; set;}
        //ADD HERE!

        //- Transmission -
        double TransmissionRate { get; set; }  //beta0 = Mean rate at which an infected cell infects another cell (per time step)
        double AcquisitionRate  { get; set; }  //rD = Rate of acquisition of detectable symptoms (per time step)
        //>>InitialEpidemMap? do I need to add this here?
        DispersalTemplate DispersalKernel { get; set; }
        double AlphaCoef { get; set; }

        //List of tree species to be ignored
        IEnumerable<ISpecies> NegSppList { get; set; }
        //List of tree species to be considered for mortality outputs
        IEnumerable<ISpecies> MortSppList { get; set; }

        ISppParameters[] SppParameters { get; set; }
        IEcoParameters[] EcoParameters { get; set; }
        List<IDisturbanceType> DisturbanceTypes { get;  }
        //ISiteVar<byte> Severity { get; set; }
    }
}


namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class Agent
        : IAgent
    {
        private string agentName;
        private int startYear;
        private int endYear;

        private SHImode shiMode;

        // - Climate - PLACEHOLDER FOR CLIMATE INPUTS
        private List<IClimateVariableDefinition> climateVarDefn;
        private List<IDerivedClimateVariable> varDefn;
        private ITempIndexModel tempIndexModel;
        private List<string> weatherIndexVars;
        private DataTable climateDataTable;

        //-- Transmission -------------
        private double transmissionRate { get; set; }  //beta0 = Mean rate at which an infected cell infects another cell (per time step)
        private double acquisitionRate { get; set; }  //rD = Rate of acquisition of detectable symptoms (per time step)
        //>>InitialEpidemMap? do I need to add this here?
        private DispersalTemplate dispersalKernel { get; set; }
        private double alphaCoef { get; set; }

        //List of tree species to be ignored
        private IEnumerable<ISpecies> negSppList;
        private ISppParameters[] sppParameters;
        private IEcoParameters[] ecoParameters;
        private List<IDisturbanceType> disturbanceTypes;
        //private ISiteVar<byte> severity;

        private IEnumerable<ISpecies> advRegenSppList;  //WHAT IS THIS?
        private int advRegenAgeCutoff; //WHAT IS THIS?

        //---------------------------------------------------------------------
        public string AgentName
        {
            get {
                return agentName;
            }
            set {
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
            get {
                return shiMode;
            }
            set {
                shiMode = value;
            }
        }
        //---------------------------------------------------------------------
        // - Climate - PLACEHOLDER FOR CLIMATE INPUTS

        public List<IClimateVariableDefinition> ClimateVars
        {
            get { return climateVarDefn; }
        
        }
        //---------------------------------------------------------------------
        public List<IDerivedClimateVariable> DerivedClimateVars
        {
            get { return varDefn; }
        }
        //---------------------------------------------------------------------
        public ITempIndexModel TempIndexModel
        {
            get
            {
                return tempIndexModel;
            }
        }
        //---------------------------------------------------------------------
        public List<string> WeatherIndexVars
        {
            get { return weatherIndexVars; }
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
        //>>InitialEpidemMap? do I need to add this here?

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
            get {
                return sppParameters;
            }
            set {
                sppParameters = value;
            }
        }
        //---------------------------------------------------------------------
        public IEcoParameters[] EcoParameters
        {
            get {
                return ecoParameters;
            }
            set {
                ecoParameters = value;
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
        /*public ISiteVar<byte> Intensity
        {
            get {
                return intensity;
            }
            set {
                intensity = value;
            }
        }*/
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
        public IEnumerable<ISpecies> MortSppList
        {
            get
            {
                return mortSppList;
            }
            set
            {
                mortSppList = value;
            }
        }
        //---------------------------------------------------------------------
        public IEnumerable<ISpecies> AdvRegenSppList   //DO WE NEED THIS? IT's NOT DEFINED IN THE IAGENT interface
        {
            get
            {
                return advRegenSppList;
            }
            set
            {
                advRegenSppList = value;
            }
        }
        //---------------------------------------------------------------------
        public int AdvRegenAgeCutoff   //DO WE NEED THIS? IT's NOT DEFINED IN THE IAGENT interface
        {
            get
            {
                return advRegenAgeCutoff;
            }
            set
            {
                advRegenAgeCutoff = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Objects and Lists must be initialized.
        /// </summary>
        public Agent(int sppCount, int ecoCount)
        {
            SppParameters = new ISppParameters[sppCount];
            EcoParameters = new IEcoParameters[ecoCount];
            disturbanceTypes = new List<IDisturbanceType>();
            negSppList = new List<ISpecies>();

            //advRegenSppList = new List<ISpecies>(); //DO WE NEED THIS? IT's NOT DEFINED IN THE IAGENT interface

            //severity = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            
            for (int i = 0; i < sppCount; i++)
                SppParameters[i] = new SppParameters();
            for (int i = 0; i < ecoCount; i++)
                EcoParameters[i] = new EcoParameters();
        }

    }

 
}
