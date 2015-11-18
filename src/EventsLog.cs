using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;


namespace Landis.Extension.BaseEDA
{
    public class EventsLog
    {
      
        //Specify attributes to be written in the log file?
        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time {set; get;}

        [DataFieldAttribute(Desc = "Agent Name")]
        public string AgentName { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Susceptible Sites in Event")]
        public int SusceptibleSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Infected Sites in Event")]
        public int InfectedSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Diseased Sites in Event")]
        public int DiseasedSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cohorts Killed for Selected Species of Interest")]
        public int CohortsKilled { set; get; }

        //[DataFieldAttribute(Desc = "Mean Severity (1-5)", Format="0.00")]
        //public double MeanSeverity { set; get; }

    }
}
