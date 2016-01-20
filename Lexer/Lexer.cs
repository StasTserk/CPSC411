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
        private readonly string _slcPattern = @"%.*\n";

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

        public string StripComments(string sourceString)
        {
            return StripMultiLineComments(
                StripSingleLineComments(sourceString)).Trim();
        }

        private string StripSingleLineComments(string sourceString)
        {
            var slcRegex = new Regex(_slcPattern);
            while (slcRegex.IsMatch(sourceString))
            {
                Console.WriteLine("Found SLC");
                sourceString = slcRegex.Replace(sourceString, "\n");
            }
            return sourceString;
        }

        private string StripMultiLineComments(string sourceString)
        {
            int commentDepth = 0;
            int commentStart = -1;
            int index = 1;



            while (index < sourceString.Length)
            {
                if (sourceString[index] == '*') // potential start of comment
                {
                    if (sourceString[index - 1] == '/') // looks like it is
                    {
                        commentDepth ++;

                        if (commentDepth == 1)
                        {
                            // start of a comment block
                            commentStart = index;
                        }
                    }
                }
                else if (sourceString[index] == '/') // possible comment close
                {
                    if (sourceString[index - 1] == '*') // looks like it is
                    {
                        commentDepth --;

                        if (commentDepth == 0)
                        {
                            // found the end of a comment block
                            // need to remove the substring of the comments
                            var stringStart = sourceString.Substring(0, commentStart-1);
                            var stringEnd = sourceString.Substring(index + 1);
                            
                            // 
                            sourceString = stringStart + " " + stringEnd;
                            index = commentStart+1;
                        }
                    }
                }
                index ++;
            }

            return sourceString;
        }

        public string ParseToken(string tokenString)
        {
            var rule = _ruleDictionary.FirstOrDefault(
                r => Regex.IsMatch(tokenString, r.Key, RegexOptions.Singleline));

            if (rule.Value == null)
            {
                throw new InvalidDataException($"Invalid token at '{tokenString.Split(' ')[0]}'");
            }

            var token = rule.Value(Regex.Match(tokenString, rule.Key, RegexOptions.Singleline).Value);
            _tokens.Add(token);
            Console.WriteLine($" *** Adding {token.StringRepresentation}");

            return Regex.Replace(tokenString, rule.Key, "").Trim();
        }

        public IEnumerable<IToken> GetTokens()
        {
            return _tokens;
        }
    }
}
