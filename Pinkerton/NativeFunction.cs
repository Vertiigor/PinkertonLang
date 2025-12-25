namespace PinkertonInterpreter
{
    internal class NativeFunction : ICallable
    {
        private readonly int _arity;
        private readonly Func<List<object?>, object?> _function;

        public NativeFunction(int arity, Func<List<object?>, object?> function)
        {
            _arity = arity;
            _function = function;
        }

        public int Arity() => _arity;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return _function(arguments);
        }

        public override string ToString() => "<native fn>";
    }

}
