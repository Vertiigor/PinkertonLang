using Interpreter.Grammar.Expressions;
using Interpreter.Grammar.Statements;
using PinkertonInterpreter.Exceptions;
using PinkertonInterpreter.Grammar;
using PinkertonInterpreter.Grammar.Expressions;
using PinkertonInterpreter.Grammar.Statements;
using Expression = PinkertonInterpreter.Grammar.Expression;

namespace PinkertonInterpreter
{
    internal class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;
        private Token Peek => _tokens[_current];
        private bool IsAtEnd => Peek.Type == TokenType.EOF;
        private Token Previous => _tokens[_current - 1];

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Statement> Parse()
        {
            List<Statement> statements = new List<Statement>();

            while (!IsAtEnd)
            {
                try
                {
                    statements.Add(ParseStatement());
                }
                catch (ParseException)
                {
                    Synchronize();
                }
            }

            return statements;
        }

        private Statement ParseStatement()
        {
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.PRINTLN)) return PrintLnStatement();
            if (Match(TokenType.VAR)) return VariableStatement();
            if (Match(TokenType.LEFT_BRACE)) return BlockStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.BREAK)) return BreakStatement();
            if (Match(TokenType.CONTINUE)) return ContinueStatement();
            if (Match(TokenType.FUNCTION)) return FunctionDeclaration();
            if (Match(TokenType.RETURN)) return ReturnStatement();

            return ExpressionStatement();
        }

        private Statement PrintLnStatement()
        {
            Expression value = Expression();

            return new PrintLnStatement(value);
        }

        private Statement BlockStatement()
        {
            List<Statement> statements = new List<Statement>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
            {
                statements.Add(ParseStatement());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect 'end' after block.");

            return new BlockStatement(statements);
        }

        private Statement IfStatement()
        {
            Expression condition = Expression();

            Consume(TokenType.THEN, "Expect 'then' after if condition.");

            Statement thenBranch = ParseStatement();
            Statement? elseBranch = null;

            if (Match(TokenType.ELSE))
            {
                elseBranch = ParseStatement();

            }

            return new IfStatement(condition, thenBranch, elseBranch);
        }

        private Expression SelectExpression()
        {
            Consume(TokenType.IF, "Expect 'if' before the condition.");

            Expression condition = Expression();
            Consume(TokenType.THEN, "Expect 'then' after if condition.");
            
            Expression thenBranch = Expression();
            Consume(TokenType.ELSE, "Expect 'else' after the condition.");

            Expression elseBranch = Expression();

            return new SelectExpression(condition, thenBranch, elseBranch);
        }

        private Statement WhileStatement()
        {
            Expression condition = Expression();
            Consume(TokenType.DO, "Expect 'do' after while condition.");

            Statement body = ParseStatement();

            return new WhileLoopStatement(condition, body);
        }

        private Statement ForStatement()
        {
            Statement? initializer;

            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VariableStatement();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expression? condition = null;
            
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }

            Expression? increment = null;
            
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }

            Statement body = ParseStatement();

            return new ForLoopStatement(initializer, condition, increment, body);
        }

        private Statement BreakStatement()
        {
            return new BreakStatement();
        }

        private Statement ContinueStatement()
        {
            return new ContinueStatement();
        }

        private Statement ReturnStatement()
        {
            Expression? value = Expression();

            return new ReturnStatement(value);
        }

        private Statement PrintStatement()
        {
            Expression value = Expression();

            return new PrintStatement(value);
        }

        private Statement ExpressionStatement()
        {
            Expression expr = Assignment();

            return new ExpressionStatement(expr);
        }

        private Statement VariableStatement()
        {
            // Variable declaration syntax:
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            // Optional initializer with ':='
            Expression? initializer = null;

            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            return new VariableStatement(name, initializer);
        }

        private Expression Assignment()
        {
            Expression expr = Pipeline();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous;
                Expression value = Assignment();

                if (expr is VariableExpression varExpr)
                    return new AssignmentExpression(varExpr.Name, value);

                throw Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expression Pipeline()
        {
            Expression expr = Not(); // Start with the next level down (Not)

            while (Match(TokenType.PIPE))
            {
                Token pipeToken = Previous;
                Expression right = Not();

                if (right is CallExpression call)
                {
                    call.Arguments.Insert(0, expr);
                    expr = call;
                }
                else
                {
                    throw Error(pipeToken, "Right side of |> must be a function call.");
                }
            }

            return expr;
        }

        private Expression Call()
        {
            Expression expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.LEFT_BRACKET))
                {
                    var index = Expression();

                    Consume(TokenType.RIGHT_BRACKET, "Expect ']' after index.");
                    
                    expr = new Grammar.Expressions.IndexExpression(expr, index);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expression FinishCall(Expression callee)
        {
            List<Expression> arguments = new();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count() >= 255)
                    {
                        Program.Error(Peek, "Can't have more than 255 arguments.");
                    }

                    arguments.Add(Expression());
                }
                while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new CallExpression(callee, paren, arguments);
        }

        private Statement FunctionDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect function name.");

            Consume(TokenType.LEFT_PAREN, "Expect '(' after function name.");

            var parameters = new List<Token>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    parameters.Add(
                        Consume(TokenType.IDENTIFIER, "Expect parameter name.")
                    );
                }
                while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            if (Match(TokenType.EQUAL))
            {
                Expression expr = Expression();
                var body2 = new List<Statement>
                {
                    new ReturnStatement(expr)
                };

                return new FunctionStatement(name, parameters, body2);
            }

            Consume(TokenType.LEFT_BRACE, "Expect 'begin' before function body.");

            var body = ((BlockStatement)BlockStatement()).Statements;

            return new FunctionStatement(name, parameters, body);
        }

        private Expression ArrayLiteral()
        {
            var elements = new List<Expression>();

            if (!Check(TokenType.RIGHT_BRACKET))
            {
                do
                {
                    elements.Add(Expression());
                }
                while (Match(TokenType.SEMICOLON));
            }

            Consume(TokenType.RIGHT_BRACKET, "Expect ']' after array literal.");

            return new ArrayLiteral(elements);
        }

        private Expression Expression()
        {
            return Assignment();
        }

        private Expression Not()
        {
            Expression expr = And();

            while (Match(TokenType.NOT))
            {
                Token operatorToken = Previous;
                Expression right = And();
                expr = new Unary(operatorToken, right);
            }

            return expr;
        }

        private Expression Or()
        {
            var expr = In();

            while (Match(TokenType.OR))
                expr = new Binary(expr, Previous, Equality());

            return expr;
        }

        private Expression And()
        {
            var expr = Or();

            while (Match(TokenType.AND))
                expr = new Binary(expr, Previous, Or());

            return expr;
        }

        private Expression In()
        {
            Expression expr = Equality();

            if (Match(TokenType.IN))
            {
                Expression right = Equality();

                return new Grammar.Expressions.InExpression(expr, right);
            }

            return expr;
        }

        private Expression Equality()
        {
            Expression expr = Comparison();

            while (Match(TokenType.AINTSO, TokenType.EQUAL_EQUAL))
            {
                Token operatorToken = Previous;
                Expression right = Comparison();
                expr = new Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expression Comparison()
        {
            Expression expr = Concat();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token operatorToken = Previous;
                Expression right = Term();
                expr = new Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expression Concat()
        {
            Expression expr = Range();

            while (Match(TokenType.CONCAT))
            {
                Token operatorToken = Previous;
                Expression right = Range();
                expr = new Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expression Range()
        {
            Expression expr = Term();

            if (Match(TokenType.RANGE)) // '..'
            {
                Token operatorToken = Previous;
                Expression right = Term();
                Expression? step = null;

                if (Match(TokenType.STEP))
                {
                    step = Term();
                }

                return new RangeExpression(expr, right, step);
            }

            return expr;
        }

        private Expression Term()
        {
            Expression expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token operatorToken = Previous;
                Expression right = Factor();
                expr = new Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expression Factor()
        {
            Expression expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR, TokenType.MOD))
            {
                Token operatorToken = Previous;
                Expression right = Unary();
                expr = new Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expression Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS, TokenType.NOT))
            {
                Token operatorToken = Previous;
                Expression right = Unary();
                return new Unary(operatorToken, right);
            }

            return Call(); // Call() is the next level down, which handles function calls and array indexing
        }

        private Expression Primary()
        {
            if (Match(TokenType.FALSE)) return new Literal(false);
            if (Match(TokenType.TRUE)) return new Literal(true);
            if (Match(TokenType.NULL)) return new Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING, TokenType.CHAR))
                return new Literal(Previous.Literal);

            if (Match(TokenType.IDENTIFIER))
                return new VariableExpression(Previous);

            if (Match(TokenType.LEFT_PAREN))
            {
                Expression expr = Expression();

                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                
                return new Grouping(expr);
            }

            if (Match(TokenType.LEFT_BRACKET)) return ArrayLiteral();

            if (Match(TokenType.SELECT)) return SelectExpression();

            throw new Exception($"Unexpected token: {Peek.Type}");
        }

        private void Synchronize()
        {
            Advance(); // Skip the current (bad) token

            while (!IsAtEnd)
            {
                // Lookahead token
                var type = Peek.Type;

                // These are considered "sync points" - likely starts of new statements
                if (type == TokenType.VAR ||
                    type == TokenType.FUNCTION ||
                    type == TokenType.PROCEDURE ||
                    type == TokenType.IF ||
                    type == TokenType.WHILE ||
                    type == TokenType.RETURN ||
                    type == TokenType.ELSE)
                {
                    return;
                }

                Advance(); // Skip token and keep scanning
            }
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek, message);
        }

        private ParseException Error(Token peek, string message)
        {
            Program.Error(peek, message);

            return new ParseException();
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();

                    return true;
                }
            }

            return false;
        }

        private Token Advance()
        {
            if (!IsAtEnd) _current++;

            return Previous;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd) return false;

            return Peek.Type == type;
        }
    }
}
