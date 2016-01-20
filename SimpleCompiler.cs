using System;
using CPSC411.Lexer;

namespace CPSC411
{
    class SimpleCompiler
    {
        static void Main(string[] args)
        {
            var sourceString = "17x17x17x17 %single line comment\n  \n \nif test :=4 /* some multiline \n comments */ \n\n4 + 4";

            var lexer = new Lexer.Lexer();

            AddRules(lexer);

            while (sourceString.Length != 0)
            {
                Console.WriteLine(sourceString);
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
            lexer.AddRule(@"if[\s]+", s => new Token {StringRepresentation = "IF"})
                .AddRule(@"then[\s]+", s => new Token {StringRepresentation = "THEN"})
                .AddRule(@"while[\s]+", s => new Token {StringRepresentation = "WHILE"})
                .AddRule(@"do[\s]+", s => new Token {StringRepresentation = "DO"})
                .AddRule(@"input[\s]+", s => new Token {StringRepresentation = "INPUT"})
                .AddRule(@"else[\s]+", s => new Token {StringRepresentation = "ELSE"})
                .AddRule(@"begin[\s]+", s => new Token {StringRepresentation = "BEGIN"})
                .AddRule(@"end[\s]+", s => new Token {StringRepresentation = "END"})
                .AddRule(@"write[\s]+", s => new Token {StringRepresentation = "WRITE"})
                .AddRule(@"[a-zA-Z][\w]*", s => new Token {StringRepresentation = $"Id({s})"})
                .AddRule(@"[\d]+", s => new Token {StringRepresentation = $"Num({s})"})
                .AddRule(@"\+", s => new Token {StringRepresentation = "ADD"})
                .AddRule(@":=", s => new Token {StringRepresentation = "ASSIGN"})
                .AddRule(@"-", s => new Token {StringRepresentation = "SUB"})
                .AddRule(@"\*", s => new Token {StringRepresentation = "MUL"})
                .AddRule(@"/", s => new Token {StringRepresentation = "DIV"})
                .AddRule(@"\(", s => new Token {StringRepresentation = "LPAR"})
                .AddRule(@"\)", s => new Token {StringRepresentation = "RPAR"})
                .AddRule(@";", s => new Token {StringRepresentation = "SEMICOLON"});
        }
    }
}
