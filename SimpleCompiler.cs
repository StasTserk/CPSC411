using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.RDParser;
using CPSC411.StackTranslator;

namespace CPSC411
{
    internal class SimpleCompiler
    {
        private static void Main(string[] args)
        {
            // Loads input string from source file
            var sourceString = GetSourceFileText(args);

            // initializes lexer and RDP classes
            var lexer = new Lexer.Lexer(Lexer.Lexer.LexerLoggingMode.None);
            var rdp = new RecursiveDescentParser();

            // adds rules to lexer and parser
            RulesModule.AddLexerRules(lexer);
            RulesModule.AddRdpRules(rdp);

            try
            {
                Console.WriteLine("Parsing tokens from source...");
                // parse tokens from source using lexer
                ParseTokens(sourceString, lexer);
                // generate AST from token list
                Console.WriteLine("\n\nGenerating AST...");
                Node rootNode = GenerateAst(rdp, lexer);

                Console.WriteLine();

                // generate AST visualization and compiled code
                GenerateGraphvis(rootNode);
                GenerateCompiledCode(rootNode);
                Console.WriteLine("Success!");
            }
            catch (InvalidTokenException e)
            {
                // Handle token parsing issues
                Console.WriteLine($"Parse Error: {e.Message}");
            }
            catch (UnexpectedTokenException e)
            {
                // unexpected token / syntax error
                Console.WriteLine(e.Message);
            }
            
        }

        /// <summary>
        /// Generates the abstract syntax tree based on the token list of the lexer
        /// </summary>
        /// <param name="rdp">Parser that will generate the AST</param>
        /// <param name="lexer">Lexer containing the token list</param>
        /// <returns></returns>
        private static Node GenerateAst(RecursiveDescentParser rdp, Lexer.Lexer lexer)
        {
            rdp.SetTokens(lexer.GetTokens());
            var parentNode = rdp.ParseTokens();
            if (rdp.HasTokensRemaining)
            {
                throw new UnexpectedTokenException(
                    $"Reached end of parse with tokens remaining! {rdp.NextToken.StringRepresentation} on line {rdp.NextToken.LineNumber}");
            }
            return parentNode;
        }

        /// <summary>
        /// Generates the stack machine code defined in the assignment description
        /// </summary>
        /// <param name="parentNode">Root node of the AST</param>
        private static void GenerateCompiledCode(Node parentNode)
        {
            Console.WriteLine("Writing compiled code...");
            File.WriteAllText("StackCode.txt",
                new StackMachineConverter().ConvertAst(parentNode));
        }

        /// <summary>
        /// Generates the graphviz representation of the AST
        /// </summary>
        /// <param name="parentNode">root node of the AST</param>
        private static void GenerateGraphvis(Node parentNode)
        {
            Console.WriteLine("Writing to graphviz format...");
            File.WriteAllText("graphviz.dot", 
                new GraphVizGenerator().GenerateGraphString(parentNode));
        }

        private static void ParseTokens(string sourceString, Lexer.Lexer lexer)
        {
            // we are treating comments as preprocessor commands so we strip them here
            sourceString = lexer.StripComments(sourceString);

            // we are now ready to parse the tokens from the text.
            var lineCount = 1; // used to track the line number, 1 indexed
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

        /// <summary>
        /// Ingests the file based on the passed command line arguments.
        /// If no filename is specified, picks a file randomly out of the ones given.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>String representing the complete text of the file.</returns>
        private static string GetSourceFileText(IReadOnlyList<string> args)
        {
            string fileName;
            if (args.Count != 0)
            {
                fileName = $"{args[0]}";
            }
            else
            {
                var files = Directory.GetFiles("./").Where(f => f.EndsWith(".txt")).ToList();
                var file = files[new Random().Next(files.Count)];
                fileName = file;
            }

            Console.WriteLine($"Reading {fileName}");
            return File.ReadAllText(fileName).Trim();
        }
    }
}