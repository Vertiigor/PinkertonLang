namespace PinkertonInterpreter
{
    public enum ValueType
    {
        Int,
        Double,
        Bool,
        String,
        Null,
        Function
    }

    internal class Value
    {
        public ValueType Type { get; }
        public object? Data { get; }

        public Value(ValueType type, object? data)
        {
            Type = type;
            Data = data;
        }
    }
}
