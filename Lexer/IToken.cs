namespace CPSC411.Lexer
{
    /// <summary>
    /// Interface representing a generic token
    /// </summary>
    public interface IToken
    {
        string StringRepresentation { get; }
        int LineNumber { get; set; }

        TokenType TokenType { get; set; }
    }
}