using System;
using cAlgo.API;

namespace CFIP.cBot
{
    [Robot(
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public sealed class CFIPExecutionBot : Robot
    {
        private const string StartupState = "WAITING";

        protected override void OnStart()
        {
            Print(
                "CFIP cBot START | state={0} | broker mutation=DISARMED | " +
                "Indicator execution extraction track=ACTIVE | contractVersion={1}",
                StartupState,
                CFIP.Contracts.ContractVersion.Current);
        }

        protected override void OnTick()
        {
            // P0 is intentionally fail-closed. No broker mutation API is exposed
            // until the corresponding migration phase has passed replacement,
            // parity and source-gate verification.
        }

        protected override void OnStop()
        {
            Print(
                "CFIP cBot STOP | broker mutation=DISARMED | " +
                "execution state was not promoted without migration evidence.");
        }
    }
}
