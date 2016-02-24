using CPSC411.Lexer;
using CPSC411.RDParser;

namespace CPSC411
{
    public static class RulesModule
    {
        /// <summary>
        /// Adds all of the rules associated with assignment 1
        /// </summary>
        /// <param name="lexer">Lexer that is going to be performing the analysis</param>
        public static void AddLexerRules(Lexer.Lexer lexer)
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
                    s => new Token {Type = TokenType.RPar})
                .AddRule(@";",
                    s => new Token {Type = TokenType.Semicolon});
        }


        /// <summary>
        /// Adds the following grammar rules to the RDP parser passed into the function:
        /// 
        /// base        -> stmt
        /// stmt        -> IF expr THEN stmt ELSE stmt
        ///              | WHILE expr DO stmt
        ///              | INPUT ID
        ///              | ID ASSIGN expr
        ///              | WRITE expr
        ///              | BEGIN stmtlist END
        /// stmtlist    -> stmt SEMICOLON stmtlist'
        /// stmtlist'   -> stmt SEMICOLON stmtlist'
        ///              | .
        /// expr        -> term expr'
        /// expr'       -> addop term expr'
        ///              | .
        /// term        -> factor term'
        /// term'       -> mulop factor term'
        ///              | .
        /// factor      -> LPAR expr RPAR
        ///              | ID
        ///              | NUM
        ///              | SUB NUM
        /// addop       -> ADD | SUB
        /// mulop       -> MUL | DIV
        /// </summary>
        /// <param name="parser">Parser that should accept the rules</param>
        public static void AddRdpRules(RecursiveDescentParser parser)
        {
            // base -> stmt
            parser.AddRule(
                name: "base",
                decider: rdp => true,
                evaluator: rdp => rdp.InvokeRule("stmt"));

            // stmt -> IF expr THEN stmt ELSE stmt
            parser.AddRule( // if
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.If),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "IF Statement", Type = NodeType.If};
                    node.AddChild(rdp.TryConsumeToken(TokenType.If));
                    node.AddChild(rdp.InvokeRule("expr"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Then));
                    node.AddChild(rdp.InvokeRule("stmt"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Else));
                    node.AddChild(rdp.InvokeRule("stmt"));
                    return node;
                });

            //  stmt -> WHILE expr DO stmt
            parser.AddRule( // while
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.While),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "WHILE Statement", Type = NodeType.While};
                    node.AddChild(rdp.TryConsumeToken(TokenType.While));
                    node.AddChild(rdp.InvokeRule("expr"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Do));
                    node.AddChild(rdp.InvokeRule("stmt"));
                    return node;
                });

            // stmt -> INPUT ID
            parser.AddRule( // input
                name: "stmt",
                decider: rpd => rpd.TryMatch(TokenType.Input),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "INPUT Statement", Type = NodeType.Input};
                    node.AddChild(rdp.TryConsumeToken(TokenType.Input));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Id));
                    return node;
                });

            // stmt -> ID ASS expr
            parser.AddRule( // assign
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.Id),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Assign Statement", Type = NodeType.Assign};
                    node.AddChild(rdp.TryConsumeToken(TokenType.Id));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Assign));
                    node.AddChild(rdp.InvokeRule("expr"));
                    return node;
                });

            // stmt -> WRITE expr
            parser.AddRule( // write
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.Write),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "WRITE Statement", Type = NodeType.Write};
                    node.AddChild(rdp.TryConsumeToken(TokenType.Write));
                    node.AddChild(rdp.InvokeRule("expr"));
                    return node;
                });

            // stmt -> BEGIN stmtlist END
            parser.AddRule( // begin
                name: "stmt",
                decider: rdp => rdp.TryMatch(TokenType.Begin),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "BEGIN Statement", Type = NodeType.Begin};
                    node.AddChild(rdp.TryConsumeToken(TokenType.Begin));
                    node.AddChild(rdp.InvokeRule("stmtlist"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.End));
                    return node;
                });

            // stmtlist -> stmt SEMICOLON stmtlist'
            parser.AddRule(
                name: "stmtlist",
                decider: rdp => rdp.TryMatch("stmt"),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Statement List", Type = NodeType.StatementList};
                    node.AddChild(rdp.InvokeRule("stmt"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Semicolon));
                    node.AddChild(rdp.InvokeRule("stmtlist'"));
                    return node;
                });

            // Transformed rule
            // stmtlist' -> stmt SEMICOLON stmtlist'
            parser.AddRule(
                name: "stmtlist'",
                decider: rdp => rdp.TryMatch("stmt"),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Statement List", Type = NodeType.MoreStatements};
                    
                    node.AddChild(rdp.InvokeRule("stmt"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Semicolon));
                    node.AddChild(rdp.InvokeRule("stmtlist'"));
                    return node;
                });

            // Transformed rule
            // stmtlist' -> .
            parser.AddRule(
                name: "stmtlist'",
                decider: rdp => !rdp.TryMatch("stmt"),
                evaluator: rdp => new Node {Contents = "Null Statement", Type = NodeType.Null});

            // expr -> term expr'
            parser.AddRule(
                name: "expr",
                decider: rdp => rdp.TryMatch("term"),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Expr Statement", Type = NodeType.Expression};
                    node.AddChild(rdp.InvokeRule("term"));
                    node.AddChild(rdp.InvokeRule("expr'"));
                    return node;
                });

            // Transformed rule
            // expr' -> addop term expr'
            parser.AddRule(
                name: "expr'",
                decider: rdp => rdp.TryMatch("addop"),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Expr' Statement", Type = NodeType.MoreExpression};
                    node.AddChild(rdp.InvokeRule("addop"));
                    node.AddChild(rdp.InvokeRule("term"));
                    node.AddChild(rdp.InvokeRule("expr'"));
                    return node;
                });

            // Transformed rule
            // expr' -> .
            parser.AddRule(
                name: "expr'",
                decider: rdp => !rdp.TryMatch("addop"),
                evaluator: rdp => new Node {Contents = "Null expr", Type = NodeType.Null});

            // term -> factor term'
            parser.AddRule(
                name: "term",
                decider: rdp => rdp.TryMatch("factor"),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Term statement", Type = NodeType.Term};
                    node.AddChild(rdp.InvokeRule("factor"));
                    node.AddChild(rdp.InvokeRule("term'"));
                    return node;
                });

            // Transformed rule
            // term' -> mulop factor term'
            parser.AddRule(
                name: "term'",
                decider: rdp => rdp.TryMatch("mulop"),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Term' statement", Type = NodeType.MoreTerms};
                    node.AddChild(rdp.InvokeRule("mulop"));
                    node.AddChild(rdp.InvokeRule("factor"));
                    node.AddChild(rdp.InvokeRule("term'"));
                    return node;
                });

            // Transformed rule
            // term' -> .
            parser.AddRule(
                name: "term'",
                decider: rdp => !rdp.TryMatch("mulop"),
                evaluator: rdp => new Node {Contents = "Null term", Type = NodeType.Null});

            // mulop -> MUL
            parser.AddRule(
                name: "mulop",
                decider: rdp => rdp.TryMatch(TokenType.Mul),
                evaluator: rdp => rdp.TryConsumeToken(TokenType.Mul));

            // mulop -> DIV
            parser.AddRule(
                name: "mulop",
                decider: rdp => rdp.TryMatch(TokenType.Div),
                evaluator: rdp => rdp.TryConsumeToken(TokenType.Div));

            // addop -> ADD
            parser.AddRule(
                name: "addop",
                decider: rdp => rdp.TryMatch(TokenType.Add),
                evaluator: rdp => rdp.TryConsumeToken(TokenType.Add));

            // addop -> SUB
            parser.AddRule(
                name: "addop",
                decider: rdp => rdp.TryMatch(TokenType.Sub),
                evaluator: rdp => rdp.TryConsumeToken(TokenType.Sub));

            // factor -> LPAR expr RPAR
            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.LPar),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Factor statement", Type = NodeType.Factor};
                    node.AddChild(rdp.TryConsumeToken(TokenType.LPar));
                    node.AddChild(rdp.InvokeRule("expr"));
                    node.AddChild(rdp.TryConsumeToken(TokenType.RPar));
                    return node;
                });

            // factor -> ID
            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.Id),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Factor statement", Type = NodeType.Factor};
                    node.AddChild(rdp.TryConsumeToken(TokenType.Id));
                    return node;
                });

            // factor -> NUM
            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.Num),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Factor statement", Type = NodeType.Factor};
                    node.AddChild(rdp.TryConsumeToken(TokenType.Num));
                    return node;
                });

            // factor -> SUB NUM
            parser.AddRule(
                name: "factor",
                decider: rdp => rdp.TryMatch(TokenType.Sub),
                evaluator: rdp =>
                {
                    var node = new Node {Contents = "Factor statement", Type = NodeType.Factor};
                    node.AddChild(rdp.TryConsumeToken(TokenType.Sub));
                    node.AddChild(rdp.TryConsumeToken(TokenType.Num));
                    return node;
                });
        }
    }
}