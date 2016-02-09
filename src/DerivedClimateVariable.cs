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
                if ((value.Equals("sum", StringComparison.OrdinalIgnoreCase)) || (value.Equals("mean", StringComparison.OrdinalIgnoreCase)))
                    function = value;
                else
                {
                    throw new InputValueException(value.ToString(), "Value must be 'sum' or 'mean'.");
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
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                dailyDerivedData[ecoregion.Index] = new Dictionary<string, double[]>();
            }
        }
        //---------------------------------------------------------------------

        public static Dictionary<string, double[]> CalculateDerivedClimateVariables(IAgent agent, IEcoregion ecoregion)
        {
            
            // FIXME

            //foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            //{
                int currentYear = PlugIn.ModelCore.CurrentTime;
                int actualYear = currentYear;
                AnnualClimate_Daily AnnualWeather = Climate.Future_DailyData[Climate.Future_DailyData.Keys.Min()][ecoregion.Index];
                int maxSpinUpYear = Climate.Spinup_DailyData.Keys.Max();

                if (PlugIn.ModelCore.CurrentTime == 0)
                {
                    AnnualWeather = Climate.Spinup_DailyData[maxSpinUpYear][ecoregion.Index];
                }
                else
                {
                    AnnualWeather = Climate.Future_DailyData[currentYear][ecoregion.Index];
                }
                int numDailyRecords = AnnualWeather.DailyTemp.Length;

                Dictionary<string,double[]> dailyDerivedClimate = new Dictionary<string,double[]>();
                double[] blankRecords = new double[numDailyRecords];
                dailyDerivedClimate.Add("JulianDay", blankRecords);

                foreach (DerivedClimateVariable derClimVar in agent.DerivedClimateVars)
                {
                    double[] formulaRecords = new double[numDailyRecords];
                    dailyDerivedClimate.Add(derClimVar.Name, formulaRecords);

                    if (derClimVar.Source == "Formula")
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

                        double variable = 0;
                        for(int i = 0; i < numDailyRecords; i++)
                        {
                             if (variableName.Equals("temp", StringComparison.OrdinalIgnoreCase))
                             {
                                 variable = AnnualWeather.DailyTemp[i];
                             }
                             else
                             {
                                 string mesg = string.Format("Only 'temp' supported for variable in formula");
                                 throw new System.ApplicationException(mesg);
                             }
                            //tempIndex = a + b * exp(c[ln(Variable / d) / e] ^ f);
                            double tempIndex = a + b * Math.Exp(c * Math.Pow((Math.Log(variable / d) / e), f));
                            dailyDerivedClimate[derClimVar.Name][i] = tempIndex;
                            dailyDerivedClimate["JulianDay"][i] = i;
                        }
                        
                        
                    }
                    else  //Not Formula
                    {
                        double[] records = new double[numDailyRecords];
                        dailyDerivedClimate.Add(derClimVar.Name, records);
                        //if daily
                        // create daily record of derived variable to mimic fields of AnnualClimate_Daily
                        if(derClimVar.Time == "Day")
                        {
                            if(derClimVar.Count <= 1)
                            {
                                for(int i = 0; i < numDailyRecords; i++)
                                {
                                    if (derClimVar.ClimateVariable.Equals("precip", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dailyDerivedClimate[derClimVar.Name][i] = AnnualWeather.DailyTemp[i];
                                    }
                                }
                            }
                            else
                            {
                                if (derClimVar.Function.Equals("sum", StringComparison.OrdinalIgnoreCase))
                                {
                                    for(int i = 0; i < numDailyRecords; i++)
                                    {
                                        double varSum = 0;
                                        if (derClimVar.ClimateVariable.Equals("precip", StringComparison.OrdinalIgnoreCase))
                                        {
                                            for (int n = 0; n < derClimVar.Count; n++)
                                            {
                                                int recIndex = i - n;
                                                if(recIndex >= 0)
                                                {
                                                    varSum += AnnualWeather.DailyPrecip[recIndex];
                                                }
                                            }
                                            dailyDerivedClimate[derClimVar.Name][i] = varSum;
                                        }
                                    }
                                }
                                
                            }
                        }
                        //if weekly
                        //  create weekly record of derived variable.  Include min and max julian days and month assignment to each weekly record

                        //if monthly
                        // create monthly record of derived variable to mimic field on AnnualClimate_Monthly
                    }
                }
                return dailyDerivedClimate;
            //}
        }
    }
}