//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

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
        //private static ISiteVar<byte> infStatus;
        private static ISiteVar<Dictionary<int, byte>> infStatus;  //dictionary with keys corresponding to each agent
                                                                   //necessary to adjust for multi-agents in the extension 

        //initial probs of being in each status
        private static ISiteVar<Dictionary<int, double>> pSusceptible;   //dictionary with keys corresponding to each agent
        private static ISiteVar<Dictionary<int, double>> pInfected;      //dictionary with keys corresponding to each agent
        private static ISiteVar<Dictionary<int, double>> pDiseased;      //dictionary with keys corresponding to each agent
        private static ISiteVar<double> foi;   //force of infection
        //private static ISiteVar<double> pSusceptible;
        //private static ISiteVar<double> pInfected;
        //private static ISiteVar<double> pDiseased;

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

        public static void Initialize(ICore modelCore, int numAgents) //---->>> see PlugIn.cs
        {

            timeOfLastEDA = modelCore.Landscape.NewSiteVar<int>();
            siteHostIndexMod = modelCore.Landscape.NewSiteVar<double>();
            siteHostIndex = modelCore.Landscape.NewSiteVar<double>();
            infStatus = modelCore.Landscape.NewSiteVar<Dictionary<int, byte>>(); //dictionary with keys corresponding to each agent
            pSusceptible = modelCore.Landscape.NewSiteVar<Dictionary<int, double>>(); //dictionary with keys corresponding to each agent
            pInfected = modelCore.Landscape.NewSiteVar<Dictionary<int, double>>(); //dictionary with keys corresponding to each agent
            pDiseased = modelCore.Landscape.NewSiteVar<Dictionary<int, double>>(); //dictionary with keys corresponding to each agent
            //infStatus = modelCore.Landscape.NewSiteVar<byte>();
            //pSusceptible = modelCore.Landscape.NewSiteVar<double>();
            //pInfected = modelCore.Landscape.NewSiteVar<double>();
            //pDiseased = modelCore.Landscape.NewSiteVar<double>();
            foi = modelCore.Landscape.NewSiteVar<double>();
            agentName = modelCore.Landscape.NewSiteVar<string>();
            biomassInsectsAgent = modelCore.Landscape.NewSiteVar<string>();

            climateVars = modelCore.Landscape.NewSiteVar<Dictionary<string, float>>();
            numberCFSconifersKilled = modelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            numberMortSppKilled = modelCore.Landscape.NewSiteVar<Dictionary<int, int>>();

            //initialize starting values
            TimeOfLastEvent.ActiveSiteValues = -10000; //why this?
            SiteHostIndexMod.ActiveSiteValues = 0.0;
            SiteHostIndex.ActiveSiteValues = 0.0;
            FOI.ActiveSiteValues = 0.0;
            //PDiseased.ActiveSiteValues = 0; uncomment only if not using multi-agent 
            AgentName.ActiveSiteValues = "";

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts"); //get age cohorts from succession extension

            //LOOP through each active pixel in the landscape and for each one of them
            //initialize a dictionary to keep track of numbers of cohorts killed as part of special dead fuel or as those for inclusion in mortality plot
            foreach (ActiveSite site in modelCore.Landscape)
            {
                ClimateVars[site] = new Dictionary<string, float>();

                //dictionary with keys corresponding to each agent
                InfStatus[site] = new Dictionary<int, byte>();
                PSusceptible[site] = new Dictionary<int, double>();
                PInfected[site] = new Dictionary<int, double>();
                PDiseased[site] = new Dictionary<int, double>();

                NumberCFSconifersKilled[site] = new Dictionary<int, int>();
                NumberMortSppKilled[site] = new Dictionary<int, int>();

                //We now need to initialize and populate the otherwise empty dictionaries 
                //with initial values for infection status and probs of being in each status
                for (int i = 0; i < numAgents; i++){
                    //should I initialize infStatus here or within Epidemics region?
                    infStatus[site].Add(i, 0);
                    pSusceptible[site].Add(i, 0);
                    pInfected[site].Add(i, 0);
                    pDiseased[site].Add(i, 0);
                    numberMortSppKilled[site].Add(i, 0);
                }

            }
            
            //register site-level variable for other extensions to read/interact with:
            modelCore.RegisterSiteVar(NumberCFSconifersKilled, "EDA.NumCFSConifers");  // Enable interactions with CFS fuels extension.
            
            //FIXME??
            //NOT SURE we need to register this site variable, since fire can read all species killed by EDA agent inside CFS above
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
        public static ISiteVar<Dictionary<int, byte>> InfStatus
        {
            get
            {
                return infStatus;
            }
            set
            {
                infStatus = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, double>> PSusceptible
        {
            get
            {
                return pSusceptible;
            }
            set
            {
                pSusceptible = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, double>> PInfected
        {
            get
            {
                return pInfected;
            }
            set
            {
                pInfected = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int, double>> PDiseased
        {
            get
            {
                return pDiseased;
            }
            set
            {
                pDiseased = value;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> FOI
        {
            get
            {
                return foi;
            }
        }
        /*
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
        */
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
