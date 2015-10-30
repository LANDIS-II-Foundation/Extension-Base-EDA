//  Copyright 2005-2014 Portland State University, University of Wisconsin, US Forest Service
//  Authors:  Robert M. Scheller, Brian Miranda
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.

using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using Landis.Library.Metadata;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.BaseEDA
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata Extension {get; set;}

        public static void InitializeMetadata(int Timestep, 
            string intensityMapFileName,   //should this name be MapFileName instead or does not matter?
            //string srdMapFileName, 
            //string nrdMapFileName,
            string mortalityMapFileName,   //should this name be epiMapFileName instead or does not matter?
            string logFileName, 
            IEnumerable<IAgent> manyAgentParameters, 
            ICore mCore)
        {
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata() {
                //String outputFolder = OutputPath.ReplaceTemplateVars("", FINISH ME LATER);
                //FolderName = System.IO.Directory.GetCurrentDirectory().Split("\\".ToCharArray()).Last(),
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
                //ProjectionFilePath = "Projection.?" //How do we get projections???
            };

            Extension = new ExtensionMetadata(mCore){
                Name = PlugIn.ExtensionName,
                TimeInterval = Timestep, //change this to PlugIn.TimeStep for other extensions
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            //PlugIn.EventLog = new MetadataTable<EventsLog>(logFileName);
            PlugIn.EventLog = new MetadataTable<EventsLog>("eda-log.csv");

            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "EventLog",
                FilePath = PlugIn.EventLog.FilePath,
                Visualize = false,
            };
            tblOut_events.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_events);

            //PlugIn.PDSILog = new MetadataTable<PDSI_Log>("PDSI_log.csv");

            //OutputMetadata tblOut_PDSI = new OutputMetadata()
            //{
            //    Type = OutputType.Table,
            //    Name = "PDSILog",
            //    FilePath = PlugIn.PDSILog.FilePath
            //};
            //tblOut_events.RetriveFields(typeof(PDSI_Log));
            //Extension.OutputMetadatas.Add(tblOut_PDSI);


            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            foreach (IAgent activeAgent in manyAgentParameters)
            {
                string mapTypePath = MapNames.ReplaceTemplateVarsMetadata(intensityMapFileName, activeAgent.AgentName);

                OutputMetadata mapOut_Intensity = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = string.Format(activeAgent.AgentName + " Outbreak Infection Intensity"),
                    FilePath = @mapTypePath,
                    Map_DataType = MapDataType.Ordinal,
                    Map_Unit = FieldUnits.Severity_Rank, //can one change this name to something else?
                    Visualize = true,
                };
                Extension.OutputMetadatas.Add(mapOut_Intensity);

                /*if (srdMapFileName != null)
                {
                    mapTypePath = MapNames.ReplaceTemplateVarsMetadata(srdMapFileName, activeAgent.AgentName);
                    OutputMetadata mapOut_SRD = new OutputMetadata()
                    {
                        Type = OutputType.Map,
                        Name = "Site Resource Dominance",
                        FilePath = @mapTypePath,
                        Map_DataType = MapDataType.Continuous,
                        Map_Unit = FieldUnits.Percentage,
                        Visualize = false,
                    };
                    Extension.OutputMetadatas.Add(mapOut_SRD);
                }

                if (nrdMapFileName != null)
                {
                    mapTypePath = MapNames.ReplaceTemplateVarsMetadata(nrdMapFileName, activeAgent.AgentName);
                    OutputMetadata mapOut_NRD = new OutputMetadata()
                    {
                        Type = OutputType.Map,
                        Name = "Neighborhood Resource Dominance",
                        FilePath = @mapTypePath,
                        Map_DataType = MapDataType.Continuous,
                        Map_Unit = FieldUnits.Percentage,
                        Visualize = false,
                    };
                    Extension.OutputMetadatas.Add(mapOut_NRD);
                }*/
            }
            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
    }
}
