// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public enum CFIPClean89BlockReason
        {
            None = 0,
            DataIncomplete = 1,
            NoDirection = 2,
            ConfidenceTooLow = 3,
            EvidenceInsufficient = 4,
            MtfDisagreement = 5,
            StructureInvalid = 6,
            EntryInvalid = 7,
            RiskInvalid = 8,
            TargetInvalid = 9,
            SpreadBlocked = 10,
            SessionBlocked = 11,
            NewsBlocked = 12,
            VolatilityBlocked = 13,
            DailyLossBlocked = 14,
            ExistingExposureBlocked = 15,
            DuplicateExecutionBlocked = 16,
            BrokerUnavailable = 17,
            BrokerConstraintsBlocked = 18,
            CooldownBlocked = 19,
            PolicyBlocked = 20,
            Unknown = 99
        }
}
