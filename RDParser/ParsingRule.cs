using System.Collections.Generic;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.Lexer;

namespace CPSC411.RDParser
{
    public class ParsingRule
    {
        private readonly RecursiveDescentParser _parent;
        public string Name { get; set; }
        private readonly IDictionary<ProductionDecider, ProductionEvaluator> _productions;

        public ParsingRule(RecursiveDescentParser parent)
        {
            _parent = parent;
            _productions = new Dictionary<ProductionDecider, ProductionEvaluator>();
        }

        public Node Invoke(IList<IToken> tokens)
        {
            var rule = _productions.FirstOrDefault(p => p.Key.Invoke(_parent));

            if (rule.Value == null)
            {
                // invalid syntax!
                throw new UnexpectedTokenException($"Unexpected token '{tokens.First().Type}' on line {tokens.FirstOrDefault()?.LineNumber}");
            }

            return rule.Value.Invoke(_parent);
        }

        public void AddProduction(ProductionDecider decider, ProductionEvaluator evaluator)
        {
            _productions.Add(decider, evaluator);
        }

        public bool CanMatch()
        {
            return _productions.Any(p => p.Key.Invoke(_parent));
        }
    }
}