//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// Parameters for the extension.
    /// </summary>
    public interface IInputParameters
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int Timestep {get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output maps with infection status (0=Susceptible;1=Infected;2=Diseased).
        /// </summary>
        string StatusMapNames {get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for mortality output maps (number of cohorts killed for each species of interest).
        /// </summary>
        string MortMapNames { get; set; }
        //---------------------------------------------------------------------
        /// <summary>
        /// Name of log file.
        /// </summary>
        string LogFileName {get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// List of Agent Files
        /// </summary>
        IEnumerable<IAgent> ManyAgentParameters{get;set;}
    }
}

namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private int timestep;
        private string statusMapNames;
        private string mortMapNames;
        private string logFileName;
        private IEnumerable<IAgent> manyAgentParameters;

        //---------------------------------------------------------------------
        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output maps with infection status (0=Susceptible;1=Infected;2=Diseased).
        /// </summary>
        public string StatusMapNames
        {
            get {
                return statusMapNames;
            }
            set {
                MapNames.CheckTemplateVars(value);
                statusMapNames = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for mortality output maps (number of cohorts killed for each species of interest).
        /// </summary>
        public string MortMapNames
        {
            get
            {
                return mortMapNames;
            }
            set
            {
                MapNames.CheckTemplateVars(value);
                mortMapNames = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Name of log file.
        /// </summary>
        public string LogFileName
        {
            get {
                return logFileName;
            }
            set {
                    // FIXME: check for null or empty path (value.Actual);
                logFileName = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// List of Agent Files
        /// </summary>
        public IEnumerable<IAgent> ManyAgentParameters
        {
            get {
                return manyAgentParameters;
            }
            set {
                manyAgentParameters = value;
            }
        }

        //---------------------------------------------------------------------
        public InputParameters()
        {
        }
       
    }
}
