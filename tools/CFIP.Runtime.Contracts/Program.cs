                {
                    Direction = 1,
                    EntryAllowed = true,
                    TriggerReady = false,
                    ActionableNow = true
                };

            TradeOpportunityCandidate retest =
                new TradeOpportunityCandidate
                {
                    Direction = 1,
                    Lane = OpportunityLane.Tactical,
                    ExecutionMode = ExecutionMode.RetestMarket,
                    ActionableNow = true,
                    PresentationOnly = false,
                    Quality = 78
                };

            ScenarioExecutionPolicyResult retestPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    retest,
                    retestDecision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                retestPolicy.CandidateEligible &&
                retestPolicy.ExecutionAuthorized,
                "Retest scenario remains executable without generic TriggerReady");

            TradeOpportunityCandidate breakout =
                new TradeOpportunityCandidate
                {
                    Direction = 1,
                    Lane = OpportunityLane.Tactical,
                    ExecutionMode = ExecutionMode.BreakoutMarket,
                    ActionableNow = true,
                    PresentationOnly = false
                };

            ScenarioExecutionPolicyResult breakoutPolicy =
                ScenarioExecutionPolicyRule.Evaluate(