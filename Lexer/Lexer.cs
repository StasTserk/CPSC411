using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CPSC411.Lexer
{
    public class Lexer
    {
        private readonly IDictionary<string, TokenParser> _ruleDictionary;
        private readonly ICollection<IToken> _tokens;
        private string _slcPattern = @"^\%";

        private string _multiLineStartPattern = @"^\/\*";
        private string _multiLineEndPattern = @"\*\/\s*";

        public delegate IToken TokenParser(string tokenString);

        public Lexer()
        {
            _ruleDictionary = new Dictionary<string, TokenParser>();
            _tokens = new List<IToken>();
        }

        public Lexer AddRule(string pattern, TokenParser rule)
        {
            _ruleDictionary.Add("^" + pattern, rule);

            // for chaining
            return this;
        }

        public Lexer SetSingleLineCommentToken(string pattern)
        {
            _slcPattern = @"^" + pattern;
            return this;
        }

        public Lexer SetMultiLineCommentTokens(string beginPattern, string endPattern)
        {
            _multiLineStartPattern = "^" + beginPattern;
            _multiLineEndPattern = endPattern + @"\s+";

            return this;
        }

        public string ParseToken(string tokenString)
        {
            tokenString = TrimComments(tokenString);

            if (tokenString == "")
            {
                return "";
            }

            var rule = _ruleDictionary.FirstOrDefault(r => Regex.IsMatch(tokenString, r.Key, RegexOptions.Singleline));

            if (rule.Value == null)
            {
                throw new InvalidDataException($"Invalid token at {tokenString}");
            }

            var token = rule.Value(Regex.Match(tokenString, rule.Key, RegexOptions.Singleline).Value);
            _tokens.Add(token);
            Console.WriteLine($" *** Adding {token.StringRepresentation}");

            return Regex.Replace(tokenString, rule.Key, "").Trim();
        }

        private string TrimComments(string tokenString)
        {
            if (Regex.IsMatch(tokenString, _slcPattern, RegexOptions.Singleline))
            {
                Console.WriteLine(" *** Dropping single line comment");
                return (Regex.Replace(tokenString, _slcPattern + @".*\n?\s*", ""));
            }
            if (Regex.IsMatch(tokenString, _multiLineStartPattern, RegexOptions.Singleline))
            {
                Console.WriteLine(" *** Removing Multiline Comment");
                return Regex.Replace(tokenString, $"{_multiLineStartPattern}.*{_multiLineEndPattern}", "", RegexOptions.Singleline);
            }
            return tokenString;
        }

        public IEnumerable<IToken> GetTokens()
        {
            return _tokens;
        }
    }
}
