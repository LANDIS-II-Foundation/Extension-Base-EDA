//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo

using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BaseEDA
{

    /// <summary>
    /// A parser that reads the extension parameters from text input.
    /// </summary>
    public class AgentParameterParser
        : TextParser<IAgent>
    {

        public static IEcoregionDataset EcoregionsDataset = PlugIn.ModelCore.Ecoregions;
        public static ISpeciesDataset SpeciesDataset = PlugIn.ModelCore.Species; //null;

        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get { return "EDA Agent"; }
        }

        //---------------------------------------------------------------------
        public AgentParameterParser()
        {
            RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        protected override IAgent Parse()
        {

            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != LandisDataValue)
                throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", LandisDataValue);

            Agent agentParameters = new Agent(PlugIn.ModelCore.Species.Count, PlugIn.ModelCore.Ecoregions.Count);

            InputVar<string> agentName = new InputVar<string>("EDAAgentName");
            ReadVar(agentName);
            agentParameters.AgentName = agentName.Value;

            InputVar<SHImode> shi = new InputVar<SHImode>("SHIMode");
            ReadVar(shi);
            agentParameters.SHImode = shi.Value;

            InputVar<int> startYear = new InputVar<int>("StartYear");
            if (CurrentName == "StartYear")
            {
                ReadVar(startYear);
                agentParameters.StartYear = startYear.Value;
            }
            else
                agentParameters.StartYear = 0;

            InputVar<int> endYear = new InputVar<int>("EndYear");
            if (CurrentName == "EndYear")
            {
                ReadVar(endYear);
                agentParameters.EndYear = endYear.Value;
            }
            else
                agentParameters.EndYear = PlugIn.ModelCore.EndTime;

            // - Climate Input - 
            //ADD HERE

            // - Transmission Input -

            InputVar<double> tr = new InputVar<double>("TransmissionRate");
            ReadVar(tr);
            agentParameters.TransmissionRate = tr.Value;

            InputVar<double> ar = new InputVar<double>("AcquisitionRate");
            ReadVar(ar);
            agentParameters.AcquisitionRate = ar.Value;

            // InitialEpidemMap ??

            InputVar<DispersalType> dt = new InputVar<DispersalType>("DispersalType");
            ReadVar(dt);
            agentParameters.DispersalType = dt.Value;

            InputVar<DispersalTemplate> dk = new InputVar<DispersalTemplate>("DispersalKernel");
            ReadVar(dk);
            agentParameters.DispersalKernel = dk.Value;

            InputVar<double> ac = new InputVar<double>("AlphaCoef");
            ReadVar(ac);
            agentParameters.AlphaCoef = ac.Value;

            //--------- Read In Ecoregion Table ---------------------------------------
            PlugIn.ModelCore.UI.WriteLine("Begin parsing ECOREGION table.");

            InputVar<string> ecoName = new InputVar<string>("Ecoregion Name");
            InputVar<double> ecoModifier = new InputVar<double>("Ecoregion Modifier");

            Dictionary<string, int> lineNumbers = new Dictionary<string, int>();
            const string DistParms = "DisturbanceModifiers";
            const string SppParms = "EDASpeciesParameters";

            while (!AtEndOfInput && CurrentName != DistParms && CurrentName != SppParms)
            {

                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoName, currentLine);
                IEcoregion ecoregion = EcoregionsDataset[ecoName.Value.Actual];
                if (ecoregion == null)
                    throw new InputValueException(ecoName.Value.String,
                                                  "{0} is not an ecoregion name.",
                                                  ecoName.Value.String);
                int lineNumber;
                if (lineNumbers.TryGetValue(ecoregion.Name, out lineNumber))
                    throw new InputValueException(ecoName.Value.String,
                                                  "The ecoregion {0} was previously used on line {1}",
                                                  ecoName.Value.String, lineNumber);
                else
                    lineNumbers[ecoregion.Name] = LineNumber;

                IEcoParameters ecoParms = new EcoParameters();
                agentParameters.EcoParameters[ecoregion.Index] = ecoParms;

                ReadValue(ecoModifier, currentLine);
                ecoParms.EcoModifier = ecoModifier.Value;

                CheckNoDataAfter("the " + ecoModifier.Name + " column",
                                 currentLine);
                GetNextLine();
            }

            if (CurrentName == DistParms)
            {
                //--------- Read In Disturbance Modifier Table -------------------------------
                PlugIn.ModelCore.UI.WriteLine("Begin parsing DISTURBANCE table.");

                ReadName(DistParms);

                InputVar<string> prescriptionName = new InputVar<string>("Disturbance Type");
                InputVar<int> duration = new InputVar<int>("Duration");
                InputVar<double> distModifier = new InputVar<double>("Disturbance Modifier");

                lineNumbers = new Dictionary<string, int>();
                Dictionary<int, int> DisturbanceTypeLineNumbers = new Dictionary<int, int>();


                while (!AtEndOfInput && CurrentName != SppParms)
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    ReadValue(distModifier, currentLine);

                    IDisturbanceType currentDisturbanceType = new DisturbanceType();
                    agentParameters.DisturbanceTypes.Add(currentDisturbanceType);

                    currentDisturbanceType.SHIModifier = distModifier.Value;

                    ReadValue(duration, currentLine);
                    currentDisturbanceType.ImpactDuration = duration.Value;

                    List<string> prescriptionNames = new List<string>();
                    TextReader.SkipWhitespace(currentLine);
                    while (currentLine.Peek() != -1)
                    {
                        ReadValue(prescriptionName, currentLine);
                        prescriptionNames.Add(prescriptionName.Value);

                        TextReader.SkipWhitespace(currentLine);
                    }
                    if (prescriptionNames.Count == 0)
                        throw NewParseException("At least one disturbance type is required.");

                    currentDisturbanceType.PrescriptionNames = prescriptionNames;

                    CheckNoDataAfter("the " + distModifier.Name + " column",
                                     currentLine);
                    GetNextLine();
                }
            }

            //--------- Read In Species Table ---------------------------------------
            PlugIn.ModelCore.UI.WriteLine("Begin parsing SPECIES table.");

            ReadName(SppParms);

            //Species Name
            InputVar<string> sppName = new InputVar<string>("Species");

            //SHI (Host Index)
            InputVar<int> lowHostAge = new InputVar<int>("Low Host Index Age");
            InputVar<int> lowHostScore = new InputVar<int>("Low Host Index Score");
            InputVar<int> mediumHostAge = new InputVar<int>("Medium Host Index Age");
            InputVar<int> mediumHostScore = new InputVar<int>("Medium Host Index Score");
            InputVar<int> highHostAge = new InputVar<int>("High Host Index Age");
            InputVar<int> highHostScore = new InputVar<int>("High Host Index Score");

            //SHV (Vulnerability)
            InputVar<int> lowVulnHostAge = new InputVar<int>("Low Vulnerability Host Age");
            InputVar<double> lowVulnHostMortProb = new InputVar<double>("Low Vulnerability Host MortProb");
            InputVar<int> mediumVulnHostAge = new InputVar<int>("Medium Vulnerability Host Age");
            InputVar<double> mediumVulnHostMortProb = new InputVar<double>("Medium Vulnerability Host MortProb");
            InputVar<int> highVulnHostAge = new InputVar<int>("High Vulnerability Host Age");
            InputVar<double> highVulnHostMortProb = new InputVar<double>("High Vulnerability Host MortProb");

            //CFS
            InputVar<bool> cfsConifer = new InputVar<bool>("CFS Conifer type:  yes/no");

            //MORTALITY SPECIES TO CONSIDER FOR PLOTTING
            InputVar<bool> mortSppFlag = new InputVar<bool>("Mortality plot:  yes/no");

            // species to ignore in SHI calculation
            const string NegSpp = "IgnoredSpecies";

            while ((!AtEndOfInput) && (CurrentName != NegSpp))
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(sppName, currentLine);
                ISpecies species = SpeciesDataset[sppName.Value.Actual];
                if (species == null)
                    throw new InputValueException(sppName.Value.String,
                                                  "{0} is not a species name.",
                                                  sppName.Value.String);
                int lineNumber;
                if (lineNumbers.TryGetValue(species.Name, out lineNumber))
                    throw new InputValueException(sppName.Value.String,
                                                  "The species {0} was previously used on line {1}",
                                                  sppName.Value.String, lineNumber);
                else
                    lineNumbers[species.Name] = LineNumber;

                ISppParameters sppParms = new SppParameters();
                agentParameters.SppParameters[species.Index] = sppParms;

                //SHI
                ReadValue(lowHostAge, currentLine);
                sppParms.LowHostAge = lowHostAge.Value;

                ReadValue(lowHostScore, currentLine);
                sppParms.LowHostScore = lowHostScore.Value;

                ReadValue(mediumHostAge, currentLine);
                sppParms.MediumHostAge = mediumHostAge.Value;

                ReadValue(mediumHostScore, currentLine);
                sppParms.MediumHostScore = mediumHostScore.Value;

                ReadValue(highHostAge, currentLine);
                sppParms.HighHostAge = highHostAge.Value;

                ReadValue(highHostScore, currentLine);
                sppParms.HighHostScore = highHostScore.Value;

                //SHV
                ReadValue(lowVulnHostAge, currentLine);
                sppParms.LowVulnHostAge = lowVulnHostAge.Value;

                ReadValue(lowVulnHostMortProb, currentLine);
                sppParms.LowVulnHostMortProb = lowVulnHostMortProb.Value;

                ReadValue(mediumVulnHostAge, currentLine);
                sppParms.MediumVulnHostAge = mediumVulnHostAge.Value;

                ReadValue(mediumVulnHostMortProb, currentLine);
                sppParms.MediumVulnHostMortProb = mediumVulnHostMortProb.Value;

                ReadValue(highVulnHostAge, currentLine);
                sppParms.HighVulnHostAge = highVulnHostAge.Value;

                ReadValue(highVulnHostMortProb, currentLine);
                sppParms.HighVulnHostMortProb = highVulnHostMortProb.Value;

                //CFS
                ReadValue(cfsConifer, currentLine);
                sppParms.CFSConifer = cfsConifer.Value;

                //MORTALITY SPECIES FLAG
                ReadValue(mortSppFlag, currentLine);
                sppParms.MortSppFlag = mortSppFlag.Value;

                CheckNoDataAfter("the " + mortSppFlag.Name + " column",
                                 currentLine);
                
                GetNextLine();
            }

            //--------- Read In Ignored Species List ---------------------------------------
            List<ISpecies> negSppList = new List<ISpecies>();
            if (!AtEndOfInput)
            {
                ReadName(NegSpp);
                InputVar<string> negSppName = new InputVar<string>("Ignored Spp Name");

                while (!AtEndOfInput)
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    ReadValue(negSppName, currentLine);
                    ISpecies species = SpeciesDataset[negSppName.Value.Actual];
                    if (species == null)
                        throw new InputValueException(negSppName.Value.String,
                                                      "{0} is not a species name.",
                                                      negSppName.Value.String);
                    int lineNumber;
                    if (lineNumbers.TryGetValue(species.Name, out lineNumber))
                        PlugIn.ModelCore.UI.WriteLine("WARNING: The species {0} was previously used on line {1}.  Being listed in the IgnoredSpecies list will override any settings in the Host table.", negSppName.Value.String, lineNumber);
                    else
                        lineNumbers[species.Name] = LineNumber;

                    negSppList.Add(species);

                    GetNextLine();

                }
            }
            agentParameters.NegSppList = negSppList;

        }                     

        public static SHImode SHIParse(string word)
        {
            if (word == "max")
                return SHImode.max;
            else if (word == "mean")
                return SHImode.mean;
            throw new System.FormatException("Valid algorithms: max, mean");
        }

        public static DispersalType DispTypeParse(string word)
        {
            if (word == "STATIC")
                return DispersalType.STATIC;
            else if (word == "DYNAMIC")
                return DispersalType.DYNAMIC;
            throw new System.FormatException("Valid algorithms: STATIC, DYNAMIC");
        }

        public static DispersalTemplate DispTParse(string word)
        {
            if (word == "PowerLaw")
                return DispersalTemplate.PowerLaw;
            else if (word == "NegExp")
                return DispersalTemplate.NegExp;
            throw new System.FormatException("Valid algorithms: PowerLaw, NegExp");
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Registers the appropriate method for reading input values.
        /// </summary>
        public static void RegisterForInputValues()
        {
            Type.SetDescription<SHImode>("Site Host Index Mode");
            InputValues.Register<SHImode>(SHIParse);

            Type.SetDescription<DispersalType>("Dispersal Type");
            InputValues.Register<DispersalType>(DispTypeParse);

            Type.SetDescription<DispersalTemplate>("Dispersal Template");
            InputValues.Register<DispersalTemplate>(DispTParse);

        }
    }
}
