//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.AgeOnlyCohorts;
using System.Collections.Generic;

namespace Landis.Extension.BaseEDA
{
    ///<summary>
    /// Site Variables for a disturbance plug-in that simulates Epidemiological Processes.
    /// </summary>
    public static class SiteVars
    {
        //harvest
        private static ISiteVar<string> harvestPrescriptionName;
        private static ISiteVar<int> timeOfLastHarvest;
        private static ISiteVar<int> harvestCohortsKilled;
        //fire
        private static ISiteVar<int> timeOfLastFire;
        private static ISiteVar<byte> fireSeverity;
        //wind
        private static ISiteVar<int> timeOfLastWind;
        private static ISiteVar<byte> windSeverity;
        //biomass leaf insect
        private static ISiteVar<int> timeOfLastBiomassInsects;
        private static ISiteVar<string> biomassInsectsAgent;
        //biological disturbance agent (BDA)
        private static ISiteVar<int> timeOfLastBDA; 
        private static ISiteVar<byte> bdaSeverity;
        //epidem:
        //Host Index defined as "susceptibility of each non-infected cell to become infected 
        //and the suitability of each infected cell to produce infectious spores of the pathogen"
        private static ISiteVar<int> timeOfLastEDA;
        private static ISiteVar<double> siteHostIndexMod;
        private static ISiteVar<double> siteHostIndex;   
        
        //STATE OF A CELL: SUSCEPTIBLE (0), INFECTED (cryptic-non symptomatic) (1), DISEASED (symptomatic) (2)
        private static ISiteVar<byte> infStatus;
        private static ISiteVar<double> pSusceptible;
        private static ISiteVar<double> pInfected;
        private static ISiteVar<double> pDiseased;

        // Climate variables
        private static ISiteVar<Dictionary<string, float>> climateVars;

        //this specific naming convention is defined by the Canadian fire model. Keep as is.
        //the list of species to be used as FUEL for the fire extension can be specified in the Fire Fuel extension!
        private static ISiteVar<Dictionary<int,int>> numberCFSconifersKilled;  
                                                                               
        //list of spp to be included in the mortality plot.
        private static ISiteVar<Dictionary<int, int>> numberMortSppKilled;

        private static ISiteVar<ISiteCohorts> cohorts;                                              
        private static ISiteVar<string> agentName;

        //---------------------------------------------------------------------

        public static void Initialize(ICore modelCore)
        {
            timeOfLastEDA  = modelCore.Landscape.NewSiteVar<int>();
            siteHostIndexMod = modelCore.Landscape.NewSiteVar<double>();
            siteHostIndex = modelCore.Landscape.NewSiteVar<double>();
            infStatus = modelCore.Landscape.NewSiteVar<byte>();
            pSusceptible = modelCore.Landscape.NewSiteVar<double>();
            pInfected = modelCore.Landscape.NewSiteVar<double>();
            pDiseased = modelCore.Landscape.NewSiteVar<double>();
            agentName = modelCore.Landscape.NewSiteVar<string>();
            biomassInsectsAgent = modelCore.Landscape.NewSiteVar<string>();


            climateVars = modelCore.Landscape.NewSiteVar<Dictionary<string, float>>();
            numberCFSconifersKilled = modelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            numberMortSppKilled = modelCore.Landscape.NewSiteVar<Dictionary<int, int>>();

            //initialize starting values
            TimeOfLastEvent.ActiveSiteValues = -10000; //why this?
            SiteHostIndexMod.ActiveSiteValues = 0.0;
            SiteHostIndex.ActiveSiteValues = 0.0;
            //InfStatus.ActiveSiteValues = 0; //not sure I should initialize to 0 all sites since I will use initial infection map OR random place starting infections
            //PSusceptible.ActiveSiteValues = 1;  //for all sites with infStatus = 0
            //PInfected.ActiveSiteValues = 0; //set = 1 for initial sites of outbreak (infStatus = 1) based on map or random (see above)
            //PDiseased.ActiveSiteValues = 0; //this should be = 0 for all sites
            
            AgentName.ActiveSiteValues = "";

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts"); //get age cohorts from succession extension

            //LOOP through each active pixel in the landscape and for each one of them
            //initialize a dictionary to keep track of numbers of cohorts killed as part of special dead fuel or as those for inclusion in mortality plot
            foreach (ActiveSite site in modelCore.Landscape)
            {
                SiteVars.ClimateVars[site] = new Dictionary<string, float>();
                NumberCFSconifersKilled[site] = new Dictionary<int, int>();
                NumberMortSppKilled[site] = new Dictionary<int, int>();
            }

            modelCore.RegisterSiteVar(NumberCFSconifersKilled, "EDA.NumCFSConifers");  // Enable interactions with CFS fuels extension.
            modelCore.RegisterSiteVar(NumberMortSppKilled, "EDA.NumMortSppKilled");  // Enable interactions with fire/fuel extension (B. Miranda add this please).

            modelCore.RegisterSiteVar(TimeOfLastEvent, "EDA.TimeOfLastEvent");
            modelCore.RegisterSiteVar(AgentName, "EDA.AgentName");

        }

