using PinkertonInterpreter.Grammar;

namespace Interpreter.Grammar.Statements
{
    internal record BreakStatement() : Statement;
    internal class BreakException : Exception { }

}
