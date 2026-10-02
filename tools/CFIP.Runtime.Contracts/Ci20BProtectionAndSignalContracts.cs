using System;

namespace cAlgo
{
    internal static class Ci20BProtectionAndSignalContracts
    {
        public static void Run()
        {
            VerifyIntelligentProtection();
            VerifyPrimaryPullbackTuning();

            Console.WriteLine(
                "CI20B protection + signal hardening contracts PASS");
        }

        private static void VerifyIntelligentProtection()
        {
            IntelligentProtectionDecision buy =
                IntelligentProtectionRule.Evaluate(
                    1,
                    100,
                    103,
                    99,
                    1,
                    1.0,
                    1,
                    0,
                    1.0,
                    0.01,
                    0.02,
                    0.90,
                    0.5,
                    0.5,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    1.0,
                    1.0,
                    1.0,
                    1.8,
                    0.10,
                    0.20,
                    20,
                    78,
                    true,
                    0.08,
                    false,
                    103);

            Check(
                buy.Changed &&
                buy.Stop > 99 &&
                buy.Stop < 100.10,
                "BUY smart break-even advances monotonically");

            IntelligentProtectionDecision sell =
                IntelligentProtectionRule.Evaluate(
                    -1,
                    100,
                    97,
                    101,
                    1,
                    1.0,
                    1,
                    0,
                    1.0,
                    0.01,
                    0.02,
                    0.90,
                    0.5,
                    0.5,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    1.0,
                    1.0,
                    1.0,
                    1.8,
                    0.10,
                    0.20,
                    20,
                    78,
                    true,
                    0.08,
                    false,
                    97);

            Check(
                sell.Changed &&
                sell.Stop < 101 &&
                sell.Stop > 99.90,
                "SELL smart break-even is symmetric");

            IntelligentProtectionDecision serverOwned =
                IntelligentProtectionRule.Evaluate(
                    1,
                    100,
                    103,
                    99.5,
                    1,
                    1.0,
                    1,
                    100.5,
                    1.0,
                    0.01,
                    0.02,
                    0.90,
                    0.5,
                    0.5,
                    true,
                    true,
                    false,
                    true,
                    true,
                    true,
                    1.0,
                    1.0,
                    1.0,
                    1.8,
                    0.10,
                    0.20,
                    20,
                    78,
                    true,
                    0.08,
                    true,
                    103);

            Check(
                !serverOwned.Changed &&
                serverOwned.Stop == 99.5,
                "server-owned break-even prevents local BE competition");

            IntelligentProtectionDecision widen =
                IntelligentProtectionRule.Evaluate(
                    1,
                    100,
                    103,
                    101,
                    1,
                    2.0,
                    1,
                    100.5,
                    1.0,
                    0.01,
                    0.02,
                    0.90,
                    0.5,
                    0.5,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    1.0,
                    1.0,
                    1.0,
                    1.8,
                    0.10,
                    0.20,
                    20,
                    78,
                    false,
                    0.08,
                    false,
                    103);

            Check(
                !widen.Changed &&
                widen.Stop == 101,
                "BUY protection never widens an existing stop");

            IntelligentProtectionDecision invalid =
                IntelligentProtectionRule.Evaluate(
                    1,
                    100,
                    103,
                    99,
                    1,
                    double.NaN,
                    1,
                    0,
                    1.0,
                    0.01,
                    0.02,
                    0.90,
                    0.5,
                    0.5,
                    true,
                    true,
                    false,
                    false,
                    true,
                    true,
                    1.0,
                    1.0,
                    1.0,
                    1.8,
                    0.10,
                    0.20,
                    20,
                    78,
                    false,
                    0.08,
                    false,
                    103);

            Check(
                !invalid.Changed &&
                invalid.Reason == "PROTECTION INPUT INVALID",
                "non-finite protection state fails closed");
        }

        private static void VerifyPrimaryPullbackTuning()
        {
            Check(
                PrimaryPullbackTuningRule.AllowsNeutralM5(
                    1,
                    0,
                    1,
                    1,
                    72,
                    70,
                    60,
                    true),
                "qualified M15/H1 primary pullback accepts neutral M5");

            Check(
                !PrimaryPullbackTuningRule.AllowsNeutralM5(
                    1,
                    -1,
                    1,
                    1,
                    72,
                    70,
                    60,
                    true),
                "opposite M5 remains blocked");

            Check(
                !PrimaryPullbackTuningRule.AllowsNeutralM5(
                    1,
                    0,
                    1,
                    -1,
                    72,
                    70,
                    60,
                    true),
                "H1 primary conflict remains blocked");

            Check(
                !PrimaryPullbackTuningRule.AllowsNeutralM5(
                    1,
                    0,
                    1,
                    1,
                    59,
                    70,
                    60,
                    true),
                "weak primary M15 quality remains blocked");

            Check(
                !PrimaryPullbackTuningRule.AllowsNeutralM5(
                    1,
                    0,
                    1,
                    1,
                    72,
                    70,
                    60,
                    false),
                "missing top-down eligibility remains blocked");
        }

        private static void Check(
            bool condition,
            string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
