using PinkertonInterpreter.Grammar;

namespace PinkertonInterpreter.Grammar.Statements
{
    internal record ContinueStatement() : Statement;
    internal class ContinueException : Exception { }
}
