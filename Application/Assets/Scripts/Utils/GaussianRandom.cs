using UnityEngine;
using System;

public class GaussianRandom : MonoBehaviour
{
    private System.Random random;

    public GaussianRandom()
    {
        random = new System.Random();
    }

    /// <summary>
    /// Generates a random number following a Gaussian distribution.
    /// </summary>
    /// <param name="mean">The mean (peak) of the distribution</param>
    /// <param name="standardDeviation">The standard deviation of the distribution</param>
    /// <returns>A random number following the specified Gaussian distribution</returns>
    public double NextGaussian(double mean, double standardDeviation)
    {
        // Box-Muller transform to generate normal distribution
        double u1 = 1.0 - random.NextDouble(); // Uniform(0,1] random
        double u2 = 1.0 - random.NextDouble();

        // Standard normal distribution with mean 0 and variance 1
        double standardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        // Transform to desired mean and standard deviation
        return mean + standardDeviation * standardNormal;
    }

    /// <summary>
    /// Generates a random integer within a range following a Gaussian distribution.
    /// </summary>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (exclusive)</param>
    /// <param name="mean">The mean (peak) of the distribution, as a value between min and max</param>
    /// <param name="standardDeviation">The standard deviation as a fraction of the range</param>
    /// <returns>A random integer following the specified Gaussian distribution within the given range</returns>
    public int NextGaussianRange(int min, int max, double mean, double standardDeviation)
    {
        // Calculate standard deviation in absolute terms
        double range = max - min;
        double stdDev = range * standardDeviation;

        // Get gaussian value
        double gaussian = NextGaussian(mean, stdDev);

        // Clamp to range and convert to integer
        int result = (int)Math.Round(Math.Clamp(gaussian, min, max - 1));
        return result;
    }
}
