using PinkertonInterpreter.Grammar;

namespace PinkertonInterpreter.Grammar.Statements
{
    internal record PrintStatement(Expression value) : Statement;
}