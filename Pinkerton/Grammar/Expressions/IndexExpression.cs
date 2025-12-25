using System;
using System.Collections.Generic;
using System.Text;

namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record IndexExpression(Expression Target, Expression Index) : Expression;
}
