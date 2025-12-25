namespace PinkertonInterpreter.Grammar.Expressions
{
    record CallExpression(
    Expression Callee,
    Token Paren,
    List<Expression> Arguments
) : Expression;

}
