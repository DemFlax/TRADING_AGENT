using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.NinjaScript;
using NinjaTrader.Gui;
using System.Windows.Media;
using System.ComponentModel.DataAnnotations;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class VolumeProfileAPI : NinjaTrader.NinjaScript.Indicators.Indicator
    {
        private HVNDetector hvnDetector;
        private LVNDetector lvnDetector;
        private Dictionary<double, double> currentProfile;
        private double currentVPOC;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Volume Profile API con VPOC, HVN, LVN expuestos para NinjaScript";
                Name = "VolumeProfileAPI";
                IsOverlay = true;
                IsSuspendedWhileInactive = true;

                AddPlot(Brushes.Red, "VPOC");
                AddPlot(Brushes.Green, "HVN_High");
                AddPlot(Brushes.Blue, "LVN_Low");

                HVNPercentile = 80.0;
                LVNPercentile = 20.0;
            }
            else if (State == State.DataLoaded)
            {
                hvnDetector = new HVNDetector();
                lvnDetector = new LVNDetector();
                currentProfile = new Dictionary<double, double>();

                hvnDetector.SetPercentile(HVNPercentile);
                lvnDetector.SetPercentile(LVNPercentile);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 20)
                return;

            UpdateVolumeProfile();

            currentVPOC = CalculateVPOC();
            Values[0][0] = currentVPOC;

            List<double> hvnLevels = hvnDetector.DetectHVN(currentProfile);
            if (hvnLevels.Count > 0)
                Values[1][0] = hvnLevels.Max();

            List<double> lvnLevels = lvnDetector.DetectLVN(currentProfile);
            if (lvnLevels.Count > 0)
                Values[2][0] = lvnLevels.Min();
        }

        private void UpdateVolumeProfile()
        {
            double price = Close[0];
            double volume = Volume[0];

            if (currentProfile.ContainsKey(price))
                currentProfile[price] += volume;
            else
                currentProfile[price] = volume;
        }

        private double CalculateVPOC()
        {
            if (currentProfile.Count == 0)
                return 0.0;

            return currentProfile.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        public double GetVPOC(int barsAgo)
        {
            if (barsAgo >= Values[0].Count)
                return 0.0;
            return Values[0][barsAgo];
        }

        public double GetHVNHigh(int barsAgo)
        {
            if (barsAgo >= Values[1].Count)
                return 0.0;
            return Values[1][barsAgo];
        }

        public double GetLVNLow(int barsAgo)
        {
            if (barsAgo >= Values[2].Count)
                return 0.0;
            return Values[2][barsAgo];
        }

        public List<double> GetAllHVN()
        {
            return hvnDetector.DetectHVN(currentProfile);
        }

        public List<double> GetAllLVN()
        {
            return lvnDetector.DetectLVN(currentProfile);
        }

        [NinjaScriptProperty]
        [Range(75, 95)]
        [Display(Name = "HVN Percentile", Order = 1, GroupName = "Parameters")]
        public double HVNPercentile { get; set; }

        [NinjaScriptProperty]
        [Range(10, 30)]
        [Display(Name = "LVN Percentile", Order = 2, GroupName = "Parameters")]
        public double LVNPercentile { get; set; }
    }
}