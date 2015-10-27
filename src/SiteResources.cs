//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.

using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System.Collections.Generic;

namespace Landis.Extension.BaseEDA
{

    public class SiteSusceptibility
    {

        //---------------------------------------------------------------------
        ///<summary>
        ///Calculate the Site Host Susceptibility (SHS) for all active sites.
        ///The SHS averages the susceptibility for each species as defined in the
        ///EDA species table.
        ///SHS ranges from 0 - 1.
        ///</summary>
        //---------------------------------------------------------------------
        public static void SiteHostSusceptibility(IAgent agent)//, int ROS)
        {
            PlugIn.ModelCore.UI.WriteLine("   Calculating EDA Site Resource Dominance.");

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {

                double sumValue = 0.0;
                double maxValue = 0.0;
                int    ageOldestCohort= 0;
                int    numValidSpp = 0;
                double speciesHostValue = 0;

                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    //get age of oldest cohort
                    ageOldestCohort = Util.GetMaxAge(SiteVars.Cohorts[site][species]);
                    ISppParameters sppParms = agent.SppParameters[species.Index];
                    if (sppParms == null)
                        continue;

                    bool negList = false;
                    foreach (ISpecies negSpp in agent.NegSppList)  //what is NegSppList?
                    {
                        if (species == negSpp)
                            negList = true;
                    }

                    if ((ageOldestCohort > 0) && (! negList))
                    {
                        numValidSpp++;
                        speciesHostValue = 0.0;

                        if (ageOldestCohort >= sppParms.LowHostAge)
                            //speciesHostValue = 0.33; VALUES ARE READ IN FROM THE EDA Spp Param Table
                            speciesHostValue = sppParms.LowHostSusceptInfProb;

                        if (ageOldestCohort >= sppParms.MediumHostAge)
                            //speciesHostValue = 0.66; VALUES ARE READ IN FROM THE EDA Spp Param Table
                            speciesHostValue = sppParms.MediumHostSusceptInfProb;

                        if (ageOldestCohort >= sppParms.HighHostAge)
                            //speciesHostValue = 1.0; VALUES ARE READ IN FROM THE EDA Spp Param Table
                            speciesHostValue = sppParms.HighHostSusceptInfProb;


                        sumValue += speciesHostValue;
                        maxValue = System.Math.Max(maxValue, speciesHostValue);
                    }
                }

                if (agent.SHSmode == SHSmode.mean)
                    SiteVars.SiteHostSuscept[site] = sumValue / (double) numValidSpp;

                if (agent.SHSmode == SHSmode.max)
                    SiteVars.SiteHostSuscept[site] = maxValue;

            }

        }  //end siteHostSuscept

