//  Copyright 2016 North Carolina State University, Center for Geospatial Analytics & 
//  Forest Service Northern Research Station, Institute for Applied Ecosystem Studies
//  Authors:  Francesco Tonini, Brian R. Miranda

using Landis.Core;
using Landis.SpatialModeling;
using System;
using System.Collections.Generic;

namespace Landis.Extension.BaseEDA
{
 
    public class Dispersal
    {

        //max distance in pixels (use max dist param from spp param table)
        public int max_dispersal_distance_pixels;

        //declare a new dictionary to hold disp prob values for each distance 
        private Dictionary<double, double> dispersal_probability;

        //public double total_p;

        //class constructor
        public Dispersal() { }

        //populating the dispersal probability lookup table for a given agent
        public void Initialize(IAgent agent)
        {

            dispersal_probability = new Dictionary<double, double>();
            Dictionary<double, int> dispersal_prob_count = new Dictionary<double, int>();
            dispersal_probability.Clear();

            //calculate how many pixels max dist in the moving window
            max_dispersal_distance_pixels = (int)(agent.DispersalMaxDist / PlugIn.ModelCore.CellLength);

            //what IS THIS FOR?
            //total_p = 0;

            for (int x = 0; x <= max_dispersal_distance_pixels; x++)
            {
                //use y=x calculates only half of a matrix
                for (int y = x; y <= max_dispersal_distance_pixels; y++)
                {
                    double dx, dy, dist, prob;

                    dx = PlugIn.ModelCore.CellLength * x;
                    dy = PlugIn.ModelCore.CellLength * y;
                    //calculate distance value for diagonal
                    dist = Math.Sqrt(dx * dx + dy * dy);

                    prob = Kernel_prob(agent, dist);

                    if (dispersal_probability.ContainsKey(dist))
                    {
                        dispersal_probability[dist] += prob;
                        dispersal_prob_count[dist]++;
                    }
                    else
                    {
                        dispersal_probability.Add(dist, prob);
                        dispersal_prob_count.Add(dist, 1);
                    }

                    //what IS THIS FOR?
                    /*if (x == 0 && y == 0)
                    {
                        total_p += prob;
                    }
                    else if (x == y || x == 0 || y == 0)
                    {
                        total_p += 4 * prob;
                    }
                    else
                    {
                        total_p += 8 * prob;
                    }
                     * */
                } //end of y loop                
            }//end of x loop

            foreach (double dist in dispersal_prob_count.Keys)
            {
                dispersal_probability[dist] = dispersal_probability[dist] / dispersal_prob_count[dist];
            }

            Console.WriteLine("Dispersal Lookup Table Initialization Done.");
        }

        private double Kernel_prob(IAgent agent, double d)
        {
            double c, prob = 0;

            if (agent.DispersalKernel == DispersalTemplate.PowerLaw)
            {
            //need to set a value for x_min (e.g. = 1):
            int x_min = 1;

            //normalization constant
            c = (double) (agent.AlphaCoef - 1) * (Math.Pow(x_min, agent.AlphaCoef - 1));
            double func = (double) Math.Pow(d, -agent.AlphaCoef);
            prob = c * func;

            }
            else if (agent.DispersalKernel == DispersalTemplate.NegExp)
            {
                double mean = (double) 1 / agent.AlphaCoef;
                c = (double) 1 / (2 * Math.PI * mean);
                double func = (double) (1 / mean) * Math.Exp(-d / mean);
                prob = c * func;
            }

            // ... additional kernels can be added here ...

            // set bounds to prob
            if (prob > 1)
                prob = 1;
            if (prob < 0)
                prob = 0;

            return prob; 
        }

        /// <summary>
        /// Get a list of distances for which dispersal probabilies have been computed
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> GetProbabilityDistances()
        {
            return dispersal_probability.Keys;
        }

        /// <summary>
        /// Get dispersal probabilities at a particular distance
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public double GetDispersalProbability(double distance)
        {
            return dispersal_probability[distance];
        }

    }
}
