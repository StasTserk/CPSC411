namespace CPSC411.Lexer
{
    public class Token : IToken
    {
        public string StringRepresentation { get; set; }
        public int LineNumber { get; set; }
    }
}