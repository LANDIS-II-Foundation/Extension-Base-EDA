//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using System.Data;

namespace Landis.Extension.BaseEDA
{

    public enum SHImode { max, mean };  //maybe add something new here, like weighted by biomass, or mean for each cohort?
    public enum DispersalTemplate { PowerLaw, NegExp };

    /// <summary>
    /// Interface to the Parameters for the BaseEDA extension
    /// </summary>
    public interface IAgent
    {
        string AgentName{ get;set; }
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

        ISppParameters[] SppParameters { get; set; }
        IEcoParameters[] EcoParameters { get; set; }
        List<IDisturbanceType> DisturbanceTypes { get;  }

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
        //VariableNames {get; set;}
        //StartMonth {get; set;}
        //EndMonth {get; set;}
        //ADD HERE!

        //-- Transmission -------------
        private double transmissionRate { get; set; }  //beta0 = Mean rate at which an infected cell infects another cell (per time step)
        private double acquisitionRate { get; set; }  //rD = Rate of acquisition of detectable symptoms (per time step)
        //>>InitialEpidemMap? do I need to add this here?
        private DispersalTemplate dispersalKernel { get; set; }
        private double alphaCoef { get; set; }

        private IEnumerable<ISpecies> negSppList;  //List of tree species to be ignored

        private ISppParameters[] sppParameters;
        private IEcoParameters[] ecoParameters;
        private List<IDisturbanceType> disturbanceTypes;

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
        //VariableNames {get; set;}
        //StartMonth {get; set;}
        //EndMonth {get; set;}
        //ADD HERE!

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
        public Agent(int sppCount, int ecoCount)
        {
            SppParameters = new ISppParameters[sppCount];
            EcoParameters = new IEcoParameters[ecoCount];
            disturbanceTypes = new List<IDisturbanceType>();
            negSppList = new List<ISpecies>();

            for (int i = 0; i < sppCount; i++)
                SppParameters[i] = new SppParameters();
            for (int i = 0; i < ecoCount; i++)
                EcoParameters[i] = new EcoParameters();
        }

    }

 
}
