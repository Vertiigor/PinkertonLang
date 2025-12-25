namespace PinkertonInterpreter
{
    internal interface ICallable
    {
        int Arity(); // сколько аргументов
        object? Call(Interpreter interpreter, List<object?> arguments);
    }

}
