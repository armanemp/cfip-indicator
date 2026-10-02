using System;

namespace cAlgo
{
    internal static class IntelligentProtectionContracts
    {
        public static void Run()
        {
            IntelligentProtectionDecision buy =
                IntelligentProtectionRule.Evaluate(
                    1, 100, 103, 99, 1, 2.0, 1, 101.5, 1.0, 0.01, 0.02,
                    0.90, 0.5, 0.5, true, true, true, true, true, true,
                    1.0, 1.0, 0.85, 1.8, 0.10, 0.20, 20, 78, true,
                    0.08, false);

            Check(
                buy.Changed && buy.Stop > 99,
                "BUY intelligent protection advances after sufficient profit");

            IntelligentProtectionDecision sell =
                IntelligentProtectionRule.Evaluate(
                    -1, 100, 97, 101, 1, 2.0, 1, 98.5, 1.0, 0.01, 0.02,
                    0.90, 0.5, 0.5, true, true, true, true, true, true,
                    1.0, 1.0, 0.85, 1.8, 0.10, 0.20, 20, 78, true,
                    0.08, false);

            Check(
                sell.Changed && sell.Stop < 101,
                "SELL intelligent protection is symmetric");

            IntelligentProtectionDecision paused =
                IntelligentProtectionRule.Evaluate(
                    1, 100, 103, 102, 1, 2.0, 1, 102.2, 1.0, 0.01, 0.02,
                    0.90, 0.5, 0.5, true, true, true, false, true, true,
                    1.0, 1.0, 0.85, 1.8, 0.10, 0.20, 20, 78, true,
                    0.08, false);

            Check(
                !paused.Changed &&
                paused.Stop == 102,
                "structural-only trailing waits between structural pulses");

            IntelligentProtectionDecision widen =
                IntelligentProtectionRule.Evaluate(
                    1, 100, 103, 102, 1, 2.0, 1, 101.0, 1.0, 0.01, 0.02,
                    0.90, 0.5, 0.5, true, true, true, true, true, true,
                    1.0, 1.0, 0.85, 1.8, 0.10, 0.20, 20, 78, false,
                    0.08, false);

            Check(
                !widen.Changed &&
                widen.Stop == 102,
                "BUY stop can never widen after protection");

            IntelligentProtectionDecision pressured =
                IntelligentProtectionRule.Evaluate(
                    1, 100, 104, 101, 1, 2.5, 1, 102.5, 1.0, 0.01, 0.02,
                    0.90, 0.5, 0.5, true, true, true, true, true, true,
                    1.0, 1.0, 0.85, 1.8, 0.10, 0.20, 85, 78, true,
                    0.08, false);

            Check(
                pressured.Changed &&
                pressured.Stop > 101,
                "high exit pressure permits tighter structural locking");

            IntelligentProtectionDecision spreadLock =
                IntelligentProtectionRule.Evaluate(
                    1, 100, 102, 99.5, 1, 1.0, 1, 100.8, 8.0, 0.01, 0.02,
                    0.90, 0.5, 0.5, true, true, false, false, true, true,
                    1.0, 1.0, 0.85, 1.8, 0.10, 0.20, 0, 78, false,
                    0.08, false);

            Check(
                spreadLock.Stop < 100.5,
                "smart BE keeps lock offset inside the original risk envelope");

            Console.WriteLine(
                "CI20 intelligent protection semantics contracts PASS");
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
