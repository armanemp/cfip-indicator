using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void RenderPanelRangeIntelligenceRows(
            ref int slot,
            int contentWidth)
        {
            RangeManipulationSnapshot range =
                _rangeManipulation;

            if (range == null ||
                !range.IsRange)
                return;

            string rangeDirection =
                range.IsManipulationWatch
                    ? DirectionText(
                        range.WatchDirection)
                    : "WAIT";

            string rangeStage =
                range.IsManipulationWatch
                    ? "MANIPULATION WATCH"
                    : range.State;

            AddPanelRow(
                ref slot,
                "RANGE INTEL  " +
                rangeStage +
                "  •  " +
                rangeDirection +
                "  •  Q" +
                range.Score +
                "  •  WATCH " +
                range.WatchScore +
                "  •  BND " +
                range.BoundaryPressure +
                "  MOM " +
                range.MomentumPressure +
                " VOL " +
                range.VolumePressure,
                range.WatchDirection > 0
                    ? TpLineColor
                    : range.WatchDirection < 0
                        ? SlLineColor
                        : PanelWarningColor,
                true,
                contentWidth);
        }
    }
}
