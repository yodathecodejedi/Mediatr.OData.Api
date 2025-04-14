using Mediatr.OData.Example.DomainRepository.Enumerations;
using Mediatr.OData.Example.DomainRepository.Extensions;
using System.Text;

namespace Mediatr.OData.Example.DomainRepository.Helpers;

internal class QueryBuilderHelper
{
    private readonly StringBuilder queryBuilder = new();
    private QueryBuilderMethod lastMethod = QueryBuilderMethod.None;
    private bool canBuild = false;
    private int queryCount = 0;

    public int QueryCount => queryCount;

    public void Clear()
    {
        queryBuilder.Clear();
        lastMethod = QueryBuilderMethod.None;
        canBuild = false;
        queryCount = 0;
    }

    public void AddQuery(string query)
    {
        var cleanQuery = query.Sanitize();
        if (lastMethod != QueryBuilderMethod.None && !queryBuilder.EndsWithSemiColon())
        {
            queryBuilder.Append(';');
        }
        queryBuilder.AppendLine();
        queryBuilder.Append(cleanQuery);
        lastMethod = QueryBuilderMethod.Query;
        canBuild = true;
        queryCount++;
    }

    public bool TryAddCondition(string condition)
    {
        if (lastMethod != QueryBuilderMethod.Query || queryBuilder.EndsWithSemiColon()) return false;
        if (!queryBuilder.EndsWithNewLine())
        {
            queryBuilder.AppendLine();
        }
        var cleanCondition = condition.Sanitize();
        queryBuilder.Append(cleanCondition);
        if (!cleanCondition.EndsWith(';'))
        {
            queryBuilder.Append(';');
        }
        lastMethod = QueryBuilderMethod.Condition;
        canBuild = true;
        return true;
    }

    public string Build()
    {
        if (canBuild)
        {
            if (!queryBuilder.EndsWithSemiColon())
            {
                queryBuilder.Append(';');
            }
            var cleanQuery = queryBuilder.ToString().Sanitize();
            return cleanQuery;
        }
        else
        {
            throw new InvalidOperationException($"Cannot build query when the last method was {lastMethod}");
        }
    }
}
