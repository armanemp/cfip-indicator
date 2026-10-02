using System;
using System.Globalization;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private SignalEnvelope _cfipProviderEnvelope;
        private cAlgo.ExecutionIntent _cfipProviderExecutionIntent;
        private int _cfipProviderExecutionIntentM5 = -1;
        private long _cfipProviderRevision;
        private string _cfipProviderFingerprint = "";
        private DateTime _cfipProviderUpdatedUtc = DateTime.MinValue;

        [Output(
            "CFIP Provider Heartbeat",
            PlotType = PlotType.DiscontinuousLine,
            LineColor = "Transparent")]
        public IndicatorDataSeries ProviderHeartbeat { get; set; }

        // Read-only cross-boundary snapshot. The cBot may consume this value,
        // but it can never replace or mutate Indicator state through this API.
        public SignalEnvelope LatestSignalEnvelope =>
            _cfipProviderEnvelope;

        public long ProviderRevision =>
            _cfipProviderRevision;

        public bool ProviderReady =>
            _initializationReady &&
            _cfipProviderEnvelope != null;

        public string ProviderState =>
            ResolveProviderState(_cfipProviderEnvelope);

        public DateTime ProviderUpdatedUtc =>
            _cfipProviderUpdatedUtc;

        private void CaptureProviderExecutionIntent(
            cAlgo.ExecutionIntent intent)
        {
            if (intent == null)
                return;

            _cfipProviderExecutionIntent = intent;
            _cfipProviderExecutionIntentM5 =
                intent.CreatedM5;
        }

        private string CurrentProviderSource => "CFIP";

    }
}