        //---------------------------------------------------------------------
        public static void InitializeTimeOfLastDisturbances()
        {
            //harvest
            harvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
            timeOfLastHarvest = PlugIn.ModelCore.GetSiteVar<int>("Harvest.TimeOfLastEvent");
            harvestCohortsKilled = PlugIn.ModelCore.GetSiteVar<int>("Harvest.CohortsKilled");
            //fire
            timeOfLastFire = PlugIn.ModelCore.GetSiteVar<int>("Fire.TimeOfLastEvent");
            fireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
            //wind
            timeOfLastWind = PlugIn.ModelCore.GetSiteVar<int>("Wind.TimeOfLastEvent");
            windSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Wind.Severity");
            //bda
            timeOfLastBDA = PlugIn.ModelCore.GetSiteVar<int>("BDA.TimeOfLastEvent");
            bdaSeverity = PlugIn.ModelCore.GetSiteVar<byte>("BDA.Severity");
            //biomass
            timeOfLastBiomassInsects = PlugIn.ModelCore.GetSiteVar<int>("BiomassInsects.TimeOfLastEvent");
            biomassInsectsAgent = PlugIn.ModelCore.GetSiteVar<string>("BiomassInsects.InsectName");

        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastEvent
        {
            get {
                return timeOfLastEDA;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<string> HarvestPrescriptionName
        {
            get
            {
                return harvestPrescriptionName;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastHarvest
        {
            get
            {
                return timeOfLastHarvest;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> HarvestCohortsKilled
        {
            get
            {
                return harvestCohortsKilled;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastFire
        {
            get
            {
                return timeOfLastFire;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<byte> FireSeverity
        {
            get
            {
                return fireSeverity;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastWind
        {
            get
            {
                return timeOfLastWind;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<byte> WindSeverity
        {
            get
            {
                return windSeverity;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastBDA
        {
            get
            {
                return timeOfLastBDA;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<byte> BDASeverity
        {
            get
            {
                return bdaSeverity;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> SiteHostIndex
        {
            get {
                return siteHostIndex;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> SiteHostIndexMod
        {
            get {
                return siteHostIndexMod;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<byte> InfStatus
        {
            get {
                return infStatus;
           }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> PSusceptible
        {
            get
            {
                return pSusceptible;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> PInfected
        {
            get
            {
                return pInfected;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> PDiseased
        {
            get
            {
                return pDiseased;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int,int>> NumberCFSconifersKilled 
        {
            get {
                return numberCFSconifersKilled;
            }
            set {
                numberCFSconifersKilled = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, int>> NumberMortSppKilled
        {
            get
            {
                return numberMortSppKilled;
            }
            set
            {
                numberMortSppKilled = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }

        }
        //---------------------------------------------------------------------
        public static ISiteVar<string> AgentName
        {
            get
            {
                return agentName;
            }

        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastBiomassInsects
        {
            get
            {
                return timeOfLastBiomassInsects;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<string> BiomassInsectsAgent
        {
            get
            {
                return biomassInsectsAgent;
            }

        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<string, float>> ClimateVars
        {
            get
            {
                return climateVars;
            }
        }
    }
}
