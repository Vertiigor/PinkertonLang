using PinkertonInterpreter.Grammar.Statements;

namespace PinkertonInterpreter
{
    internal class Function : ICallable
    {
        private readonly FunctionStatement _declaration;
        private readonly Environment _closure;

        public Function(FunctionStatement declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public int Arity() => _declaration.Parameters.Count;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var environment = new Environment(_closure);

            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(
                    _declaration.Parameters[i].Lexeme,
                    arguments[i]
                );
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnException ret)
            {
                return ret.Value;
            }

            return null;
        }
    }
}
