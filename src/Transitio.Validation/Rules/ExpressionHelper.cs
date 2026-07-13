using System;
using System.Linq.Expressions;

namespace Transitio.Validation;

internal static class ExpressionHelper
{
    /// <summary>
    /// Extracts the member name from a property/field access expression, unwrapping any
    /// implicit conversion the compiler inserts for value types.
    /// </summary>
    public static string GetMemberName<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var body = expression.Body;

        if (body is UnaryExpression unary)
            body = unary.Operand;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException(
            "RuleFor expression must be a simple property or field access (e.g. x => x.Name).",
            nameof(expression));
    }
}
