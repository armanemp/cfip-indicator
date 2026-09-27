using System;

namespace cAlgo
{
    internal sealed class DecisionEngine
    {
        private readonly DecisionEvaluator _evaluator;

        public DecisionEngine()
        {
            _evaluator = new DecisionEvaluator();
        }

        public Decision Evaluate(DecisionInputSnapshot input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return _evaluator.Evaluate(input);
        }
    }
}
