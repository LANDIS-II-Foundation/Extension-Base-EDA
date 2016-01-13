//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo

using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System;

namespace Landis.Extension.BaseEDA
{
    public class Epidemic
        : ICohortDisturbance

    {
        private static IEcoregionDataset ecoregions;
        private IAgent epidemicParms;
        private double random;

        // - TOTAL -
        private int totalSitesInfected;
        private int totalSitesDiseased; //disease does not ALWAYS translated into mortality (damage)
        private int totalSitesDamaged;  //sites that are diseased AND experienced mortality
        private int totalCohortsKilled; //all mortality
        private int totalMortSppCohortsKilled; //only mortality of spp of interest (Mort Flag = YES)

        // - SITE -
        //private double siteVulnerability;
        private int siteCohortsKilled; //customize this to ONLY include the species of interest
        private int siteCFSconifersKilled;
        private int siteMortSppKilled; //spp that are included in the mortality species list
        private int[] sitesInEvent;

        private ActiveSite currentSite; // current site where cohorts are being affected

        // - Transmission - 
        private enum DispersalTemplate { PowerLaw, NegExp };   
        private enum InitialCondition   { map, none };   //is this to use initial map of infected sites?
        private enum SHImode { SHImax, SHImean };

        //---------------------------------------------------------------------
        static Epidemic()
        {
        }
        //---------------------------------------------------------------------
        public int TotalCohortsKilled
        {
            get {
                return totalCohortsKilled;
            }
        }
        //---------------------------------------------------------------------
        public int MortSppCohortsKilled
        {
            get
            {
                return totalMortSppCohortsKilled;
            }
        }
        //---------------------------------------------------------------------
        public int TotalSitesInfected
        {
            get
            {
                return totalSitesInfected;
            }
        }
        //---------------------------------------------------------------------
        public int TotalSitesDiseased
        {
            get
            {
                return totalSitesDiseased;
            }
        }
        //---------------------------------------------------------------------
        public int TotalSitesDamaged
        {
            get
            {
                return totalSitesDamaged;
            }
        }
        //---------------------------------------------------------------------
        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.type;
            }
        }
        //---------------------------------------------------------------------
        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }
        //---------------------------------------------------------------------
        IAgent EpidemicParameters
        {
            get
            {
                return epidemicParms;
            }
        }
        //---------------------------------------------------------------------
        public int[] SitesInEvent
        {
            get
            {
                return sitesInEvent;
            }
        }
        //---------------------------------------------------------------------
        ///<summary>
        ///Initialize an Epidemic - defined as an agent outbreak for an entire landscape
        ///at a single EDA timestep.  One epidemic per agent per EDA timestep
        ///</summary>

        public static void Initialize(IAgent agent)
        {
            PlugIn.ModelCore.UI.WriteLine("   Initializing agent {0}.", agent.AgentName);

            ecoregions = PlugIn.ModelCore.Ecoregions;

            //.ActiveSiteValues allows you to reset all active site at once.
            SiteVars.SiteHostIndexMod.ActiveSiteValues = 0;
            SiteVars.SiteHostIndex.ActiveSiteValues = 0;

        }

        //---------------------------------------------------------------------
        ///<summary>
        ///Simulate an Epidemic - This is the controlling function that calls the
        ///subsequent function.  The basic logic of an epidemic resides here.
        ///</summary>
        public static Epidemic Simulate(IAgent agent, int currentTime)
        {

            Epidemic CurrentEpidemic = new Epidemic(agent);
            PlugIn.ModelCore.UI.WriteLine("   New EDA Epidemic Started.");

            SiteResources.SiteHostIndexCompute(agent);   
            SiteResources.SiteHostIndexModCompute(agent);
            ClimateVariableDefinition.CalculateClimateVariables(agent);
            CurrentEpidemic.ComputeSiteInfStatus(agent);

            return CurrentEpidemic;
        }

        //---------------------------------------------------------------------
        // Epidemic Constructor
        private Epidemic(IAgent agent)
        {
            sitesInEvent = new int[ecoregions.Count];
            foreach(IEcoregion ecoregion in ecoregions)
                sitesInEvent[ecoregion.Index] = 0;
            epidemicParms = agent;
            totalCohortsKilled = 0;
            totalMortSppCohortsKilled = 0;
            totalSitesInfected = 0;
            totalSitesDiseased = 0;
            totalSitesDamaged = 0;
        }
        //---------------------------------------------------------------------
        //Go through all active sites and update their infection status according to the
        //probs of being S, I, D.
        private void ComputeSiteInfStatus(IAgent agent)
        {

            double deltaPSusceptible = 0;  //do I need to initialize to 0 these?
            double deltaPInfected = 0;     //do I need to initialize to 0 these?
            double deltaPDiseased = 0;     //do I need to initialize to 0 these?

            int siteCohortsKilled = 0; //why initialize this here since you reset to 0 inside the foreach loop?
            int[] cohortsKilled = new int[3];

            //for each active site calculate the probability of changing status between S-I-D
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {

                siteCohortsKilled = 0; //see comment above...
                random = 0;

                double myRand = PlugIn.ModelCore.GenerateUniform();

                double weatherIndex = CalculateWeatherIndex(agent, site);

                //force of infection depends on the dispersal kernel, weather index, SHI of neighboring sites and itself, pSusceptible & pInfected of neighboring sites 
                //FOI_i = Beta * sum(SHI_j * SHI_i * PSusceptible_j * PInfected_j * Kernel(dist_i_j))
                
                double FOI = 0;  // FIXME

                deltaPSusceptible = -FOI * SiteVars.PSusceptible[site];
                deltaPInfected = FOI * SiteVars.PSusceptible[site] - agent.AcquisitionRate * SiteVars.PInfected[site];  //rD = acquisition rate
                deltaPDiseased = agent.AcquisitionRate * SiteVars.PInfected[site];

                SiteVars.PSusceptible[site] = SiteVars.PSusceptible[site] + deltaPSusceptible;
                SiteVars.PInfected[site] = SiteVars.PInfected[site] + deltaPInfected;
                SiteVars.PDiseased[site] = SiteVars.PDiseased[site] + deltaPDiseased;

                // SUSCEPTIBLE --->> INFECTED
                if (SiteVars.InfStatus[site] == 0 && SiteVars.PInfected[site] >= myRand)  //if site is Susceptible (S) 
                {
                    //update state of current site from S to I
                    SiteVars.InfStatus[site] = 1;
                    totalSitesInfected++;
                }

                // INFECTED --->> DISEASED -mortality-
                if (SiteVars.InfStatus[site] == 2 && SiteVars.PDiseased[site] >= myRand) //if site is "diseased" then apply the mortality to affected cohorts 
                {
                    totalSitesDiseased++;
                    random = myRand;
                    cohortsKilled = KillSiteCohorts(site);
                    siteCohortsKilled = cohortsKilled[0];
                    siteMortSppKilled = cohortsKilled[2];

                    //check with Brian to better understand what this does... 
                    if (SiteVars.NumberCFSconifersKilled[site].ContainsKey(PlugIn.ModelCore.CurrentTime))
                    {
                        int prevKilled = SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime];
                        SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime] = prevKilled + cohortsKilled[1];
                    }
                    else
                    {
                        SiteVars.NumberCFSconifersKilled[site].Add(PlugIn.ModelCore.CurrentTime, cohortsKilled[1]);
                    }

                    //check with Brian to better understand what this does... 
                    if (SiteVars.NumberMortSppKilled[site].ContainsKey(PlugIn.ModelCore.CurrentTime))
                    {
                        int prevKilled = SiteVars.NumberMortSppKilled[site][PlugIn.ModelCore.CurrentTime];
                        SiteVars.NumberMortSppKilled[site][PlugIn.ModelCore.CurrentTime] = prevKilled + cohortsKilled[2];
                    }
                    else
                    {
                        SiteVars.NumberMortSppKilled[site].Add(PlugIn.ModelCore.CurrentTime, cohortsKilled[2]);
                    }

                    //if there is at least one cohort killed by current epidemic event
                    if (siteCohortsKilled > 0)
                    {
                        totalCohortsKilled += siteCohortsKilled;  //cumulate number of cohorts killed
                        totalSitesDamaged++; //cumulate number of sites damaged
                        SiteVars.TimeOfLastEvent[site] = PlugIn.ModelCore.CurrentTime;

                        //if there is at least one cohort killed by current epidemic event (among selected species of interest - FLAG YES)
                        if (siteMortSppKilled > 0)
                            totalMortSppCohortsKilled += siteMortSppKilled; //cumulate number of cohorts killed
                    }
                }
            }
        }  

        //---------------------------------------------------------------------
        //A small helper function for going through list of cohorts at a site
        //and checking them with the filter provided by RemoveMarkedCohort(ICohort).
        private int[] KillSiteCohorts(ActiveSite site)
        {
            siteCohortsKilled = 0;
            siteCFSconifersKilled = 0;
            siteMortSppKilled = 0;

            currentSite = site;

            SiteVars.Cohorts[site].RemoveMarkedCohorts(this);

            int[] cohortsKilled = new int[3];

            cohortsKilled[0] = siteCohortsKilled;
            cohortsKilled[1] = siteCFSconifersKilled;
            cohortsKilled[2] = siteMortSppKilled;

            return cohortsKilled;
        }        
        
        //---------------------------------------------------------------------
        // This is a filter to determine which cohorts are removed.
        // Each cohort is passed into the function and tested whether it should
        // be killed.
        bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            
            bool killCohort = false;

            ISppParameters sppParms = epidemicParms.SppParameters[cohort.Species.Index];
 
            if (cohort.Age >= sppParms.LowVulnHostAge)
            {
                if (random <= sppParms.LowVulnHostMortProb)
                   killCohort = true;
            }

            if (cohort.Age >= sppParms.MediumVulnHostAge)
            {
                if (random <= sppParms.MediumVulnHostMortProb)
                    killCohort = true;
            }

            if (cohort.Age >= sppParms.HighVulnHostAge)
            {
                if (random <= sppParms.HighVulnHostMortProb)
                    killCohort = true;
            }

            if (killCohort)
            {
                siteCohortsKilled++;

                if (sppParms.CFSConifer)
                    siteCFSconifersKilled++;

                if (sppParms.MortSppFlag)
                    siteMortSppKilled++;

            }

            return killCohort;
        }

        double CalculateWeatherIndex(IAgent agent, Site site)
        {
            double weatherIndex = 1;

            foreach(string weatherVar in agent.WeatherIndexVars)
            {
                foreach(DerivedClimateVariable derClimVar in agent.DerivedClimateVars)
                {
                    if (derClimVar.Name == weatherVar)
                    {

                    }
                }
                if(weatherVar == "TempIndex")
                {
                    int indexa = agent.TempIndexModel.Parameters.FindIndex(i => i == "a");
                    double a = Double.Parse(agent.TempIndexModel.Values[indexa]);
                    int indexb = agent.TempIndexModel.Parameters.FindIndex(i => i == "b");
                    double b = Double.Parse(agent.TempIndexModel.Values[indexb]);
                    int indexc = agent.TempIndexModel.Parameters.FindIndex(i => i == "c");
                    double c = Double.Parse(agent.TempIndexModel.Values[indexc]);
                    int indexd = agent.TempIndexModel.Parameters.FindIndex(i => i == "d");
                    double d = Double.Parse(agent.TempIndexModel.Values[indexd]);
                    int indexe = agent.TempIndexModel.Parameters.FindIndex(i => i == "e");
                    double e = Double.Parse(agent.TempIndexModel.Values[indexe]);
                    int indexf = agent.TempIndexModel.Parameters.FindIndex(i => i == "f");
                    double f = Double.Parse(agent.TempIndexModel.Values[indexf]);
                    int indexVar = agent.TempIndexModel.Parameters.FindIndex(i => i == "Variable");
                    string variableName = agent.TempIndexModel.Values[indexVar];

                    double variable = 0;
                    foreach(IClimateVariableDefinition climateVar in agent.ClimateVars)
                    {
                        if(climateVar.Name == variableName)
                        {
                            variable = SiteVars.ClimateVars[site][variableName];
                        }
                    }
                    //tempIndex = a + b * exp(c[ln(Variable / d) / e] ^ f);
                    double tempIndex = a + b * Math.Exp(c * Math.Pow((Math.Log(variable / d) / e),f));
                    

                    weatherIndex *= tempIndex;
                }
            }


            return weatherIndex;
        }

    }



}

