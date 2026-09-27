using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Enable Early Prediction", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool EnableEarlyPrediction { get; set; }

        [Parameter("Minimum Early Confidence", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 56, MinValue = 40, MaxValue = 95)]
        public int MinimumEarlyConfidence { get; set; }

        [Parameter("Prediction Lookahead Bars", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 10, MinValue = 2, MaxValue = 30)]
        public int PredictionLookaheadBars { get; set; }

        [Parameter("Show Prediction Zone", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool ShowPredictionZone { get; set; }

        [Parameter("Show Prediction Targets", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool ShowPredictionTargets { get; set; }

        [Parameter("Use Liquidity Forecast", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool UseLiquidityForecast { get; set; }

        [Parameter("Alert On Early Setup", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool AlertOnEarlySetup { get; set; }

        [Parameter("Alert On BOS", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool AlertOnBos { get; set; }

        [Parameter("Alert On MSS / CHOCH", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool AlertOnMssChoch { get; set; }

        [Parameter("Alert On Liquidity Sweep", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool AlertOnLiquiditySweep { get; set; }

        [Parameter("Enable Outcome Telemetry", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool EnableOutcomeTelemetry { get; set; }

        [Parameter("Outcome Maximum M5 Bars", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 72, MinValue = 10, MaxValue = 500)]
        public int OutcomeMaximumM5Bars { get; set; }

        [Parameter("Enable Confidence Calibration", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool EnableConfidenceCalibration { get; set; }

        [Parameter("Use Empirical Calibration", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool UseEmpiricalCalibration { get; set; }

        [Parameter("Calibration Directional Minimum Samples", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 6, MinValue = 2, MaxValue = 250)]
        public int CalibrationDirectionalMinimumSamples { get; set; }

        [Parameter("Calibration Minimum Samples", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 5, MinValue = 1, MaxValue = 100)]
        public int CalibrationMinimumSamples { get; set; }

        [Parameter("Calibration Max Confidence Adjustment", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 8, MinValue = 0, MaxValue = 20)]
        public int CalibrationMaxConfidenceAdjustment { get; set; }

        [Parameter("Show Outcome Diagnostics", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
        public bool ShowOutcomeDiagnostics { get; set; }
    }
}
