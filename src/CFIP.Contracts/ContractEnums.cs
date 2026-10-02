namespace CFIP.Contracts
{
    public enum TradeDirection
    {
        None = 0,
        Buy = 1,
        Sell = -1
    }

    public enum OpportunityLane
    {
        Strategic = 0,
        Tactical = 1,
        CounterHtfTactical = 2,
        MicroReaction = 3
    }

    public enum SignalStage
    {
        Unavailable = 0,
        Initializing = 1,
        Watch = 2,
        Prediction = 3,
        Confirmed = 4,
        Active = 5,
        Expired = 6,
        Blocked = 7
    }

    public enum ExecutionAction
    {
        None = 0,
        Market = 1,
        Aggressive = 2,
        PendingStop = 3,
        PendingLimit = 4
    }

    public enum ManagementCommandType
    {
        Keep = 0,
        ModifyProtection = 1,
        AdvanceTarget = 2,
        BreakEven = 3,
        PartialClose = 4,
        FullClose = 5,
        CancelPending = 6
    }

    public enum BrokerAction
    {
        None = 0,
        SubmitMarket = 1,
        SubmitMarketRange = 2,
        SubmitAggressive = 3,
        SubmitPendingStop = 4,
        SubmitPendingLimit = 5,
        CancelPending = 6,
        ClosePosition = 7,
        PartialClose = 8,
        ModifyStop = 9,
        ModifyTarget = 10,
        ModifyTargetLadder = 11
    }

    public enum BrokerReportStatus
    {
        Submitted = 0,
        Accepted = 1,
        Rejected = 2,
        Confirmed = 3,
        RecoveryRequired = 4,
        Expired = 5
    }

    public enum LifecycleEventType
    {
        SignalPublished = 0,
        PlanPublished = 1,
        ExecutionRequested = 2,
        BrokerAccepted = 3,
        BrokerRejected = 4,
        PositionConfirmed = 5,
        PendingConfirmed = 6,
        ProtectionConfirmed = 7,
        PartialClosed = 8,
        TargetAdvanced = 9,
        PositionClosed = 10,
        PendingCancelled = 11,
        RecoveryRequired = 12,
        RecoveryCompleted = 13
    }
}
