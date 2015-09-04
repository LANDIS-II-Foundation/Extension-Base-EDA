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
        // Site Host Susceptibility to infection by DISEASE AGENT using 3 classes (Low, Medium, High)
        int LowHostAge { get; set; }
        double LowHostSusceptInfProb { get; set; }
        int MediumHostAge { get; set; }
        double MediumHostSusceptInfProb { get; set; }
        int HighHostAge { get; set; }
        double HighHostSusceptInfProb { get; set; }

        // Site Host Vulnerability (SHV) to DISEASE (and mortality) using 3 classes (Resistant, Tolerant, Vulnerable)
        int ResistantHostAge { get; set; }
        double ResistantHostMortProb { get; set; }
        int TolerantHostAge { get; set; }
        double TolerantHostMortProb { get; set; }
        int VulnerableHostAge { get; set; }
        double VulnerableHostMortProb { get; set; }

        //Conifer Flag for dead fuels
        bool CFSConifer{ get; set; }
    }
}


namespace Landis.Extension.BaseEDA
{
    public class SppParameters
        : ISppParameters
    {

        // Site Host Susceptibility to infection by DISEASE AGENT using 3 classes (Low, Medium, High)
        private int lowHostAge;
        private double lowHostSusceptInfProb;
        private int mediumHostAge;
        private double mediumHostSusceptInfProb;
        private int highHostAge;
        private double highHostSusceptInfProb;

        // Site Host Vulnerability (SHV) to DISEASE (and mortality) using 3 classes (Resistant, Tolerant, Vulnerable)
        private int resistantHostAge;
        private double resistantHostMortProb;
        private int tolerantHostAge;
        private double tolerantHostMortProb;
        private int vulnerableHostAge;
        private double vulnerableHostMortProb;

        private bool cfsConifer;

        //---------------------------------------------------------------------

        /// <summary>
        /// AGE: age of hosts with low susceptibility
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
        /// INFECTION PROBABILITY: infection probability for hosts with low susceptibility 
        /// </summary>
        public double LowHostSusceptInfProb
        {
            get
            {
                return lowHostSusceptInfProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                lowHostSusceptInfProb = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with medium susceptibility
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
        /// INFECTION PROBABILITY: infection probability for hosts with medium susceptibility 
        /// </summary>
        public double MediumHostSusceptInfProb
        {
            get
            {
                return mediumHostSusceptInfProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                mediumHostSusceptInfProb = value;
            }
        }
        /// <summary>
        /// AGE: age of hosts with high susceptibility
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
        /// INFECTION PROBABILITY: infection probability for hosts with high susceptibility 
        /// </summary>
        public double HighHostSusceptInfProb
        {
            get
            {
                return highHostSusceptInfProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                highHostSusceptInfProb = value;
            }
        }

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
        /// MORTALITY: mortality probability for resistant hosts
        /// </summary>
        public double ResistantHostMortProb
        {
            get
            {
                return resistantHostMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                resistantHostMortProb = value;
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
        /// MORTALITY: mortality probability for tolerant hosts
        /// </summary>
        public double TolerantHostMortProb
        {
            get
            {
                return tolerantHostMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                tolerantHostMortProb = value;
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
        /// MORTALITY: mortality probability for vulnerable hosts
        /// </summary>
        public double VulnerableHostMortProb
        {
            get
            {
                return vulnerableHostMortProb;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                vulnerableHostMortProb = value;
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
            //susceptibility to infection by disease agent
            this.lowHostAge = 999;
            this.lowHostSusceptInfProb = 0;
            this.mediumHostAge = 999;
            this.mediumHostSusceptInfProb = 0;
            this.highHostAge = 999;
            this.highHostSusceptInfProb = 0;
            //vulnerability to disease (and mortality)
            this.resistantHostAge = 999;
            this.resistantHostMortProb = 0;
            this.tolerantHostAge = 999;
            this.tolerantHostMortProb = 0;
            this.vulnerableHostAge = 999;
            this.vulnerableHostMortProb = 0;
            //conifer flag
            this.cfsConifer = false;
        }
 
    }
}
