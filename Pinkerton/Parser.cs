using Interpreter.Grammar.Expressions;
using Interpreter.Grammar.Statements;
using PinkertonInterpreter.Exceptions;
using PinkertonInterpreter.Grammar;
using PinkertonInterpreter.Grammar.Expressions;
using PinkertonInterpreter.Grammar.Statements;

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
            if (Match(TokenType.INPUT)) return InputStatement();
            if (Match(TokenType.LEFT_BRACE)) return BlockStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.BREAK)) return BreakStatement();
            if (Match(TokenType.CONTINUE)) return ContinueStatement();
            if (Match(TokenType.FUNCTION)) return FunctionDeclaration();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            // сюда потом можно добавить: if (Match(TokenType.IF)) return IfStatement();
            // if (Match(TokenType.WHILE)) return WhileStatement();
            // ...

            return ExpressionStatement();
        }

        private Statement PrintLnStatement()
        {
            // Не Match(TokenType.SCREAM) здесь — токен уже съеден в ParseStatement()
            Expression value = Expression();
            //Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new PrintLnStatement(value);
        }

        private Statement BlockStatement()
        {
            List<Statement> statements = new List<Statement>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
            {
                statements.Add(ParseStatement());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

            return new BlockStatement(statements);
        }

        private Statement IfStatement()
        {
            //Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expression condition = Expression();
            //Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");
            Consume(TokenType.THEN, "Expect 'then' after if condition.");
            Statement thenBranch = ParseStatement();
            Statement? elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = ParseStatement();
            }
            return new IfStatement(condition, thenBranch, elseBranch);
        }

        private Statement WhileStatement()
        {
            //Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expression condition = Expression();
            Consume(TokenType.DO, "Expect 'do' after while condition.");
            //Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");
            Statement body = ParseStatement();
            return new WhileLoopStatement(condition, body);
        }

        private Statement BreakStatement()
        {
            //Consume(TokenType.SEMICOLON, "Expect ';' after break.");
            return new BreakStatement();
        }

        private Statement ContinueStatement()
        {
            //Consume(TokenType.SEMICOLON, "Expect ';' after continue.");
            return new ContinueStatement();
        }

        private Statement ReturnStatement()
        {
            Expression? value = Expression();

            //Consume(TokenType.SEMICOLON, "Expect ';' after return value.");

            return new ReturnStatement(value);
        }

        private Statement PrintStatement()
        {
            // Не Match(TokenType.SCREAM) здесь — токен уже съеден в ParseStatement()
            Expression value = Expression();
            //Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new PrintStatement(value);
        }

        private Statement InputStatement()
        {
            var name = Consume(TokenType.IDENTIFIER, "Expect variable name after 'input'.");
            //Consume(TokenType.SEMICOLON, "Expect ';' after input statement.");
            return new InputStatement(name);
        }


        private Statement ExpressionStatement()
        {
            Expression expr = Assignment(); // вместо Expression()
            //Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new ExpressionStatement(expr);
        }


        private Statement VariableStatement()
        {
            // 1. Имя переменной
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            // 2. Опциональный тип через 'as'
            Token? typeToken = null;
            if (Match(TokenType.AS))
            {
                // Проверяем несколько вариантов типов вручную
                if (Check(TokenType.INT)) typeToken = Advance();
                else if (Check(TokenType.DOUBLE_KW)) typeToken = Advance();
                else if (Check(TokenType.FLOAT_KW)) typeToken = Advance();
                else if (Check(TokenType.STRING_KW)) typeToken = Advance();
                else if (Check(TokenType.BOOL_KW)) typeToken = Advance();
                else throw Error(Peek, "Expect type after 'as'.");
            }

            // 3. Опциональное присваивание через 'is'
            Expression? initializer = null;
            if (Match(TokenType.EQUAL)) // твой 'is'
            {
                initializer = Expression();
            }

            // 4. Обязательный конец с ';'
            //Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

            return new VariableStatement(name, initializer /*, typeToken*/);
        }

        private Expression Assignment()
        {
            Expression expr = Equality(); // или Expression() предыдущего уровня

            if (Match(TokenType.EQUAL)) // твой 'is'
            {
                Token equals = Previous;
                Expression value = Assignment(); // рекурсивно для цепочек

                if (expr is VariableExpression varExpr)
                {
                    return new AssignmentExpression(varExpr.Name, value);
                }

                throw Error(equals, "Invalid assignment target.");
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
                    expr = new IndexExpression(expr, index);
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
            Consume(TokenType.LEFT_BRACE, "Expect '{' before function body.");

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
            return Not();
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
            var expr = Equality();

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
            Expression expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token operatorToken = Previous;
                Expression right = Term();
                expr = new Binary(expr, operatorToken, right);
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

            while (Match(TokenType.SLASH, TokenType.STAR, TokenType.REMAINDER))
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

            return Call(); // ← ВАЖНО
        }


        private Expression Primary()
        {
            if (Match(TokenType.FALSE)) return new Literal(false);
            if (Match(TokenType.TRUE)) return new Literal(true);
            if (Match(TokenType.NULL)) return new Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
                return new Literal(Previous.Literal);

            if (Match(TokenType.IDENTIFIER))
                return new VariableExpression(Previous);

            if (Match(TokenType.LEFT_PAREN))
            {
                Expression expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            if (Match(TokenType.LEFT_BRACKET))
            {
                return ArrayLiteral();
            }


            throw new Exception($"Unexpected token: {Peek.Type}");
        }



        private void Synchronize()
        {
            Advance(); // Skip the current (bad) token

            while (!IsAtEnd)
            {
                // Lookahead token
                var type = Peek.Type;

                // These are considered "sync points" — likely starts of new statements
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
