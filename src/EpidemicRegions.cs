//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using System;
using System.IO;
using Landis.SpatialModeling;

namespace Landis.Extension.BaseEDA
{
    public class EpidemicRegions
    {

        //---------------------------------------------------------------------
        public static void ReadMap(string path, int agentIndex)
        {
            IInputRaster<BytePixel> map;

            try
            {
                map = PlugIn.ModelCore.OpenRaster<BytePixel>(path);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(mesg);
            }

            if (map.Dimensions != PlugIn.ModelCore.Landscape.Dimensions)
            {
                string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the ecoregions map", path);
                throw new System.ApplicationException(mesg);
            }

            using (map)
            {
                BytePixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    byte mapCode = pixel.MapCode.Value;
                    if (mapCode > 1)
                    {
                        string mesg = string.Format("Error: The input map {0} must have 0-1 values, where 0 = susceptible, 1 = infected", path);
                        throw new System.ApplicationException(mesg);
                    }

                    if (site.IsActive)
                    {
                        SiteVars.InfStatus[site][agentIndex] = mapCode;
                        if (mapCode == 0)
                        {
                            SiteVars.PSusceptible[site][agentIndex] = 1;
                            SiteVars.PInfected[site][agentIndex] = mapCode;
                        }
                        else
                        {
                            SiteVars.PSusceptible[site][agentIndex] = 0;
                            SiteVars.PInfected[site][agentIndex] = mapCode;
                        }
                    }
                }
            }
        }

    }
}