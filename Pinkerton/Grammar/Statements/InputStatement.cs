using PinkertonInterpreter;
using PinkertonInterpreter.Grammar;

namespace PinkertonInterpreter.Grammar.Statements
{
    internal record InputStatement(Token Name) : Statement;
}
