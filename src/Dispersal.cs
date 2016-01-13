using Landis.Core;
using Landis.SpatialModeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Troschuetz.Random;

namespace Landis.Extension.BaseEDA
{
    //public enum Dispersal_Model { DOUBLE_EXPONENTIAL, TWODT };
    //public enum Dispersal_Type { STATIC, DYNAMIC };    // either “STATIC”, meaning that the dispersal model does not depend on circumstances that can change during a simulation, or “DYNAMIC”

    public class Dispersal
    {

        //-------------------------------------------------------
        ///<summary>
        ///Calculate the distance between two Sites
        ///</summary>
        public static double DistanceBetweenSites(Site a, Site b)
        {

            int Col = (int)a.Location.Column - (int)b.Location.Column;
            int Row = (int)a.Location.Row - (int)b.Location.Row;

            double aSq = System.Math.Pow(Col, 2);
            double bSq = System.Math.Pow(Row, 2);
            //PlugIn.ModelCore.Log.WriteLine("Col={0}, Row={1}.", Col, Row);
            //PlugIn.ModelCore.Log.WriteLine("aSq={0}, bSq={1}.", aSq, bSq);
            //PlugIn.ModelCore.Log.WriteLine("Distance in Grid Units = {0}.", System.Math.Sqrt(aSq + bSq));
            return (System.Math.Sqrt(aSq + bSq) * (double)PlugIn.ModelCore.CellLength);

        }

        //class constructor
        public Dispersal() { }


        public double[] dispersal_parameters;
        public int max_dispersal_distance_pixels;

        //GET THIS FROM INPUT PARAMETERS
        public int time_step;             // the length of a disturbance time step (in years)

        public int mc_draws;              // the number of Monte Carlo draws to use when estimating dispersal probabilities
        public int rand_seed;             // seed for random number generator

        private Dictionary<double, double> dispersal_probability;

        private ContinuousUniformDistribution uniform; // uniform.nextDouble() generates a double floating point number between 0 and 1

        public double total_p;
        
       //class constructor
       // public Dispersal() { }

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

        private double DrawFromUniformDistribution(double min, double max)
        {
            return min + (max - min) * uniform.NextDouble();
        }
        
        // populating the dispersal probability lookup table for a given agent
        public void Initialize(IAgent agent)
        {
            rand_seed = 4789;
            Generator gen = new MT19937Generator(rand_seed);
            uniform = new ContinuousUniformDistribution(gen);

            uniform.Alpha = 0.0;
            uniform.Beta = 1.0;

            dispersal_probability = new Dictionary<double, double>();
            Dictionary<double, int> dispersal_prob_count = new Dictionary<double, int>();
            dispersal_probability.Clear();

            max_dispersal_distance_pixels = (int)(agent.DispersalMaxDist / PlugIn.ModelCore.CellLength);

            total_p = 0;

            for (int x = 0; x <= max_dispersal_distance_pixels; x++)
            {
                for (int y = x; y <= max_dispersal_distance_pixels; y++)
                {

                    double dx, dy, r, p;
                    dx = PlugIn.ModelCore.CellLength * x;
                    dy = PlugIn.ModelCore.CellLength * y;
                    //calculate distance value for diagonal
                    r = Math.Sqrt(dx * dx + dy * dy);

                    p = dispersal_prob(agent, x, y);
                    if (dispersal_probability.ContainsKey(r))
                    {
                        dispersal_probability[r] += p;
                        dispersal_prob_count[r]++;
                    }
                    else
                    {
                        dispersal_probability.Add(r, p);
                        dispersal_prob_count.Add(r, 1);
                    }

                    if (x == 0 && y == 0)
                    {
                        total_p += p;
                    }
                    else if (x == y || x == 0 || y == 0)
                    {
                        total_p += 4 * p;
                    }
                    else
                    {
                        total_p += 8 * p;
                    }
                } //end of y loop                
            }//end of x loop

            foreach (double r in dispersal_prob_count.Keys)
            {
                dispersal_probability[r] = dispersal_probability[r] / dispersal_prob_count[r];
            }

            Console.WriteLine("Dispersal Lookup Table Initialization Done.");
        }

