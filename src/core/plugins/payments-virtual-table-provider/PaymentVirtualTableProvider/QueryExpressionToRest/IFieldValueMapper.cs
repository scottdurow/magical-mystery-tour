using System.Collections.Generic;

namespace PaymentVirtualTableProvider.QueryExpressionToRest;

public interface IFieldValueMapper
{
    KeyValuePair<string, object> MapToApi(string fieldName, object value);
    KeyValuePair<string, object> MapToDataverse(string fieldName, object value);
}