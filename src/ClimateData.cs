//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Landis.Core;

namespace Landis.Extension.BaseEDA
{
    class ClimateData
    {
        //---------------------------------------------------------------------

        public static DataTable ReadWeatherFile(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("   Loading Climate Data...");

            CSVParser weatherParser = new CSVParser();

            DataTable weatherTable = weatherParser.ParseToDataTable(path);

            return weatherTable;
        }
          //---------------------------------------------------------------------

        public static ExternalClimateData ReadClimateData(IEnumerable<IAgent> manyAgentParameters)
        {
            ExternalClimateData climateData = new ExternalClimateData();
            climateData.ExternalData = new Dictionary<string, ExternalClimateYear>();
            List<string> fileList = new List<string>();
            foreach (IAgent agent in manyAgentParameters)
            {
                foreach (IClimateVariableDefinition climateVar in agent.ClimateVars)
                {
                    if (!(climateVar.SourceName.Equals("Library", StringComparison.OrdinalIgnoreCase)))
                    {
                        fileList.Add(climateVar.SourceName);
                        //climateData.ExternalData.Add(climateVar.SourceName, null);
                    }
                }
                
            }
            // Remove duplicate filenames from list
            List<string> filteredList = fileList.Distinct().ToList();
            foreach (string filename in filteredList)
            {
                //Read climate table
                DataTable weatherTable = ReadWeatherFile(filename);
                string[] columnNames = weatherTable.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();
                List<int> yearList = new List<int>();
                DataTable uniqueYears = weatherTable.DefaultView.ToTable(true, "Year");
                foreach (DataRow dr in uniqueYears.Rows)
                {
                    int year = (int)dr["Year"];
                    yearList.Add(year);
                }   
                List<int> ecoList = new List<int>();
                DataTable uniqueEco = weatherTable.DefaultView.ToTable(true, "EcoregionIndex");
                foreach (DataRow dr in uniqueEco.Rows)
                {
                    int eco = (int)dr["EcoregionIndex"];
                    ecoList.Add(eco);
                }
                ExternalClimateYear yearValues = new ExternalClimateYear();
                yearValues.YearClimate = new Dictionary<int,ExternalClimateEcoregion>();
                foreach (int year in yearList)
                {
                    ExternalClimateEcoregion ecoregionValues = new ExternalClimateEcoregion();
                    ecoregionValues.EcoregionClimate = new Dictionary<int, ExternalClimateVariableValues>();
                    foreach (int eco in ecoList)
                    {
                        ExternalClimateVariableValues externalValues = new ExternalClimateVariableValues();
                        externalValues.ClimateVariableValues = new Dictionary<string, double[]>();
                        foreach (string columnName in columnNames)
                        {
                            // Check for Year and EcoregionIndex variables
                            // All else store as climate variables (if numeric)
                            //FIXME - N/A if restricted to Climate Library
                            bool numericField = true;
                            if (columnName != "Year" && columnName != "EcoregionIndex")
                            {
                                List<double> varList = new List<double>();
                                foreach (DataRow dr in weatherTable.Rows)
                                {
                                    double value;
                                    if (!double.TryParse(dr[columnName].ToString(), out value))
                                    {
                                        numericField = false;
                                        break;
                                    }
                                    varList.Add(value);
                                }
                                if (numericField)
                                {
                                    double[] varArray = varList.ToArray<double>();
                                    externalValues.ClimateVariableValues.Add(columnName, varArray);
                                }
                            }
                        }
                        ecoregionValues.EcoregionClimate.Add(eco, externalValues);
                    }
                    yearValues.YearClimate.Add(year, ecoregionValues);
                }
                climateData.ExternalData.Add(filename, yearValues);
            }


            return climateData;
        }
        //---------------------------------------------------------------------
    }
}
