using System.Collections.Generic;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.Lexer;

namespace CPSC411.RDParser
{
    /// <summary>
    /// represents a function that can generate a node given the current token list
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public delegate Node ProductionEvaluator(RecursiveDescentParser parser);

    /// <summary>
    /// represents a function that can check if a parsing rule applies to the current token list
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public delegate bool ProductionDecider(RecursiveDescentParser parser);

    /// <summary>
    /// Class representing a parsing rule and it's associated production
    /// </summary>
    public class ParsingRule
    {
        /// <summary>
        /// Parent parser of the rule
        /// </summary>
        private readonly RecursiveDescentParser _parent;

        /// <summary>
        /// Rule Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Collecting that contains the productions as a pair of produciton decider and production evaluator functions
        /// </summary>
        private readonly IDictionary<ProductionDecider, ProductionEvaluator> _productions;

        /// <summary>
        /// Constructor for ParsingRule Class
        /// </summary>
        /// <param name="parent">Recursive Descent Parser to which the rule belongs</param>
        public ParsingRule(RecursiveDescentParser parent)
        {
            _parent = parent;
            _productions = new Dictionary<ProductionDecider, ProductionEvaluator>();
        }

        /// <summary>
        /// Invoke the rule based on the passed set of tokens.
        /// Tries to match to one of the sub rules then generates a node based on the evaluator
        /// </summary>
        /// <param name="tokens">List of tokens to be passed</param>
        /// <returns>Node representing the output of the production</returns>
        public Node Invoke(IList<IToken> tokens)
        {
            // find the correct production
            var rule = _productions.FirstOrDefault(p => p.Key.Invoke(_parent));

            // if one is found
            if (rule.Value != null)
            {
                // generate the correct node
                return rule.Value.Invoke(_parent);
            }

            // no valid production found, so invalid syntax
            var token = tokens.FirstOrDefault();
            throw new UnexpectedTokenException($"Unexpected token '{token?.Type}' on line {token?.LineNumber}");
        }

        /// <summary>
        /// Adds a production to the rule
        /// </summary>
        /// <param name="decider">Function to evaluate if the rule applies</param>
        /// <param name="evaluator">Function to generate a node from the token list</param>
        public void AddProduction(ProductionDecider decider, ProductionEvaluator evaluator)
        {
            _productions.Add(decider, evaluator);
        }

        /// <summary>
        /// Returns wether or not the rule can match the parent parser's token list
        /// </summary>
        /// <returns>True of the next token is one that the rule can match</returns>
        public bool CanMatch()
        {
            // equivalent translation:
            // are there any production decider functions that evaluate to true 
            // based on the current tokens
            return _productions.Any(p => p.Key.Invoke(_parent));
        }
    }
}