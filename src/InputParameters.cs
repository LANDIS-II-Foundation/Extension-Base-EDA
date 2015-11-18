//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo

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
        /// Template for the filenames for output INFECTED maps.
        /// </summary>
        string InfMapNames {get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output DISEASED maps.
        /// </summary>
        string DisMapNames{get;set;}
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
        private string infMapNames;
        private string disMapNames;
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
        /// Template for the filenames for output INFECTED maps.
        /// </summary>
        public string InfMapNames
        {
            get {
                return infMapNames;
            }
            set {
                MapNames.CheckTemplateVars(value);
                infMapNames = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output DISEASED maps.
        /// </summary>
        public string DisMapNames
        {
            get
            {
                return disMapNames;
            }
            set
            {
                MapNames.CheckTemplateVars(value);
                disMapNames = value;
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
