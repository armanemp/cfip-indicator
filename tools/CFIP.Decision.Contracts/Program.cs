            VerifySmartBreakEven();
            VerifyEntryGeometry();
            VerifyEntrySignalTiming();

            Console.WriteLine("Decision contracts OK");
        }

        private static void VerifyEntryGeometry()
        {
            EntryGeometrySnapshot retest =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,
                    100.08,
                    99.00,
                    100.00,
                    0.10,
                    100.00,
                    101.00,
                    100.00,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    false,
                    0.75,
                    0.50);

            Assert(
                retest.Mode == ExecutionMode.RetestMarket &&
                retest.InsideZone &&
                !retest.TriggerReached &&
                !retest.IsLate &&
                Math.Abs(retest.ActualEntry - retest.Market) < 1e-12,
                "BUY retest geometry uses the canonical tolerant zone");

            EntryGeometrySnapshot continuationRetest =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,
                    99.85,
                    99.00,
                    100.00,
                    0.10,
                    99.80,
                    101.00,
                    99.80,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    true,
                    0.75,
                    0.50);

            Assert(
                continuationRetest.Mode == ExecutionMode.RetestMarket &&
                continuationRetest.InsideZone &&
                !continuationRetest.TriggerReached &&
                Math.Abs(continuationRetest.ActualEntry - continuationRetest.Market) < 1e-12,
                "continuation context must not hide an in-zone BUY retest");

            EntryGeometrySnapshot breakout =
                EntryGeometryRule.Evaluate(
                    -1,
                    ExecutionMode.None,
                    98.995,
                    99.00,
                    100.00,
                    0.02,
                    99.50,
                    99.00,
                    99.60,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    false,
                    0.75,
                    0.50);

            Assert(
                breakout.Mode == ExecutionMode.BreakoutMarket &&
                breakout.TriggerReached &&
                breakout.Anchor == 99.00 &&
                !breakout.IsLate,
                "SELL breakout geometry is directionally symmetric");

            EntryGeometrySnapshot lateBreakout =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,
                    102.00,
                    99.00,
                    100.00,
                    0.10,
                    99.50,
                    100.00,
                    99.50,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    false,
                    0.75,
                    0.50);

            Assert(
                lateBreakout.Mode == ExecutionMode.BreakoutMarket &&
                Math.Abs(lateBreakout.TriggerExtensionAtr - 2.0) < 1e-12 &&
                lateBreakout.IsLate,
                "breakout late state is derived from trigger extension");

            EntryGeometrySnapshot lateRetest =
                EntryGeometryRule.Evaluate(
                    -1,
                    ExecutionMode.None,
                    99.00,
                    98.00,
                    99.20,
                    0.05,
                    98.20,
                    97.50,
                    98.20,
                    1.00,
                    0.01,
                    0.10,
                    false,
                    false,
                    0.75,
                    0.50);

            Assert(
                lateRetest.Mode == ExecutionMode.RetestMarket &&
                Math.Abs(lateRetest.EntryDistanceAtr - 0.8) < 1e-12 &&
                lateRetest.IsLate,
                "retest late state is derived from ideal-entry distance");

            EntryGeometrySnapshot waiting =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,