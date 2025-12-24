using Interpreter.Grammar.Expressions;
using Interpreter.Grammar.Statements;
using PinkertonInterpreter.Grammar;
using PinkertonInterpreter.Grammar.Expressions;
using PinkertonInterpreter.Grammar.Statements;

namespace PinkertonInterpreter
{
    internal class Interpreter
    {
        public static Environment Globals = new();      // глобальные переменные
        private Environment _environment = Globals;     // текущая область видимости

        public Interpreter()
        {
            _environment = Globals;

            Globals.Define("PI", Math.PI);

            Globals.Define("E", Math.E);

            Globals.Define("sin", new Func<double, double>(Math.Sin));
            Globals.Define("cos", new Func<double, double>(Math.Cos));
            Globals.Define("tan", new Func<double, double>(Math.Tan));
            Globals.Define("cot", new Func<double, double>(x => 1.0 / Math.Tan(x)));
            Globals.Define("sqrt", new Func<double, double>(Math.Sqrt));
        }

        public object? Evaluate(Expression expr) => expr switch
        {
            Literal(var value) => value,

            Grouping(var inner) => Evaluate(inner),

            VariableExpression(var name) => _environment.Get(name),

            AssignmentExpression(var name, var value) => Assignment(name, value),

            CallExpression call => Call(call),

            Unary(var op, var right) => EvaluateUnary(op, Evaluate(right)),

            Binary(var left, var op, var right) => op.Type switch
            {
                TokenType.AND => IsTruthy(Evaluate(left)) ? Evaluate(right) : false,
                TokenType.OR => IsTruthy(Evaluate(left)) ? true : Evaluate(right),
                _ => EvaluateBinary(Evaluate(left), op, Evaluate(right))
            },

            _ => throw new Exception("Unknown expression")
        };

        // Новый метод для Statement
        public object? Execute(Statement stmt) => stmt switch
        {
            PrintStatement(var p) => Print(p),

            InputStatement(var name) => Input(name),

            VariableStatement(var name, var initializer) => DefineVariable(name, initializer),

            IfStatement(var condition, var thenBranch, var elseBranch) =>
                IsTruthy(Evaluate(condition))
                    ? Execute(thenBranch)
                    : elseBranch != null ? Execute(elseBranch) : null,

            WhileLoopStatement(var condition, var body) => WhileLoop(condition, body),

            BreakStatement => throw new BreakException(),

            ContinueStatement => throw new ContinueException(),

            ExpressionStatement(var expr) => Evaluate(expr),

            BlockStatement(var statements) => ExecuteBlock(statements, new Environment(_environment)),

            _ => throw new Exception("Unknown statement")
        };

        public object? ExecuteBlock(List<Statement> statements, Environment env)
        {
            var previous = _environment;
            _environment = env;

            try
            {
                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _environment = previous; // возвращаем старую область после блока
            }

            return null;
        }

        private object? Print(Expression p)
        {
            var value = Evaluate(p);

            Console.WriteLine(value);

            return null;
        }

        private object? Input(Token name)
        {
            var input = Console.ReadLine();

            if (input == null)
                throw new Exception("Input error");

            // пока просто строка
            _environment.Assign(name.Lexeme, input);

            return input;
        }

        private object? WhileLoop(Expression condition, Statement body)
        {
            while (IsTruthy(Evaluate(condition)))
            {
                try
                {
                    Execute(body);
                }
                catch (BreakException)
                {
                    break; // ВАЖНО
                }
                catch (ContinueException)
                {
                    continue; // ВАЖНО
                }
            }
            return null;
        }

        private object? Call(CallExpression expr)
        {
            var callee = Evaluate(expr.Callee);

            var args = expr.Arguments
                .Select(Evaluate)
                .ToList();

            if (callee is Delegate fn)
            {
                return fn.DynamicInvoke(args.ToArray());
            }

            throw new Exception("Can only call functions");
        }


        private object? Assignment(Token name, Expression value)
        {
            var evaluatedValue = Evaluate(value);
            _environment.Assign(name.Lexeme, evaluatedValue);
            return evaluatedValue;
        }

        private object? DefineVariable(Token name, Expression? initializer)
        {
            var value = initializer != null ? Evaluate(initializer) : null;
            _environment.Define(name.Lexeme, value);
            return null;
        }

        private static object? EvaluateUnary(Token op, object? right)
        {
            return op.Type switch
            {
                TokenType.MINUS => -(double)(right ?? 0),
                TokenType.BANG or TokenType.NOT => !IsTruthy(right),
                _ => throw new Exception($"Unknown unary operator: {op.Type}")
            };
        }

        private static object? EvaluateBinary(object? left, Token op, object? right)
        {
            return op.Type switch
            {
                TokenType.PLUS => Add(left, right),
                TokenType.MINUS => (double)left - (double)right,
                TokenType.STAR => (double)left * (double)right,
                TokenType.SLASH => (double)left / (double)right,
                TokenType.REMAINDER => (double)left % (double)right,
                TokenType.EQUAL_EQUAL => Equals(left, right),
                TokenType.AINTSO => !Equals(left, right),
                TokenType.GREATER => (double)left > (double)right,
                TokenType.LESS => (double)left < (double)right,
                TokenType.GREATER_EQUAL => (double)left >= (double)right,
                TokenType.LESS_EQUAL => (double)left <= (double)right,
                _ => throw new Exception($"Unknown binary operator: {op.Type}")
            };
        }

        private static bool IsTruthy(object? value)
        {
            return value switch
            {
                null => false,
                bool b => b,
                _ => true
            };
        }

        private static object Add(object? a, object? b)
        {
            if (a is int ai && b is int bi) return ai + bi;
            if (a is double ad && b is double bd) return ad + bd;
            if (a is string sa && b is string sb) return sa + sb;
            if (a is string s && b != null) return s + b.ToString();
            if (a != null && b is string s2) return a.ToString() + s2;
            throw new Exception("Operands must be two numbers or strings");
        }
    }
}
