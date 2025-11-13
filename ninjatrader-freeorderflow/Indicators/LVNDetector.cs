using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Windows.Media.Media3D;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class LVNDetector
    {
        private double lvnPercentile = 20.0;

        /// <summary>
        /// Detecta Low Volume Nodes (zonas con volumen ≤ percentil 20)
        /// </summary>
        public List<double> DetectLVN(Dictionary<double, double> volumeProfile)
        {
            if (volumeProfile == null || volumeProfile.Count == 0)
                return new List<double>();

            var sortedVolumes = volumeProfile.Values.OrderBy(v => v).ToList();
            int thresholdIndex = (int)(sortedVolumes.Count * (lvnPercentile / 100.0));

            if (thresholdIndex >= sortedVolumes.Count)
                thresholdIndex = sortedVolumes.Count - 1;

            double threshold = sortedVolumes[thresholdIndex];

            return volumeProfile
                .Where(kvp => kvp.Value <= threshold)
                .Select(kvp => kvp.Key)
                .OrderBy(price => price)
                .ToList();
        }

        public void SetPercentile(double percentile)
        {
            if (percentile >= 10 && percentile <= 30)
                lvnPercentile = percentile;
        }

        public double GetPercentile()
        {
            return lvnPercentile;
        }
    }
}