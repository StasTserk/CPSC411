namespace CPSC411.Lexer
{
    /// <summary>
    /// Interface representing a generic token
    /// </summary>
    public interface IToken
    {
        string StringRepresentation { get; }
        int LineNumber { get; set; }
        string Contents { get; set; }
        TokenType Type { get; set; }
    }
}