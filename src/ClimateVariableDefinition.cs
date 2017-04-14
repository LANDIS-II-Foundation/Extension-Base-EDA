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
            Dictionary<IEcoregion, double> ecoClimateVars = new Dictionary<IEcoregion, double>();

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                double transformValue = 0;
                if (ecoregion.Active)
                {
                    // Calculate Derived Climate Variables
                    Dictionary<string, double[]> dailyDerivedClimate = DerivedClimateVariable.CalculateDerivedClimateVariables(agent, ecoregion);
                    int numDailyRecords = dailyDerivedClimate[dailyDerivedClimate.Keys.First()].Length;
                    double[] blankRecords = new double[numDailyRecords];
                    for (int i = 0; i < numDailyRecords; i++)
                    {
                        blankRecords[i] = 1;
                    }
                    dailyDerivedClimate.Add("WeatherIndex", blankRecords);

                    foreach (string weatherVar in agent.WeatherIndexVars)
                    {
                        bool varMatch = false;
                        foreach (DerivedClimateVariable derClimVar in agent.DerivedClimateVars)
                        {
                            if (derClimVar.Name.Equals(weatherVar, StringComparison.OrdinalIgnoreCase))
                            {
                                for (int i = 0; i < numDailyRecords; i++)
                                {
                                    double tempIndex = dailyDerivedClimate[weatherVar][i];
                                    dailyDerivedClimate["WeatherIndex"][i] *= tempIndex;
                                }
                                varMatch = true;
                            }
                        }

                        //if weatherVar is raw climate value (not derived)
                        foreach (IClimateVariableDefinition climVar in agent.ClimateVars)
                        {
                            if (climVar.Name.Equals(weatherVar, StringComparison.OrdinalIgnoreCase))
                            {
                                double[] variableArray;
                                if (climVar.SourceName.Equals("Library", StringComparison.OrdinalIgnoreCase))
                                {
                                    AnnualClimate_Daily AnnualWeather = Climate.Future_DailyData[Climate.Future_DailyData.Keys.Min()][ecoregion.Index];
                                    int minFutureYear = Climate.Future_DailyData.Keys.Min();
                                    int year = PlugIn.ModelCore.CurrentTime;
                                    if (year < 1)
                                    {
                                        AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                                    }
                                    else
                                    {
                                        if (year < minFutureYear)
                                        {
                                            year = minFutureYear + (year - 1);
                                        }
                                        if (year != Climate.Future_DailyData.Keys.Min())
                                        {
                                            AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                                        }
                                    }


                                    if (climVar.ClimateLibVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase))
                                    {
                                        variableArray = AnnualWeather.DailyTemp;
                                    }
                                    else if (climVar.ClimateLibVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
                                    {
                                        variableArray = AnnualWeather.DailyPrecip;
                                    }
                                    else
                                    {
                                        string mesg = string.Format("Only 'DailyTemp' and 'DailyPrecip' are supported for ClimateVar in ClimateVariables");
                                        throw new System.ApplicationException(mesg);
                                    }
                                }
                                else
                                {
                                    string mesg = string.Format("ClimateVariables must come from the climate library (Source = 'Library')");
                                    throw new System.ApplicationException(mesg);
                                    //ExternalClimateVariableValues weatherData = PlugIn.loadedClimateData.ExternalData[climVar.SourceName].YearClimate[PlugIn.ModelCore.CurrentTime].EcoregionClimate[ecoregion.Index];
                                    //variableArray = weatherData.ClimateVariableValues[climVar.ClimateLibVariable];                                  
                                }
                                for (int i = 0; i < numDailyRecords; i++)
                                {
                                    double tempIndex = variableArray[i];
                                    dailyDerivedClimate["WeatherIndex"][i] *= tempIndex;
                                }
                                varMatch = true;
                            }
                        }
                        if(!varMatch)
                        {
                            string mesg = string.Format("WeatherIndex variable {1} is not a ClimateVariable or a DerivedClimateVariable.",weatherVar);
                            throw new System.ApplicationException(mesg);
                        }

                    }
                    //Summarize annual
                    double monthTotal = 0;
                    int monthCount = 0;
                    double varValue = 0;
                    int minMonth = agent.AnnualWeatherIndex.MinMonth;
                    int maxMonth = agent.AnnualWeatherIndex.MaxMonth;
                    var monthRange = Enumerable.Range(minMonth, (maxMonth - minMonth) + 1);
                    int[] monthMaxJulDay = new int[] { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

                    
                    foreach (int monthIndex in monthRange)
                    {
                        //Select days that match month
                        int minDay = monthMaxJulDay[monthIndex - 1] + 1;
                        int maxDay = monthMaxJulDay[monthIndex];
                        for (int day = minDay; day <= maxDay; day++)
                        {
                            //for each day in month
                            varValue = dailyDerivedClimate["WeatherIndex"][day - 1];
                            monthTotal += varValue;
                            monthCount++;
                        }
                    }
                    double avgValue = monthTotal / (double)monthCount;

                    if (agent.AnnualWeatherIndex.Function.Equals("sum", StringComparison.OrdinalIgnoreCase))
                    {
                        transformValue = monthTotal;
                    }
                    else if (agent.AnnualWeatherIndex.Function.Equals("mean", StringComparison.OrdinalIgnoreCase))
                    {
                        transformValue = avgValue;
                    }
                    else
                    {
                        string mesg = string.Format("Annual Weather Index function is {1}; expected 'sum' or 'mean'.", agent.AnnualWeatherIndex.Function);
                        throw new System.ApplicationException(mesg);
                    }
                }
                ecoClimateVars[ecoregion] = transformValue;

            }

                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                {
                    IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                    double climateValue = 0;
                    if (ecoregion != null)
                    {
                        climateValue = ecoClimateVars[ecoregion];
                    }
                    // Write Site Variable
                    SiteVars.ClimateVars[site]["AnnualWeatherIndex"] = (float)climateValue;
                }
            

            }
           //---------------------------------------------------------------------

        public static void CalculateHistoricClimateVariables(IAgent agent)
        {
            //Dictionary<IEcoregion, double> ecoClimateVars = new Dictionary<IEcoregion, double>();
            //int[] monthlyYears = Climate.Spinup_MonthlyData.Keys.ToArray();
            //int[] years = (Enumerable.Range(0, PlugIn.ModelCore.EndTime+1).ToArray());
            List<int> yearList = (Enumerable.Range(0, PlugIn.ModelCore.EndTime+1).ToList());
           
            if (Climate.Spinup_DailyData != null)
            {
                yearList = Climate.Spinup_DailyData.Keys.ToList();
            }
            else
            {
                int dataIndex = 0;
                foreach (KeyValuePair<string, ExternalClimateYear> climateYear in PlugIn.loadedClimateData.ExternalData)
                {
                    List<int> climateYearArray = climateYear.Value.YearClimate.Keys.ToList();
                    if (dataIndex == 0)
                    {
                        yearList = climateYearArray;
                    }
                    else
                    {
                        foreach(int year in climateYearArray)
                        {
                            if(!yearList.Contains(year))
                            {
                                yearList.Add(year);
                            }
                        }
                    }
                }
            }
            yearList.Sort();
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {               
                    double ecoTotal = 0;
                    if (ecoregion.Active)
                    {
                        foreach (int year in yearList)
                        {

                            // Calculate Derived Climate Variables
                            Dictionary<string, double[]> dailyDerivedClimate = DerivedClimateVariable.CalculateHistoricDerivedClimateVariables(agent, ecoregion, year);
                            int numDailyRecords = dailyDerivedClimate[dailyDerivedClimate.Keys.First()].Length;
                            double[] blankRecords = new double[numDailyRecords];
                            for (int i = 0; i < numDailyRecords; i++)
                            {
                                blankRecords[i] = 1;
                            }
                            dailyDerivedClimate.Add("WeatherIndex", blankRecords);

                            foreach (string weatherVar in agent.WeatherIndexVars)
                            {
                                bool varMatch = false;
                                foreach (DerivedClimateVariable derClimVar in agent.DerivedClimateVars)
                                {
                                    if (derClimVar.Name.Equals(weatherVar, StringComparison.OrdinalIgnoreCase))
                                    {
                                        for (int i = 0; i < numDailyRecords; i++)
                                        {
                                            double tempIndex = dailyDerivedClimate[weatherVar][i];
                                            dailyDerivedClimate["WeatherIndex"][i] *= tempIndex;
                                        }
                                        varMatch = true;
                                    }
                                }

                                //if weatherVar is raw climate value (not derived)
                                foreach (IClimateVariableDefinition climVar in agent.ClimateVars)
                                {
                                    if (climVar.Name.Equals(weatherVar, StringComparison.OrdinalIgnoreCase))
                                    {
                                        double[] variableArray;
                                        if (climVar.SourceName.Equals("Library", StringComparison.OrdinalIgnoreCase))
                                        {
                                            AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                                            if (climVar.ClimateLibVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase))
                                            {
                                                variableArray = AnnualWeather.DailyTemp;
                                            }
                                            else if (climVar.ClimateLibVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
                                            {
                                                variableArray = AnnualWeather.DailyPrecip;
                                            }
                                            else
                                            {
                                                string mesg = string.Format("Only 'DailyTemp' and 'DailyPrecip' are supported for ClimateVar in ClimateVariables");
                                                throw new System.ApplicationException(mesg);
                                            }
                                        }
                                        else
                                        {
                                            string mesg = string.Format("ClimateVariables must come from the climate library (Source = 'Library')");
                                            throw new System.ApplicationException(mesg);
                                            //ExternalClimateVariableValues weatherData = PlugIn.loadedClimateData.ExternalData[climVar.SourceName].YearClimate[PlugIn.ModelCore.CurrentTime].EcoregionClimate[ecoregion.Index];
                                            //variableArray = weatherData.ClimateVariableValues[climVar.ClimateLibVariable];                                  
                                        }
                                        for (int i = 0; i < numDailyRecords; i++)
                                        {
                                            double tempIndex = variableArray[i];
                                            dailyDerivedClimate["WeatherIndex"][i] *= tempIndex;
                                        }
                                        varMatch = true;
                                    }
                                }
                                if (!varMatch)
                                {
                                    string mesg = string.Format("WeatherIndex variable {1} is not a ClimateVariable or a DerivedClimateVariable.", weatherVar);
                                    throw new System.ApplicationException(mesg);
                                }

                            }
                            //Summarize annual
                            double monthTotal = 0;
                            int monthCount = 0;
                            double varValue = 0;
                            int minMonth = agent.AnnualWeatherIndex.MinMonth;
                            int maxMonth = agent.AnnualWeatherIndex.MaxMonth;
                            var monthRange = Enumerable.Range(minMonth, (maxMonth - minMonth) + 1);
                            int[] monthMaxJulDay = new int[] { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

                            double transformValue = 0;
                            foreach (int monthIndex in monthRange)
                            {
                                //Select days that match month
                                int minDay = monthMaxJulDay[monthIndex - 1] + 1;
                                int maxDay = monthMaxJulDay[monthIndex];
                                for (int day = minDay; day <= maxDay; day++)
                                {
                                    //for each day in month
                                    varValue = dailyDerivedClimate["WeatherIndex"][day - 1];
                                    monthTotal += varValue;
                                    monthCount++;
                                }
                            }
                            double avgValue = monthTotal / (double)monthCount;

                            if (agent.AnnualWeatherIndex.Function.Equals("sum", StringComparison.OrdinalIgnoreCase))
                            {
                                transformValue = monthTotal;
                            }
                            else if (agent.AnnualWeatherIndex.Function.Equals("mean", StringComparison.OrdinalIgnoreCase))
                            {
                                transformValue = avgValue;
                            }
                            else
                            {
                                string mesg = string.Format("Annual Weather Index function is {1}; expected 'sum' or 'mean'.", agent.AnnualWeatherIndex.Function);
                                throw new System.ApplicationException(mesg);
                            }

                            ecoTotal += transformValue;

                        }
                    }
                    agent.EcoWeatherIndexNormal[ecoregion.Index] = ecoTotal / yearList.Count;
                
            }
            
        }
    }
}