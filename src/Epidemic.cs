//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Landis.Extension.BaseEDA
{
    public class Epidemic
        : ICohortDisturbance

    {
        private static IEcoregionDataset ecoregions;
        private IAgent epidemicParms;
        private double random;

        // Grids that are precalculated to improve performance of ComputeSiteFOI
        private double[,] siteHostIndexMod;
        private double[,] pInfected;
        private double[,] pDiseased;

        // Kernel used to improve performance of ComputeSiteFOI
        private double[,] kernel;
        private int kernelCellRadius;

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
            SiteVars.FOI.ActiveSiteValues = 0;

        }

        //---------------------------------------------------------------------
        ///<summary>
        ///Simulate an Epidemic - This is the controlling function that calls the
        ///subsequent function.  The basic logic of an epidemic resides here.
        ///</summary>
        public static Epidemic Simulate(IAgent agent, int currentTime, int agentIndex)
        {

            Epidemic CurrentEpidemic = new Epidemic(agent);
            PlugIn.ModelCore.UI.WriteLine("   New EDA Epidemic Started.");

            SiteResources.SiteHostIndexCompute(agent);   
            SiteResources.SiteHostIndexModCompute(agent);
            ClimateVariableDefinition.CalculateClimateVariables(agent);
            CurrentEpidemic.ComputeSiteInfStatus(agent, agentIndex);

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
        private void ComputeSiteInfStatus(IAgent agent, int agentIndex)
        {
            PlugIn.ModelCore.UI.WriteLine("   Computing weather index and force of infection for each cell...");
            int siteCohortsKilled = 0; //why initialize this here since you reset to 0 inside the foreach loop?
            int[] cohortsKilled = new int[3];

            // Precalculate the grids and kernel once per time step to improve performance
            PrepareGrids(agentIndex);
            kernel = CreateKernel(agent);

            // Uncomment the following line (and provide a legitimate path and grid) to see the grid
            //LogGrid(pathOfOutputFile, grid);

            // for each active site calculate the probability of changing status between S-I-D
            // Perform in parallel to better utilize processors
            Parallel.ForEach(PlugIn.ModelCore.Landscape, (site) =>
            {
                //get weather index for the current site
                double weatherIndex = SiteVars.ClimateVars[site]["AnnualWeatherIndex"];

                //normalize by historic average weatherindex
                double normalizedWI = weatherIndex / agent.EcoWeatherIndexNormal[PlugIn.ModelCore.Ecoregion[site].Index];

                //calculate force of infection for current site                
                SiteVars.FOI[site] = ComputeSiteFOI(agent, site, normalizedWI, agentIndex);
            });

            PlugIn.ModelCore.UI.WriteLine("   Computing infection status for each cell...");
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                siteCohortsKilled = 0;
                random = 0;

                double myRand = PlugIn.ModelCore.GenerateUniform();

                double deltaPSusceptible = 0.0;
                double deltaPInfected = 0.0;
                double deltaPDiseased = 0.0;

                deltaPSusceptible = -SiteVars.FOI[site] * SiteVars.PSusceptible[site][agentIndex];
                deltaPInfected = SiteVars.FOI[site] * SiteVars.PSusceptible[site][agentIndex] - agent.AcquisitionRate * SiteVars.PInfected[site][agentIndex];  //rD = acquisition rate
                deltaPDiseased = agent.AcquisitionRate * SiteVars.PInfected[site][agentIndex];

                //update probs of being in each considered status (S, I, D)
                SiteVars.PSusceptible[site][agentIndex] += deltaPSusceptible;
                if (SiteVars.PSusceptible[site][agentIndex] > 1) { SiteVars.PSusceptible[site][agentIndex] = 1; }
                else if (SiteVars.PSusceptible[site][agentIndex] < 0) { SiteVars.PSusceptible[site][agentIndex] = 0; }

                SiteVars.PInfected[site][agentIndex] += deltaPInfected;
                if (SiteVars.PInfected[site][agentIndex] > 1) { SiteVars.PInfected[site][agentIndex] = 1; }
                else if (SiteVars.PInfected[site][agentIndex] < 0) { SiteVars.PInfected[site][agentIndex] = 0; }

                SiteVars.PDiseased[site][agentIndex] += deltaPDiseased;
                if (SiteVars.PDiseased[site][agentIndex] > 1) { SiteVars.PDiseased[site][agentIndex] = 1; }
                else if (SiteVars.PDiseased[site][agentIndex] < 0) { SiteVars.PDiseased[site][agentIndex] = 0; }

                //update status of cell based on updated probabilities
                // SUSCEPTIBLE --->> INFECTED
                if (SiteVars.InfStatus[site][agentIndex] == 0 && SiteVars.PInfected[site][agentIndex] >= myRand)  //if site is Susceptible (S) 
                {
                    //update state of current site from S to I
                    SiteVars.InfStatus[site][agentIndex] += 1;
                    totalSitesInfected++;
                }
                // INFECTED --->> DISEASED
                else if (SiteVars.InfStatus[site][agentIndex] == 1)
                {
                    totalSitesInfected++;

                    if (SiteVars.PDiseased[site][agentIndex] >= myRand)
                    {
                        //update state of current site from I to D
                        SiteVars.InfStatus[site][agentIndex] += 1;
                        totalSitesDiseased++;

                        //check for cohort mortality against same random unif number
                        random = myRand;
                        cohortsKilled = KillSiteCohorts(site);
                        siteCohortsKilled = cohortsKilled[0];
                        siteMortSppKilled = cohortsKilled[2];

                        if (SiteVars.NumberCFSconifersKilled[site].ContainsKey(PlugIn.ModelCore.CurrentTime))
                        {
                            int prevKilled = SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime];
                            SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime] = prevKilled + cohortsKilled[1];
                        }
                        else
                        {
                            SiteVars.NumberCFSconifersKilled[site].Add(PlugIn.ModelCore.CurrentTime, cohortsKilled[1]);
                        }

                        SiteVars.NumberMortSppKilled[site][agentIndex] = cohortsKilled[2];

                        //if there is at least one cohort killed by current epidemic event
                        if (siteCohortsKilled > 0)
                        {
                            PlugIn.ModelCore.UI.WriteLine("   Computing cohort mortality for each cell...");
                            totalCohortsKilled += siteCohortsKilled;  //cumulate number of cohorts killed
                            totalSitesDamaged++; //cumulate number of sites damaged
                            SiteVars.TimeOfLastEvent[site] = PlugIn.ModelCore.CurrentTime;

                            //if there is at least one cohort killed by current epidemic event (among selected species of interest - FLAG YES)
                            if (siteMortSppKilled > 0)
                                totalMortSppCohortsKilled += siteMortSppKilled; //cumulate number of cohorts killed
                        }
                    }
                }
                else if (SiteVars.InfStatus[site][agentIndex] == 2)
                {
                    totalSitesDiseased++;

                    //check for cohort mortality against same random unif number
                    random = myRand;
                    cohortsKilled = KillSiteCohorts(site);
                    siteCohortsKilled = cohortsKilled[0];
                    siteMortSppKilled = cohortsKilled[2];

                    if (SiteVars.NumberCFSconifersKilled[site].ContainsKey(PlugIn.ModelCore.CurrentTime))
                    {
                        int prevKilled = SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime];
                        SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime] = prevKilled + cohortsKilled[1];
                    }
                    else
                    {
                        SiteVars.NumberCFSconifersKilled[site].Add(PlugIn.ModelCore.CurrentTime, cohortsKilled[1]);
                    }

                    SiteVars.NumberMortSppKilled[site][agentIndex] = cohortsKilled[2];

                    //if there is at least one cohort killed by current epidemic event
                    if (siteCohortsKilled > 0)
                    {
                        PlugIn.ModelCore.UI.WriteLine("   Computing cohort mortality for each cell...");
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

        // check if the coordinates are inside the map 
        private bool isInside(int row, int col) 
        {
            return (row >= 1 && col >= 1 && col <= PlugIn.ModelCore.Landscape.Dimensions.Columns && row <= PlugIn.ModelCore.Landscape.Dimensions.Rows); 
        }

         //Calculate the distance from a location to a center
        //point (row and column = 0).
        private static double DistanceFromCenter(Site site, double row, double column)
        {
            double CellLength = PlugIn.ModelCore.CellLength;
            //row = System.Math.Abs(row) * CellLength;
            //column = System.Math.Abs(column) * CellLength;
            double dx = (row - site.Location.Row) * CellLength;
            double dy = (column - site.Location.Column) * CellLength;
            //double aSq = System.Math.Pow(column, 2);
            //double bSq = System.Math.Pow(row, 2);
            //return System.Math.Sqrt(aSq + bSq);
            return System.Math.Sqrt(System.Math.Pow(dx,2) + System.Math.Pow(dy,2));
        }

        //define func to calculate Force of Infection (FOI) for a given agent and site
        //force of infection depends on the dispersal kernel, weather index, SHI of neighboring sites and itself, pInfected & pDiseased of neighboring sites         
        double ComputeSiteFOI(IAgent agent, Site targetSite, double beta, int agentIndex)
        {
            int maxRadius = agent.DispersalMaxDist;

            //Dispersal dsp = new Dispersal();

            //PlugIn.ModelCore.UI.WriteLine("Looking for infection sources within the chosen neighborhood...");

            // Different implementations for calculating site FOI
            //double cumSum = SequentialLoop(targetSite, maxRadius, agentIndex);
            //double cumSum = SequentialLoopWithChanges(targetSite, maxRadius, agentIndex);
            double cumSum = KernelLoop(targetSite, agentIndex);

            //calculate force of infection: beta * cumSum
            double beta_t = beta * agent.TransmissionRate;   //beta_t = w(t) * beta_0
            double forceOfInf = beta_t * cumSum;

            return forceOfInf;
        }

        // Original implementation of ComputeSiteFOI
        double SequentialLoop(Site targetSite, int maxRadius, int agentIndex)
        {
            double kernelProb, cumSum = 0.0, centroidDistance = 0.0, CellLength = PlugIn.ModelCore.CellLength;
            int source_row, source_col;

            int numCellRadius = (int)(maxRadius / CellLength);

            for (int row = (numCellRadius * -1); row <= numCellRadius; row++)
            {
                for (int col = (numCellRadius * -1); col <= numCellRadius; col++)
                {

                    if (row == 0 && col == 0) continue; //we do not want to consider source cells overlapping with target (current) cell

                    //calculate location of source pixel 
                    source_row = targetSite.Location.Row + row;
                    source_col = targetSite.Location.Column + col;

                    if (isInside(source_row, source_col))
                    {
                        Site sourceSite = PlugIn.ModelCore.Landscape[source_row, source_col];
                        if (sourceSite != null && sourceSite.IsActive)
                        {
                            //distance of source pixel from current target site
                            centroidDistance = DistanceFromCenter(targetSite, source_row, source_col);
                            //check if source cell is within max disp dist
                            if (centroidDistance <= maxRadius && centroidDistance > 0)
                            {
                                //check if source pixel is infectious (=infected or diseased):
                                if (SiteVars.InfStatus[sourceSite][agentIndex] == 1 || SiteVars.InfStatus[sourceSite][agentIndex] == 2)
                                {
                                    //read kernel prob
                                    //kernelProb = dsp.GetDispersalProbability(centroidDistance);
                                    kernelProb = Dispersal.dispersal_probability[centroidDistance];

                                    //A_j: site host index modified -source-
                                    //B_i: site host index modified -target, current site-
                                    //C_j_I+D_i_S: conditional prob of site j being infected or disease, given site i is susceptible
                                    //to a first order of approximation this is ~= (P_Ij + P_Dj)
                                    //cumsum = sum(A_j * B_i * C_j_I+D_i_S * Kernel(d_ij))
                                    cumSum += SiteVars.SiteHostIndexMod[sourceSite] * SiteVars.SiteHostIndexMod[targetSite] *
                                                      (SiteVars.PInfected[sourceSite][agentIndex] + SiteVars.PDiseased[sourceSite][agentIndex]) * kernelProb;

                                }//end check if source site is infectious
                            }//end check if distance < maxdist

                        }//end check if source site is NOT null AND Active
                    }//end check if source isInside

                }//end col loop 
            }//end row loop

            return cumSum;
        }

        // Implementation of ComputeSiteFOI tweaked to remove redundant calls and improve performance
        double SequentialLoopImproved(Site targetSite, int maxRadius, int agentIndex)
        {
            double kernelProb, cumSum = 0.0, centroidDistance = 0.0, CellLength = PlugIn.ModelCore.CellLength;
            int numCellRadius = (int)(maxRadius / CellLength);
            int maxRadiusSquared = maxRadius * maxRadius;
            double cellLengthSquared = CellLength * CellLength;

            int targetRow = targetSite.Location.Row;
            int targetColumn = targetSite.Location.Column;

            int minRowIndex = (targetRow - numCellRadius > 0) ? targetRow - numCellRadius : 0;
            int minColumnIndex = (targetColumn - numCellRadius > 0) ? targetColumn - numCellRadius : 0;
            int maxRowIndex = (targetRow + numCellRadius < PlugIn.ModelCore.Landscape.Dimensions.Rows) 
                ? targetRow + numCellRadius : PlugIn.ModelCore.Landscape.Dimensions.Rows;
            int maxColumnIndex = (targetColumn + numCellRadius < PlugIn.ModelCore.Landscape.Dimensions.Columns)
               ? targetColumn + numCellRadius : PlugIn.ModelCore.Landscape.Dimensions.Columns;

            for (int row = minRowIndex; row <= maxRowIndex; row++)
            {
                bool withinRange = false;
                for (int col = minColumnIndex; col <= maxColumnIndex; col++)
                {
                    if (row == targetRow && col == targetColumn) continue; //we do not want to consider source cells overlapping with target (current) cell

                    double centroidSquared = ((targetRow - row) * (targetRow - row) + (targetColumn - col) * (targetColumn - col)) * cellLengthSquared;
                    if (centroidSquared > maxRadiusSquared)
                    {
                        if (withinRange) break;
                        else continue;
                    }
                    else
                    {
                        withinRange = true;
                    }

                    Site sourceSite = PlugIn.ModelCore.Landscape[row, col];
                    if (sourceSite.IsActive && (SiteVars.InfStatus[sourceSite][agentIndex] == 1 || SiteVars.InfStatus[sourceSite][agentIndex] == 2))
                    {
                        //distance of source pixel from current target site
                        centroidDistance = System.Math.Sqrt(centroidSquared);
                        //read kernel prob
                        //kernelProb = dsp.GetDispersalProbability(centroidDistance);
                        kernelProb = Dispersal.dispersal_probability[centroidDistance];

                        //A_j: site host index modified -source-
                        //B_i: site host index modified -target, current site-
                        //C_j_I+D_i_S: conditional prob of site j being infected or disease, given site i is susceptible
                        //to a first order of approximation this is ~= (P_Ij + P_Dj)
                        //cumsum = sum(A_j * B_i * C_j_I+D_i_S * Kernel(d_ij))
                        cumSum += SiteVars.SiteHostIndexMod[sourceSite] * SiteVars.SiteHostIndexMod[targetSite] *
                                            (SiteVars.PInfected[sourceSite][agentIndex] + SiteVars.PDiseased[sourceSite][agentIndex]) * kernelProb;

                    }//end check if source site is NOT null AND Active
                }//end col loop 
            }//end row loop

            return cumSum;
        }

        // Currently the fastest implementation available for ComputeSiteFOI
        double KernelLoop(Site targetSite, int agentIndex)
        {
            double cumSum = 0.0, targetHostIndexModValue;

            // Only need to get the target site info once since it doesn't change
            // If its value is 0, we can short circuit this function since the sum is always multiplied by this value
            targetHostIndexModValue = SiteVars.SiteHostIndexMod[targetSite];
            if (targetHostIndexModValue == 0.0)
            {
                return cumSum;
            }

            // Get the target site row and column
            int tr = targetSite.Location.Row, tc = targetSite.Location.Column;

            // Calculate the row and column indices for the kernel loop
            // If any index would be outside of the map, change that index to be the edge of the map instead
            int minRowIndex = (tr - kernelCellRadius > 0) ? -kernelCellRadius : 1 - tr;
            int minColumnIndex = (tc - kernelCellRadius > 0) ? -kernelCellRadius : 1 - tc;
            int maxRowIndex = (tr + kernelCellRadius < PlugIn.ModelCore.Landscape.Dimensions.Rows)
                ? kernelCellRadius : PlugIn.ModelCore.Landscape.Dimensions.Rows - tr;
            int maxColumnIndex = (tc + kernelCellRadius < PlugIn.ModelCore.Landscape.Dimensions.Columns)
               ? kernelCellRadius : PlugIn.ModelCore.Landscape.Dimensions.Columns - tc;

            // Loop over the kernel and calculate the cumulative sum
            // Since the values for SiteHostIndexMod, PInfected, and PDiseased were precalculated at the beginning of the
            // time step, we can now simply retrieve those values and perform simple (and very fast) multiplication
            for (int y = minRowIndex; y <= maxRowIndex; y++)
            {
                for (int x = minColumnIndex; x <= maxColumnIndex; x++)
                {
                    int fr = y + kernelCellRadius, fc = x + kernelCellRadius, sr = tr + y, sc = tc + x;
                    cumSum += kernel[fr, fc] * siteHostIndexMod[sr, sc] * targetHostIndexModValue * (pInfected[sr, sc] + pDiseased[sr, sc]);
                }
            }

            return cumSum;
        }

        // Pre-calculate grids containing relevant values for SiteHostIndexMod, PInfected, and PDiseased,
        // which are the three values needed to calculate the FOI
        // This allows us to later do extremely fast lookups for these values
        void PrepareGrids(int agentIndex)
        {
            int rows = PlugIn.ModelCore.Landscape.Dimensions.Rows + 1;
            int columns = PlugIn.ModelCore.Landscape.Dimensions.Columns + 1;

            PlugIn.ModelCore.UI.WriteLine("   Initializing grids of {0} x {1} cells", rows, columns);

            siteHostIndexMod = new double[rows, columns];
            pInfected = new double[rows, columns];
            pDiseased = new double[rows, columns];

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                double siteHostIndexValue = 0.0;
                double pInfectedValue = 0.0;
                double pDiseasedValue = 0.0;

                int row = site.Location.Row;
                int col = site.Location.Column;

                if (SiteVars.InfStatus[site][agentIndex] == 1 || SiteVars.InfStatus[site][agentIndex] == 2) {
                    siteHostIndexValue = SiteVars.SiteHostIndexMod[site];
                    pInfectedValue = SiteVars.PInfected[site][agentIndex];
                    pDiseasedValue = SiteVars.PDiseased[site][agentIndex];
                }

                siteHostIndexMod[row, col] = siteHostIndexValue;
                pInfected[row, col] = pInfectedValue;
                pDiseased[row, col] = pDiseasedValue;
            }
        }

        // Precalculate a "kernel" based on the agent DispersalMaxDist
        // This allows us to only perform relatively costly centroid distance calculations once, after
        // which we can quickly loop up the value during calculations in the KernelLoop function
        // See https://en.wikipedia.org/wiki/Kernel_(image_processing)
        double[,] CreateKernel(IAgent agent)
        {
            int maxRadius = agent.DispersalMaxDist;
            double CellLength = PlugIn.ModelCore.CellLength;
            kernelCellRadius = (int)(maxRadius / CellLength);

            int width = 2 * kernelCellRadius + 1;
            double[,] kernel = new double[width, width];

            PlugIn.ModelCore.UI.WriteLine("   Initializing new filter of {0}x{1} cells", width, width);

            for (int row = (kernelCellRadius * -1); row <= kernelCellRadius; row++)
            {
                for (int col = (kernelCellRadius * -1); col <= kernelCellRadius; col++)
                {
                    double kernelProb = 0.0;
                    if (!(row == 0 && col == 0))
                    {
                        double centroidDistance = System.Math.Sqrt(System.Math.Pow(row * CellLength, 2) + System.Math.Pow(col * CellLength, 2));
                        if (centroidDistance <= maxRadius)
                        {
                            kernelProb = Dispersal.dispersal_probability[centroidDistance];
                        }
                    }

                    kernel[row + kernelCellRadius, col + kernelCellRadius] = kernelProb;
                }
            }

            return kernel;
        }

        // Logs out a grid using the path of the file provided
        void LogGrid(string path, double[,] grid)
        {
            // Change 'false' to 'true' to append grids in the output rather than overriding on each call
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                int rowLength = grid.GetLength(0);
                int colLength = grid.GetLength(1);

                PlugIn.ModelCore.UI.WriteLine("   Logging grid {0} of size {1} x {2} ", path, rowLength, colLength);

                for (int row = 0; row < rowLength; row++)
                {
                    for (int col = 0; col < colLength; col++)
                    {
                        writer.Write(string.Format("{0:N6} ", grid[row, col]));
                    }
                    writer.Write(Environment.NewLine + Environment.NewLine);
                }
            }
        }

        //define function to calculate weather index for a given agent and site
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
                    int indexa = agent.VarFormula.Parameters.FindIndex(i => i == "a");
                    double a = Double.Parse(agent.VarFormula.Values[indexa]);
                    int indexb = agent.VarFormula.Parameters.FindIndex(i => i == "b");
                    double b = Double.Parse(agent.VarFormula.Values[indexb]);
                    int indexc = agent.VarFormula.Parameters.FindIndex(i => i == "c");
                    double c = Double.Parse(agent.VarFormula.Values[indexc]);
                    int indexd = agent.VarFormula.Parameters.FindIndex(i => i == "d");
                    double d = Double.Parse(agent.VarFormula.Values[indexd]);
                    int indexe = agent.VarFormula.Parameters.FindIndex(i => i == "e");
                    double e = Double.Parse(agent.VarFormula.Values[indexe]);
                    int indexf = agent.VarFormula.Parameters.FindIndex(i => i == "f");
                    double f = Double.Parse(agent.VarFormula.Values[indexf]);
                    int indexVar = agent.VarFormula.Parameters.FindIndex(i => i == "Variable");
                    string variableName = agent.VarFormula.Values[indexVar];

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

