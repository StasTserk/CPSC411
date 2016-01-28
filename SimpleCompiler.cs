using System;
using System.IO;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.Lexer;

namespace CPSC411
{
    class SimpleCompiler
    {
        static void Main(string[] args)
        {
            // Loads input string from source file
            var sourceString = GetSourceFileText(args);

            // initializes lexer class and adds rules for minisculus tokens
            var lexer = new Lexer.Lexer(Lexer.Lexer.LexerLoggingMode.None);
            AddRules(lexer);

            // we are treating comments as preprocessor commands so we strip them here
            sourceString = lexer.StripComments(sourceString);

            // we are now ready to parse the tokens from the text.
            var lineCount = 1; // used to track the line number, 1 indexed
            try
            {
                // Iterate through all lines of the code and extract tokens from them.
                foreach (var line in sourceString.Split('\n').Select(line => line.Trim()))
                {
                    var subLine = line;
                    while (subLine.Length != 0)
                    {
                        // this call pops off a token and returns the remaining string
                        // token that was removed is added to the lexer's token selection
                        subLine = lexer.ParseToken(subLine, lineCount);
                    }
                    // a line has been parsed, so remember we are working with the next line
                    lineCount ++;
                }

                // after tokens are extracted, print the list of tokens
                foreach (var token in lexer.GetTokens())
                {
                    Console.Write($"[{token.StringRepresentation}], ");
                }
            }
            catch (InvalidTokenException e)
            {
                // Handle token parsing issues
                Console.WriteLine($"Parse Error: {e.Message}");
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
                fileName = $"{args[0]}";
            }
            else
            {
                var files = Directory.GetFiles("./").Where(f=>f.EndsWith(".txt")).ToList();
                var file = files[new Random().Next(files.Count)];
                fileName = file;
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
                .AddRule(@"[a-zA-Z_][a-zA-Z0-9_]*", 
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
