//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.

using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System.Collections.Generic;

namespace Landis.Extension.BaseEDA
{

    public class SiteResources
    {

        //---------------------------------------------------------------------
        ///<summary>
        ///Calculate the Site Host Susceptibility (SHS) for all active sites.
        ///The SHS averages the susceptibility for each species as defined in the
        ///EDA species table.
        ///SHS ranges from 0 - 1.
        ///</summary>
        //---------------------------------------------------------------------
        public static void SiteHostSusceptibility(IAgent agent, int ROS)
        {
            PlugIn.ModelCore.UI.WriteLine("   Calculating BDA Site Resource Dominance.");

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {

                double sumValue = 0.0;
                double maxValue = 0.0;
                int    ageOldestCohort= 0;
                int    numValidSpp = 0;
                double speciesHostValue = 0;

                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    ageOldestCohort = Util.GetMaxAge(SiteVars.Cohorts[site][species]);
                    ISppParameters sppParms = agent.SppParameters[species.Index];
                    if (sppParms == null)
                        continue;

                    bool negList = false;
                    foreach (ISpecies negSpp in agent.NegSppList)
                    {
                        if (species == negSpp)
                            negList = true;
                    }

                    if ((ageOldestCohort > 0) && (! negList))
                    {
                        numValidSpp++;
                        speciesHostValue = 0.0;

                        if (ageOldestCohort >= sppParms.MinorHostAge)
                            //speciesHostValue = 0.33;
                            speciesHostValue = sppParms.MinorHostSRD;

                        if (ageOldestCohort >= sppParms.SecondaryHostAge)
                            //speciesHostValue = 0.66;
                            speciesHostValue = sppParms.SecondaryHostSRD;

                        if (ageOldestCohort >= sppParms.PrimaryHostAge)
                            //speciesHostValue = 1.0;
                            speciesHostValue = sppParms.PrimaryHostSRD;


                        sumValue += speciesHostValue;
                        maxValue = System.Math.Max(maxValue, speciesHostValue);
                    }
                }

                if (agent.SRDmode == SRDmode.mean)
                    SiteVars.SiteHostSuscept[site] = sumValue / (double) numValidSpp;

                if (agent.SRDmode == SRDmode.max)
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
                    double  SRDM = 0.0;

                    // Next check the disturbance types.  This will provide the disturbance modifier
                    // Check for harvest effects on SRD
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
                                        disturbMod = disturbance.SRDModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
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
                                            disturbMod = disturbance.SRDModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                            sumDisturbMods += disturbMod;
                                        }
                                    }
                                    else if (pName.Trim() == "Fire") // Generic for all fire
                                    {
                                        disturbMod = disturbance.SRDModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
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
                                            disturbMod = disturbance.SRDModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                            sumDisturbMods += disturbMod;
                                        }
                                    }
                                    else if (pName.Trim() == "Wind") // Generic for all wind
                                    {
                                        disturbMod = disturbance.SRDModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
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
                                        disturbMod = disturbance.SRDModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
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
                                        disturbMod = disturbance.SRDModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                    }


                    //PlugIn.ModelCore.Log.WriteLine("   Summation of Disturbance Modifiers = {0}.", sumMods);
                    //---- APPLY ECOREGION MODIFIERS --------
                    IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];


                    SRDM = SiteVars.SiteHostSuscept[site] +
                           sumDisturbMods +
                           agent.EcoParameters[ecoregion.Index].EcoModifier;

                    SRDM = System.Math.Max(0.0, SRDM);
                    SRDM = System.Math.Min(1.0, SRDM);

                    SiteVars.SiteHostSusceptMod[site] = SRDM;
                }//end of one site

                else SiteVars.SiteHostSusceptMod[site] = 0.0;
            } //end Active sites
        } //end Function

        //---------------------------------------------------------------------
        ///<summary>
        ///Calculate SITE VULNERABILITY
        ///Following equations found in Sturtevant et al. 2004.
        ///Ecological Modeling 180: 153-174.
        ///</summary>
        //---------------------------------------------------------------------
        public static void SiteVulnerability(IAgent agent,
                                                int ROS,
                                                bool considerNeighbor)
        {
            double   SRD, SRDMod, NRD;
            double   CaliROS3 = ((double) ROS / 3) * agent.BDPCalibrator;

            //PlugIn.ModelCore.Log.WriteLine("   Calculating BDA SiteVulnerability.");

            if (considerNeighbor)      //take neigborhood into consideration
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
            {
                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                {
                    SRDMod = SiteVars.SiteHostSusceptMod[site];

                    double vulnerable = (double)(CaliROS3 * SRDMod);
                    SiteVars.Vulnerability[site] = System.Math.Max(0, vulnerable);
                }
            }
        }

//End of SiteResources
    }
}
