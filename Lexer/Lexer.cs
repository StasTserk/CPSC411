using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CPSC411.Exceptions;

namespace CPSC411.Lexer
{
    public class Lexer
    {
        /// <summary>
        /// Dictionary containing a pattern for matching a token string and the delegate
        /// that converts the text of the token to an IToken object
        /// </summary>
        private readonly IDictionary<string, TokenParser> _ruleDictionary;

        /// <summary>
        /// Private collection of all tokens that have been parsed in so far
        /// </summary>
        private readonly ICollection<IToken> _tokens;

        /// <summary>
        /// Pattern for matching single line comments
        /// </summary>
        private readonly string _slcPattern = @"\%[^\n]*\n";

        /// <summary>
        /// Mode determining wether or not the lexer should print to console
        /// </summary>
        private readonly LexerLoggingMode _loggingMode;

        /// <summary>
        /// Definition of method signiature for generating a token
        /// </summary>
        /// <param name="tokenString">String representing the token</param>
        /// <returns>IToken object representing the token that was parsed out</returns>
        public delegate IToken TokenParser(string tokenString);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingMode">Wether or not the lexer should print to console as it works.</param>
        public Lexer(LexerLoggingMode loggingMode)
        {
            _loggingMode = loggingMode;
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
                StripSingleLineComments(sourceString));
        }

        /// <summary>
        /// Private function that removes all single line comments.
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        private string StripSingleLineComments(string sourceString)
        {
            var slcRegex = new Regex(_slcPattern, RegexOptions.Multiline);

            sourceString = slcRegex.Replace(sourceString, "\n");

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
        /// <param name="lineNumber">Line number on which the token appears</param>
        /// <returns>string not containing the token that was parsed out</returns>
        public string ParseToken(string tokenString, int lineNumber)
        {
            // First we try to pull out a rule for generating a token by matching the pattern
            // that is used as the key to the token string
            var rule = _ruleDictionary.FirstOrDefault(
                r => Regex.IsMatch(tokenString, r.Key, RegexOptions.Singleline));

            // if we fail to find a rule, that means we encountered a token we do not recognize
            if (rule.Value == null)
            {
                throw new InvalidTokenException($"Invalid token encountered at line {lineNumber} -  '{tokenString.Split(' ')[0]}'");
            }

            // assuming we did encounter a recognizable token, we generate the token by invoking the
            // rule's value - the delegate that produces the token given the token's substring
            var token = rule.Value(Regex.Match(tokenString, rule.Key, RegexOptions.Singleline).Value);
            token.LineNumber = lineNumber;
            _tokens.Add(token);

            // after adding the token, we possibly log that we added to token and then pop
            // the characters we consumed off of the string.
            Log($" *** Adding {token.StringRepresentation}");
            return Regex.Replace(tokenString, rule.Key, "").Trim();
        }

        /// <summary>
        /// Simple logging function that only writes to the console if the lexer is set in verbose loggingMode
        /// </summary>
        /// <param name="s">string that is ti be logged</param>
        private void Log(string s)
        {
            if (_loggingMode == LexerLoggingMode.Verbose)
            {
                Console.WriteLine(s);
            }
        }

        /// <summary>
        /// Returns the list of tokens that have been parsed out so far.
        /// </summary>
        /// <returns>the list of tokens that have been parsed out.</returns>
        public IEnumerable<IToken> GetTokens()
        {
            return _tokens;
        }

        /// <summary>
        /// Enum representing the logging 'volume' of the lexer
        /// </summary>
        public enum LexerLoggingMode
        {
            None,
            Verbose
        }
    }
}
