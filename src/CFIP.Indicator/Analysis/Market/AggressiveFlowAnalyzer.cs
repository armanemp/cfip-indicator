using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal readonly struct AggressiveFlowSample
    {
        internal AggressiveFlowSample(DateTime utc, int direction)
        {
            Utc = utc;
            Direction = direction;
        }

        internal DateTime Utc { get; }
        internal int Direction { get; }
    }

    /// <summary>
    /// Single owner for the realtime aggressive-flow proxy.
    /// cTrader Tick exposes quote prices, not executed trade size; therefore
    /// this class intentionally reports directional tick activity, never
    /// broker/exchange executed volume.
    /// </summary>
    internal sealed class AggressiveFlowAnalyzer
    {
        internal const int MaxSamples = 4096;
        private static readonly TimeSpan Window = TimeSpan.FromSeconds(30);

        private readonly Queue<AggressiveFlowSample> _samples = new Queue<AggressiveFlowSample>(MaxSamples);
        private double _previousMid;
        private bool _hasPreviousMid;
        private long _sequence;

        internal void Reset()
        {
            _samples.Clear();
            _previousMid = 0;
            _hasPreviousMid = false;
            _sequence = 0;
        }

        internal AggressiveFlowSnapshot Snapshot(DateTime nowUtc)
        {
            nowUtc = nowUtc.ToUniversalTime();
            Trim(nowUtc);
            int buy = 0, sell = 0, neutral = 0;
            foreach (var sample in _samples)
            {
                if (sample.Direction > 0) buy++;
                else if (sample.Direction < 0) sell++;
                else neutral++;
            }

            int total = buy + sell + neutral;
            return new AggressiveFlowSnapshot(
                buy, sell, neutral, total, ++_sequence, nowUtc);
        }

        internal void ProcessTick(Tick tick)
        {
            double bid = tick.Bid;
            double ask = tick.Ask;
            if (!IsFinitePositive(bid) || !IsFinitePositive(ask) || ask < bid)
                return;

            double mid = (bid + ask) * 0.5;
            int direction = 0;

            if (_hasPreviousMid)
            {
                if (mid > _previousMid)
                    direction = 1;
                else if (mid < _previousMid)
                    direction = -1;
            }

            _previousMid = mid;
            _hasPreviousMid = true;

            _samples.Enqueue(
                new AggressiveFlowSample(
                    tick.Time.ToUniversalTime(),
                    direction));

            while (_samples.Count > MaxSamples)
                _samples.Dequeue();

            Trim(tick.Time.ToUniversalTime());
        }

        private void Trim(DateTime nowUtc)
        {
            DateTime cutoff = nowUtc.ToUniversalTime() - Window;
            while (_samples.Count > 0 &&
                   _samples.Peek().Utc < cutoff)
                _samples.Dequeue();
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

    }

    internal readonly struct AggressiveFlowSnapshot
    {
        internal AggressiveFlowSnapshot(
            int buyTicks,
            int sellTicks,
            int neutralTicks,
            int totalTicks,
            long sequence,
            DateTime observedUtc)
        {
            BuyTicks = buyTicks;
            SellTicks = sellTicks;
            NeutralTicks = neutralTicks;
            TotalTicks = totalTicks;
            Sequence = sequence;
            ObservedUtc = observedUtc;
        }

        internal int BuyTicks { get; }
        internal int SellTicks { get; }
        internal int NeutralTicks { get; }
        internal int TotalTicks { get; }
        internal long Sequence { get; }
        internal DateTime ObservedUtc { get; }

        internal double BuyShare =>
            TotalTicks > 0 ? (double)BuyTicks / TotalTicks : 0;

        internal double SellShare =>
            TotalTicks > 0 ? (double)SellTicks / TotalTicks : 0;

        internal bool IsFresh(DateTime nowUtc, TimeSpan maxAge)
        {
            if (ObservedUtc == DateTime.MinValue || maxAge < TimeSpan.Zero)
                return false;

            TimeSpan age = nowUtc.ToUniversalTime() - ObservedUtc.ToUniversalTime();
            return age >= TimeSpan.Zero && age <= maxAge;
        }
    }
}
