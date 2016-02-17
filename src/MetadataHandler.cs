//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda


using Landis.Core;
using Landis.Library.Metadata;
using System.Collections.Generic;
using System.IO;

namespace Landis.Extension.BaseEDA
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata Extension {get; set;}

        public static void InitializeMetadata(int Timestep, 
            string statusMapFileName,
            string mortalityMapFileName,   
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
            if (logFileName != null)
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
                PlugIn.EventLog = new MetadataTable<EventsLog>(logFileName);
                //PlugIn.EventLog = new MetadataTable<EventsLog>("eda-log.csv");

                OutputMetadata tblOut_events = new OutputMetadata()
                {
                    Type = OutputType.Table,
                    Name = "EventLog",
                    FilePath = PlugIn.EventLog.FilePath,
                    Visualize = false,
                };
                tblOut_events.RetriveFields(typeof(EventsLog));
                Extension.OutputMetadatas.Add(tblOut_events);
            }

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            foreach (IAgent activeAgent in manyAgentParameters)
            {
                string mapTypePath = MapNames.ReplaceTemplateVarsMetadata(statusMapFileName, activeAgent.AgentName);

                OutputMetadata mapOut_Status = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = string.Format(activeAgent.AgentName + " Cell Infection Status "),
                    FilePath = @mapTypePath,
                    Map_DataType = MapDataType.Ordinal,
                    Map_Unit = FieldUnits.Severity_Rank, //based on the metadata library (https://github.com/LANDIS-II-Foundation/Libraries/blob/master/metadata/trunk/src/FieldUnits.cs)
                    Visualize = true,                    //it seems like Severity_Rank can have values between 1-5 
                };
                Extension.OutputMetadatas.Add(mapOut_Status);

                if (mortalityMapFileName != null)
                {
                    mapTypePath = MapNames.ReplaceTemplateVarsMetadata(mortalityMapFileName, activeAgent.AgentName);
                    OutputMetadata mapOut_MORT = new OutputMetadata()
                    {
                        Type = OutputType.Map,
                        Name = "Cohort Mortality (Flagged Species)",
                        FilePath = @mapTypePath,
                        Map_DataType = MapDataType.Continuous,
                        Map_Unit = FieldUnits.Count,
                        Visualize = false,
                    };
                    Extension.OutputMetadatas.Add(mapOut_MORT);
                }

            }
            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);

        }
    }
}
