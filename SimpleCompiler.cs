using System;
using System.IO;
using CPSC411.Lexer;

namespace CPSC411
{
    class SimpleCompiler
    {
        static void Main(string[] args)
        {
            var sourceString = GetSourceFileText(args);

            var lexer = new Lexer.Lexer();

            AddRules(lexer);
            sourceString = lexer.StripComments(sourceString);
            var lineCount = 0;

            foreach (var line in sourceString.Split('\n'))
            {
                var subLine = line.Trim();
                while (subLine.Length != 0)
                {
                    //Console.WriteLine(sourceString);
                    subLine = lexer.ParseToken(subLine, lineCount);
                }
                lineCount ++;
            }

            foreach (var token in lexer.GetTokens())
            {
                Console.WriteLine($"Line {token.LineNumber} - [{token.StringRepresentation}], ");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Ingests the file based on the passed command line arguments.
        /// If no filename is specified, picks a file randomly out of the ones given.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>String representing the complete text of the file.</returns>
        private static string GetSourceFileText(string [] args)
        {
            var fileName = "";
            if (args.Length != 0)
            {
                fileName = $"SampleFiles/{args[0]}";
            }
            else
            {
                var files = Directory.GetFiles("SampleFiles/");
                var file = files[new Random().Next(files.Length)];
                fileName = file.ToString();
            }

            Console.WriteLine($"Reading {fileName}");
            return System.IO.File.ReadAllText(fileName).Trim();
        }

        /// <summary>
        /// Adds all of the rules associated with assignment 1
        /// </summary>
        /// <param name="lexer">Lexer that is going to be performing the analysis</param>
        private static void AddRules(Lexer.Lexer lexer)
        {
            lexer.AddRule(@"if\b", 
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
                .AddRule(@"[a-zA-Z][a-zA-Z0-9_]*", 
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
