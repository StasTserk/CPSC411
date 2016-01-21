using System;
using CPSC411.Lexer;

namespace CPSC411
{
    class SimpleCompiler
    {
        static void Main(string[] args)
        {
            var sourceString = System.IO.File.ReadAllText("file.txt").Trim();

            var lexer = new Lexer.Lexer();

            AddRules(lexer);
            sourceString = lexer.StripComments(sourceString);

            while (sourceString.Length != 0)
            {
                //Console.WriteLine(sourceString);
                sourceString = lexer.ParseToken(sourceString);
            }

            foreach (var token in lexer.GetTokens())
            {
                Console.Write($"[{token.StringRepresentation}], ");
            }
            Console.WriteLine();
        }

        private static void AddRules(Lexer.Lexer lexer)
        {
            lexer.AddRule(@"if\b+", 
                s => new Token {StringRepresentation = "IF"})
                .AddRule(@"then\b", 
                s => new Token {StringRepresentation = "THEN"})
                .AddRule(@"while\b", 
                s => new Token {StringRepresentation = "WHILE"})
                .AddRule(@"do\b", 
                s => new Token {StringRepresentation = "DO"})
                .AddRule(@"input\b", 
                s => new Token {StringRepresentation = "INPUT"})
                .AddRule(@"else\b", 
                s => new Token {StringRepresentation = "ELSE"})
                .AddRule(@"begin\b", 
                s => new Token {StringRepresentation = "BEGIN"})
                .AddRule(@"end\b", 
                s => new Token {StringRepresentation = "END"})
                .AddRule(@"write\b", 
                s => new Token {StringRepresentation = "WRITE"})
                .AddRule(@"[a-zA-Z][\w]*", 
                s => new Token {StringRepresentation = $"Id({s})"})
                .AddRule(@"[\d]+", 
                s => new Token {StringRepresentation = $"Num({s})"})
                .AddRule(@"\+", 
                s => new Token {StringRepresentation = "ADD"})
                .AddRule(@":=", 
                s => new Token {StringRepresentation = "ASSIGN"})
                .AddRule(@"-", 
                s => new Token {StringRepresentation = "SUB"})
                .AddRule(@"\*", 
                s => new Token {StringRepresentation = "MUL"})
                .AddRule(@"/", 
                s => new Token {StringRepresentation = "DIV"})
                .AddRule(@"\(", 
                s => new Token {StringRepresentation = "LPAR"})
                .AddRule(@"\)", 
                s => new Token {StringRepresentation = "RPAR"})
                .AddRule(@";", 
                s => new Token {StringRepresentation = "SEMICOLON"});
        }
    }
}
