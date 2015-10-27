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
        private static ISiteVar<int> timeOfLastEDA;
        private static ISiteVar<string> harvestPrescriptionName;
        private static ISiteVar<int> timeOfLastHarvest;
        private static ISiteVar<int> harvestCohortsKilled;
        private static ISiteVar<int> timeOfLastFire;
        private static ISiteVar<byte> fireSeverity;
        private static ISiteVar<int> timeOfLastWind;
        private static ISiteVar<byte> windSeverity; 
        private static ISiteVar<double> siteHostSusceptMod;
        private static ISiteVar<double> siteHostSuscept;
        private static ISiteVar<double> epidemDistProb;  //according to the user manual, this corresponds to BPD so I changed accordingly to avoid confusion
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<Dictionary<int,int>> numberCFSconifersKilled;  //do we need to rename this? of this variable is used "as is" by fire?
        private static ISiteVar<ISiteCohorts> cohorts;
        private static ISiteVar<int> timeOfNext;
        private static ISiteVar<string> agentName;
        private static ISiteVar<int> timeOfLastBiomassInsects;  //WHAT IS THIS PARAMETER?
        private static ISiteVar<string> biomassInsectsAgent;    //WHAT IS A "BIOMASS" INSECT AGENT? DIFFERENT FROM OTHER BDA's?

        //---------------------------------------------------------------------

        public static void Initialize(ICore modelCore)
        {
            timeOfLastEDA  = modelCore.Landscape.NewSiteVar<int>();
            siteHostSusceptMod = modelCore.Landscape.NewSiteVar<double>();
            siteHostSuscept = modelCore.Landscape.NewSiteVar<double>();
            epidemDistProb = modelCore.Landscape.NewSiteVar<double>();  //this used to be "vulnerability" or "BDP" in the BDA extension
            disturbed = modelCore.Landscape.NewSiteVar<bool>();
            numberCFSconifersKilled = modelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            timeOfNext = modelCore.Landscape.NewSiteVar<int>();
            agentName = modelCore.Landscape.NewSiteVar<string>();
            biomassInsectsAgent = modelCore.Landscape.NewSiteVar<string>();

            //initialize starting values
            SiteVars.TimeOfLastEvent.ActiveSiteValues = -10000; //why this?
            SiteVars.SiteHostSusceptMod.ActiveSiteValues = 0.0;
            SiteVars.SiteHostSuscept.ActiveSiteValues = 0.0;
            SiteVars.EpidemDistProb.ActiveSiteValues = 0.0;
            SiteVars.TimeOfNext.ActiveSiteValues = 9999;    //why this?
            SiteVars.AgentName.ActiveSiteValues = "";

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts"); //get age cohorts from succession extension

            //LOOP through each active pixel in the landscape and for each one of them
            //initialize a dictionary to keep track of numbers of cohorts killed as part of special dead fuel
            foreach(ActiveSite site in modelCore.Landscape)
                SiteVars.NumberCFSconifersKilled[site] = new Dictionary<int, int>();

            // Added for v1.1 to enable interactions with CFS fuels extension.
            modelCore.RegisterSiteVar(SiteVars.NumberCFSconifersKilled, "EDA.NumCFSConifers");
            modelCore.RegisterSiteVar(SiteVars.TimeOfLastEvent, "EDA.TimeOfLastEvent");
            modelCore.RegisterSiteVar(SiteVars.AgentName, "EDA.AgentName");
            // Added to enable interactions with other extensions (Presalvage harvest)
            modelCore.RegisterSiteVar(SiteVars.TimeOfNext, "EDA.TimeOfNext");

        }

        //---------------------------------------------------------------------

        public static void InitializeTimeOfLastDisturbances()
        {
            harvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
            timeOfLastHarvest = PlugIn.ModelCore.GetSiteVar<int>("Harvest.TimeOfLastEvent");
            harvestCohortsKilled = PlugIn.ModelCore.GetSiteVar<int>("Harvest.CohortsKilled");
            timeOfLastFire = PlugIn.ModelCore.GetSiteVar<int>("Fire.TimeOfLastEvent");
            fireSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
            timeOfLastWind = PlugIn.ModelCore.GetSiteVar<int>("Wind.TimeOfLastEvent");
            windSeverity = PlugIn.ModelCore.GetSiteVar<byte>("Wind.Severity");
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
        public static ISiteVar<double> SiteHostSuscept
        {
            get {
                return siteHostSuscept;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> SiteHostSusceptMod
        {
            get {
                return siteHostSusceptMod;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> EpidemDistProb
        {
            get {
                return epidemDistProb;
            }
        }
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
