﻿namespace CPSC411.Lexer
{
    /// <summary>
    /// Concrete class representing a generic token.
    /// In the future may be expanded to be more specialized of different types of tokens.
    /// </summary>
    public class Token : IToken
    {
        public string StringRepresentation => string.IsNullOrEmpty(Contents) ? $"{Type}" : $"{Type}[{Contents}]";
        public int LineNumber { get; set; }
        public string Contents { get; set; }
        public TokenType Type { get; set; }
    }
}