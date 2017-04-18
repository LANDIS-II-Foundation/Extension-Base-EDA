//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda, Chris Jones

using Landis.SpatialModeling;

namespace Landis.Extension.BaseEDA
{
    public class BytePixel : Pixel
    {
        public Band<byte> MapCode = "The numeric code for each raster cell";

        public BytePixel()
        {
            SetBands(MapCode);
        }
    }
}

