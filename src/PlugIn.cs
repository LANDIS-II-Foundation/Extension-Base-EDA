//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.
//  Modified for budworm-BDA version by Brian Miranda, 2012

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Landis.Core;
using Landis.Library.Metadata;
using Landis.SpatialModeling;
using Landis.Library.Climate;
using System.Data;

namespace Landis.Extension.BaseEDA
{
    ///<summary>
    /// A disturbance plug-in that simulates Pathogen Dispersal and Disease.
    /// </summary>

    public class PlugIn
        : ExtensionMain
    {
        public static readonly ExtensionType type = new ExtensionType("disturbance:eda");
        public static readonly string ExtensionName = "Base EDA";
        public static MetadataTable<EventsLog> EventLog;

        private string statusMapName; 
        private string mortMapNames;

        private IEnumerable<IAgent> manyAgentParameters;
        private static IInputParameters parameters;
        private static ICore modelCore;
        private bool reinitialized;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, type)
        {
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile,
                                            ICore mCore)
        {
            modelCore = mCore;
            InputParameterParser.EcoregionsDataset = modelCore.Ecoregions;
            InputParameterParser parser = new InputParameterParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }

        //---------------------------------------------------------------------


        /// <summary>
        /// Initializes the extension with a data file.
        /// </summary>
        public override void Initialize()
        {
            reinitialized = false;

            //initialize metadata
            MetadataHandler.InitializeMetadata(parameters.Timestep,
               parameters.StatusMapNames,
               parameters.MortMapNames,
               parameters.LogFileName,
               parameters.ManyAgentParameters,
               ModelCore);

            //get input params map names
            Timestep = parameters.Timestep;
            statusMapName = parameters.StatusMapNames;
            mortMapNames = parameters.MortMapNames;

            //initialize site variables
            SiteVars.Initialize(modelCore);

            manyAgentParameters = parameters.ManyAgentParameters;
            foreach (IAgent activeAgent in manyAgentParameters)
            {
                if (activeAgent == null)
                    ModelCore.UI.WriteLine("Agent Parameters NOT loading correctly.");

                //DO ANYTHING TO THE AGENT INITIALIZATION PHASE?

            }

        }

        public new void InitializePhase2() 
        {
                SiteVars.InitializeTimeOfLastDisturbances();
                reinitialized = true;
        }

        //---------------------------------------------------------------------
        ///<summary>
        /// Run the EDA extension at a particular timestep.
        ///</summary>
        public override void Run()
        {
            ModelCore.UI.WriteLine("   Processing landscape for EDA events ...");
            if(!reinitialized)
                InitializePhase2();

            int eventCount = 0;

            foreach(IAgent activeAgent in manyAgentParameters)
            {

                Epidemic.Initialize(activeAgent);
                Epidemic currentEpic = Epidemic.Simulate(activeAgent, ModelCore.CurrentTime);
                
                if (currentEpic != null)
                {
                    LogEvent(ModelCore.CurrentTime, currentEpic, activeAgent);

                    //----- Write Infection Status maps (SUSCEPTIBLE (0), INFECTED (cryptic-non symptomatic) (1), DISEASED (symptomatic) (2) --------
                    string path = MapNames.ReplaceTemplateVars(statusMapName, activeAgent.AgentName, ModelCore.CurrentTime);
                    using (IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(path, modelCore.Landscape.Dimensions))
                    {
                        BytePixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                            {
                                pixel.MapCode.Value = (byte) (SiteVars.InfStatus[site] + 1);
                            }
                            else
                            {
                                //Inactive site
                                pixel.MapCode.Value = 0;
                            }
                            outputRaster.WriteBufferPixel();
                        }
                    }

                    if (!(mortMapNames == null))
                    {

                        //----- Write Cohort Mortality Maps (number dead cohorts for selected species) --------
                        string path2 = MapNames.ReplaceTemplateVars(mortMapNames, activeAgent.AgentName, ModelCore.CurrentTime);
                        using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path2, modelCore.Landscape.Dimensions))
                        {
                            ShortPixel pixel = outputRaster.BufferPixel;
                            foreach (Site site in ModelCore.Landscape.AllSites)
                            {
                                if (site.IsActive)
                                {
                                    pixel.MapCode.Value = (short)(SiteVars.NumberMortSppKilled[site].Sum(x => x.Value));  //How to distinguish from inactive = 0?
                                }
                                else
                                {
                                    //Inactive site
                                    pixel.MapCode.Value = 0;  //see comment above...can we make this NA(null) or how to account for 0 mortality?
                                }
                                outputRaster.WriteBufferPixel();
                            }
                        }
                    }
                    
                    eventCount++;
                }
            }
        }

        private void LogEvent(int currentTime,
                             Epidemic CurrentEvent,
                             IAgent agent)
        {
            EventLog.Clear();
            EventsLog el = new EventsLog();

            el.Time = currentTime;
            el.AgentName = agent.AgentName;
            el.InfectedSites = CurrentEvent.TotalSitesInfected;  //total number of infected sites
            el.DiseasedSites = CurrentEvent.TotalSitesDiseased;  //total number of diseased sites
            el.DamagedSites = CurrentEvent.TotalSitesDamaged;    //total number of damaged (i.e. with mortality) sites
            el.TotalCohortsKilled = CurrentEvent.TotalCohortsKilled; //total number of cohorts killed (all species)
            el.CohortsMortSppKilled = CurrentEvent.MortSppCohortsKilled; //total number of cohorts killed (species of interest)

            EventLog.AddObject(el);
            EventLog.WriteToFile();
        }

    }
}
