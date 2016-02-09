using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.Lexer;
using CPSC411.RDParser;

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
            
            AddLexerRules(lexer);

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

            
            var rdp = new RecursiveDescentParser(lexer.GetTokens().ToList());
            AddRdpRules(rdp);
            try
            {
                var parentNode = rdp.ParseTokens();
                var gen = new GraphVizGenerator();
                Console.WriteLine(parentNode);

                Console.WriteLine();
                Console.WriteLine("Writing to graphviz format...");
                File.WriteAllText("graphviz", gen.GenerateGraphString(parentNode));
            }
            catch (UnexpectedTokenException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void AddRdpRules(RecursiveDescentParser parser)
        {
            parser.AddRule(
                name: "base",
                decider: rdp => true,
                evaluator: rdp => rdp.InvokeRule("stmt"));

            parser.AddRule( // if
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.If),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "IF Statement"};
                    node.AddChild(rdp.TryConsumeToken(TokenType.If));
                    node.AddChild(rdp.InvokeRule("expr"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Then));
                    node.AddChild(rdp.InvokeRule("stmt"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Else));
                    node.AddChild(rdp.InvokeRule("stmt"));
                    return node;
                });

            parser.AddRule( // while
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.While),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "WHILE Statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.While));
                    node.AddChild(rdp.InvokeRule("expr"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Do));
                    node.AddChild(rdp.InvokeRule("stmt"));
                    return node;
                });

            parser.AddRule( // input
                name: "stmt",
                decider: rpd => rpd.TryMatch(TokenType.Input),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "INPUT Statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Input));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Id));
                    return node;
                });

            parser.AddRule( // assign
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.Id),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Assign Statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Id));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Assign));
                    node.AddChild(rdp.InvokeRule("expr"));
                    return node;
                });

            parser.AddRule( // write
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.Write),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "WRITE Statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Write));
                    node.AddChild(rdp.InvokeRule("expr"));
                    return node;
                });

            parser.AddRule( // begin
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.Begin),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "BEGIN Statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Begin));
                    node.AddChild(rdp.InvokeRule("stmtlist"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.End));
                    return node;
                });

            parser.AddRule(
                name: "stmtlist",
                decider: rdp => rdp.TryMatch("stmt"),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Statement List" };
                    node.AddChild(rdp.InvokeRule("stmt"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Semicolon));
                    node.AddChild(rdp.InvokeRule("stmtlist"));
                    return node;
                });

            parser.AddRule(
                name: "stmtlist",
                decider: rdp => !rdp.TryMatch("stmt"),
                evaluator: rdp => new Node { Contents = "Null Statement" });

            parser.AddRule(
                name: "expr",
                decider: rdp => rdp.TryMatch("term"),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Expr Statement" };
                    node.AddChild(rdp.InvokeRule("term"));
                    node.AddChild(rdp.InvokeRule("expr'"));
                    return node;
                });

            parser.AddRule(
                name: "expr'",
                decider: rdp => rdp.TryMatch("addop"),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Expr' Statement" };
                    node.AddChild(rdp.InvokeRule("addop"));
                    node.AddChild(rdp.InvokeRule("term"));
                    node.AddChild(rdp.InvokeRule("expr'"));
                    return node;
                });

            parser.AddRule(
                name: "expr'",
                decider: rdp => !rdp.TryMatch("addop"),
                evaluator: rdp => new Node {Contents = "Null expr"});

            parser.AddRule(
                name: "term",
                decider: rdp => rdp.TryMatch("factor"),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Term statement" };
                    node.AddChild(rdp.InvokeRule("factor"));
                    node.AddChild(rdp.InvokeRule("term'"));
                    return node;
                });

            parser.AddRule(
                name: "term'",
                decider: rdp => rdp.TryMatch("mulop"),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Term' statement" };
                    node.AddChild(rdp.InvokeRule("mulop"));
                    node.AddChild(rdp.InvokeRule("factor"));
                    node.AddChild(rdp.InvokeRule("term'"));
                    return node;
                });

            parser.AddRule(
                name: "term'",
                decider: rdp => !rdp.TryMatch("mulop"),
                evaluator: rdp => new Node() {Contents = "Null term"});

            parser.AddRule(
                name: "mulop",
                decider: rdp => rdp.TryMatch(TokenType.Mul),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "MUL statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Mul));
                    return node;
                });

            parser.AddRule(
                name: "mulop",
                decider: rdp => rdp.TryMatch(TokenType.Div),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "DIV statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Div));
                    return node;
                });

            parser.AddRule(
                name: "addop",
                decider: rdp => rdp.TryMatch(TokenType.Add),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "ADD statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Add));
                    return node;
                });

            parser.AddRule(
                name: "addop",
                decider: rdp => rdp.TryMatch(TokenType.Sub),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "SUB statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Sub));
                    return node;
                });

            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.LPar),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Factor statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.LPar));
                    node.AddChild(rdp.InvokeRule("expr"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.RPar));
                    return node;
                });

            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.Id),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Factor statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Id));
                    return node;
                });

            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.Num),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Factor statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Num));
                    return node;
                });

            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.Sub),
                evaluator: rdp =>
                {
                    var node = new Node { Contents = "Factor statement" };
                    node.AddChild(rdp.TryConsumeToken(TokenType.Sub));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Num));
                    return node;
                });
        }

        /// <summary>
        /// Ingests the file based on the passed command line arguments.
        /// If no filename is specified, picks a file randomly out of the ones given.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>String representing the complete text of the file.</returns>
        private static string GetSourceFileText(IReadOnlyList<string> args)
        {
            var fileName = "";
            if (args.Count != 0)
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
            return File.ReadAllText(fileName).Trim();
        }

        /// <summary>
        /// Adds all of the rules associated with assignment 1
        /// </summary>
        /// <param name="lexer">Lexer that is going to be performing the analysis</param>
        private static void AddLexerRules(Lexer.Lexer lexer)
        {
            lexer.AddRule(@"if\b", 
                s => new Token {Type = TokenType.If})
                .AddRule(@"then\b", 
                s => new Token {Type = TokenType.Then})
                .AddRule(@"while\b", 
                s => new Token {Type = TokenType.While})
                .AddRule(@"do\b", 
                s => new Token {Type = TokenType.Do})
                .AddRule(@"input\b", 
                s => new Token {Type = TokenType.Input})
                .AddRule(@"else\b", 
                s => new Token {Type = TokenType.Else})
                .AddRule(@"begin\b", 
                s => new Token {Type = TokenType.Begin})
                .AddRule(@"end\b", 
                s => new Token {Type = TokenType.End})
                .AddRule(@"write\b", 
                s => new Token {Type = TokenType.Write})
                .AddRule(@"[a-zA-Z_][a-zA-Z0-9_]*", 
                s => new Token {Contents = s, Type = TokenType.Id})
                .AddRule(@"[\d]+", 
                s => new Token {Contents = s, Type = TokenType.Num})
                .AddRule(@"\+", 
                s => new Token {Type = TokenType.Add})
                .AddRule(@":=", 
                s => new Token {Type = TokenType.Assign})
                .AddRule(@"-", 
                s => new Token {Type = TokenType.Sub})
                .AddRule(@"\*", 
                s => new Token {Type = TokenType.Mul})
                .AddRule(@"/", 
                s => new Token {Type = TokenType.Div})
                .AddRule(@"\(", 
                s => new Token {Type = TokenType.LPar})
                .AddRule(@"\)", 
                s => new Token { Type = TokenType.RPar})
                .AddRule(@";", 
                s => new Token {Type = TokenType.Semicolon});
        }
    }
}
