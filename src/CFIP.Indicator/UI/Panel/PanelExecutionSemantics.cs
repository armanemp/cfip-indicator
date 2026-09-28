using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private string ExecutionLevelSemanticsText(
            ExecutionModel model)
        {
            if (model == null)
                return "EXECUTION WAITING";

            switch (model.Mode)
            {
                case ExecutionMode.WaitingForTrigger:
                case ExecutionMode.ContinuationStop:
                    return "TRIGGER " +
                           Price(model.Trigger) +
                           " • ENTRY AFTER TRIGGER";

                case ExecutionMode.RetestMarket:
                    return "IDEAL " +
                           Price(model.IdealEntry) +
                           " • MARKET " +
                           Price(model.ActualEntry) +
                           " • TRIGGER N/A";

                case ExecutionMode.BreakoutMarket:
                    return "ENTRY " +
                           Price(model.ActualEntry) +
                           " • TRIGGER PASSED";

                case ExecutionMode.ReversalLimit:
                    return "LIMIT " +
                           Price(model.IdealEntry) +
                           " • TRIGGER N/A";

                default:
                    return "NO EXECUTION LEVEL";
            }
        }
    }
}