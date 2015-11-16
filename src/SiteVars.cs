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
        //private static ISiteVar<int> timeOfLastBDA; CHANGE THIS TO MATCH BDA
        //private static ISiteVar<byte> windSeverity; CHANGE THIS TO MATCH SEVERITY CAUSED BY BDA

        //epidem
        private static ISiteVar<int> timeOfLastEDA;
        private static ISiteVar<double> siteHostIndexMod;
        private static ISiteVar<double> siteHostIndex;   //Host Index defined as "susceptibility of each non-infected cell to become infected 
                                                         //and the suitability of each infected cell to produce infectious spores of the pathogen"
        //private static ISiteVar<double> epidemDistProb;  
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<Dictionary<int,int>> numberCFSconifersKilled;  //this specific naming convention is defined by the Canadian fire model. Keep as is.
        private static ISiteVar<ISiteCohorts> cohorts;                         //the list of species to be used as FUEL for the fire extension can be specified in the Fire Fuel extension!                     
        private static ISiteVar<int> timeOfNext;
        private static ISiteVar<string> agentName;

        //---------------------------------------------------------------------

        public static void Initialize(ICore modelCore)
        {
            timeOfLastEDA  = modelCore.Landscape.NewSiteVar<int>();
            siteHostIndexMod = modelCore.Landscape.NewSiteVar<double>();
            siteHostIndex = modelCore.Landscape.NewSiteVar<double>();
            //epidemDistProb = modelCore.Landscape.NewSiteVar<double>(); 
            disturbed = modelCore.Landscape.NewSiteVar<bool>();
            numberCFSconifersKilled = modelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            timeOfNext = modelCore.Landscape.NewSiteVar<int>();
            agentName = modelCore.Landscape.NewSiteVar<string>();
            biomassInsectsAgent = modelCore.Landscape.NewSiteVar<string>();

            //initialize starting values
            TimeOfLastEvent.ActiveSiteValues = -10000; //why this?
            SiteHostIndexMod.ActiveSiteValues = 0.0;
            SiteHostIndex.ActiveSiteValues = 0.0;
            //EpidemDistProb.ActiveSiteValues = 0.0;
            TimeOfNext.ActiveSiteValues = 9999;    //why this?
            AgentName.ActiveSiteValues = "";

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts"); //get age cohorts from succession extension

            //LOOP through each active pixel in the landscape and for each one of them
            //initialize a dictionary to keep track of numbers of cohorts killed as part of special dead fuel
            foreach(ActiveSite site in modelCore.Landscape)
                NumberCFSconifersKilled[site] = new Dictionary<int, int>();

            // Enable interactions with CFS fuels extension.
            modelCore.RegisterSiteVar(NumberCFSconifersKilled, "EDA.NumCFSConifers");

            modelCore.RegisterSiteVar(TimeOfLastEvent, "EDA.TimeOfLastEvent");
            modelCore.RegisterSiteVar(AgentName, "EDA.AgentName");
            // Added to enable interactions with other extensions (Presalvage harvest)
            modelCore.RegisterSiteVar(TimeOfNext, "EDA.TimeOfNext");

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
            //timeOfLastBDA = PlugIn.ModelCore.GetSiteVar<int>("BDA.TimeOfLastEvent"); //HOW DO WE CHANGE THIS?
            //windSeverity = PlugIn.ModelCore.GetSiteVar<byte>("BDA.Severity");  //HOW DO WE CHANGE THIS?
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
        //public static ISiteVar<double> EpidemDistProb
        //{
        //    get {
        //        return epidemDistProb;
        //    }
        //}
        //---------------------------------------------------------------------

        public static ISiteVar<bool> Disturbed
        {
            get {
                return disturbed;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int,int>> NumberCFSconifersKilled  //should we change this name or fire ext needs it as is?
        {
            get {
                return numberCFSconifersKilled;
            }
            set {
                numberCFSconifersKilled = value;
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
        public static ISiteVar<int> TimeOfNext
        {
            get
            {
                return timeOfNext;
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
    }
}