        private double dispersal_prob(IAgent agent, int x, int y)
        {
            double prob;
            double[] p = new double[mc_draws];
            double half_pixel_size = PlugIn.ModelCore.CellLength / 2;
            for (int m = 0; m < mc_draws; m++)
            {
                double x0 = DrawFromUniformDistribution(0.0, 1.0) * PlugIn.ModelCore.CellLength - half_pixel_size;
                double y0 = DrawFromUniformDistribution(0.0, 1.0) * PlugIn.ModelCore.CellLength - half_pixel_size;
                double xl = x * PlugIn.ModelCore.CellLength + DrawFromUniformDistribution(0.0, 1.0) * PlugIn.ModelCore.CellLength - half_pixel_size;
                double yl = y * PlugIn.ModelCore.CellLength + DrawFromUniformDistribution(0.0, 1.0) * PlugIn.ModelCore.CellLength - half_pixel_size;
                p[m] = displacement_prob(agent,x0, y0, xl, yl);
            }
            double p_mean = p.Average();
            prob = PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength * p_mean;

            // set bounds to prob
            if (prob > 1)
                prob = 1;
            if (prob < 0)
                prob = 0;

            return prob;
        }

        private double displacement_prob(IAgent agent, double x0, double y0, double xl, double yl)
        {
            double dx, dy, r, prob = 0;
            dx = xl - x0;   // displacement in x direction
            dy = yl - y0;   // displacement in y direction
            r = Math.Sqrt(dx * dx + dy * dy); // distance

            if (agent.DispersalKernel == DispersalTemplate.PowerLaw)
            {
                //WRITE HERE

                //double w1, w2, mean1, mean2, c, part1, part2;
                //w1 = dispersal_parameters[0];
                //w2 = 1 - w1;
                //mean1 = dispersal_parameters[1];
                //mean2 = dispersal_parameters[2];

                //// set bounds to means
                //if (mean1 < 0)
                //    mean1 = 0;
                //if (mean2 < 0)
                //    mean2 = 0;

                //c = 1 / (2 * Math.PI * (w1 * mean1 + w2 * mean2));
                //part1 = (w1 / mean1) * Math.Exp(-r / mean1);
                //part2 = (w2 / mean2) * Math.Exp(-r / mean2);
                //prob = c * (part1 + part2);
            }
            else if (agent.DispersalKernel == DispersalTemplate.NegExp)
            {
                //WRITE HERE

                //double a, b, part1, part2;
                //a = dispersal_parameters[0];  // shape parameter
                //b = dispersal_parameters[1];  // scale parameter
                //part1 = a / (Math.PI * b);
                //part2 = 1 + (r * r) / b;
                //prob = part1 * 1 / (Math.Pow(part2, a + 1));
            }

            // ... additional kernels can be added here ...

            // set bounds to prob
            if (prob < 0)
                prob = 0;

            return prob;
        }

        /*private double marginal_prob(double r)
        {
            double prob = 0;
            if (dispersal_model == Dispersal_Model.DOUBLE_EXPONENTIAL)
            {
                double w1, w2, mean1, mean2, c, p1, p2;
                w1 = s.dispersal_parameters[0];
                w2 = 1 - w1;
                mean1 = s.dispersal_parameters[1];
                mean2 = s.dispersal_parameters[2];

                // set bounds to means
                if (mean1 < 0)
                    mean1 = 0;
                if (mean2 < 0)
                    mean2 = 0;

                c = r / ((w1 * mean1 + w2 * mean2));
                p1 = (w1 / mean1) * Math.Exp(-r / mean1);
                p2 = (w2 / mean2) * Math.Exp(-r / mean2);
                prob = c * (p1 + p2);
            }
            else if (dispersal_model == Dispersal_Model.TWODT)
            {
                double a, b, part1, part2;
                a = s.dispersal_parameters[0];
                b = s.dispersal_parameters[1];
                part1 = a / (Math.PI * b);
                part2 = 1 + r * r / b;
                prob = 2 * Math.PI * r * part1 * 1 / (Math.Pow(part2, a + 1));
            }

            // set bounds to prob
            if (prob > 1)
                prob = 1;
            if (prob < 0)
                prob = 0;

            return prob;
        }
        */
    }
}
