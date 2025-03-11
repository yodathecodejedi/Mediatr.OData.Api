using System.Linq.Expressions;
using System.Reflection;

namespace Mediatr.OData.Api.Utils;

public static class ObjectUtils
{
    public static object GetTargetObject(MemberExpression memberSelectorExpression, object target)
    {
        var expressionStack = new Stack<MemberExpression>();
        while (memberSelectorExpression.Expression is MemberExpression memberExpression)
        {
            expressionStack.Push(memberSelectorExpression);
            memberSelectorExpression = memberExpression;
        }

        expressionStack.Push(memberSelectorExpression);

        while (expressionStack.Count > 1)
        {
            var expression = expressionStack.Pop();
            var propertyInfo = expression.Member as PropertyInfo;
            target = propertyInfo?.GetValue(target, null)!;
        }

        return target;
    }

    //public static Action<TModel, TKey> BuildSetter<TModel, TKey>(PropertyInfo propertyInfo)
    //{
    //    ArgumentNullException.ThrowIfNull(propertyInfo);

    //    if (!propertyInfo.CanWrite)
    //        throw new InvalidOperationException($"The property '{propertyInfo.Name}' does not have a setter.");

    //    var targetType = typeof(TModel);

    //    // Create parameter expressions
    //    var targetParameter = Expression.Parameter(targetType, "target");
    //    var valueParameter = Expression.Parameter(typeof(TKey), "value");

    //    // Convert the value to the property's type
    //    var convertedValue = Expression.Convert(valueParameter, propertyInfo.PropertyType);

    //    // Create a property access memberExpression
    //    var propertyAccess = Expression.Property(targetParameter, propertyInfo);

    //    // Create an assignment memberExpression
    //    var assign = Expression.Assign(propertyAccess, convertedValue);

    //    // Compile the setter
    //    var setterLambda = Expression.Lambda<Action<TModel, TKey>>(assign, targetParameter, valueParameter);
    //    return setterLambda.Compile();
    //}
}
