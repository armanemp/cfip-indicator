// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89MarketFrame
        {
            private readonly ReadOnlyCollection<CFIPClean89FeatureEvidence> _features;
    
            public string Timeframe { get; private set; }
            public DateTime ClosedBarTimeUtc { get; private set; }
    
            public double Open { get; private set; }
            public double High { get; private set; }
            public double Low { get; private set; }
            public double Close { get; private set; }
    
            // Raw indicator measurements.
            public double Atr { get; private set; }
            public double AtrRatio { get; private set; }
            public double Rsi { get; private set; }
            public double Adx { get; private set; }
            public double DmiBias { get; private set; }
            public double EmaFast { get; private set; }
            public double EmaSlow { get; private set; }
            public double EmaSpreadAtr { get; private set; }
            public double EmaSlopeAtr { get; private set; }
            public double MomentumAtr { get; private set; }
            public double MacdHistogram { get; private set; }
            public double Vwap { get; private set; }
            public double VolumeRatio { get; private set; }
            public double BodyAtr { get; private set; }
            public double RangeAtr { get; private set; }
    
            // Typed interpretation; this is market bias, not final trade direction.
            public CFIPClean89Direction BiasDirection { get; private set; }
            public int BiasStrength { get; private set; }
            public int MarketQuality { get; private set; }
            public int BullScore { get; private set; }
            public int BearScore { get; private set; }
            public int BullScoreNormalized { get; private set; }
            public int BearScoreNormalized { get; private set; }
            public int IndependentEvidence { get; private set; }
            public int EnabledEvidenceFeatures { get; private set; }
    
            public CFIPClean89Regime Regime { get; private set; }
            public int RegimeQuality { get; private set; }
            public bool Choppy { get; private set; }
            public bool HealthyVolatility { get; private set; }
            public bool DataValid { get; private set; }
    
            public IReadOnlyList<CFIPClean89FeatureEvidence> Features
            {
                get { return _features; }
            }
    
            public int DirectionScore
            {
                get
                {
                    return
                        BiasDirection == CFIPClean89Direction.Buy
                            ? BiasStrength
                            : BiasDirection == CFIPClean89Direction.Sell
                                ? -BiasStrength
                                : 0;
                }
            }
    
            public CFIPClean89MarketFrame(
                string timeframe,
                DateTime closedBarTimeUtc,
                double open,
                double high,
                double low,
                double close,
                double atr,
                double atrRatio,
                double rsi,
                double adx,
                double dmiBias,
                double emaFast,
                double emaSlow,
                double emaSpreadAtr,
                double emaSlopeAtr,
                double momentumAtr,
                double macdHistogram,
                double vwap,
                double volumeRatio,
                double bodyAtr,
                double rangeAtr,
                CFIPClean89Direction biasDirection,
                int biasStrength,
                int marketQuality,
                int bullScore,
                int bearScore,
                int bullScoreNormalized,
                int bearScoreNormalized,
                int independentEvidence,
                int enabledEvidenceFeatures,
                CFIPClean89Regime regime,
                int regimeQuality,
                bool choppy,
                bool healthyVolatility,
                bool dataValid,
                IList<CFIPClean89FeatureEvidence> features)
            {
                Timeframe = timeframe ?? string.Empty;
                ClosedBarTimeUtc = closedBarTimeUtc;
                Open = open;
                High = high;
                Low = low;
                Close = close;
                Atr = Math.Max(0, atr);
                AtrRatio = Math.Max(0, atrRatio);
                Rsi = Math.Max(0, Math.Min(100, rsi));
                Adx = Math.Max(0, Math.Min(100, adx));
                DmiBias =
                    Math.Max(
                        -1,
                        Math.Min(
                            1,
                            dmiBias));
                EmaFast = emaFast;
                EmaSlow = emaSlow;
                EmaSpreadAtr = emaSpreadAtr;
                EmaSlopeAtr = emaSlopeAtr;
                MomentumAtr = momentumAtr;
                MacdHistogram = macdHistogram;
                Vwap = Math.Max(0, vwap);
                VolumeRatio = Math.Max(0, volumeRatio);
                BodyAtr = Math.Max(0, bodyAtr);
                RangeAtr = Math.Max(0, rangeAtr);
                BiasDirection = biasDirection;
                BiasStrength = Math.Max(0, Math.Min(100, biasStrength));
                MarketQuality =
                    Math.Max(0,
                        Math.Min(100, marketQuality));
                BullScore = Math.Max(0, bullScore);
                BearScore = Math.Max(0, bearScore);
                BullScoreNormalized =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            bullScoreNormalized));
                BearScoreNormalized =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            bearScoreNormalized));
                IndependentEvidence = Math.Max(0, independentEvidence);
                EnabledEvidenceFeatures =
                    Math.Max(
                        0,
                        enabledEvidenceFeatures);
                Regime = regime;
                RegimeQuality =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            regimeQuality));
                Choppy = choppy;
                HealthyVolatility = healthyVolatility;
                DataValid = dataValid;
                _features =
                    new ReadOnlyCollection<CFIPClean89FeatureEvidence>(
                        new List<CFIPClean89FeatureEvidence>(
                            features ??
                            new List<CFIPClean89FeatureEvidence>()));
            }
        }
}
