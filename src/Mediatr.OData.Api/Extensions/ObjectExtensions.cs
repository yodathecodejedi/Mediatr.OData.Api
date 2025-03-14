﻿using Mediatr.OData.Api.Utils;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Mediatr.OData.Api.Extensions;

public static class ObjectExtensions
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };


    public static object JsonConvert(this object source, Type targetType, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (targetType.IsEnum && source is string enumStr)
        {
            var enumResult = Enum.Parse(targetType, enumStr, ignoreCase: true);
            return enumResult;
        }

        var sourceStr = source is string str
            ? str
            : source.ToJsonString();

        jsonSerializerOptions ??= _serializerOptions;
        if (string.IsNullOrEmpty(sourceStr))
            return default!;

        var result = JsonSerializer.Deserialize(sourceStr, targetType, jsonSerializerOptions);
        return result!;
    }

    public static object JsonConvert(this string source, Type targetType, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (string.IsNullOrEmpty(source))
            return default!;

        jsonSerializerOptions ??= _serializerOptions;
        var result = JsonSerializer.Deserialize(source, targetType, jsonSerializerOptions);
        return result!;
    }

    public static string ToJsonString(this object source)
    {
        return JsonSerializer.Serialize(source);
    }

    public static object? ToType(this string? value, Type type)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException();

        return JsonSerializer.Deserialize(value, type, _serializerOptions);
    }

    public static object? GetPropertyValue(this object target, string propName)
    {
        ArgumentNullException.ThrowIfNull(target);

        var property = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property {propName} not found in {target.GetType().Name}");
        return property.GetValue(target);
    }

    public static T SetPropertyValue<T>(this T target, string propName, object? value)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var property = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property {propName} not found in {target.GetType().Name}");
        property.SetValue(target, value, null);

        return target;
    }

    public static T SetPropertyValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLambda, TValue value)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (memberLambda.Body is MemberExpression memberSelectorExpression)
        {
            var property = memberSelectorExpression.Member as PropertyInfo;
            var targetObject = ObjectUtils.GetTargetObject(memberSelectorExpression, target);
            if (property != null && targetObject != null)
            {
                property.SetValue(targetObject, value, null);
            }
        }

        return target;
    }
}
