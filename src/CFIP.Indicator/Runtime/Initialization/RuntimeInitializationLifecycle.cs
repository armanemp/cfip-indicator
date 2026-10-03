// CFIP Indicator — RuntimeInitializationLifecycle.cs
using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void QueueStartupCalculationSeed()
        {
            if (_startupCalculationSeedDone ||
                _startupCalculationSeedQueued ||
                !_initializationReady)
                return;

            _startupCalculationSeedQueued = true;

            BeginInvokeOnMainThread(
                () =>
                {
                    _startupCalculationSeedQueued = false;

                    if (_runtimeShuttingDown ||
                        !_initializationReady ||
                        _startupCalculationSeedDone)
                        return;

                    try
                    {
                        RunStartupCalculationSeed();
                    }
                    catch (Exception ex)
                    {
                        HandleRuntimeFault(
                            ex,
                            -1,
                            "STARTUP CALCULATION SEED");
                    }
                });
        }

        protected override void OnDestroy()
                                {
                                    _runtimeShuttingDown = true;

                                    try
                                    {
                                        Timer.Stop();
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP OnDestroy timer stop failed: {0}",
                                            ex.ToString());
                                    }

                                    try
                                    {
                                        UnsubscribeCbotChartLifecycleEvents();
                                        UnhookHistoricalBarsEvents();
                                        Positions.Opened -= OnPositionOpened;
                                        Positions.Closed -= OnPositionClosed;
                                        Positions.Modified -= OnPositionModified;
                                        Account.Switched -= OnAccountSwitched;
                                        Account.Switched -= OnOutcomeMemoryAccountSwitched;
                                        PendingOrders.Created -= OnPendingOrderCreated;
                                        PendingOrders.Modified -= OnPendingOrderModified;
                                        PendingOrders.Filled -= OnPendingOrderFilled;
                                        PendingOrders.Cancelled -= OnPendingOrderCancelled;
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP OnDestroy event unsubscription failed: {0}",
                                            ex.ToString());
                                    }

                                    DisposeEconomicNewsClient();

                                    PersistOutcomeHistory();
                                    PersistPortableMemorySnapshot();
                                    FlushBufferedPersistenceOnShutdown();

                                    RemoveAllChartObjects();
                                    RemovePanel();
                                    _panelAlertHistory.Clear();
                                    _panelAlertMessageRows.Clear();
                                    _panelAlertMessageStack = null;
                                    _alertDeliveryQueue.ClearPendingAlerts();
                                    base.OnDestroy();
                                }

    }
}
