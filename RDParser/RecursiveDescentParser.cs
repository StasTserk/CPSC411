using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.Lexer;

namespace CPSC411.RDParser
{

    public delegate Node ProductionEvaluator(RecursiveDescentParser parser);

    public delegate bool ProductionDecider(RecursiveDescentParser parser);

    public class RecursiveDescentParser
    {
        private readonly ICollection<ParsingRule> _rules;
        private readonly IList<IToken> _tokens;
        private int _indent = 0;

        public RecursiveDescentParser(IList<IToken> tokens)
        {
            _tokens = tokens;
            _rules = new List<ParsingRule>();
        }

        public Node ParseTokens()
        {
            return _rules.First().Invoke(_tokens);
        }

        public Node InvokeRule(string ruleName)
        {
            
            LogIndented($"Invoking {ruleName}...");
            _indent++;
            var node = _rules.First(r => r.Name == ruleName).Invoke(_tokens);
            _indent--;
            LogIndented($"Done invoking {ruleName}");
            return node;
        }

        private void LogIndented(string s)
        {
            //Console.WriteLine($"{new string(' ', _indent)}{s}");
        }

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

        public bool TryMatch(TokenType tokenType)
        {
            return _tokens.First().Type == tokenType;
        }

        public bool TryMatch(string ruleName)
        {
            return _rules.First(r => r.Name == ruleName).CanMatch();
        }

        public Node TryConsumeToken(TokenType tokenType)
        {
            LogIndented($"Consuming {tokenType}");
            if (!TryMatch(tokenType))
                throw new UnexpectedTokenException(
                    $"Unexpected token '{_tokens.FirstOrDefault()?.Type}' on line {_tokens.FirstOrDefault()?.LineNumber}, Expected '{tokenType}'");
            var removedToken = _tokens[0];
            _tokens.RemoveAt(0);
            return new Node {Contents = $"{removedToken.StringRepresentation}"};
        }
    }
}
