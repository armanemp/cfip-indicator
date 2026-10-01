using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private bool TryBuildParallelScenarioGeometry(
            int closedM5,
            int direction,
            out ParallelScenarioGeometry geometry)
        {
            geometry = null;

            if (_m5Bars == null ||
                closedM5 < 30 ||
                closedM5 >= _m5Bars.Count ||
                (direction != 1 &&
                 direction != -1))
                return false;

            if (_parallelGeometryCacheM5 != closedM5)
            {
                _parallelGeometryCacheM5 = closedM5;
                _parallelGeometryCache.Clear();
            }

            if (_parallelGeometryCache.TryGetValue(
                    direction,
                    out geometry))
                return geometry != null;

            ExecutionModel execution =
                BuildExecutionModel(
                    closedM5,
                    direction);

            if (execution == null)
                return false;

            double atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (!IsFinitePositive(atr))
                return false;

            double entry =
                IsFinitePositive(execution.IdealEntry)
                    ? execution.IdealEntry
                    : execution.ActualEntry;

            if (!IsFinitePositive(entry))
                return false;

            entry =
                NormalizePrice(entry);

            double stop =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    entry,
                    atr,
                    out _,
                    out _);

            if (!IsValidStop(
                    direction,
                    entry,
                    stop))
            {
                stop =
                    IsValidStop(
                        direction,
                        entry,
                        execution.Invalidation)
                        ? execution.Invalidation
                        : direction == 1
                            ? entry - atr * FallbackSlAtr
                            : entry + atr * FallbackSlAtr;

                stop =
                    NormalizePrice(stop);
            }

            if (!IsValidStop(
                    direction,
                    entry,
                    stop))
                return false;

            double risk =
                Math.Abs(
                    entry - stop);

            if (!IsFinitePositive(risk))
                return false;

            geometry =
                new ParallelScenarioGeometry
                {
                    Direction = direction,
                    Atr = atr,
                    Entry = entry,
                    Stop = stop,
                    Risk = risk,
                    Execution = execution
                };

            _parallelGeometryCache[direction] =
                geometry;

            return true;
        }
    }
}
