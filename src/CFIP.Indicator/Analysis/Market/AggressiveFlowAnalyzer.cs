// ============================================================================
// CFIP Indicator — AggressiveFlowAnalyzer.cs
// Single owner for realtime aggressive-flow proxy derived from cTrader ticks.
// IMPORTANT: cTrader Algo Tick exposes Time/Bid/Ask only; it does not expose
// executed trade size. This component therefore never labels tick count as
// traded volume. A true tape/trade-volume feed must be injected separately.
// ============================================================================

using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class AggressiveFlowSnapshot
    {
        public static readonly AggressiveFlowSnapshot Empty =
            new AggressiveFlowSnapshot(0, 0, 0, 0);

        public AggressiveFlowSnapshot(
            long buyTicks,
            long sellTicks,
            long neutralTicks,
            long sequence)
        {
            BuyTicks = Math.Max(0, buyTicks);
            SellTicks = Math.Max(0, sellTicks);
            NeutralTicks = Math.Max(0, neutralTicks);
            Sequence = Math.Max(0, sequence);
        }

        public long BuyTicks { get; }
        public long SellTicks { get; }
        public long NeutralTicks { get; }
        public long Sequence { get; }

        public long ClassifiedTicks => BuyTicks + SellTicks;

        public double BuyShare
        {
            get
            {
                long total = ClassifiedTicks;
                return total > 0
                    ? (double)BuyTicks / total
                    : 0.0;
            }
        }

        public double SellShare
        {
            get { return 1.0 - BuyShare; }
        }
    }

    internal sealed class AggressiveFlowAnalyzer
    {
        private const int MaximumRetainedTicks = 4096;

        private readonly Queue<int> _classifications =
            new Queue<int>(MaximumRetainedTicks);

        private Ticks _ticks;
        private double _previousBid;
        private double _previousAsk;
        private bool _hasPreviousQuote;
        private long _buyTicks;
        private long _sellTicks;
        private long _neutralTicks;
        private long _sequence;

        public AggressiveFlowSnapshot Snapshot { get; private set; } =
            AggressiveFlowSnapshot.Empty;

        public void Attach(Ticks ticks)
        {
            if (ReferenceEquals(_ticks, ticks))
                return;

            Detach();

            _ticks = ticks;
            if (_ticks == null)
                return;

            try
            {
                int available = Math.Min(512, _ticks.Count);

                // Last(0) is newest; process the bounded history oldest -> newest.
                for (int i = available - 1; i >= 0; i--)
                    ProcessTick(_ticks.Last(i));

                _ticks.Tick += OnTick;
            }
            catch
            {
                Reset();
            }
        }

        public void Detach()
        {
            if (_ticks != null)
                _ticks.Tick -= OnTick;

            _ticks = null;
        }

        private void OnTick(TicksTickEventArgs args)
        {
            if (args == null ||
                args.Ticks == null)
                return;

            ProcessTick(args.Ticks.LastTick);
        }

        private void ProcessTick(Tick tick)
        {
            double bid = tick.Bid;
            double ask = tick.Ask;

            if (!IsFinitePositive(bid) ||
                !IsFinitePositive(ask) ||
                ask < bid)
                return;

            int classification = Classify(
                bid,
                ask);

            if (!_hasPreviousQuote)
                classification = 0;

            _previousBid = bid;
            _previousAsk = ask;
            _hasPreviousQuote = true;

            if (_classifications.Count >= MaximumRetainedTicks)
                RemoveOldest();

            _classifications.Enqueue(classification);

            if (classification > 0)
                _buyTicks++;
            else if (classification < 0)
                _sellTicks++;
            else
                _neutralTicks++;

            _sequence++;

            Snapshot =
                new AggressiveFlowSnapshot(
                    _buyTicks,
                    _sellTicks,
                    _neutralTicks,
                    _sequence);
        }

        private int Classify(
            double bid,
            double ask)
        {
            double midpoint =
                (bid + ask) * 0.5;

            double previousMidpoint =
                (_previousBid + _previousAsk) * 0.5;

            if (midpoint > previousMidpoint)
                return 1;

            if (midpoint < previousMidpoint)
                return -1;

            // Tie-break only when the quote itself moves. This remains a
            // quote-direction proxy, never an executed-volume claim.
            if (ask > _previousAsk)
                return 1;

            if (bid < _previousBid)
                return -1;

            return 0;
        }

        private void RemoveOldest()
        {
            int oldest = _classifications.Dequeue();

            if (oldest > 0)
                _buyTicks = Math.Max(0, _buyTicks - 1);
            else if (oldest < 0)
                _sellTicks = Math.Max(0, _sellTicks - 1);
            else
                _neutralTicks = Math.Max(0, _neutralTicks - 1);
        }

        private void Reset()
        {
            _classifications.Clear();
            _previousBid = 0;
            _previousAsk = 0;
            _hasPreviousQuote = false;
            _buyTicks = 0;
            _sellTicks = 0;
            _neutralTicks = 0;
            _sequence = 0;
            Snapshot = AggressiveFlowSnapshot.Empty;
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
