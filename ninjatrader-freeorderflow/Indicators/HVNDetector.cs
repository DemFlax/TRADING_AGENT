using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Windows.Media.Media3D;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class HVNDetector
    {
        private double hvnPercentile = 80.0;

        /// <summary>
        /// Detecta High Volume Nodes (zonas con volumen ≥ percentil 80)
        /// </summary>
        public List<double> DetectHVN(Dictionary<double, double> volumeProfile)
        {
            if (volumeProfile == null || volumeProfile.Count == 0)
                return new List<double>();

            var sortedVolumes = volumeProfile.Values.OrderByDescending(v => v).ToList();
            int thresholdIndex = (int)(sortedVolumes.Count * (1 - hvnPercentile / 100.0));

            if (thresholdIndex >= sortedVolumes.Count)
                thresholdIndex = sortedVolumes.Count - 1;

            double threshold = sortedVolumes[thresholdIndex];

            return volumeProfile
                .Where(kvp => kvp.Value >= threshold)
                .Select(kvp => kvp.Key)
                .OrderBy(price => price)
                .ToList();
        }

        public void SetPercentile(double percentile)
        {
            if (percentile >= 75 && percentile <= 95)
                hvnPercentile = percentile;
        }

        public double GetPercentile()
        {
            return hvnPercentile;
        }
    }
}