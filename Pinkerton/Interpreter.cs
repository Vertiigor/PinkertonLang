using Interpreter.Grammar.Expressions;
using Interpreter.Grammar.Statements;
using PinkertonInterpreter.Grammar;
using PinkertonInterpreter.Grammar.Expressions;
using PinkertonInterpreter.Grammar.Statements;

namespace PinkertonInterpreter
{
    internal class Interpreter
    {
        public static Environment Globals = new();      // global variables and functions
        private Environment _environment = Globals;     // current environment (can be global or local)

        public Interpreter()
        {
            _environment = Globals;

            // Define built-in functions and constants

            Globals.Define("Pi", Math.PI);

            Globals.Define("E", Math.E);

            Globals.Define("sqrt",
                new NativeFunction(1, args => Math.Sqrt(Convert.ToDouble(args[0]))));

            Globals.Define("pow",
                new NativeFunction(2, args => Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))));

            Globals.Define("sin",
                new NativeFunction(1, args => Math.Sin(Convert.ToDouble(args[0]))));

            Globals.Define("cos",
                new NativeFunction(1, args => Math.Cos(Convert.ToDouble(args[0]))));

            Globals.Define("tan",
                new NativeFunction(1, args => Math.Tan(Convert.ToDouble(args[0]))));

            Globals.Define("cot",
                new NativeFunction(1, args => 1.0 / Math.Tan(Convert.ToDouble(args[0]))));

