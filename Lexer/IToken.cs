namespace CPSC411.Lexer
{
    public interface IToken
    {
        string StringRepresentation { get; }
        int LineNumber { get; set; }
    }
}