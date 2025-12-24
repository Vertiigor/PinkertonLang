
using PinkertonInterpreter;
using PinkertonInterpreter.Grammar;

namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record VariableExpression(Token Name) : Expression;
}
