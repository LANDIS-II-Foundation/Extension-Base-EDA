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
        // Site Host Vulnerability (SHV) to DISEASE AGENT using 3 classes (Resistant, Tolerant, Vulnerable)
        int ResistantHostAge { get; set; }
        double ResistantHostVulnThrsh { get; set; }
        int TolerantHostAge { get; set; }
        double TolerantHostVulnThrsh { get; set; }
        int VulnerableHostAge { get; set; }
        double VulnerableHostVulnThrsh { get; set; }

        // Site Host Susceptibility to DISEASE using 3 classes (Low, Medium, High)
        int LowHostAge { get; set; }
        double LowHostSusceptMortProb { get; set; }
        int MediumHostAge { get; set; }
        double MediumHostSusceptMortProb { get; set; }
        int HighHostAge { get; set; }
        double HighHostSusceptMortProb { get; set; }

        //Conifer Flag for dead fuels
        bool CFSConifer{ get; set; }
    }
}


namespace Landis.Extension.BaseEDA
{
    public class SppParameters
        : ISppParameters
    {
        // Site Host Vulnerability (SHV) to DISEASE AGENT using 3 classes (Resistant, Tolerant, Vulnerable)
        private int resistantHostAge;
        private double resistantHostVulnThrsh;
        private int tolerantHostAge;
        private double tolerantHostVulnThrsh;
        private int vulnerableHostAge;
        private double vulnerableHostVulnThrsh;

        // Site Host Susceptibility to DISEASE using 3 classes (Low, Medium, High)
        private int lowHostAge;
        private double lowHostSusceptMortProb;
        private int mediumHostAge;
        private double mediumHostSusceptMortProb;
        private int highHostAge;
        private double highHostSusceptMortProb;

        private bool cfsConifer;

        //---------------------------------------------------------------------


        /// <summary>
        /// AGE: age of resistant hosts
        /// </summary>
        public int ResistantHostAge
        {
            get
            {
                return resistantHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                resistantHostAge = value;
            }
        }
        /// <summary>
        /// THRESHOLD: threshold vulnerability value of resistant hosts
        /// </summary>
        public double ResistantHostVulnThrsh
        {
            get
            {
                return resistantHostVulnThrsh;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                resistantHostVulnThrsh = value;
            }
        }
        /// <summary>
        /// AGE: age of tolerant hosts
        /// </summary>
        public int TolerantHostAge
        {
            get
            {
                return tolerantHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                tolerantHostAge = value;
            }
        }
        /// <summary>
        /// THRESHOLD: threshold vulnerability value of tolerant hosts
        /// </summary>
        public double TolerantHostVuln
        {
            get
            {
                return tolerantHostVuln;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                tolerantHostVuln = value;
            }
        }
        /// <summary>
        /// AGE: age of vulnerable hosts
        /// </summary>
        public int VulnerableHostAge
        {
            get
            {
                return vulnerableHostAge;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 999)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 999.");
                vulnerableHostAge = value;
            }
        }
        /// <summary>
        /// THRESHOLD: threshold vulnerability value of vulnerable hosts
        /// </summary>
        public double VulnerableHostVuln
        {
            get
            {
                return vulnerableHostVuln;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                vulnerableHostVuln = value;
            }
        }

        /// <summary>
        /// AGE: age of hosts with low susceptibility
        /// </summary>
        public int LowHostAge
        {
            get {
                return lowHostAge;
            }
            set {
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
        /// MORTALITY PROBABILITY: mortality probability of hosts with low susceptibility 
        /// </summary>
        public double LowHostSusceptMortProb
        {
            get
            {
                return lowHostSusceptMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                lowHostSusceptMortProb = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with medium susceptibility
        /// </summary>
        public int MediumHostAge
        {
            get {
                return mediumHostAge;
            }
            set {
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
        /// MORTALITY PROBABILITY: mortality probability of hosts with medium susceptibility 
        /// </summary>
        public double MediumHostSusceptMortProb
        {
            get
            {
                return mediumHostSusceptMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                mediumHostSusceptMortProb = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with high susceptibility
        /// </summary>
        public int HighHostAge
        {
            get {
                return highHostAge;
            }
            set {
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
        /// MORTALITY PROBABILITY: mortality probability of hosts with high susceptibility 
        /// </summary>
        public double HighHostSusceptMortProb
        {
            get
            {
                return highHostSusceptMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                highHostSusceptMortProb = value;
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
            //vulnerability to disease agent
            this.resistantHostAge = 999;
            this.resistantHostVulnThrsh = 0;
            this.tolerantHostAge = 999;
            this.tolerantHostVulnThrsh = 0;
            this.vulnerableHostAge = 999;
            this.vulnerableHostVulnThrsh = 0;
            //susceptibility to disease
            this.lowHostAge = 999;
            this.lowHostSusceptMortProb = 0;
            this.mediumHostAge = 999;
            this.mediumHostSusceptMortProb = 0;
            this.highHostAge = 999;
            this.highHostSusceptMortProb = 0;
            //conifer flag
            this.cfsConifer = false;
        }
 
    }
}
