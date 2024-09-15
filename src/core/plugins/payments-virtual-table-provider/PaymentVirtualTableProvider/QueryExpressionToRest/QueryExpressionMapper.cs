using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PaymentVirtualTableProvider.QueryExpressionToRest;
using System;
using System.Linq;
using System.Text;


public class QueryExpressionMapper
{
    private IFieldValueMapper _mapper;

    public QueryExpressionMapper(IFieldValueMapper mapper)
    {
        _mapper = mapper;
    }
    public string ToRestQuery(QueryExpression query)
    {
        var sb = new StringBuilder();
        sb.Append("$");

        // Extract filters
        if (query.Criteria != null && (query.Criteria.Conditions.Any() || query.Criteria.Filters.Any()))
        {
            var filter = ExtractFilter(query.Criteria);
            sb.Append($"filter={filter}&");
        }

        // Extract orderby
        if (query.Orders != null && query.Orders.Any())
        {
            var orderby = string.Join(",", query.Orders.Select(o =>
            {
                var mapped = _mapper.MapToApi(o.AttributeName, "");
                var orderType = o.OrderType == OrderType.Ascending ? "asc" : "desc";
                return $"{mapped.Key} {orderType}";
            }));
            
            sb.Append($"orderby={orderby}&");
        }

        // Extract top (if no value then set to top 1000)
        if (query.TopCount.HasValue)
        {
            sb.Append($"top={query.TopCount.Value}&");
        }
        else
        {
            sb.Append("top=1000&");
        }

        // Extract skip
        if (query.PageInfo != null && query.PageInfo.PageNumber > 1)
        {
            sb.Append($"skip={(query.PageInfo.PageNumber - 1) * query.PageInfo.Count}&");
        }

        // Remove the trailing '&' if present
        var result = sb.ToString().TrimEnd('&');
        return result;
    }

    private string ExtractFilter(FilterExpression filter)
    {
        var conditions = filter.Conditions.Select(c => ExtractCondition(c));
        var filters = filter.Filters.Select(f => ExtractFilter(f));

        var allConditions = conditions.Concat(filters);
        return string.Join($" {filter.FilterOperator.ToString().ToLower()} ", allConditions);
    }

    private string ExtractCondition(ConditionExpression condition)
    {
        var mapped = _mapper.MapToApi(condition.AttributeName,condition.Values.First());

        return $"{mapped.Key} {GetOperator(condition.Operator)} {FormatValue(mapped.Value)}";
    }

    private string GetOperator(ConditionOperator op)
    {
        return op switch
        {
            ConditionOperator.Equal => "eq",
            ConditionOperator.NotEqual => "ne",
            ConditionOperator.GreaterThan => "gt",
            ConditionOperator.LessThan => "lt",
            ConditionOperator.GreaterEqual => "ge",
            ConditionOperator.LessEqual => "le",
            ConditionOperator.On => "eq",
            ConditionOperator.OnOrAfter => "ge",
            ConditionOperator.OnOrBefore => "le",
            _ => throw new NotSupportedException($"ConditionOperator '{op}' is not supported."),
        };
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            string s => $"'{s}'",
            DateTime dt => $"'{dt:O}'",
            EntityReference er => $"'{er.Id.ToString()}'",
            _ => value.ToString()
        };
    }
}


