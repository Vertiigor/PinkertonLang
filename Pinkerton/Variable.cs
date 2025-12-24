namespace PinkertonInterpreter
{
    internal class Variable
    {
        public ValueType DeclaredType { get; }
        public Value Value { get; set; }

        public Variable(ValueType declaredType, Value value)
        {
            DeclaredType = declaredType;
            Value = value;
        }
    }

}
