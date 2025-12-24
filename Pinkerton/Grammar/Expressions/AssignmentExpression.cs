using PinkertonInterpreter;
using PinkertonInterpreter.Grammar;
namespace Interpreter.Grammar.Expressions
{
    internal record AssignmentExpression(Token Name, Expression Value) : Expression;
}
