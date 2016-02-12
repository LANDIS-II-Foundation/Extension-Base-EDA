//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda

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
        ///Calculate the Site Host Index (SHI) for all active sites.
        ///The SHI averages the host value for each species as defined in the
        ///EDA species table.
        ///SHI ranges from 0 - 1.
        ///</summary>
        //---------------------------------------------------------------------
        public static void SiteHostIndexCompute(IAgent agent)
        {
            PlugIn.ModelCore.UI.WriteLine("   Calculating EDA Total Site Host Index.");

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {

                double sumValue = 0.0;
                double maxValue = 0.0;
                int    ageOldestCohort= 0;
                int    numValidSpp = 0;
                double speciesHostValue = 0;

                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    //get age of oldest cohort: maybe change this to use ALL cohort ages. How to do so?
                    ageOldestCohort = Util.GetMaxAge(SiteVars.Cohorts[site][species]);
                    ISppParameters sppParms = agent.SppParameters[species.Index];
                    if (sppParms == null)
                        continue;

                    //this chunk of code check if the current species in the current active site
                    //is part of the ignored species list. If any of the species are in that list, then negList --> true
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

                        if (ageOldestCohort >= sppParms.LowHostAge)
                            //speciesHostValue = 0.33; VALUES ARE READ IN FROM THE EDA Spp Param Table
                            speciesHostValue = sppParms.LowHostScore;

                        if (ageOldestCohort >= sppParms.MediumHostAge)
                            //speciesHostValue = 0.66; VALUES ARE READ IN FROM THE EDA Spp Param Table
                            speciesHostValue = sppParms.MediumHostScore;

                        if (ageOldestCohort >= sppParms.HighHostAge)
                            //speciesHostValue = 1.0; VALUES ARE READ IN FROM THE EDA Spp Param Table
                            speciesHostValue = sppParms.HighHostScore;


                        sumValue += speciesHostValue;
                        maxValue = System.Math.Max(maxValue, speciesHostValue);
                    }
                }

                if (agent.SHImode == SHImode.mean)
                    SiteVars.SiteHostIndex[site] = sumValue / numValidSpp; //division will be (double) because sumValue was initialized to 0.0

                if (agent.SHImode == SHImode.max)
                    SiteVars.SiteHostIndex[site] = maxValue;

            }

        }  //end SiteHostIndexCompute

        //---------------------------------------------------------------------
        ///<summary>
        ///Calculate the Site Resource Dominance MODIFIER for all active sites.
        ///Site Resource Dominance Modifier takes into account other disturbances and
        ///any ecoregion modifiers defined.
        ///SRDMods range from 0 - 1.
        ///</summary>
        //---------------------------------------------------------------------
        public static void SiteHostIndexModCompute(IAgent agent)
        {

            PlugIn.ModelCore.UI.WriteLine("   Calculating EDA Modified Site Host Index.");

            //we need to calculate a relative SHIM for each site by dividing absolute SHIM with the average SHIM_mean over the landscape
            //The relative index allows us to compare the transmission rate β against homogeneous landscape conditions (where hi=1) 
            //and to interpret β as the rate of secondary infection of typical cells by a single infected typical cell in a non-infected landscape 

            //define a variable to store cumulative (running sum) SHIM over the landscape
            double SHMI_CumSum = 0.0;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {

                if (SiteVars.SiteHostIndex[site] > 0.0)
                {
                    int     lastDisturb = 0;
                    int     duration = 0;
                    double  disturbMod = 0.0;
                    double  sumDisturbMods = 0.0;
                    double  SHIM = 0.0;

                    //---- APPLY DISTURBANCE MODIFIERS (DMs) --------
                    // The assumption for DMs is that their impact decreases LINEARLY over time up to a max impact duration
      
                    IEnumerable<IDisturbanceType> disturbanceTypes = agent.DisturbanceTypes;
                    foreach (DisturbanceType disturbance in disturbanceTypes)
                    {
                        //Check for harvest effects on SHI
                        if (SiteVars.HarvestCohortsKilled != null && SiteVars.HarvestCohortsKilled[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastHarvest[site];
                            duration = disturbance.ImpactDuration;

                            if (SiteVars.TimeOfLastHarvest != null && (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if ((SiteVars.HarvestPrescriptionName != null && SiteVars.HarvestPrescriptionName[site].Trim() == pName.Trim()) || (pName.Trim() == "Harvest"))
                                    {
                                        disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for fire severity effects on SHI
                        if (SiteVars.FireSeverity != null && SiteVars.FireSeverity[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastFire[site];
                            duration = disturbance.ImpactDuration;

                            if (SiteVars.TimeOfLastFire != null && (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if (pName.StartsWith("FireSeverity"))
                                    {
                                        if ((pName.Substring((pName.Length - 1), 1)).ToString() == SiteVars.FireSeverity[site].ToString())
                                        {
                                            disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                            sumDisturbMods += disturbMod;
                                        }
                                    }
                                    else if (pName.Trim() == "Fire") // Generic for all fire
                                    {
                                        disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for wind severity effects on SHI
                        if (SiteVars.WindSeverity != null && SiteVars.WindSeverity[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastWind[site];
                            duration = disturbance.ImpactDuration;

                            if (SiteVars.TimeOfLastWind != null &&
                                (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if (pName.StartsWith("WindSeverity"))
                                    {
                                        if ((pName.Substring((pName.Length - 1), 1)).ToString() == SiteVars.WindSeverity[site].ToString())
                                        {
                                            disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                            sumDisturbMods += disturbMod;
                                        }
                                    }
                                    else if (pName.Trim() == "Wind") // Generic for all wind
                                    {
                                        disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for Biological Disturbance Agents (BDA) effects on SHI
                        if (SiteVars.BDASeverity != null && SiteVars.BDASeverity[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastBDA[site];
                            duration = disturbance.ImpactDuration;

                            if (SiteVars.TimeOfLastBDA != null &&
                                (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if (pName.StartsWith("BDASeverity"))
                                    {
                                        if ((pName.Substring((pName.Length - 1), 1)).ToString() == SiteVars.BDASeverity[site].ToString())
                                        {
                                            disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                            sumDisturbMods += disturbMod;
                                        }
                                    }
                                    else if (pName.Trim() == "BDA") // Generic for all BDAs
                                    {
                                        disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for Biomass Insects effects on SHI
                        if (SiteVars.TimeOfLastBiomassInsects != null && SiteVars.TimeOfLastBiomassInsects[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastBiomassInsects[site];
                            duration = disturbance.ImpactDuration;

                            if (SiteVars.TimeOfLastBiomassInsects != null && (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration))
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if((SiteVars.BiomassInsectsAgent[site].Trim() == pName.Trim()) || (pName.Trim() == "BiomassInsects"))
                                    {
                                        disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                        //Check for other EDA agent effects on SHI
                        if (SiteVars.TimeOfLastEvent[site] > 0)
                        {
                            lastDisturb = SiteVars.TimeOfLastEvent[site];
                            duration = disturbance.ImpactDuration;

                            if (PlugIn.ModelCore.CurrentTime - lastDisturb <= duration)
                            {
                                foreach (string pName in disturbance.PrescriptionNames)
                                {
                                    if ((SiteVars.AgentName[site].Trim() == pName.Trim()) || (pName.Trim() == "BDA"))
                                    {
                                        disturbMod = disturbance.SHIModifier * System.Math.Max(0, (double)(PlugIn.ModelCore.CurrentTime - lastDisturb)) / duration;
                                        sumDisturbMods += disturbMod;
                                    }
                                }
                            }
                        }
                    }
                
                    //---- APPLY ECOREGION MODIFIERS --------
                    IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                    
                    SHIM = SiteVars.SiteHostIndex[site] +
                           sumDisturbMods +
                           agent.EcoParameters[ecoregion.Index].EcoModifier;

                    //constrain the absolute value of SHIM within range 0-1
                    SHIM = System.Math.Max(0.0, SHIM);
                    SHIM = System.Math.Min(1.0, SHIM);

                    SiteVars.SiteHostIndexMod[site] = SHIM;

                    //add current site absolute index to the cumulative sum over the landscape
                    SHMI_CumSum += SHIM;

                }//end of one site

                else SiteVars.SiteHostIndexMod[site] = 0.0;
            } //end Active sites

            //Calculating Relative Modified Site Host Index Values for Active Sites
            PlugIn.ModelCore.UI.WriteLine("   Calculating Relative Modified Site Host Index Values for Active Sites.");

            double SHIM_mean = (double) SHMI_CumSum / PlugIn.ModelCore.Landscape.ActiveSiteCount;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                if (SiteVars.SiteHostIndexMod[site] > 0.0)
                {
                    //override index value with new relative (normalized) value by the landscape mean
                    SiteVars.SiteHostIndexMod[site] = SiteVars.SiteHostIndexMod[site] / SHIM_mean;
                }
            }// end Active sites

        } //end Function

    }//End of SiteResources
}
