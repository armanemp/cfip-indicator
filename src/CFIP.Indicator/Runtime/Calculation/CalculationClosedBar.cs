using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ProcessNewClosedBar(
            int hostIndex,
            int closedM5,
            DateTime reference,
            MtfClosedContext mtf)
        {
            int m1Index = mtf.M1;
            int m2Index = mtf.M2;
            int m15Index = mtf.M15;
            int m30Index = mtf.M30;
            int h1Index = mtf.H1;
            int h4Index = mtf.H4;
            int d1Index = mtf.D1;
            int w1Index = mtf.W1;

            if (m15Index < 30 ||
                m30Index < 30 ||
                h1Index < 30 ||
                h4Index < 30)
            {
                _status =
                    "WAITING FOR MTF DATA";
                RenderPanel();
                return false;
            }

            if (m1Index >= 30)
            {
                bool cachedM1Frame =
                    _m1Frame != null &&
                    ReferenceEquals(
                        _m1Frame.Bars,
                        _m1Bars) &&
                    _m1Frame.Index == m1Index;

                if (!cachedM1Frame)
                {
                    _m1Frame =
                        AnalyzeFrameCached(
                            _m1Frame,
                            _m1Bars,
                            m1Index);
                }
            }
            else
            {
                _m1Frame = null;
            }

            _m2Frame =
                m2Index >= 20
                    ? AnalyzeFrameCached(
                        _m2Frame,
                        _m2Bars,
                        m2Index)
                    : null;

            _m5Frame =
                AnalyzeFrameCached(
                    _m5Frame,
                    _m5Bars,
                    closedM5);

            _m2PrecisionSnapshot =
                M2PrecisionRule.Evaluate(
                    _m2Frame,
                    m2Index,
                    _m5Frame == null ? 0 : _m5Frame.Direction);

            _m15Frame =
                AnalyzeFrameCached(
                    _m15Frame,
                    _m15Bars,
                    m15Index);

            _m30Frame =
                AnalyzeFrameCached(
                    _m30Frame,
                    _m30Bars,
                    m30Index);

            _h1Frame =
                AnalyzeFrameCached(
                    _h1Frame,
                    _h1Bars,
                    h1Index);

            _h4Frame =
                AnalyzeFrameCached(
                    _h4Frame,
                    _h4Bars,
                    h4Index);

            _d1Frame =
                d1Index >= 10
                    ? AnalyzeFrameCached(
                        _d1Frame,
                        _d1Bars,
                        d1Index)
                    : null;

            _w1Frame =
                w1Index >= 10
                    ? AnalyzeFrameCached(
                        _w1Frame,
                        _w1Bars,
                        w1Index)
                    : null;

            _marketStateSnapshot =
                BuildMarketStateSnapshot(
                    reference,
                    mtf);

            if (_marketStateSnapshot == null ||
                !_marketStateSnapshot.MatchesReference(reference) ||
                !_marketStateSnapshot.IsAlignedWithClosedIndices(
                    mtf.M1,
                    mtf.M5,
                    mtf.M15,
                    mtf.M30,
                    mtf.H1,
                    mtf.H4,
                    mtf.D1,
                    mtf.W1))
                throw new InvalidOperationException(
                    "Canonical market-state snapshot is missing or misaligned.");

            _decision =
                BuildDecision(
                    closedM5,
                    reference,
                    mtf);

            RefreshMarketSuitability(
                closedM5,
                _decision == null
                    ? 0
                    : _decision.Direction,
                true);

            _prediction =
                BuildEarlyPrediction(
                    closedM5);

            ArchiveRuntimePrediction(
                _prediction,
                closedM5);

            RenderPredictionObjects(
                _prediction,
                closedM5);

            EmitContextAlerts(
                closedM5);

            ReconcilePreTradePlanDirection(
                closedM5);

            // Refresh the execution geometry for the new canonical M5 before
            // deciding whether its mode is allowed to bypass the generic trigger.
            // This prevents a prior-bar Breakout/Waiting mode from accidentally
            // blocking a current-bar in-zone Retest plan.
            UpdateExecutionModel(
                closedM5);

            TryEnsureAutomaticPlan(
                closedM5);

            // Alerting reads the exact Plan created by the authoritative
            // actionability gate, eliminating Decision-vs-Plan price drift.
            ProcessDecisionAlerts(
                closedM5);

            _lastEvaluatedM5 =
                closedM5;

            UpdateHistoricalSignalPresentation(
                hostIndex);
        
            return true;
        }

        private void UpdateHistoricalSignalPresentation(
            int hostIndex)
        {
            if (ShowHistoricalSignals)
            {
                int hostBar =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            hostIndex));

                if (_lastHistoricalHostBar !=
                    hostBar)
                {
                    RenderHistoricalSignals();
                    _lastHistoricalHostBar =
                        hostBar;
                }
            }
            else
            {
                RemoveHistoricalObjects();
                _lastHistoricalHostBar =
                    -1;
            }
        }
    }
}
