//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using Landis.Library.Climate;
using System.Data;
using System;
using Landis.Core;
using System.Linq;
using Landis.SpatialModeling;

namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// The definition of a reclass map.
    /// </summary>
    public interface IClimateVariableDefinition
    {
        /// <summary>
        /// Var name
        /// </summary>
        string Name
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Climate Library Variable
        /// </summary>
        string ClimateLibVariable
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Source Name
        /// </summary>
        string SourceName
        {
            get;
            set;
        }
         //---------------------------------------------------------------------
        /// <summary>
        /// Climate Data
        /// </summary>
        AnnualClimate_Monthly ClimateData
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Transformation
        /// </summary>
        string Transform
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
    }

    /// <summary>
    /// The definition of a reclass map.
    /// </summary>
    public class ClimateVariableDefinition
        : IClimateVariableDefinition
    {
        private string name;
        private string climateLibVariable;
        private string sourceName;
        private AnnualClimate_Monthly climateData;
        private string transform;
        //---------------------------------------------------------------------

        /// <summary>
        /// Var name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Climate Library Variable
        /// </summary>
        public string ClimateLibVariable
        {
            get
            {
                return climateLibVariable;
            }
            set
            {
                climateLibVariable = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Source Name
        /// </summary>
        public string SourceName
        {
            get
            {
                return sourceName;
            }
            set
            {
                sourceName = value;
            }
        }
        //---------------------------------------------------------------------
 
        //---------------------------------------------------------------------
        /// <summary>
        /// Climate Data
        /// </summary>
        public AnnualClimate_Monthly ClimateData
        {
            get
            {
                return climateData;
            }
            set
            {
                climateData = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Transformation
        /// </summary>
        public string Transform
        {
            get
            {
                return transform;
            }
            set
            {
                transform = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public ClimateVariableDefinition()
        {
        }
        //---------------------------------------------------------------------
        
        public static DataTable ReadWeatherFile(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("   Loading Climate Data...");

            CSVParser weatherParser = new CSVParser();

            DataTable weatherTable = weatherParser.ParseToDataTable(path);

            return weatherTable;
        }
        //---------------------------------------------------------------------

        public static void CalculateClimateVariables(IAgent agent)
        {
            // Calculate Derived Climate Variables
            DerivedClimateVariable.CalculateDerivedClimateVariables(agent);

            // Calculate Climate Variables
            /*
            foreach (IClimateVariableDefinition climateVar in agent.ClimateVars)
            {
                Dictionary<IEcoregion, Dictionary<string, double>> ecoClimateVars = new Dictionary<IEcoregion, Dictionary<string, double>>();

                string varName = climateVar.Name;
                string climateLibVar = climateVar.ClimateLibVariable;
                string transform = climateVar.Transform;

                int currentYear = PlugIn.ModelCore.CurrentTime;
                int actualYear = currentYear;

                int firstActiveEco = 0;
                foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                {
                    if (ecoregion.Active)
                    {
                        firstActiveEco = ecoregion.Index;
                        break;
                    }
                }
                if (Climate.Future_MonthlyData != null)
                {
                    AnnualClimate_Monthly AnnualWeather = Climate.Future_MonthlyData[Climate.Future_MonthlyData.Keys.Min()][firstActiveEco];
                    int maxSpinUpYear = Climate.Spinup_MonthlyData.Keys.Max();

                    if (PlugIn.ModelCore.CurrentTime > 0)
                    {
                        currentYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_MonthlyData.Keys.Min();

                        AnnualWeather = Climate.Future_MonthlyData[currentYear][firstActiveEco];
                       
                    }
                    if (PlugIn.ModelCore.CurrentTime == 0)
                    {
                        AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear][firstActiveEco];
                       
                    }
                    actualYear = AnnualWeather.Year;
                }

                if (climateVar.SourceName.Equals("library", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                    {
                        if (ecoregion.Active)
                        {
                            if (!ecoClimateVars.ContainsKey(ecoregion))
                            {
                                ecoClimateVars.Add(ecoregion, new Dictionary<string, double>());
                            }
                            AnnualClimate_Monthly AnnualWeather = Climate.Future_MonthlyData[Climate.Future_MonthlyData.Keys.Min()][ecoregion.Index];
                            int maxSpinUpYear = Climate.Spinup_MonthlyData.Keys.Max();

                            if (PlugIn.ModelCore.CurrentTime == 0)
                            {
                                    AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear][ecoregion.Index];
                            }
                            else
                            {
                                AnnualWeather = Climate.Future_MonthlyData[currentYear][ecoregion.Index];
                            }

                            double monthTotal = 0;
                            int monthCount = 0;
                            double varValue = 0;
                            var monthRange = Enumerable.Range(minMonth, (maxMonth - minMonth) + 1);

                            foreach (int monthIndex in monthRange)
                            {
                                
                                if (climateVar.ClimateLibVariable.Equals("precip", StringComparison.OrdinalIgnoreCase))
                                {
                                    double monthPrecip = AnnualWeather.MonthlyPrecip[monthIndex - 1];
                                    varValue = monthPrecip * 10.0; //Convert cm to mm
                                }
                                else if (climateVar.ClimateLibVariable.Equals("temp", StringComparison.OrdinalIgnoreCase))
                                {
                                    double monthTemp = AnnualWeather.MonthlyTemp[monthIndex - 1];
                                    varValue = monthTemp;
                                }
                                else
                                {
                                    string mesg = string.Format("Climate variable {0} is {1}; expected 'precip' or 'temp'.", climateVar.Name, climateVar.ClimateLibVariable);
                                    throw new System.ApplicationException(mesg);
                                }
                                monthTotal += varValue;
                                monthCount++;
                            }
                            double avgValue = monthTotal / (double)monthCount;
                            double transformValue = avgValue;
                            if (transform.Equals("log10", StringComparison.OrdinalIgnoreCase))
                            //if (transform == "Log10")
                            {
                                transformValue = Math.Log10(avgValue + 1);
                            }
                            else if (transform.Equals("ln", StringComparison.OrdinalIgnoreCase))
                            //else if (transform == "ln")
                            {
                                transformValue = Math.Log(avgValue + 1);
                            }
                            if (!ecoClimateVars[ecoregion].ContainsKey(varName))
                            {
                                ecoClimateVars[ecoregion].Add(varName, 0.0);
                            }
                            ecoClimateVars[ecoregion][varName] = transformValue;
                        }
                    }

                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                        double climateValue = 0;
                        if (ecoregion != null)
                        {
                            climateValue = ecoClimateVars[ecoregion][varName];
                        }
                        // Write Site Variable
                        SiteVars.ClimateVars[site][varName] = (float)climateValue;
                    }
                }
                else if (climateVar.SourceName.Equals("Derived", StringComparison.OrdinalIgnoreCase))
                { 
                  // FIXME
                  // Use values from the derived climate data in place of from the Climate Library

                }
                else  // Use the provided climate data table
                {
                    double monthTotal = 0;
                    int monthCount = 0;
                    double varValue = 0;
                    var monthRange = Enumerable.Range(minMonth, (maxMonth - minMonth) + 1);
                    foreach (int monthIndex in monthRange)
                    {
                        string selectString = "Year = '" + actualYear + "' AND Month = '" + monthIndex + "'";
                        DataRow[] rows = agent.ClimateDataTable.Select(selectString);
                        if (rows.Length == 0)
                        {
                            string mesg = string.Format("Climate data is empty. No record exists for variable {0} in year {1}.", climateVar.Name, actualYear);
                            if (actualYear == 0)
                            {
                                mesg = mesg + "  Note that if using the options Monthly_AverageAllYears or Daily_AverageAllYears you should provide average values for climate variables listed as Year 0.";
                            }
                            throw new System.ApplicationException(mesg);
                        }
                        foreach (DataRow row in rows)
                        {
                            varValue = Convert.ToDouble(row[climateVar.ClimateLibVariable]);
                        }
                        monthTotal += varValue;
                        monthCount++;
                    }
                    double avgValue = monthTotal / (double)monthCount;
                    double transformValue = avgValue;
                    if (transform.Equals("log10", StringComparison.OrdinalIgnoreCase))
                    //if (transform == "Log10")
                    {
                        transformValue = Math.Log10(avgValue + 1);
                    }
                    else if (transform.Equals("ln", StringComparison.OrdinalIgnoreCase))
                    //else if (transform == "ln")
                    {
                        transformValue = Math.Log(avgValue + 1);
                    }
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        SiteVars.ClimateVars[site][varName] = (float)transformValue;
                    }
                }
            }*/
        }
            
    }
}