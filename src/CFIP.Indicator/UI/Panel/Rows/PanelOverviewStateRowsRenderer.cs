using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelOverviewStateRows(
            ref int slot,
            int contentWidth)
        {
            int authoritativeDirection =
                GetAuthoritativeDirection();

            _authoritativeDirection =
                authoritativeDirection;

            _authoritativeState =
                GetAuthoritativeState(
                    authoritativeDirection);

            string stableState =
                GetStablePanelState(
                    _authoritativeState);

            int stateDirection =
                stableState.StartsWith(
                    "BUY",
                    StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : stableState.StartsWith(
                        "SELL",
                        StringComparison.OrdinalIgnoreCase)
                        ? -1
                        : 0;

            AddPanelRow(
                ref slot,
                "CFIP SMART   •  " +
                stableState,
                PanelDirectionColor(
                    stateDirection),
                true,
                contentWidth);

            AddPanelRow(
                ref slot,
                GetCanonicalSignalPanelStatus(),
                GetCanonicalSignalPanelStatusColor(),
                true,
                contentWidth);

            AddPanelRow(
                ref slot,
                "SMART ACTION  •  " +
                _authoritativeState,
                PanelDirectionColor(
                    authoritativeDirection),
                true,
                contentWidth);

            AddPanelRow(
                ref slot,
                "SYNC  " +
                GetSignalSynchronizationText(),
                GetSignalSynchronizationColor(),
                true,
                contentWidth);

            RenderPanelSignalPipelineRows(
                ref slot,
                contentWidth);

            _panelClockRow =
                slot;

            AddPanelRow(
                ref slot,
                SymbolName +
                "  •  EXEC M15  •  " +
                Server.TimeInUtc.ToString(
                    "HH:mm:ss") +
                " UTC",
                PanelMutedTextColor,
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                GetSessionPanelText(),
                GetSessionPanelColor(),
                true,
                contentWidth);

            AddPanelRow(
                ref slot,
                "SUITABILITY  " +
                _marketSuitabilityScore +
                "/100  •  " +
                _marketSuitabilityState +
                "  •  " +
                CompactText(
                    _marketSuitabilityReason,
                    54),
                _marketSuitabilityScore >=
                    Math.Max(
                        50,
                        MinimumMarketSuitability)
                    ? TpLineColor
                    : PanelWarningColor,
                true,
                contentWidth);
        }
    }
}