        //---------------------------------------------------------------------
        ///<summary>
        ///Calculate the Site Resource Dominance MODIFIER for all active sites.
        ///Site Resource Dominance Modifier takes into account other disturbances and
        ///any ecoregion modifiers defined.
        ///SRDMods range from 0 - 1.
        ///</summary>
        //---------------------------------------------------------------------
        public static void SiteHostSusceptibilityModifier(IAgent agent)
        {

            PlugIn.ModelCore.UI.WriteLine("   Calculating EDA Modified Site Host Susceptibility.");
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {

                if (SiteVars.SiteHostSuscept[site] > 0.0)
                {
                    int     lastDisturb = 0;
                    int     duration = 0;
                    double  disturbMod = 0;
                    double  sumDisturbMods = 0.0;
                    double  SHSM = 0.0;

                    // Next check the disturbance types.  This will provide the disturbance modifier
                    // Check for harvest effects on SHS
                    IEnumerable<IDisturbanceType> disturbanceTypes = agent.DisturbanceTypes;
                    foreach (DisturbanceType disturbance in disturbanceTypes)
                    {
                        if (SiteVars.HarvestCohortsKilled != null && SiteVars.HarvestCohortsKilled[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastHarvest[site];
                            duration = disturbance.MaxAge;

                            if (SiteVars.TimeOfLastHarvest != null && (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if ((SiteVars.HarvestPrescriptionName != null && SiteVars.HarvestPrescriptionName[site].Trim() == pName.Trim()) || (pName.Trim() == "Harvest"))
                                    {
                                        disturbMod = disturbance.SHSModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for fire severity effects
                        if (SiteVars.FireSeverity != null && SiteVars.FireSeverity[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastFire[site];
                            duration = disturbance.MaxAge;

                            if (SiteVars.TimeOfLastFire != null && (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if (pName.StartsWith("FireSeverity"))
                                    {
                                        if ((pName.Substring((pName.Length - 1), 1)).ToString() == SiteVars.FireSeverity[site].ToString())
                                        {
                                            disturbMod = disturbance.SHSModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                            sumDisturbMods += disturbMod;
                                        }
                                    }
                                    else if (pName.Trim() == "Fire") // Generic for all fire
                                    {
                                        disturbMod = disturbance.SHSModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for wind severity effects
                        if (SiteVars.WindSeverity != null && SiteVars.WindSeverity[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastWind[site];
                            duration = disturbance.MaxAge;

                            if (SiteVars.TimeOfLastWind != null &&
                                (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if (pName.StartsWith("WindSeverity"))
                                    {
                                        if ((pName.Substring((pName.Length - 1), 1)).ToString() == SiteVars.WindSeverity[site].ToString())
                                        {
                                            disturbMod = disturbance.SHSModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                            sumDisturbMods += disturbMod;
                                        }
                                    }
                                    else if (pName.Trim() == "Wind") // Generic for all wind
                                    {
                                        disturbMod = disturbance.SHSModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for Biomass Insects effects
                        if (SiteVars.TimeOfLastBiomassInsects != null && SiteVars.TimeOfLastBiomassInsects[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastBiomassInsects[site];
                            duration = disturbance.MaxAge;

                            if (SiteVars.TimeOfLastBiomassInsects != null && (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if((SiteVars.BiomassInsectsAgent[site].Trim() == pName.Trim()) || (pName.Trim() == "BiomassInsects"))
                                    {
                                        disturbMod = disturbance.SHSModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for other BDA agent effects
                        if (SiteVars.TimeOfLastEvent[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastEvent[site];
                            duration = disturbance.MaxAge;

                            if (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration)
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if ((SiteVars.AgentName[site].Trim() == pName.Trim()) || (pName.Trim() == "BDA"))
                                    {
                                        disturbMod = disturbance.SHSModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                    }


                    //PlugIn.ModelCore.Log.WriteLine("   Summation of Disturbance Modifiers = {0}.", sumMods);
                    //---- APPLY ECOREGION MODIFIERS --------
                    IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];


                    SHSM = SiteVars.SiteHostSuscept[site] +
                           sumDisturbMods +
                           agent.EcoParameters[ecoregion.Index].EcoModifier;

                    SHSM = System.Math.Max(0.0, SHSM);
                    SHSM = System.Math.Min(1.0, SHSM);

                    SiteVars.SiteHostSusceptMod[site] = SHSM;
                }//end of one site

                else SiteVars.SiteHostSusceptMod[site] = 0.0;
            } //end Active sites
        } //end Function

        //---------------------------------------------------------------------
        ///<summary>
        ///Calculate SITE EPIDEMIOLOGICAL DISTURBANCE PROBABILITY (EDP)
        ///</summary>
        //---------------------------------------------------------------------
        public static void SiteDistProbability(IAgent agent)//,
                                                //int ROS,
                                                //bool considerNeighbor)
        {
            double SHS, SHSMod;
            //double SRD, SRDMod;
            //double NRD; 
            //double   CaliROS3 = ((double) ROS / 3) * agent.BDPCalibrator;

            //PlugIn.ModelCore.Log.WriteLine("   Calculating BDA SiteVulnerability.");

            /*if (considerNeighbor)      //take neigborhood into consideration
            {
                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                {

                    SRD = SiteVars.SiteHostSuscept[site];

                    //If a site has been chosen for an outbreak and there are
                    //resources available for an outbreak:
                    if (SRD > 0)
                    {
                        SRDMod = SiteVars.SiteHostSusceptMod[site];
                        double tempSV = 0.0;

                        //Equation (8) in Sturtevant et al. 2004.
                        tempSV = SRDMod + (NRD * agent.NeighborWeight);
                        tempSV = tempSV / (1 + agent.NeighborWeight);
                        double vulnerable = (double)(CaliROS3 * tempSV);
                        //PlugIn.ModelCore.Log.WriteLine("tempSV={0}, SRDMod={1}, NRD={2}, neighborWeight={3}.", tempSV, SRDMod,NRD,agent.NeighborWeight);

                        SiteVars.Vulnerability[site] = System.Math.Max(0.0, vulnerable);
                        //PlugIn.ModelCore.Log.WriteLine("Site Vulnerability = {0}, CaliROS3={1}, tempSV={2}.", SiteVars.Vulnerability[site], CaliROS3, tempSV);
                    }
                    else
                        SiteVars.Vulnerability[site] = 0.0;
                }
            }
            else        //Do NOT take neigborhood into consideration
            {*/
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
              {
                  SHSMod = SiteVars.SiteHostSusceptMod[site];

                //double vulnerable = (double)(CaliROS3 * SRDMod);
                SiteVars.EpidemDistProb[site] = System.Math.Max(0, SHSMod);
              }
            //}
        }

//End of SiteSusceptibility
    }
}
