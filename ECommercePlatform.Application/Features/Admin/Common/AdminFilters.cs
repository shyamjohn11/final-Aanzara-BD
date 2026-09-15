using System.Linq.Expressions;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Admin.Common;

/// <summary>
/// Combines filter expressions without Expression.Invoke (which EF Core cannot
/// translate) by rebinding both sides onto one shared parameter.
/// </summary>
public static class AdminFilters
{
    public static Expression<Func<T, bool>> True<T>()
        where T : AdminEntity
        => _ => true;

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
        where T : AdminEntity
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var body = Expression.AndAlso(
            new ParameterReplacer(left.Parameters[0], parameter).Visit(left.Body)!,
            new ParameterReplacer(right.Parameters[0], parameter).Visit(right.Body)!);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private sealed class ParameterReplacer(
        ParameterExpression source, ParameterExpression target)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == source ? target : base.VisitParameter(node);
    }
}
