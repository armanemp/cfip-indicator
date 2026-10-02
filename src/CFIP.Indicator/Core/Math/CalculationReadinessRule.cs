using System;

namespace cAlgo
{
    internal enum CalculationReadinessState
    {
        BuildingHistory = 0,
        WaitingForClosedM5 = 1,
        WaitingForMtfData = 2,
        Ready = 3
    }

    internal static class CalculationReadinessRule
    {
        internal static CalculationReadinessState ResolveState(
            bool hasBars,
            bool hasMinimumHistory,
            bool hasPrimaryDecisionHistory,
            int closedM5)
        {
            if (!hasBars ||
                !hasMinimumHistory)
                return CalculationReadinessState.BuildingHistory;

            if (closedM5 < 1)
                return CalculationReadinessState.WaitingForClosedM5;

            if (!hasPrimaryDecisionHistory)
                return CalculationReadinessState.WaitingForMtfData;

            return CalculationReadinessState.Ready;
        }

        internal static bool IsProbeDue(
            DateTime lastProbeUtc,
            DateTime nowUtc,
            int intervalMilliseconds)
        {
            if (lastProbeUtc == DateTime.MinValue)
                return true;

            DateTime now =
                CanonicalTimeRule.EnsureUtc(
                    nowUtc);

            DateTime last =
                CanonicalTimeRule.EnsureUtc(
                    lastProbeUtc);

            if (now < last)
                return true;

            return
                (now - last).TotalMilliseconds >=
                Math.Max(
                    1,
                    intervalMilliseconds);
        }

        internal static int ProbeIntervalMilliseconds(
            CalculationReadinessState state)
        {
            switch (state)
            {
                case CalculationReadinessState.WaitingForMtfData:
                    return 500;

                case CalculationReadinessState.WaitingForClosedM5:
                    return 250;

                case CalculationReadinessState.BuildingHistory:
                    return 250;

                default:
                    return 0;
            }
        }

        internal static string StatusText(
            CalculationReadinessState state)
        {
            switch (state)
            {
                case CalculationReadinessState.BuildingHistory:
                    return "BUILDING DATA";

                case CalculationReadinessState.WaitingForClosedM5:
                    return "WAITING FOR CLOSED M5";

                case CalculationReadinessState.WaitingForMtfData:
                    return "WAITING FOR MTF DATA";

                default:
                    return "READY";
            }
        }
    }
}
