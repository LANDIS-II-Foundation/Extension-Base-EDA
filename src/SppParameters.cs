//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// Extra Spp Paramaters
    /// </summary>
    public interface ISppParameters
    {
        /// <summary>
        /// </summary>
        // Site Host Index to infection by DISEASE AGENT using 3 classes (Low, Medium, High)
        int LowHostAge { get; set; }
        double LowHostScore { get; set; }
        int MediumHostAge { get; set; }
        double MediumHostScore { get; set; }
        int HighHostAge { get; set; }
        double HighHostScore { get; set; }

        // Site Host Vulnerability (SHV) to DISEASE (and mortality) using 3 classes (Low Vulnerability, Medium Vulnerability, High Vulnerability)
        int LowVulnHostAge { get; set; }
        double LowVulnHostMortProb { get; set; }
        int MediumVulnHostAge { get; set; }
        double MediumVulnHostMortProb { get; set; }
        int HighVulnHostAge { get; set; }
        double HighVulnHostMortProb { get; set; }

        //Conifer Flag for dead fuels (specifically designed for Canadian fire model)
        bool CFSConifer{ get; set; }
    }
}


namespace Landis.Extension.BaseEDA
{
    public class SppParameters
        : ISppParameters
    {

        // Site Host Index:  susceptibility to become infected and suitability to produce infectious spores of the
        //                   pathogen using 3 classes (Low, Medium, High)
        private int lowHostAge;
        private double lowHostScore;
        private int mediumHostAge;
        private double mediumHostScore;
        private int highHostAge;
        private double highHostScore;

        // Site Host Vulnerability (SHV) to DISEASE (and mortality) using 3 classes (Low Vulnerability, Medium Vulnerability, High Vulnerability)
        private int lowVulnHostAge;
        private double lowVulnHostMortProb;
        private int mediumVulnHostAge;
        private double mediumVulnHostMortProb;
        private int highVulnHostAge;
        private double highVulnHostMortProb;

        private bool cfsConifer;

        //---------------------------------------------------------------------

        /// <summary>
        /// AGE: age of hosts with LOW susceptibility/suitability to produce infectious spores of the pathogen
        /// </summary>
        public int LowHostAge
        {
            get
            {
                return lowHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                lowHostAge = value;
            }
        }
        /// <summary>
        /// HOST SCORE: score for hosts with LOW susceptibility/suitability to produce infectious spores of the pathogen
        /// </summary>
        public double LowHostScore
        {
            get
            {
                return lowHostScore;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                lowHostScore = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with MEDIUM susceptibility/suitability to produce infectious spores of the pathogen
        /// </summary>
        public int MediumHostAge
        {
            get
            {
                return mediumHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                mediumHostAge = value;
            }
        }
        /// <summary>
        /// HOST SCORE: score for hosts with MEDIUM susceptibility/suitability to produce infectious spores of the pathogen
        /// </summary>
        public double MediumHostScore
        {
            get
            {
                return mediumHostScore;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                mediumHostScore = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with HIGH susceptibility/suitability to produce infectious spores of the pathogen
        /// </summary>
        public int HighHostAge
        {
            get
            {
                return highHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                highHostAge = value;
            }
        }
        /// <summary>
        /// HOST SCORE: score for hosts with HIGH susceptibility/suitability to produce infectious spores of the pathogen
        /// </summary>
        public double HighHostScore
        {
            get
            {
                return highHostScore;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                highHostScore = value;
            }
        }

        /// <summary>
        /// AGE: age of hosts with LOW vulnerability to disease caused by epidem agent
        /// </summary>
        public int LowVulnHostAge
        {
            get
            {
                return lowVulnHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                lowVulnHostAge = value;
            }
        }
        /// <summary>
        /// MORTALITY: mortality probability for hosts with LOW vulnerability to disease caused by epidem agent
        /// </summary>
        public double LowVulnHostMortProb
        {
            get
            {
                return lowVulnHostMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                lowVulnHostMortProb = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with MEDIUM vulnerability to disease caused by epidem agent
        /// </summary>
        public int MediumVulnHostAge
        {
            get
            {
                return mediumVulnHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                mediumVulnHostAge = value;
            }
        }
        /// <summary>
        /// MORTALITY: mortality probability for hosts with MEDIUM vulnerability to disease caused by epidem agent
        /// </summary>
        public double MediumVulnHostMortProb
        {
            get
            {
                return mediumVulnHostMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                mediumVulnHostMortProb = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with HIGH vulnerability to disease caused by epidem agent
        /// </summary>
        public int HighVulnHostAge
        {
            get
            {
                return highVulnHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                highVulnHostAge = value;
            }
        }
        /// <summary>
        /// MORTALITY: mortality probability for hosts with HIGH vulnerability to disease caused by epidem agent
        /// </summary>
        public double HighVulnHostMortProb
        {
            get
            {
                return highVulnHostMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                highVulnHostMortProb = value;
            }
        }

        /// <summary>
        /// CONIFER FLAG: Host species can be flagged as contributing to a specialty dead fuel class, 
        /// which allows the dead cohorts of these species to be considered by fuel extensions 
        /// that account for disturbance-related fuels 
        /// </summary>
        public bool CFSConifer
        {
            get {
                return cfsConifer;
            }

            set {
                cfsConifer = value;
            }
        }

        //---------------------------------------------------------------------
        public SppParameters()
        {
            //susceptibility to infection by disease agent/suitability to produce spores once infected
            this.lowHostAge = 999;
            this.lowHostScore = 0;
            this.mediumHostAge = 999;
            this.mediumHostScore = 0;
            this.highHostAge = 999;
            this.highHostScore = 0;
            //vulnerability to disease (and mortality)
            this.lowVulnHostAge = 999;
            this.lowVulnHostMortProb = 0;
            this.mediumVulnHostAge = 999;
            this.mediumVulnHostMortProb = 0;
            this.highVulnHostAge = 999;
            this.highVulnHostMortProb = 0;
            //conifer flag
            this.cfsConifer = false;
        }
 
    }
}
