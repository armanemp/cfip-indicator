using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SubscribeCbotChartLifecycleEvents()
        {
            ChartRobots.RobotAdded += OnChartCbotAdded;
            ChartRobots.RobotRemoved += OnChartCbotRemoved;
            ChartRobots.RobotModified += OnChartCbotModified;
            ChartRobots.RobotStarted += OnChartCbotStarted;
            ChartRobots.RobotStopped += OnChartCbotStopped;
        }

        private void UnsubscribeCbotChartLifecycleEvents()
        {
            ChartRobots.RobotAdded -= OnChartCbotAdded;
            ChartRobots.RobotRemoved -= OnChartCbotRemoved;
            ChartRobots.RobotModified -= OnChartCbotModified;
            ChartRobots.RobotStarted -= OnChartCbotStarted;
            ChartRobots.RobotStopped -= OnChartCbotStopped;
        }

        private void OnChartCbotAdded(
            ChartRobotAddedEventArgs args)
        {
            RefreshCbotPanelAfterLifecycleEvent(
                "CBOT ATTACHED");
        }

        private void OnChartCbotRemoved(
            ChartRobotRemovedEventArgs args)
        {
            RefreshCbotPanelAfterLifecycleEvent(
                "CBOT DETACHED");
        }

        private void OnChartCbotModified(
            ChartRobotModifiedEventArgs args)
        {
            RefreshCbotPanelAfterLifecycleEvent(
                "CBOT MODIFIED");
        }

        private void OnChartCbotStarted(
            ChartRobotStartedEventArgs args)
        {
            RefreshCbotPanelAfterLifecycleEvent(
                "CBOT STARTED");
        }

        private void OnChartCbotStopped(
            ChartRobotStoppedEventArgs args)
        {
            RefreshCbotPanelAfterLifecycleEvent(
                "CBOT STOPPED");
        }

        private void RefreshCbotPanelAfterLifecycleEvent(
            string reason)
        {
            BeginInvokeOnMainThread(
                () =>
                {
                    try
                    {
                        RefreshCbotExecutionStateIfDue(true);
                        InvalidatePanelExecutionProtectionStateCache();
                        RequestPanelContentRefresh();
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP cBot lifecycle panel refresh failed ({0}): {1}",
                            reason ?? "CBOT",
                            ex.Message);
                    }
                });
        }
    }
}
