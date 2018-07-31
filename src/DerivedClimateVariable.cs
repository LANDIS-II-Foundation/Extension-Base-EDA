//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using Landis.Utilities;
using Landis.Core;
using Landis.Library.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// The definition of a reclass map.
    /// </summary>
    public interface IDerivedClimateVariable
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
        /// Source name
        /// </summary>
        string Source
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Climate  Variable
        /// </summary>
        string ClimateVariable
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Function  (Sum or Mean)
        /// </summary>
        string Function
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Time period (Day, Week, Month)
        /// </summary>
        string Time
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Count
        /// </summary>
        int Count
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
    }

    /// <summary>
    /// The definition of a reclass map.
    /// </summary>
    public class DerivedClimateVariable
        : IDerivedClimateVariable
    {
        private string name;
        private string source;
        private string climateVariable;
        private string function;
        private string time;
        private int count;
        private Dictionary<string, double[]>[] dailyDerivedData;
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
        /// Source name
        /// </summary>
        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Climate Variable
        /// </summary>
        public string ClimateVariable
        {
            get
            {
                return climateVariable;
            }
            set
            {
                climateVariable = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Function
        /// </summary>
        public string Function
        {
            get
            {
                return function;
            }
            set
            {
                if ((value.Equals("sum", StringComparison.OrdinalIgnoreCase)) || (value.Equals("mean", StringComparison.OrdinalIgnoreCase))|| (value.Equals("none", StringComparison.OrdinalIgnoreCase)))
                    function = value;
                else
                {
                    throw new InputValueException(value.ToString(), "Value must be 'Sum', 'Mean', or 'None'.");
                }

            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Time
        /// </summary>
        public string Time
        {
            get
            {
                return time;
            }
            set
            {
                if ((value.Equals("day", StringComparison.OrdinalIgnoreCase)) || (value.Equals("week", StringComparison.OrdinalIgnoreCase)) || (value.Equals("month", StringComparison.OrdinalIgnoreCase)))
                    time = value;
                else
                {
                    throw new InputValueException(value.ToString(), "Value must be 'day', 'week' or 'month'.");
                }
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Count
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Daily Derived Data
        /// </summary>
        public Dictionary<string,double[]>[] DailyDerivedData
        {
            get
            {
                return dailyDerivedData;
            }
            set
            {
                dailyDerivedData = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public DerivedClimateVariable()
        {
            //char[] charArray;
            //charArray = new char[0];
            //function = new string(charArray);
            //time = new string(charArray);
            dailyDerivedData = new Dictionary<string,double[]>[PlugIn.ModelCore.Ecoregions.Count];
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                dailyDerivedData[ecoregion.Index] = new Dictionary<string, double[]>();
            }
        }
        //---------------------------------------------------------------------

        public static Dictionary<string, double[]> CalculateDerivedClimateVariables(IAgent agent, IEcoregion ecoregion)
        {
                int year = PlugIn.ModelCore.CurrentTime;
                /*int actualYear = currentYear;
                AnnualClimate_Daily AnnualWeather = Climate.Future_DailyData[Climate.Future_DailyData.Keys.Min()][ecoregion.Index];
                int maxSpinUpYear = Climate.Spinup_DailyData.Keys.Max();
                int minFutureYear = Climate.Future_DailyData.Keys.Min();
                actualYear = minFutureYear + (currentYear - 1);

                if (currentYear == 0)
                {
                    AnnualWeather = Climate.Spinup_DailyData[maxSpinUpYear][ecoregion.Index];
                }
                else
                {
                    AnnualWeather = Climate.Future_DailyData[actualYear][ecoregion.Index];
                }
                int numDailyRecords = AnnualWeather.DailyTemp.Length;
            */
                int numDailyRecords = 365;
                if (Climate.Spinup_DailyData != null)
                {
                    AnnualClimate_Daily AnnualWeather = Climate.Future_DailyData[Climate.Future_DailyData.Keys.Min()][ecoregion.Index];
                    int minFutureYear = Climate.Future_DailyData.Keys.Min();
                    if (year < 1)
                    {
                        AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                    }
                    else
                    {
                        if(year < minFutureYear)
                        {
                            year = minFutureYear + (year - 1);
                        }
                        if (year != Climate.Future_DailyData.Keys.Min())
                        {
                            AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                        }
                    }
                    
                    numDailyRecords = AnnualWeather.DailyTemp.Length;
                }
                Dictionary<string, double[]> dailyDerivedClimate = new Dictionary<string, double[]>();
                double[] blankRecords = new double[numDailyRecords];
                dailyDerivedClimate.Add("JulianDay", blankRecords);


                foreach (DerivedClimateVariable derClimVar in agent.DerivedClimateVars)
                {
                    double[] records = new double[numDailyRecords];
                    dailyDerivedClimate.Add(derClimVar.Name, records);

                    if (derClimVar.Source.Equals("formula", StringComparison.OrdinalIgnoreCase))
                    {
                        int indexa = agent.VarFormula.Parameters.FindIndex(i => i == "a");
                        double a = Double.Parse(agent.VarFormula.Values[indexa]);
                        int indexb = agent.VarFormula.Parameters.FindIndex(i => i == "b");
                        double b = Double.Parse(agent.VarFormula.Values[indexb]);
                        int indexc = agent.VarFormula.Parameters.FindIndex(i => i == "c");
                        double c = Double.Parse(agent.VarFormula.Values[indexc]);
                        int indexd = agent.VarFormula.Parameters.FindIndex(i => i == "d");
                        double d = Double.Parse(agent.VarFormula.Values[indexd]);
                        int indexe = agent.VarFormula.Parameters.FindIndex(i => i == "e");
                        double e = Double.Parse(agent.VarFormula.Values[indexe]);
                        int indexf = agent.VarFormula.Parameters.FindIndex(i => i == "f");
                        double f = Double.Parse(agent.VarFormula.Values[indexf]);
                        int indexVar = agent.VarFormula.Parameters.FindIndex(i => i == "Variable");
                        string variableName = agent.VarFormula.Values[indexVar];
                        IClimateVariableDefinition selectClimateVar = new ClimateVariableDefinition();
                        bool select = false;
                        foreach(IClimateVariableDefinition climateVar in agent.ClimateVars)
                        {
                            if (variableName.Equals(climateVar.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                selectClimateVar = climateVar;
                                select = true;
                            }
                        }
                        if (select)
                        {
                            double[] variableArray;
                            if (selectClimateVar.SourceName.Equals("Library", StringComparison.OrdinalIgnoreCase))
                            {
                                AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                                if (PlugIn.ModelCore.CurrentTime > 0)
                                {
                                    AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                                }
                                if (selectClimateVar.ClimateLibVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase))
                                {
                                    variableArray = AnnualWeather.DailyTemp;
                                }
                                else if (selectClimateVar.ClimateLibVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
                                {
                                    variableArray = AnnualWeather.DailyPrecip;
                                }
                                else
                                {
                                    string mesg = string.Format("Only 'DailyTemp' and 'DailyPrecip' are supported for ClimateVar in ClimateVariables");
                                    throw new System.ApplicationException(mesg);
                                }
                                for (int i = 0; i < numDailyRecords; i++)
                                {
                                    double variable = variableArray[i];
                                    //tempIndex = a + b * exp(c[ln(Variable / d) / e] ^ f);
                                    double tempIndex = a + b * Math.Exp(c * Math.Pow((Math.Log(variable / d) / e), f));
                                    dailyDerivedClimate[derClimVar.Name][i] = tempIndex;
                                    dailyDerivedClimate["JulianDay"][i] = i + 1;
                                }
                            }
                            else
                            {
                                string mesg = string.Format("ClimateVariables must come from the climate library (Source = 'Library')");
                                throw new System.ApplicationException(mesg);
                                //Read climate table
                                //ExternalClimateVariableValues weatherData = PlugIn.loadedClimateData.ExternalData[selectClimateVar.SourceName].YearClimate[year].EcoregionClimate[ecoregion.Index];
                                //variableArray = weatherData.ClimateVariableValues[selectClimateVar.ClimateLibVariable];

                            }
                            for (int i = 0; i < numDailyRecords; i++)
                            {
                                double variable = variableArray[i];
                                //tempIndex = a + b * exp(c[ln(Variable / d) / e] ^ f);
                                double tempIndex = a + b * Math.Exp(c * Math.Pow((Math.Log(variable / d) / e), f));
                                dailyDerivedClimate[derClimVar.Name][i] = tempIndex;
                                dailyDerivedClimate["JulianDay"][i] = i + 1;
                            }


                        }
                        else
                        {
                            string mesg = string.Format("Variable {1} is not included in ClimateVariables)", variableName);
                            throw new System.ApplicationException(mesg);
                        }
                    }
                    else  //Not Formula
                    {
                        //if daily
                        // create daily record of derived variable to mimic fields of AnnualClimate_Daily
                        if (derClimVar.Time.Equals("day", StringComparison.OrdinalIgnoreCase))
                        {
                            if (derClimVar.Count <= 1)
                            {
                                for (int i = 0; i < numDailyRecords; i++)
                                {
                                    if (derClimVar.Source.Equals("Library", StringComparison.OrdinalIgnoreCase))
                                    {
                                        AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                                        if (PlugIn.ModelCore.CurrentTime > 0)
                                        {
                                            AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                                        }
                                        if (derClimVar.ClimateVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
                                        {
                                            dailyDerivedClimate[derClimVar.Name][i] = AnnualWeather.DailyPrecip[i];
                                            dailyDerivedClimate["JulianDay"][i] = i + 1;
                                        }
                                        else if (derClimVar.ClimateVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase))
                                        {
                                            dailyDerivedClimate[derClimVar.Name][i] = AnnualWeather.DailyTemp[i];
                                            dailyDerivedClimate["JulianDay"][i] = i + 1;
                                        }
                                        else
                                        {
                                            string mesg = string.Format("Only 'DailyPrecip' and 'DailyTemp' supported for ClimateVar in DerivedClimateVariables");
                                            throw new System.ApplicationException(mesg);
                                        }
                                    }
                                    else
                                    {
                                        // Read climate file
                                        // Extract variable
                                        string mesg = string.Format("ClimateVariables must come from the climate library (Source = 'Library')");
                                        throw new System.ApplicationException(mesg);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < numDailyRecords; i++)
                                {
                                    double varSum = 0;
                                    double varCount = 0;
                                    if (derClimVar.Source.Equals("Library", StringComparison.OrdinalIgnoreCase))
                                    {
                                        AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                                        if (PlugIn.ModelCore.CurrentTime > 0)
                                        {
                                            AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                                        }
                                        if (derClimVar.ClimateVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
                                        {
                                            for (int n = 0; n < derClimVar.Count; n++)
                                            {
                                                int recIndex = i - n;
                                                if (recIndex >= 0)
                                                {
                                                    varSum += AnnualWeather.DailyPrecip[recIndex];
                                                    varCount += 1;
                                                }
                                            }
                                        }
                                        else if ((derClimVar.ClimateVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase)))
                                        {
                                            for (int n = 0; n < derClimVar.Count; n++)
                                            {
                                                int recIndex = i - n;
                                                if (recIndex >= 0)
                                                {
                                                    varSum += AnnualWeather.DailyTemp[recIndex];
                                                    varCount += 1;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string mesg = string.Format("Only 'DailyPrecip' and 'DailyTemp' supported for ClimateVar in DerivedClimateVariables");
                                            throw new System.ApplicationException(mesg);
                                        }
                                        if (derClimVar.Function.Equals("sum", StringComparison.OrdinalIgnoreCase))
                                        {
                                            dailyDerivedClimate[derClimVar.Name][i] = varSum;
                                        }
                                        else if (derClimVar.Function.Equals("mean", StringComparison.OrdinalIgnoreCase))
                                        {
                                            dailyDerivedClimate[derClimVar.Name][i] = varSum / varCount;
                                        }
                                        else
                                        {
                                            string mesg = string.Format("Only 'Sum' and 'Mean' supported for Function in DerivedClimateVariables");
                                            throw new System.ApplicationException(mesg);
                                        }
                                        dailyDerivedClimate["JulianDay"][i] = i + 1;
                                    }
                                    else
                                    {
                                        //Read Climate
                                        // Read variable
                                        string mesg = string.Format("ClimateVariables must come from the climate library (Source = 'Library')");
                                        throw new System.ApplicationException(mesg);

                                    }
                                }
                            }
                        }
                        else
                        {
                            //if weekly - FIXME
                            //  create weekly record of derived variable.  Include min and max julian days and month assignment to each weekly record

                            //if monthly - FIXME
                            // create monthly record of derived variable to mimic field on AnnualClimate_Monthly
                            string mesg = string.Format("Only 'Day' supported for DerivedClimateVariable Time");
                            throw new System.ApplicationException(mesg);
                        }
                    }
                }
                return dailyDerivedClimate;
        }
        //---------------------------------------------------------------------

        public static Dictionary<string, double[]> CalculateHistoricDerivedClimateVariables(IAgent agent, IEcoregion ecoregion, int year)
        {
            //int currentYear = year;
            //int actualYear = currentYear;
            //AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[Climate.Spinup_DailyData.Keys.Max()][ecoregion.Index];
            //int maxSpinUpYear = Climate.Spinup_DailyData.Keys.Max();
            //int minFutureYear = AnnualWeather.Year;
            //actualYear = minFutureYear + (currentYear - 1);
            int numDailyRecords = 365;
            if (Climate.Spinup_DailyData != null)
            {
                AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                numDailyRecords = AnnualWeather.DailyTemp.Length;
            }
            Dictionary<string, double[]> dailyDerivedClimate = new Dictionary<string, double[]>();
            double[] blankRecords = new double[numDailyRecords];
            dailyDerivedClimate.Add("JulianDay", blankRecords);

            foreach (DerivedClimateVariable derClimVar in agent.DerivedClimateVars)
            {
                double[] records = new double[numDailyRecords];
                dailyDerivedClimate.Add(derClimVar.Name, records);

                if (derClimVar.Source.Equals("formula", StringComparison.OrdinalIgnoreCase))
                {
                    int indexa = agent.VarFormula.Parameters.FindIndex(i => i == "a");
                    double a = Double.Parse(agent.VarFormula.Values[indexa]);
                    int indexb = agent.VarFormula.Parameters.FindIndex(i => i == "b");
                    double b = Double.Parse(agent.VarFormula.Values[indexb]);
                    int indexc = agent.VarFormula.Parameters.FindIndex(i => i == "c");
                    double c = Double.Parse(agent.VarFormula.Values[indexc]);
                    int indexd = agent.VarFormula.Parameters.FindIndex(i => i == "d");
                    double d = Double.Parse(agent.VarFormula.Values[indexd]);
                    int indexe = agent.VarFormula.Parameters.FindIndex(i => i == "e");
                    double e = Double.Parse(agent.VarFormula.Values[indexe]);
                    int indexf = agent.VarFormula.Parameters.FindIndex(i => i == "f");
                    double f = Double.Parse(agent.VarFormula.Values[indexf]);
                    int indexVar = agent.VarFormula.Parameters.FindIndex(i => i == "Variable");
                    string variableName = agent.VarFormula.Values[indexVar];
                    IClimateVariableDefinition selectClimateVar = new ClimateVariableDefinition();
                    bool select = false;
                    foreach (IClimateVariableDefinition climateVar in agent.ClimateVars)
                    {
                        if (variableName.Equals(climateVar.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            selectClimateVar = climateVar;
                            select = true;
                        }
                    }
                    if (select)
                    {
                        double[] variableArray;
                        if (selectClimateVar.SourceName.Equals("Library", StringComparison.OrdinalIgnoreCase))
                        {
                            AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                            if (PlugIn.ModelCore.CurrentTime > 0)
                            {
                                AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                            }
                            if (selectClimateVar.ClimateLibVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase))
                            {
                                variableArray = AnnualWeather.DailyTemp;
                            }
                            else if (selectClimateVar.ClimateLibVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
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
                            //Read climate table
                            //ExternalClimateVariableValues weatherData = PlugIn.loadedClimateData.ExternalData[selectClimateVar.SourceName].YearClimate[year].EcoregionClimate[ecoregion.Index];
                            //variableArray = weatherData.ClimateVariableValues[selectClimateVar.ClimateLibVariable];
                            
                        }
                        for (int i = 0; i < numDailyRecords; i++)
                        {
                            double variable = variableArray[i];
                            //tempIndex = a + b * exp(c[ln(Variable / d) / e] ^ f);
                            double tempIndex = a + b * Math.Exp(c * Math.Pow((Math.Log(variable / d) / e), f));
                            dailyDerivedClimate[derClimVar.Name][i] = tempIndex;
                            dailyDerivedClimate["JulianDay"][i] = i + 1;
                        }
                    }
                    else
                    {
                        string mesg = string.Format("Variable {1} is not included in ClimateVariables)", variableName);
                        throw new System.ApplicationException(mesg);
                    }
                }
                else  //Not Formula
                {
                    //if daily
                    // create daily record of derived variable to mimic fields of AnnualClimate_Daily
                    if (derClimVar.Time.Equals("day", StringComparison.OrdinalIgnoreCase))
                    {
                        if (derClimVar.Count <= 1)
                        {
                            for (int i = 0; i < numDailyRecords; i++)
                            {
                                if (derClimVar.Source.Equals("Library", StringComparison.OrdinalIgnoreCase))
                                {
                                    AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                                    if (PlugIn.ModelCore.CurrentTime > 0)
                                    {
                                        AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                                    }
                                    if (derClimVar.ClimateVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dailyDerivedClimate[derClimVar.Name][i] = AnnualWeather.DailyPrecip[i];
                                        dailyDerivedClimate["JulianDay"][i] = i + 1;
                                    }
                                    else if (derClimVar.ClimateVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dailyDerivedClimate[derClimVar.Name][i] = AnnualWeather.DailyTemp[i];
                                        dailyDerivedClimate["JulianDay"][i] = i + 1;
                                    }
                                    else
                                    {
                                        string mesg = string.Format("Only 'DailyPrecip'and 'DailyTemp' supported for ClimateVar in DerivedClimateVariables");
                                        throw new System.ApplicationException(mesg);
                                    }
                                }
                                else
                                {
                                    string mesg = string.Format("ClimateVariables must come from the climate library (Source = 'Library')");
                                    throw new System.ApplicationException(mesg);
                                    // Read climate file
                                    // Extract variable
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < numDailyRecords; i++)
                            {
                                double varSum = 0;
                                double varCount = 0;
                                
                                    for (int n = 0; n < derClimVar.Count; n++)
                                    {
                                        int recIndex = i - n;
                                        if (recIndex >= 0)
                                        {
                                            if (derClimVar.Source.Equals("Library", StringComparison.OrdinalIgnoreCase))
                                            {
                                                AnnualClimate_Daily AnnualWeather = Climate.Spinup_DailyData[year][ecoregion.Index];
                                                if (PlugIn.ModelCore.CurrentTime > 0)
                                                {
                                                    AnnualWeather = Climate.Future_DailyData[year][ecoregion.Index];
                                                }
                                                if (derClimVar.ClimateVariable.Equals("DailyPrecip", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    varSum += AnnualWeather.DailyPrecip[recIndex];
                                                }
                                                else if (derClimVar.ClimateVariable.Equals("DailyTemp", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    varSum += AnnualWeather.DailyTemp[recIndex];
                                                }
                                                else
                                                {
                                                    string mesg = string.Format("Only 'DailyPrecip' and 'DailyTemp' supported for ClimateVar in DerivedClimateVariables");
                                                    throw new System.ApplicationException(mesg);
                                                }
                                            }
                                            else
                                            {
                                                string mesg = string.Format("ClimateVariables must come from the climate library (Source = 'Library')");
                                                throw new System.ApplicationException(mesg);
                                                //Read Climate
                                                // Read variable
                                            }
                                            varCount += 1;
                                        }
                                    }
                                    if (derClimVar.Function.Equals("sum", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dailyDerivedClimate[derClimVar.Name][i] = varSum;
                                    }
                                    else if (derClimVar.Function.Equals("mean", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dailyDerivedClimate[derClimVar.Name][i] = varSum / varCount;
                                    }
                                    else
                                    {
                                        string mesg = string.Format("Only 'Sum' and 'Mean' supported for Function in DerivedClimateVariables");
                                        throw new System.ApplicationException(mesg);
                                    }
                                    dailyDerivedClimate["JulianDay"][i] = i + 1;
                                
                            }
                        }
                    }
                    else
                    {
                        //if weekly - FIXME
                        //  create weekly record of derived variable.  Include min and max julian days and month assignment to each weekly record

                        //if monthly - FIXME
                        // create monthly record of derived variable to mimic field on AnnualClimate_Monthly
                        string mesg = string.Format("Only 'Day' supported for DerivedClimateVariable Time");
                        throw new System.ApplicationException(mesg);
                    }
                }
            }
            return dailyDerivedClimate;
        }

        public static DataTable ReadWeatherFile(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("   Loading Climate Data...");

            CSVParser weatherParser = new CSVParser();

            DataTable weatherTable = weatherParser.ParseToDataTable(path);

            return weatherTable;
        }
        //---------------------------------------------------------------------
    }
}