using System.Collections.Generic;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.Lexer;

namespace CPSC411.RDParser
{
    /// <summary>
    /// Recursive descent parsing algorithm implmenetation
    /// </summary>
    public class RecursiveDescentParser
    {
        private readonly ICollection<ParsingRule> _rules;
        private IList<IToken> _tokens;

        public bool HasTokensRemaining => _tokens.Any();
        public IToken NextToken => _tokens.First();

        public RecursiveDescentParser()
        {
            _tokens = new List<IToken>();
            _rules = new List<ParsingRule>();
        }

        /// <summary>
        /// Generates the AST based on the current token list
        /// </summary>
        /// <returns>Root node of the generated AST</returns>
        public Node ParseTokens()
        {
            return _rules.First().Invoke(_tokens);
        }

        /// <summary>
        /// Sets the list of tokens to be parsed by the RDP
        /// </summary>
        /// <param name="tokens">List of tokens to be added</param>
        public void SetTokens(IEnumerable<IToken> tokens)
        {
            _tokens = tokens.ToList();
        }

        /// <summary>
        /// Invokes a parsing rule
        /// </summary>
        /// <param name="ruleName">Name of the rule that is to be invoked</param>
        /// <returns>Node generating by the evaluator of the invoked rule</returns>
        public Node InvokeRule(string ruleName)
        {
            var node = _rules.First(r => r.Name == ruleName).Invoke(_tokens);
            return node;
        }

        /// <summary>
        /// Adds a parsing rule to the recursive descent parser.
        /// If the rule with the name already exists, it adds another production to said rule.
        /// </summary>
        /// <param name="name">Name of the rule</param>
        /// <param name="decider">Function to evaluate if we can apply the rule</param>
        /// <param name="evaluator">Function that generates an AST node</param>
        /// <returns>Self for chaining purposes</returns>
        public RecursiveDescentParser AddRule(string name, ProductionDecider decider, ProductionEvaluator evaluator)
        {
            var rule = _rules.FirstOrDefault(r => r.Name == name);
            if (rule == null)
            {
                rule = new ParsingRule(this)
                {
                    Name = name
                };
                _rules.Add(rule);
            }
            rule.AddProduction(decider, evaluator);

            return this;
        }

        /// <summary>
        /// Checks that a specific token type is the next token to be consumed. Does not consume the token.
        /// </summary>
        /// <param name="tokenType">The token type that is to be consumed</param>
        /// <returns>Wether or not the next token matches the token type</returns>
        public bool TryMatch(TokenType tokenType)
        {
            return _tokens.First().Type == tokenType;
        }

        /// <summary>
        /// Checks that a named rule can match the current next token to be consumed
        /// </summary>
        /// <param name="ruleName">Name of the rule to check</param>
        /// <returns>Wether the rule can match the next token.</returns>
        public bool TryMatch(string ruleName)
        {
            return _rules.First(r => r.Name == ruleName).CanMatch();
        }

        /// <summary>
        /// Tries to consume a terminal token of a given type
        /// </summary>
        /// <param name="tokenType">Type of token to be consumed</param>
        /// <returns>Node representing the terminal that was consumed</returns>
        public Node TryConsumeToken(TokenType tokenType)
        {
            // check if we the token we expect to consume exists
            if (!TryMatch(tokenType))
            {
                throw new UnexpectedTokenException(
                    $"Unexpected token '{_tokens.FirstOrDefault()?.Type}' on line {_tokens.FirstOrDefault()?.LineNumber}, Expected '{tokenType}'");
            }

            // remember the token we are to consume for the next step and remove it from the list
            var removedToken = _tokens[0];
            _tokens.RemoveAt(0);

            // build the node and return it
            return new Node
            {
                Contents = $"{removedToken.StringRepresentation}",
                IsTerminal = true,
                Data = removedToken.Contents,
                Type = NodeType.Terminal,
                TerminalTokenType = removedToken.Type
            };
        }
    }
}
