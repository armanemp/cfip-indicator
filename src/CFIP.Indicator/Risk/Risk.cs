using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public sealed class RiskRequest
        {
            public double RiskPercentEquity { get; private set; }
            public double RequestedVolumeInUnits { get; private set; }
            public double MaxRiskAmount { get; private set; }
    
            public RiskRequest(
                double riskPercentEquity,
                double requestedVolumeInUnits,
                double maxRiskAmount)
            {
                RiskPercentEquity =
                    Math.Max(0, riskPercentEquity);
                RequestedVolumeInUnits =
                    Math.Max(0, requestedVolumeInUnits);
                MaxRiskAmount =
                    Math.Max(0, maxRiskAmount);
            }
        }
    
        public sealed class ExecutionEnvelope
        {
            public double MaxEntryChaseAtr { get; private set; }
            public double MaxBrokerSlippagePips { get; private set; }
            public double MaxBreakoutFillDeviationPips { get; private set; }
            public double MaxPlanRebaseDistancePips { get; private set; }
    
            public ExecutionEnvelope(
                double maxEntryChaseAtr,
                double maxBrokerSlippagePips,
                double maxBreakoutFillDeviationPips,
                double maxPlanRebaseDistancePips)
            {
                MaxEntryChaseAtr =
                    Math.Max(0, maxEntryChaseAtr);
                MaxBrokerSlippagePips =
                    Math.Max(0, maxBrokerSlippagePips);
                MaxBreakoutFillDeviationPips =
                    Math.Max(0, maxBreakoutFillDeviationPips);
                MaxPlanRebaseDistancePips =
                    Math.Max(0, maxPlanRebaseDistancePips);
            }
        }
    
    public static class RiskPolicy
    {
        public static double ResolveRiskPercent(
            ConfigSnapshot configuration,
            DecisionPolicyMode policyMode,
            MarketSuitabilitySnapshot suitability)
        {
            if (configuration == null)
                return 0;

            string key =
                policyMode == DecisionPolicyMode.Aggressive
                    ? "AggressiveRiskPercentEquity"
                    : "RiskPercentEquity";

            double baseRisk = Math.Max(
                0.05,
                configuration.Get(key, 0.50));

            if (!configuration.Get("UseSmartRiskScaling", true) ||
                suitability == null)
                return baseRisk;

            double multiplier =
                Math.Max(0.25, Math.Min(1.0, suitability.RiskMultiplier));

            return Math.Max(
                0.05,
                Math.Min(baseRisk, baseRisk * multiplier));
        }
    }
    
}
