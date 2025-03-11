using Microsoft.AspNetCore.OData.Query;
using System.Linq.Expressions;

namespace Mediatr.OData.Api.Extensions;

public static class ODataQueryOptionsExtensions
{
    public static object? ApplyToGetByKey<T>(this ODataQueryOptions options, IQueryable<T>? data, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var single = data.FirstOrDefault(predicate);

        //Haal mijn data op, op de manier zoals wij dat fijn vinden
        var result = options.ApplyTo(single, new ODataQuerySettings { },
        AllowedQueryOptions.OrderBy |
        AllowedQueryOptions.Filter |
        AllowedQueryOptions.DeltaToken |
        AllowedQueryOptions.Search |
        AllowedQueryOptions.Top |
        AllowedQueryOptions.Skip |
        AllowedQueryOptions.SkipToken
        );

        return result;
    }
}

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>() { return f => true; }
    public static Expression<Func<T, bool>> False<T>() { return f => false; }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                        Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>
              (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                         Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>
              (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }
}