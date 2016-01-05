//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.BaseEDA
{
    /// <summary>
    /// The Temp Index equation
    /// </summary>
    public interface ITempIndexModel
    {
        //---------------------------------------------------------------------
        /// <summary>
        /// Parameters
        /// </summary>
        List<string> Parameters
        {
            get;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Values
        /// </summary>
        List<string> Values
        {
            get;
        }
        //---------------------------------------------------------------------
    }

    /// <summary>
    /// The definition of a species model.
    /// </summary>
    public class TempIndexModel
        : ITempIndexModel
    {
        private List<string> parameters;
        private List<string> values;
        //---------------------------------------------------------------------
        /// <summary>
        /// Parameters
        /// </summary>
        public List<string> Parameters
        {
            get {
                return parameters;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Values
        /// </summary>
        public List<string> Values
        {
            get
            {
                return values;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public TempIndexModel()
        {
            parameters = new List<string>();
            values = new List<string>();
        }
        //---------------------------------------------------------------------

    }
}
