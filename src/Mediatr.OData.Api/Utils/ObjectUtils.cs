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
}
