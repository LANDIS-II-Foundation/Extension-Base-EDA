//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using Landis.Library.Climate;
using System.Data;
using System;

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
        private string climateVariable;
        private string function;
        private string time;
        private int count;
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
        /// Initialize a new instance.
        /// </summary>
        public DerivedClimateVariable()
        {
        }
        //---------------------------------------------------------------------
    }
}