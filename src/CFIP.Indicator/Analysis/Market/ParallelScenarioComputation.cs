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
                _parallelExecutionModelCache.Clear();
                _parallelPreviewCache.Clear();
            }

            if (_parallelGeometryCache.TryGetValue(
                    direction,
                    out geometry))
                return geometry != null;

            ExecutionModel execution;

            if (!TryGetParallelExecutionModel(
                    closedM5,
                    direction,
                    out execution))
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
                if (IsValidStop(
                        direction,
                        entry,
                        execution.Invalidation))
                {
                    stop = execution.Invalidation;
                }
                else
                {
                    StructuralStopGeometrySnapshot fallbackGeometry =
                        StructuralStopGeometryRule.EvaluateFallback(
                            direction,
                            entry,
                            atr,
                            FallbackSlAtr,
                            Symbol.TickSize,
                            Symbol.Digits);

                    if (!fallbackGeometry.IsValid)
                        return false;

                    stop = fallbackGeometry.Stop;
                }
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
                    ExecutionQuality = execution.Quality
                };

            _parallelGeometryCache[direction] =
                geometry;

            return true;
        }
        private bool TryGetParallelExecutionModel(
            int closedM5,
            int direction,
            out ExecutionModel execution)
        {
            execution = null;

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
                _parallelExecutionModelCache.Clear();
                _parallelPreviewCache.Clear();
            }

            if (_parallelExecutionModelCache.TryGetValue(
                    direction,
                    out execution))
                return execution != null;

            execution =
                BuildExecutionModel(
                    closedM5,
                    direction);

            if (execution == null)
                return false;

            _parallelExecutionModelCache[direction] =
                execution;

            return true;
        }

    }
}
