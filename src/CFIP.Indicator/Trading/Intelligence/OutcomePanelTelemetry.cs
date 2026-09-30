using System;

namespace cAlgo
{
    public partial class CFIPIndicator : cAlgo.API.Indicator
    {
         private string OutcomeHistoryPanelText()
                         {
                             if (!EnableOutcomeTelemetry ||
                                 _outcomeHistory == null ||
                                 _outcomeHistory.Count == 0)
                                 return "HIST 0";
 
                             int start =
                                 Math.Max(
                                     0,
                                     _outcomeHistory.Count -
                                     PanelOutcomeWindow);
 
                             int count = 0;
                             int wins = 0;
                             double totalR = 0;
                             int totalLifecycle = 0;
 
                             for (int i = start;
                                  i < _outcomeHistory.Count;
                                  i++)
                             {
                                 OutcomeObservation observation =
                                     _outcomeHistory[i];
 
                                 if (observation == null)
                                     continue;
 
                                 count++;
                                 if (observation.Profitable)
                                     wins++;
 
                                 totalR += observation.RealizedR;
                                 totalLifecycle +=
                                     observation.LifecycleBars;
                             }
 
                             if (count == 0)
                                 return "HIST 0";
 
                             double winRate =
                                 100.0 * wins / count;
 
                             return
                                 "HIST " +
                                 count +
                                 "  •  WR " +
                                 winRate.ToString("F0") +
                                 "%  •  AVG R " +
                                 (totalR / count).ToString("F2") +
                                 "  •  LIFE " +
                                 ((double)totalLifecycle / count).ToString("F1") +
                                 " M5";
                         }
 
         private string ExecutionTelemetryPanelText()
                         {
                             if (!EnableOutcomeTelemetry ||
                                 _executionTelemetryHistory == null ||
                                 _executionTelemetryHistory.Count == 0)
                                 return "BROKER TRACE 0";
 
                             int start =
                                 Math.Max(
                                     0,
                                     _executionTelemetryHistory.Count -
                                     PanelExecutionWindow);
 
                             int count = 0;
                             int rejected = 0;
                             int recovery = 0;
                             ExecutionTelemetryRecord latest = null;
 
                             for (int i = start;
                                  i < _executionTelemetryHistory.Count;
                                  i++)
                             {
                                 ExecutionTelemetryRecord record =
                                     _executionTelemetryHistory[i];
 
                                 if (record == null)
                                     continue;
 
                                 count++;
                                 latest = record;
 
                                 if (string.Equals(
                                         record.State,
                                         "REJECTED",
                                         StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(
                                         record.State,
                                         "FAILED",
                                         StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(
                                         record.State,
                                         "NULL RESULT",
                                         StringComparison.OrdinalIgnoreCase))
                                     rejected++;
 
                                 if (string.Equals(
                                         record.Path,
                                         "RECOVERY",
                                         StringComparison.OrdinalIgnoreCase))
                                     recovery++;
                             }
 
                             if (latest == null)
                                 return "BROKER TRACE 0";
 
                             return
                                 "BROKER TRACE " +
                                 count +
                                 "  •  LAST " +
                                 latest.State +
                                 " / " +
                                 latest.Path +
                                 "  •  ERR " +
                                 rejected +
                                 "  •  REC " +
                                 recovery;
                         }
 
    }
}
