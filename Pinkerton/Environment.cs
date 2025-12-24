namespace PinkertonInterpreter
{
    internal class Environment
    {
        private readonly Dictionary<string, object?> _values = new();
        private readonly Environment? _enclosing;

        public Environment(Environment? enclosing = null)
        {
            _enclosing = enclosing;
        }

        public void Define(string name, object? value)
        {
            _values[name] = value;
        }

        public object? Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var value))
            {
                return value;
            }

            if (_enclosing != null) return _enclosing.Get(name);

            throw new Exception($"Undefined variable '{name.Lexeme}'.");
        }

        public void Assign(string name, object? value)
        {
            if (_values.ContainsKey(name))
            {
                _values[name] = value;
                return;
            }

            if (_enclosing != null)
            {
                _enclosing.Assign(name, value);
                return;
            }

            throw new Exception($"Undefined variable '{name}'.");
        }
    }

}
