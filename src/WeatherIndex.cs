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
    public interface IWeatherIndex
    {
       
        /// <summary>
        /// Min Month
        /// </summary>
        int MinMonth
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Max Month
        /// </summary>
        int MaxMonth
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Function
        /// </summary>
        string Function
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
    }

    /// <summary>
    /// The definition of a reclass map.
    /// </summary>
    public class WeatherIndex
        : IWeatherIndex
    {
        private int minMonth;
        private int maxMonth;
        private string function;
        //---------------------------------------------------------------------
        /// <summary>
        /// Min Month
        /// </summary>
        public int MinMonth
        {
            get
            {
                return minMonth;
            }
            set
            {
                if((value <1) || (value >12))
                    throw new InputValueException(value.ToString(), "Value must be >=1 and <= 12.");
                minMonth = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Max Month
        /// </summary>
        public int MaxMonth
        {
            get
            {
                return maxMonth;
            }
            set
            {
                if ((value < 1) || (value > 12))
                    throw new InputValueException(value.ToString(), "Value must be >=1 and <= 12.");
                if(value < minMonth)
                    throw new InputValueException(value.ToString(), "Value must be <= MinMonth.");
                maxMonth = value;
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
        /// Initialize a new instance.
        /// </summary>
        public WeatherIndex()
        {
        }
        //---------------------------------------------------------------------

        public static void CalculateAnnualWeatherIndex(IAgent agent)
        {
            double monthTotal = 0;
            int monthCount = 0;
            double varValue = 0;
            string varName = "AnnualWeatherIndex";
            int minMonth = agent.AnnualWeatherIndex.MinMonth;
            int maxMonth = agent.AnnualWeatherIndex.MaxMonth;
            var monthRange = Enumerable.Range(minMonth, (maxMonth - minMonth) + 1);
            Dictionary<IEcoregion, double> ecoClimateVars = new Dictionary<IEcoregion, double>();
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                double transformValue = 0;
                foreach (int monthIndex in monthRange)
                {
                    //Select days that match month

                    //for each day in month
                    //varValue = WeatherIndex;
                    monthTotal += varValue;
                    monthCount++;
                }
                double avgValue = monthTotal / (double)monthCount;
                
                if(agent.AnnualWeatherIndex.Function.Equals("sum", StringComparison.OrdinalIgnoreCase))
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

                ecoClimateVars[ecoregion] = transformValue;

            }
            foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                double climateValue = 0;
                if (ecoregion != null)
                {
                    climateValue = ecoClimateVars[ecoregion];
                }
                // Write Site Variable
                SiteVars.ClimateVars[site][varName] = (float)climateValue;
            }




        }
            
    }
}