            Globals.Define("log",
                new NativeFunction(2, args =>
                {
                    if (args.Count == 1)
                        return Math.Log(Convert.ToDouble(args[0]));
                    else if (args.Count == 2)
                        return Math.Log(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]));
                    else
                        throw new Exception("log function takes 1 or 2 arguments");
                }));

            Globals.Define("exp",
                new NativeFunction(1, args => Math.Exp(Convert.ToDouble(args[0]))));

            Globals.Define("abs",
                new NativeFunction(1, args => Math.Abs(Convert.ToDouble(args[0]))));

            Globals.Define("floor",
                new NativeFunction(1, args => Math.Floor(Convert.ToDouble(args[0]))));

            Globals.Define("ceil",
                new NativeFunction(1, args => Math.Ceiling(Convert.ToDouble(args[0]))));

            Globals.Define("round",
                new NativeFunction(1, args => Math.Round(Convert.ToDouble(args[0]))));

            Globals.Define("max",
                new NativeFunction(2, args => Math.Max(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))));

            Globals.Define("min",
                new NativeFunction(2, args => Math.Min(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))));

            Globals.Define("random",
                new NativeFunction(2, args =>
                {
                    var min = Convert.ToDouble(args[0]);
                    var max = Convert.ToDouble(args[1]);
                    return new Random().NextDouble() * (max - min) + min;
                }));

            Globals.Define("toNumber",
                new NativeFunction(1, args => Convert.ToDouble(args[0])));

            Globals.Define("toBoolean",
                new NativeFunction(1, args => Convert.ToBoolean(args[0])));

            Globals.Define("toString",
                new NativeFunction(1, args => Convert.ToString(args[0])));

            Globals.Define("toChar",
                new NativeFunction(1, args => Convert.ToChar(Convert.ToInt32(args[0]))));

            Globals.Define("toOrd",
                new NativeFunction(1, args => Convert.ToInt32(Convert.ToChar(args[0]))));

            Globals.Define("isEmpty",
                new NativeFunction(1, args => (args[0] as List<object>).Count == 0));

            Globals.Define("size",
                new NativeFunction(1, args => Convert.ToDouble((args[0] as List<object>).Count)));

            Globals.Define("length",
                new NativeFunction(1, args => Convert.ToDouble(Convert.ToString(args[0]).Length)));

            Globals.Define("charAt",
                new NativeFunction(2, args => Convert.ToString(args[0]).ElementAt(Convert.ToInt32(args[1]))));

            Globals.Define("add",
                new NativeFunction(2, args =>
                {
                    (args[0] as List<object>).Add(args[1]);

                    return null;
                }));

            Globals.Define("insert",
                new NativeFunction(3, args =>
                {
                    (args[0] as List<object>).Insert((Convert.ToInt32(args[2])), args[1]);

                    return null;
                }));

            Globals.Define("remove",
                new NativeFunction(2, args =>
                {
                    (args[0] as List<object>).RemoveAt(Convert.ToInt32(args[1]));

                    return null;
                }));

            Globals.Define("clear",
                new NativeFunction(1, args =>
                {
                    (args[0] as List<object>).Clear();

                    return null;
                }));

            Globals.Define("contains",
                new NativeFunction(2, args =>
                {
                    var result = (args[0] as List<object>).Contains(args[1]);

                    return result;
                }));

            Globals.Define("assign",
                new NativeFunction(3, args =>
                {
                    (args[0] as List<object>)[Convert.ToInt32(args[1])] = args[2];

                    return null;
                }));

            Globals.Define("sort",
                new NativeFunction(1, args =>
                {
                    (args[0] as List<object>).Sort();
                    return null;
                }));

            Globals.Define("sorted",
                new NativeFunction(1, args =>
                {
                    var list = (args[0] as List<object>);
                    var sortedList = new List<object>(list);
                    
                    sortedList.Sort();

                    return sortedList;
                }));

            Globals.Define("reverse",
                new NativeFunction(1, args =>
                {
                    (args[0] as List<object>).Reverse();

                    return null;
                }));

            Globals.Define("reversed",
                new NativeFunction(1, args =>
                {
                    var list = (args[0] as List<object>);
                    var reversedList = new List<object>(list);
                    
                    reversedList.Reverse();

                    return reversedList;
                }));

            Globals.Define("join",
                new NativeFunction(2, args =>
                {
                    var list = args[0] as List<object>;
                    var separator = Convert.ToString(args[1]);
                    return '[' + string.Join(separator + ' ', list) + ']';
                }));

            Globals.Define("readLine",
                new NativeFunction(0, args => Console.ReadLine()));

            Globals.Define("read",
                new NativeFunction(0, args => Console.Read()));

            Globals.Define("foreach",
                new NativeFunction(2, args =>
                {
                    var list = args[0] as List<object>;
                    var action = args[1] as ICallable;
                    foreach (var item in list)
                    {
                        action.Call(this, new List<object> { item });
                    }
                    return null;
                }));

            Globals.Define("map",
                new NativeFunction(2, args =>
                {
                    var list = args[0] as List<object>;
                    var func = args[1] as ICallable;
                    var result = new List<object>();

                    foreach (var item in list)
                    {
                        result.Add(func.Call(this, new List<object> { item }));
                    }
                    
                    return result;
                }));

            Globals.Define("filter",
                new NativeFunction(2, args =>
                {
                    var list = args[0] as List<object>;
                    var predicate = args[1] as ICallable;
                    var result = new List<object>();

                    foreach (var item in list)
                    {
                        if (Convert.ToBoolean(predicate.Call(this, new List<object> { item })))
                        {
                            result.Add(item);
                        }
                    }

                    return result;
                }));

            Globals.Define("indexOf",
                new NativeFunction(2, args =>
                {
                    var list = args[0] as List<object>;
                    var item = args[1];
                    return list.IndexOf(item);
                }));
        }

        public object? Evaluate(Expression expr) => expr switch
        {
            Literal(var value) => value,

            Grouping(var inner) => Evaluate(inner),

            VariableExpression(var name) => _environment.Get(name),

            AssignmentExpression(var name, var value) => Assignment(name, value),

            CallExpression call => Call(call),

            ArrayLiteral(var elements) =>
                elements.Select(Evaluate).ToList(),

            RangeExpression(var left, var right, var step) => EvaluateRange(left, right, step),

            IndexExpression(var target, var index) => GetByIndex(target, index),

            SelectExpression(var con, var thenEx, var elseEx) => Select(con, thenEx, elseEx),

            Unary(var op, var right) => EvaluateUnary(op, Evaluate(right)),

            Binary(var left, var op, var right) => op.Type switch
            {
                TokenType.AND => IsTruthy(Evaluate(left)) ? Evaluate(right) : false,
                TokenType.OR => IsTruthy(Evaluate(left)) ? true : Evaluate(right),
                _ => EvaluateBinary(Evaluate(left), op, Evaluate(right))
            },

            InExpression(var left, var right) => EvaluateIn(Evaluate(left), Evaluate(right)),

            _ => throw new Exception("Unknown expression")
        };

        private object? GetByIndex(Expression target, Expression index)
        {
            var array = Evaluate(target) as List<object?>
                ?? throw new Exception("Target is not an array");

            if (index is RangeExpression range)
            {
                var result = new List<object?>();
                var rangeList = EvaluateRange(range.left, range.right, range.step) as List<object?>;

                foreach (var idx in rangeList)
                {
                    var j = Convert.ToInt32(idx);

                    if (j < 0 || j >= array.Count)
                        throw new Exception("Index out of bounds");
                    
                    result.Add(array[j]);
                }

                return result;
            }

            var i = Convert.ToInt32(Evaluate(index));

            return array[i];
        }

        private object EvaluateRange(Expression left, Expression right, Expression? step)
        {
            // 'a'..'z' or 1..10

            var leftValue = Evaluate(left);
            var rightValue = Evaluate(right);

            if (leftValue is char cl && rightValue is char cr)
            {
                var startC = (int)cl;
                var endC = (int)cr;
                var s = step != null ? Convert.ToInt32(Evaluate(step)) : 1;
                var listChar = new List<object>();

                if (startC <= endC)
                {
                    for (int i = startC; i <= endC; i += s)
                        listChar.Add((char)i);
                }
                else
                {
                    for (int i = startC; i >= endC; i -= s)
                        listChar.Add((char)i);
                }

                return listChar;
            }

            var start = Convert.ToDouble(leftValue);
            var end = Convert.ToDouble(rightValue);
            var stepValue = step != null ? Convert.ToDouble(Evaluate(step)) : 1.0;
            var list = new List<object>();

            if (stepValue == 0)
                throw new Exception("Step value cannot be zero.");

            if (start < end && stepValue < 0)
                throw new Exception("Step value must be positive for an increasing range.");

            if (start > end && stepValue > 0)
                throw new Exception("Step value must be negative for a decreasing range.");

            if (start <= end)
            {
                for (double i = start; i <= end; i += stepValue)
                    list.Add(i);
            }
            else
            {
                for (double i = start; i >= end; i -= stepValue)
                    list.Add(i);
            }

            return list;
        }

        private static object EvaluateIn(object? left, object? right)
        {
            if (right is List<object> list)
                return list.Contains(left);

            throw new Exception("Right operand of 'in' must be a range or array.");
        }

        private object? Select(Expression condition, Expression thenEx, Expression elseEx)
        {
            if (IsTruthy(Evaluate(condition)))
            {
                return Evaluate(thenEx);
            }
            else
            {
                return Evaluate(elseEx);
            }
        }

        public object? Execute(Statement stmt) => stmt switch
        {
            PrintStatement(var p) => Print(p),

            PrintLnStatement(var p) => PrintLn(p),

            ReturnStatement(var value) =>
                throw new ReturnException(value != null ? Evaluate(value) : null),

            VariableStatement(var name, var initializer) => DefineVariable(name, initializer),

            IfStatement(var condition, var thenBranch, var elseBranch) =>
                IsTruthy(Evaluate(condition))
                    ? Execute(thenBranch)
                    : elseBranch != null ? Execute(elseBranch) : null,

            WhileLoopStatement(var condition, var body) => WhileLoop(condition, body),

            ForLoopStatement(var initializer, var condition, var increment, var body) => ForLoop(initializer, condition, increment, body),

            BreakStatement => throw new BreakException(),

            ContinueStatement => throw new ContinueException(),

            FunctionStatement(var name, _, _) => Function(stmt as FunctionStatement, name),


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
                _environment = previous; // return to previous environment after block execution
            }

            return null;
        }

        private object? Print(Expression p)
        {
            var value = Evaluate(p);

            Console.Write(value);

            return null;
        }

        private object? PrintLn(Expression p)
        {
            var value = Evaluate(p);

            Console.WriteLine(value);
            
            return null;
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
                    break; // Let's exit the loop immediately
                }
                catch (ContinueException)
                {
                    continue; // Let's continue to the next iteration
                }
            }
            return null;
        }

        private object? ForLoop(Statement initializer, Expression condition, Expression increment, Statement body)
        {
            Execute(initializer);

            while (IsTruthy(Evaluate(condition)))
            {
                try
                {
                    Execute(body);
                }
                catch (BreakException)
                {
                    break; // Let's exit the loop immediately
                }
                catch (ContinueException)
                {
                    // Let's execute the increment and continue the loop
                }
            
                Evaluate(increment);
            }

            return null;
        }

        private object? Call(CallExpression call)
        {
            var callee = Evaluate(call.Callee);

            if (callee is not ICallable function)
                throw new Exception("Can only call functions.");

            var args = call.Arguments.Select(Evaluate).ToList();

            if (args.Count != function.Arity())
                throw new Exception("Wrong number of arguments.");

            return function.Call(this, args);
        }



        private object? Function(FunctionStatement stmt, Token token)
        {
            var function = new Function(stmt, _environment);

            _environment.Define(token.Lexeme, function);

            return null;
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
                TokenType.MOD => (double)left % (double)right,
                TokenType.EQUAL_EQUAL => Equals(left, right),
                TokenType.AINTSO => !Equals(left, right),
                TokenType.GREATER => (double)left > (double)right,
                TokenType.LESS => (double)left < (double)right,
                TokenType.GREATER_EQUAL => (double)left >= (double)right,
                TokenType.LESS_EQUAL => (double)left <= (double)right,
                TokenType.CONCAT => Add(left, right),
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
            if (a is List<object> listA && b is List<object> listB)
            {
                var result = new List<object>(listA);

                result.AddRange(listB);
                
                return result;
            }
            throw new Exception("Operands must be two numbers or strings");
        }
    }
}
