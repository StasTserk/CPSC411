using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Add a rule for converting a regex pattern to a token
        /// Order in which this is provided is important, with more specific
        /// rules needing to be provided before more general ones.
        /// 
        /// Returns self to allow for chaining.
        /// </summary>
        /// <param name="pattern">Regex pattern that needs to be matched</param>
        /// <param name="rule">Function that generates a token based on the input</param>
        /// <returns></returns>
        public Lexer AddRule(string pattern, TokenParser rule)
        {
            _ruleDictionary.Add("^" + pattern, rule);

            // for chaining
            return this;
        }

        /// <summary>
        /// Strips all comments from a source string starting with single line comments
        /// Then all multiline comments. This is to allow for single line comments to take
        /// precedence over multi line comments.
        /// 
        /// Currently the expected behaviour is: slc take absolute precedence over everything
        /// After, block commens are removed, respecting any nested block comments as being
        /// part of the outer comment. (ie comments are not necessarily from the first 
        /// "open comment" token to the first "close comment" token. Instead, comments
        /// can nest, such that 2 consecutive "open" tokens need two consecutive "close"
        /// tokens for the text to be no longer omitted.
        /// 
        /// removing comments from the text preserves newline count.
        /// </summary>
        /// <param name="sourceString">Original string containing some comments.</param>
        /// <returns>Same source code string containing no comments.</returns>
        public string StripComments(string sourceString)
        {
            return StripMultiLineComments(
                StripSingleLineComments(sourceString)).Trim();
        }

        /// <summary>
        /// Private function that removes all single line comments.
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
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
            var commentDepth = 0;
            var commentStart = -1;
            var index = 1;
            var newlineCount = 0;


            while (index < sourceString.Length)
            {
                switch (sourceString[index])
                {
                    case '*':
                        if (sourceString[index - 1] == '/') // looks like it is
                        {
                            commentDepth ++;

                            if (commentDepth == 1)
                            {
                                // start of a comment block
                                commentStart = index;
                            }
                        }
                        break;
                    case '/':
                        if (sourceString[index - 1] == '*') // looks like it is
                        {
                            commentDepth --;
                            switch (commentDepth)
                            {
                                case -1: // we haven't delved into a comment block yet so we
                                    commentDepth = 0; // skipped past a divide multiply together
                                    break;
                                case 0:
                                    // found the end of a comment block
                                    // need to remove the substring of the comments
                                    var builder = new StringBuilder();
                                    builder.Append(sourceString.Substring(0, commentStart-1));
                                    builder.Append('\n', newlineCount);
                                    builder.Append(" " + sourceString.Substring(index + 1));
                            
                                    // 
                                    sourceString = builder.ToString();
                                    index = commentStart+1;
                                    break;
                            }
                        }
                        break;
                    case '\n': // we want to preserve line count in our file
                        if (commentDepth > 0)
                        {
                            newlineCount ++;
                        }
                        break;
                }
                index ++;
            }

            // at this point we have traversed the entire string and should have no problems
            // it is possible we are still in a comment block.
            // This is currently undefined behaviour so we'll ignore that case for now.
            return sourceString;
        }

        /// <summary>
        /// Parses the first token that it encounters out of the given string,
        /// Stores it in it's list of tokens and returns the string without
        /// the token that was parsed out.
        /// </summary>
        /// <param name="tokenString">string that is to be tokenized</param>
        /// <returns>string not containing the token that was parsed out</returns>
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

        /// <summary>
        /// Returns the list of tokens that have been parsed out so far.
        /// </summary>
        /// <returns>the list of tokens that have been parsed out.</returns>
        public IEnumerable<IToken> GetTokens()
        {
            return _tokens;
        }
    }
}
