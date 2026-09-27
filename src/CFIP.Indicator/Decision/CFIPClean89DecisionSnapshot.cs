// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89DecisionSnapshot
        {
            private readonly ReadOnlyCollection<CFIPClean89BlockReason> _blockReasons;
    
            public CFIPClean89Direction Direction { get; private set; }
            public int Confidence { get; private set; }
            public int Quality { get; private set; }
            public int Edge { get; private set; }
            public int MtfAgreement { get; private set; }
            public int IndependentEvidence { get; private set; }
            public int StructuralConfirmations { get; private set; }
            public string Regime { get; private set; }
            public int RegimeQuality { get; private set; }
    
            // Decision-level eligibility. Entry/Trigger eligibility belongs to Phase 7.
            public bool DecisionEligible { get; private set; }
    
            public CFIPClean89DecisionPolicyMode PolicyMode { get; private set; }
            public IReadOnlyList<CFIPClean89BlockReason> BlockReasons
            {
                get { return _blockReasons; }
            }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89DecisionSnapshot(
                CFIPClean89Direction direction,
                int confidence,
                int quality,
                int edge,
                int mtfAgreement,
                int independentEvidence,
                int structuralConfirmations,
                string regime,
                int regimeQuality,
                bool decisionEligible,
                CFIPClean89DecisionPolicyMode policyMode,
                IList<CFIPClean89BlockReason> blockReasons,
                CFIPClean89Provenance provenance)
            {
                Direction = direction;
                Confidence = Math.Max(0, Math.Min(100, confidence));
                Quality = Math.Max(0, Math.Min(100, quality));
                Edge = Math.Max(-100, Math.Min(100, edge));
                MtfAgreement = Math.Max(0, Math.Min(100, mtfAgreement));
                IndependentEvidence = Math.Max(0, independentEvidence);
                StructuralConfirmations = Math.Max(0, structuralConfirmations);
                Regime = regime ?? string.Empty;
                RegimeQuality = Math.Max(0, Math.Min(100, regimeQuality));
                DecisionEligible = decisionEligible;
                PolicyMode = policyMode;
    
                var normalizedBlocks =
                    new List<CFIPClean89BlockReason>(
                        blockReasons ??
                        new List<CFIPClean89BlockReason>());
    
                if (DecisionEligible &&
                    (Direction == CFIPClean89Direction.Wait ||
                     normalizedBlocks.Count > 0))
                    throw new ArgumentException(
                        "Eligible decision cannot be WAIT or blocked.",
                        "decisionEligible");
    
                if (Direction == CFIPClean89Direction.Wait &&
                    !normalizedBlocks.Contains(CFIPClean89BlockReason.NoDirection))
                    throw new ArgumentException(
                        "WAIT decision requires a NoDirection block.",
                        "decision");
    
                if (Direction != CFIPClean89Direction.Wait &&
                    !DecisionEligible &&
                    normalizedBlocks.Count == 0)
                    throw new ArgumentException(
                        "Blocked directional decision requires at least one block.",
                        "decisionEligible");
    
                if (!DecisionEligible &&
                    (PolicyMode == CFIPClean89DecisionPolicyMode.Confirmed ||
                     PolicyMode == CFIPClean89DecisionPolicyMode.Aggressive))
                    throw new ArgumentException(
                        "Confirmed/Aggressive policy requires decision eligibility.",
                        "policyMode");
    
                if (DecisionEligible &&
                    PolicyMode != CFIPClean89DecisionPolicyMode.Confirmed &&
                    PolicyMode != CFIPClean89DecisionPolicyMode.Aggressive)
                    throw new ArgumentException(
                        "Eligible decision requires Confirmed or Aggressive policy.",
                        "policyMode");
    
                if (PolicyMode == CFIPClean89DecisionPolicyMode.Pending &&
                    (Direction == CFIPClean89Direction.Wait ||
                     normalizedBlocks.Count == 0))
                    throw new ArgumentException(
                        "Pending policy requires a directional blocked setup.",
                        "policyMode");
    
                _blockReasons =
                    new ReadOnlyCollection<CFIPClean89BlockReason>(
                        normalizedBlocks);
                Provenance =
                    provenance ??
                    CFIPClean89Provenance.Direct(
                        "UNKNOWN",
                        "UNSPECIFIED");
            }
        }
}
